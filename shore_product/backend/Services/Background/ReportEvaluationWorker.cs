using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;
using ProductApi.Services.AI;
using ProductApi.DTOs;
using System.Text.Json;

namespace ProductApi.Services.Background
{
    public class ReportEvaluationWorker : BackgroundService
    {
        private readonly ReportEvaluationQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReportEvaluationWorker> _logger;

        public ReportEvaluationWorker(ReportEvaluationQueue queue, IServiceScopeFactory scopeFactory, ILogger<ReportEvaluationWorker> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var reportId in _queue.ReadAllAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var aiService = scope.ServiceProvider.GetRequiredService<IGeminiEvaluationService>();

                    var reportIdObj = new object[] { reportId };
                    var report = await dbContext.NoonReports.FindAsync(reportIdObj, stoppingToken);

                    if (report == null)
                    {
                        _logger.LogWarning($"Report {reportId} not found.");
                        continue;
                    }

                    var maritimeReport = await dbContext.Set<MaritimeReport>().FindAsync(new object[] { report.MaritimeReportId }, stoppingToken);
                    var voyageId = maritimeReport?.VoyageId;

                    var historyQuery = dbContext.NoonReports.AsQueryable();
                    
                    if (voyageId.HasValue)
                    {
                        historyQuery = from nr in dbContext.NoonReports
                                       join mr in dbContext.Set<MaritimeReport>() on nr.MaritimeReportId equals mr.Id
                                       where mr.VoyageId == voyageId.Value && nr.ReportDate >= report.ReportDate.AddDays(-7)
                                       select nr;
                    }
                    else
                    {
                        historyQuery = historyQuery.Where(r => r.ReportDate >= report.ReportDate.AddDays(-7));
                    }

                    var history = await historyQuery
                        .OrderBy(r => r.ReportDate)
                        .Select(r => new ShipMetricsDto 
                        {
                            ReportDate = r.ReportDate,
                            EngineTemp = r.SeaTemperature ?? 0.0, 
                            FuelConsumption = r.FuelOilConsumed ?? 0.0, 
                            Rpm = r.MainEngineRPM ?? 0.0,
                            Speed = r.SpeedOverGround ?? 0.0
                        })
                        .ToListAsync(stoppingToken);

                    string jsonData = JsonSerializer.Serialize(history);

                    var aiResult = await aiService.EvaluateReportAsync(jsonData, stoppingToken);

                    dbContext.Set<ReportEvaluation>().Add(new ReportEvaluation
                    {
                        Id = Guid.NewGuid(),
                        ReportId = reportId,
                        Status = aiResult.Status,
                        ContentVi = aiResult.ContentVi
                    });

                    await dbContext.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to evaluate report {ReportId}", reportId);
                }

                await Task.Delay(4000, stoppingToken); // Chống Rate Limit
            }
        }
    }
}
