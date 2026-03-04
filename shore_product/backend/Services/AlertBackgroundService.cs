using ProductApi.Services;

namespace ProductApi.Services
{
    public class AlertBackgroundService : BackgroundService
    {
        private readonly ILogger<AlertBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _period = TimeSpan.FromMinutes(15); // Run every 15 minutes

        public AlertBackgroundService(ILogger<AlertBackgroundService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Alert Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessAlerts();
                    await Task.Delay(_period, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Alert Background Service is stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Alert Background Service");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait 5 minutes on error
                }
            }
        }

        private async Task ProcessAlerts()
        {
            using var scope = _serviceProvider.CreateScope();
            var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();

            try
            {
                await alertService.ProcessAutomaticAlerts();
                _logger.LogDebug("Automatic alerts processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing automatic alerts");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Alert Background Service is stopping");
            await base.StopAsync(cancellationToken);
        }
    }
}