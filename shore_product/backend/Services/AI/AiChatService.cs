using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.DTOs;
using ProductApi.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProductApi.Services.AI
{
    public class AiChatService : IAiChatService
    {
        private readonly AppDbContext _context;
        private readonly IGeminiEvaluationService _geminiService;
        private readonly ILogger<AiChatService> _logger;

        // System prompt template - cố định, tái sử dụng
        private const string SYSTEM_PROMPT_TEMPLATE = @"Bạn là một chuyên gia phân tích dữ liệu hàng hải chuyên nghiệp.

THÔNG TIN NGỮ CẢNH:
- Báo cáo ngày hôm nay: {TODAY_DATA}
- Dữ liệu tóm tắt 7 ngày qua: {LAST_7DAYS_SUMMARY}

YÊUẦU CÔNG VIỆC:
Người dùng hỏi: {QUESTION}

Hãy:
1. Trả lời câu hỏi dựa trên phân tích số liệu kỹ thuật sâu
2. So sánh đa chiều với xu hướng 7 ngày cùng giải thích các nguyên nhân khách/chủ quan (thời tiết, tải máy...)
3. Đưa ra nhận xét chuyên gia, rủi ro dự báo và các lời khuyên dài hạn (trung bình 400 từ)

Format: Trả lời bằng Tiếng Việt, có tính học thuật chuyên ngành, cung cấp dẫn chứng dài và chi tiết giúp ban quản lý bờ nắm rõ tình hình tàu.";

        public AiChatService(AppDbContext context, IGeminiEvaluationService geminiService, ILogger<AiChatService> logger)
        {
            _context = context;
            _geminiService = geminiService;
            _logger = logger;
        }

        public async Task<AiChatResponse> ChatAsync(AiChatRequest request, CancellationToken token)
        {
            try
            {
                // Xác định khoảng thời gian: dùng từ request hoặc mặc định 30 ngày gần nhất
                var toDate = request.ToDate ?? DateTime.UtcNow;
                var maxDaysBack = 30;
                var defaultFromDate = toDate.AddDays(-maxDaysBack);
                var fromDate = request.FromDate ?? defaultFromDate;

                // Nếu FromDate quá xa (>30 ngày), giới hạn lại
                if ((toDate - fromDate).TotalDays > maxDaysBack)
                {
                    fromDate = toDate.AddDays(-maxDaysBack);
                    _logger.LogWarning($"Date range exceeded {maxDaysBack} days, limiting to {maxDaysBack} days");
                }

                var reportsQuery = _context.NoonReports.AsQueryable();

                // Nếu có VesselId, join với MaritimeReport để filter
                if (request.VesselId.HasValue)
                {
                    reportsQuery = from nr in _context.NoonReports
                                   join mr in _context.Set<MaritimeReport>() on nr.MaritimeReportId equals mr.Id
                                   where mr.VoyageId != null
                                   select nr;
                }

                var reports = await reportsQuery
                    .Where(r => r.ReportDate >= fromDate && r.ReportDate <= toDate)
                    .OrderByDescending(r => r.ReportDate)
                    .Select(r => new
                    {
                        r.ReportDate,
                        r.MainEngineRPM,
                        r.SpeedOverGround,
                        r.FuelOilConsumed,
                        r.DieselOilConsumed,
                        r.FuelOilROB,
                        r.DieselOilROB,
                        r.WeatherConditions,
                        r.SeaState,
                        r.OperationalRemarks,
                        r.MachineryRemarks
                    })
                    .ToListAsync(token);

                if (reports.Count == 0)
                {
                    return new AiChatResponse
                    {
                        Success = false,
                        Answer = $"Không tìm thấy dữ liệu báo cáo từ {fromDate:dd/MM/yyyy} đến {toDate:dd/MM/yyyy}."
                    };
                }

                // Rút gọn ngữ cảnh và format dữ liệu
                var todayData = reports.FirstOrDefault();
                var compressedContext = CompressContext(reports.Cast<dynamic>().ToList());

                var todayJson = todayData != null 
                    ? JsonSerializer.Serialize(todayData, new JsonSerializerOptions { WriteIndented = false })
                    : "Chưa có";

                // Xây dựng prompt từ template
                string finalPrompt = BuildOptimizedPrompt(
                    request.Question,
                    todayJson,
                    compressedContext);

                _logger.LogInformation($"AI Chat - Period: {fromDate:dd/MM/yyyy} to {toDate:dd/MM/yyyy}, Reports: {reports.Count}, Prompt size: {finalPrompt.Length}");

                // Gọi API
                var geminiResult = await _geminiService.ChatWithReportsAsync(request.Question, finalPrompt, token);

                if (!geminiResult.Success)
                {
                    return new AiChatResponse
                    {
                        Success = false,
                        Answer = geminiResult.Content,
                        ErrorSource = geminiResult.ErrorSource,
                        RetryAfterSeconds = geminiResult.RetryAfterSeconds,
                        Sources = "Gemini API"
                    };
                }

                // Lưu kết quả vào DB
                SaveAiEvaluation(request.VesselId, request.Question, geminiResult.Content);

                return new AiChatResponse
                {
                    Answer = geminiResult.Content,
                    Sources = $"Phân tích dựa trên {reports.Count} báo cáo từ {reports.Last().ReportDate:dd/MM/yyyy} đến {reports.First().ReportDate:dd/MM/yyyy}",
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AI chat service");
                return new AiChatResponse
                {
                    Success = false,
                    Answer = "Có lỗi xảy ra trong quá trình xử lý yêu cầu. Vui lòng thử lại sau."
                };
            }
        }

        /// <summary>
        /// Rút gọn ngữ cảnh bằng cách so sánh dữ liệu 7 ngày
        /// Chỉ giữ lại những thông số có sự thay đổi đáng chú ý
        /// </summary>
        private string CompressContext(List<dynamic> reports)
        {
            if (reports.Count < 1) return "Dữ liệu không đủ để phân tích";

            dynamic latest = reports[0];
            dynamic oldest = reports.Count > 1 ? reports[reports.Count - 1] : latest;

            var metrics = new
            {
                engineRPM = new
                {
                    current = latest.MainEngineRPM,
                    previous = oldest.MainEngineRPM
                },
                speed = new
                {
                    current = latest.SpeedOverGround,
                    previous = oldest.SpeedOverGround
                },
                fuel = new
                {
                    totalConsumed = reports.Sum(r => (double?)(r.FuelOilConsumed ?? 0) ?? 0),
                    remaining = latest.FuelOilROB
                },
                weather = latest.WeatherConditions,
                sea_state = latest.SeaState
            };

            return JsonSerializer.Serialize(metrics, new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// Xây dựng prompt tối ưu từ template cố định + dữ liệu
        /// </summary>
        private string BuildOptimizedPrompt(string question, string todayData, string compressedContext)
        {
            return SYSTEM_PROMPT_TEMPLATE
                .Replace("{TODAY_DATA}", todayData)
                .Replace("{LAST_7DAYS_SUMMARY}", compressedContext)
                .Replace("{QUESTION}", question);
        }

        /// <summary>
        /// Lưu kết quả AI evaluation vào DB
        /// </summary>
        private void SaveAiEvaluation(Guid? vesselId, string question, string answer)
        {
            try
            {
                // Nếu bạn có bảng AiEvaluation, hãy lưu kết quả tại đây
                // Ví dụ:
                // var evaluation = new AiEvaluation
                // {
                //     VesselId = vesselId,
                //     Question = question,
                //     Answer = answer,
                //     CreatedAt = DateTime.UtcNow
                // };
                // _context.AiEvaluations.Add(evaluation);
                // _context.SaveChanges();

                _logger.LogInformation($"AI evaluation completed for vessel {vesselId}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to save AI evaluation");
            }
        }
    }
}
