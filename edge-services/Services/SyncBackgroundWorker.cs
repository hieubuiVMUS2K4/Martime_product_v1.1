using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace MaritimeEdge.Services;

public class SyncBackgroundWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SyncBackgroundWorker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(60); // Run every minute

    public SyncBackgroundWorker(IServiceProvider serviceProvider, ILogger<SyncBackgroundWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Sync Background Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var syncService = scope.ServiceProvider.GetRequiredService<ISyncService>();
                    await syncService.ExecuteSyncAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during sync execution.");
            }

            // Wait for next cycle
            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("Sync Background Worker stopping.");
    }
}
