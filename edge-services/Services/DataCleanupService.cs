using MaritimeEdge.Data;
using Microsoft.EntityFrameworkCore;

namespace MaritimeEdge.Services;

/// <summary>
/// Background service to automatically cleanup old telemetry data
/// Runs daily at configured time (default: 2 AM)
/// Respects retention policies defined in appsettings.json
/// </summary>
public class DataCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DataCleanupService> _logger;
    private readonly IConfiguration _configuration;

    public DataCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<DataCleanupService> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Check if cleanup is enabled
        var enabled = _configuration.GetValue<bool>("DataCleanup:Enabled", true);
        
        if (!enabled)
        {
            _logger.LogInformation("Data Cleanup Service is DISABLED in configuration");
            return;
        }

        var cleanupHour = _configuration.GetValue<int>("DataCleanup:CleanupHour", 2);
        
        _logger.LogInformation(
            "Data Cleanup Service started - will run daily at {Hour}:00", 
            cleanupHour);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Calculate next run time (today or tomorrow at cleanupHour:00)
                var now = DateTime.Now;
                var nextRun = new DateTime(now.Year, now.Month, now.Day, cleanupHour, 0, 0);
                
                if (nextRun < now)
                {
                    nextRun = nextRun.AddDays(1);
                }

                var delay = nextRun - now;
                
                _logger.LogInformation(
                    "Next cleanup scheduled at {NextRun} (in {Hours}h {Minutes}m)", 
                    nextRun.ToString("yyyy-MM-dd HH:mm:ss"),
                    (int)delay.TotalHours,
                    delay.Minutes);

                await Task.Delay(delay, stoppingToken);

                // Run cleanup
                await RunCleanup(stoppingToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Data Cleanup Service is shutting down");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Data Cleanup Service scheduler");
                
                // Wait 1 hour before retry on error
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        _logger.LogInformation("Data Cleanup Service stopped");
    }

    private async Task RunCleanup(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EdgeDbContext>();

        _logger.LogInformation("Starting data cleanup...");

        var startTime = DateTime.UtcNow;
        var totalDeleted = 0;

        try
        {
            // Get retention policies from configuration
            var retentionDays = _configuration.GetSection("Database:RetentionDays")
                .Get<Dictionary<string, int>>() ?? new Dictionary<string, int>();

            // Use EdgeDbContext's built-in cleanup method
            await context.CleanupOldDataAsync(retentionDays);

            var duration = DateTime.UtcNow - startTime;

            _logger.LogInformation(
                "Data cleanup completed successfully in {Duration}s", 
                duration.TotalSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during data cleanup");
        }
    }
}
