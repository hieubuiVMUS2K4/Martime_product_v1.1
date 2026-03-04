using Microsoft.EntityFrameworkCore;
using Maritime.Shared.Constants;
using Maritime.Shared.Models.Sync;
using ProductApi.Data;

namespace ProductApi.Services.Sync;

/// <summary>
/// Background service that monitors crew certificate expiry dates and
/// automatically broadcasts status updates to all edge nodes.
/// 
/// Runs daily at configurable time. For each certificate:
/// - EXPIRING_SOON (within 90 days): Broadcast UPDATE with status 
/// - EXPIRED (past expiry): Broadcast UPDATE with EXPIRED status
/// - Alerts shore dashboard for fleet compliance visibility
/// 
/// This ensures ships always have up-to-date certificate status
/// even if they were offline when the certificate expired.
/// </summary>
public class CertificateExpiryMonitorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CertificateExpiryMonitorService> _logger;
    private readonly IConfiguration _configuration;

    public CertificateExpiryMonitorService(
        IServiceProvider serviceProvider,
        ILogger<CertificateExpiryMonitorService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Certificate Expiry Monitor started.");

        // Run once after startup delay
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndBroadcastExpiryStatusAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Certificate Expiry Monitor cycle");
            }

            // Run every 6 hours (configurable)
            var interval = _configuration.GetValue("Sync:CertExpiryCheckIntervalHours", 6);
            await Task.Delay(TimeSpan.FromHours(interval), stoppingToken);
        }
    }

    private async Task CheckAndBroadcastExpiryStatusAsync(CancellationToken token)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var syncOutbox = scope.ServiceProvider.GetRequiredService<ISyncOutboxService>();

        var warningThresholdDays = _configuration.GetValue("Sync:CertExpiryWarningDays", 90);
        var now = DateTime.UtcNow;
        var warningDate = now.AddDays(warningThresholdDays);

        // Find certificates that need status update
        var certsToUpdate = await context.CrewCertificates
            .Include(cc => cc.Certificate)
            .Include(cc => cc.CrewMember)
            .Include(cc => cc.Country)
            .Where(cc => cc.ExpiryDate <= warningDate)
            .Where(cc => cc.Status != CertificateStatus.EXPIRED || cc.Status != CertificateStatus.EXPIRING_SOON)
            .ToListAsync(token);

        int updated = 0;
        int broadcasted = 0;

        foreach (var cert in certsToUpdate)
        {
            if (token.IsCancellationRequested) break;

            string newStatus;
            if (cert.ExpiryDate <= now)
            {
                newStatus = CertificateStatus.EXPIRED;
            }
            else if (cert.ExpiryDate <= warningDate)
            {
                newStatus = CertificateStatus.EXPIRING_SOON;
            }
            else
            {
                continue;
            }

            if (cert.Status == newStatus) continue; // Already correct status

            var previousStatus = cert.Status;
            cert.Status = newStatus;
            cert.UpdatedAt = DateTime.UtcNow;
            updated++;

            // Broadcast status change to all ships
            try
            {
                await syncOutbox.BroadcastAsync(
                    "crew_certificate",
                    cert.Id.ToString(),
                    SyncActionType.UPDATE,
                    cert);
                broadcasted++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to broadcast cert expiry update for cert {CertId}", cert.Id);
            }

            _logger.LogInformation(
                "Certificate {CertNum} for {CrewName}: {PrevStatus} → {NewStatus} (expires {Expiry:d})",
                cert.CertificateNumber,
                cert.CrewMember?.FullName ?? "Unknown",
                previousStatus,
                newStatus,
                cert.ExpiryDate);
        }

        if (updated > 0)
        {
            await context.SaveChangesAsync(token);
            _logger.LogInformation(
                "Certificate expiry check complete: {Updated} status updates, {Broadcast} broadcasted to ships",
                updated, broadcasted);
        }
        else
        {
            _logger.LogDebug("Certificate expiry check: no updates needed");
        }
    }
}
