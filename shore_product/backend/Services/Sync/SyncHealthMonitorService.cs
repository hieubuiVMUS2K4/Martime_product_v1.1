using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;

namespace ProductApi.Services.Sync;

/// <summary>
/// Background service that monitors sync health across all connected ships.
/// 
/// Responsibilities:
/// - Detect offline nodes (no heartbeat within threshold)
/// - Update pending outbox counts per node
/// - Clean up stale outbox items (delivered or expired)
/// - Generate sync table statistics snapshots
/// - Reconcile unsynced crew/cert records
/// - Clean up old idempotency records
/// 
/// Runs every 5 minutes (configurable).
/// </summary>
public class SyncHealthMonitorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SyncHealthMonitorService> _logger;
    private readonly IConfiguration _configuration;

    public SyncHealthMonitorService(
        IServiceProvider serviceProvider,
        ILogger<SyncHealthMonitorService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Sync Health Monitor started.");

        // Startup delay
        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunHealthCheckAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Sync Health Monitor cycle");
            }

            var intervalMinutes = _configuration.GetValue("Sync:HealthCheckIntervalMinutes", 5);
            await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
        }
    }

    private async Task RunHealthCheckAsync(CancellationToken token)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // 1. Detect offline nodes
        await DetectOfflineNodesAsync(context, token);

        // 2. Update pending outbox counts
        await UpdatePendingCountsAsync(context, token);

        // 3. Clean up old delivered outbox items (keep 30 days)
        await CleanupDeliveredOutboxAsync(context, token);

        // 4. Clean up old idempotency records (keep 7 days)
        await CleanupIdempotencyRecordsAsync(context, token);

        // 5. Reconcile unsynced data (queue for outbox)
        await ReconcileUnsyncedAsync(context, token);

        // 6. Refresh table stats snapshot
        await RefreshTableStatsAsync(context, token);
    }

    private async Task DetectOfflineNodesAsync(AppDbContext context, CancellationToken token)
    {
        var offlineThresholdMinutes = _configuration.GetValue("Sync:OfflineThresholdMinutes", 30);
        var threshold = DateTime.UtcNow.AddMinutes(-offlineThresholdMinutes);

        var nodes = await context.SyncNodeTrackers
            .Where(n => n.IsOnline)
            .Where(n => n.LastHeartbeatAt == null || n.LastHeartbeatAt < threshold)
            .ToListAsync(token);

        foreach (var node in nodes)
        {
            node.IsOnline = false;
            node.UpdatedAt = DateTime.UtcNow;
            _logger.LogWarning("Node {NodeId} ({ShipName}) marked OFFLINE — no heartbeat since {LastBeat}",
                node.NodeId, node.ShipName, node.LastHeartbeatAt);
        }

        if (nodes.Count > 0)
            await context.SaveChangesAsync(token);
    }

    private async Task UpdatePendingCountsAsync(AppDbContext context, CancellationToken token)
    {
        var nodes = await context.SyncNodeTrackers.ToListAsync(token);

        foreach (var node in nodes)
        {
            node.PendingOutboxCount = await context.SyncOutbox
                .Where(o => o.DeliveredAt == null)
                .Where(o => o.TargetNode == node.NodeId || o.TargetNode == "*")
                .CountAsync(token);
        }

        await context.SaveChangesAsync(token);
    }

    private async Task CleanupDeliveredOutboxAsync(AppDbContext context, CancellationToken token)
    {
        var retentionDays = _configuration.GetValue("Sync:OutboxRetentionDays", 30);
        var cutoff = DateTime.UtcNow.AddDays(-retentionDays);

        var staleItems = await context.SyncOutbox
            .Where(o => o.DeliveredAt != null && o.DeliveredAt < cutoff)
            .Take(500) // Batch delete to avoid locking
            .ToListAsync(token);

        if (staleItems.Count > 0)
        {
            context.SyncOutbox.RemoveRange(staleItems);
            await context.SaveChangesAsync(token);
            _logger.LogInformation("Cleaned up {Count} delivered outbox items older than {Days} days",
                staleItems.Count, retentionDays);
        }
    }

    private async Task CleanupIdempotencyRecordsAsync(AppDbContext context, CancellationToken token)
    {
        var retentionDays = _configuration.GetValue("Sync:IdempotencyRetentionDays", 7);
        var cutoff = DateTime.UtcNow.AddDays(-retentionDays);

        var staleRecords = await context.SyncIdempotencyRecords
            .Where(r => r.ProcessedAt < cutoff)
            .Take(1000)
            .ToListAsync(token);

        if (staleRecords.Count > 0)
        {
            context.SyncIdempotencyRecords.RemoveRange(staleRecords);
            await context.SaveChangesAsync(token);
            _logger.LogDebug("Cleaned up {Count} idempotency records", staleRecords.Count);
        }
    }

    private async Task ReconcileUnsyncedAsync(AppDbContext context, CancellationToken token)
    {
        // Find crew records that should be synced but aren't in outbox
        var unsyncedCrew = await context.CrewMembers
            .Where(c => !c.IsSynced && c.OriginNode == "SHORE")
            .CountAsync(token);

        var unsyncedCerts = await context.CrewCertificates
            .Where(c => !c.IsSynced && c.OriginNode == "SHORE")
            .CountAsync(token);

        if (unsyncedCrew > 0 || unsyncedCerts > 0)
        {
            _logger.LogWarning("Found unsynced records: {Crew} crew, {Certs} certificates. " +
                "Use ICrewSyncOrchestrator.ReconcileUnsyncedDataAsync() to queue them.",
                unsyncedCrew, unsyncedCerts);
        }
    }

    private async Task RefreshTableStatsAsync(AppDbContext context, CancellationToken token)
    {
        // Compute stats from sync_logs grouped by node+table
        var freshStats = await context.SyncLogs
            .GroupBy(l => new { l.OriginNode, l.TableName })
            .Select(g => new SyncTableStats
            {
                NodeId = g.Key.OriginNode,
                TableName = g.Key.TableName,
                TotalSynced = g.LongCount(l => l.Status == "SUCCESS"),
                TotalConflicts = g.LongCount(l => l.Status == "CONFLICT"),
                TotalFailed = g.LongCount(l => l.Status == "FAILED"),
                LastSyncAt = g.Max(l => l.ProcessedAt),
                SnapshotAt = DateTime.UtcNow
            })
            .ToListAsync(token);

        if (freshStats.Count == 0) return;

        // Upsert: clear old stats, insert new
        var existingStats = await context.SyncTableStats.ToListAsync(token);
        context.SyncTableStats.RemoveRange(existingStats);
        await context.SyncTableStats.AddRangeAsync(freshStats, token);
        await context.SaveChangesAsync(token);

        _logger.LogDebug("Refreshed {Count} table stat entries", freshStats.Count);
    }
}
