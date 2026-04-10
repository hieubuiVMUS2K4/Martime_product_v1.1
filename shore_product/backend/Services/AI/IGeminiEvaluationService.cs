using ProductApi.DTOs;

namespace ProductApi.Services.AI
{
    public interface IGeminiEvaluationService
    {
        Task<AiEvaluationResponse> EvaluateReportAsync(string jsonData, CancellationToken token);
        Task<GeminiChatResult> ChatWithReportsAsync(string message, string reportsJson, CancellationToken token);
    }
}
