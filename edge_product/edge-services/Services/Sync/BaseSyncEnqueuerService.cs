using System.Text.Json;
using MaritimeEdge.Data;
using MaritimeEdge.Models;
using Maritime.Shared.Interfaces;
using Maritime.Shared.Models.Sync;
using Microsoft.EntityFrameworkCore;

namespace MaritimeEdge.Services.Sync;

/// <summary>
/// Base class for sync enqueuer services to eliminate code duplication across
/// AlertSyncEnqueuerService, PositionSyncEnqueuerService, EngineSyncEnqueuerService.
/// 
/// Extracted common pattern:
/// 1. Get unsynced records in batches
/// 2. Serialize to JSON payload
/// 3. Create SyncQueue items
/// 4. Mark entities as synced
/// 5. Save changes
/// </summary>
public abstract class BaseSyncEnqueuerService<TEntity> : BackgroundService 
    where TEntity : class, ISyncableEntity
{
    protected readonly IServiceProvider ServiceProvider;
    protected readonly ILogger Logger;
    protected readonly IConfiguration Configuration;

    protected int IntervalSeconds { get; private set; }
    protected int BatchSize { get; private set; }
    protected string VesselImo { get; private set; }
    protected abstract string ConfigPrefix { get; } // e.g., "AlertSyncEnqueuer", "PositionSyncEnqueuer"
    protected abstract string EntityTableName { get; } // e.g., "safety_alarm", "engine_event"

    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    protected BaseSyncEnqueuerService(
        IServiceProvider serviceProvider,
        ILogger logger,
        IConfiguration configuration)
    {
        ServiceProvider = serviceProvider;
        Logger = logger;
        Configuration = configuration;

        IntervalSeconds = Configuration.GetValue($"{ConfigPrefix}:IntervalSeconds", 10);
        BatchSize = Configuration.GetValue($"{ConfigPrefix}:BatchSize", 50);
        VesselImo = Configuration["SyncSecurity:NodeId"]
                 ?? Configuration["Vessel:IMO"]
                 ?? "UNKNOWN";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var enabled = Configuration.GetValue($"{ConfigPrefix}:Enabled", true);
        if (!enabled)
        {
            Logger.LogInformation("{ServiceName} is DISABLED in configuration", GetType().Name);
            return;
        }

        Logger.LogInformation(
            "{ServiceName} started. Interval: {Interval}s, BatchSize: {BatchSize}",
            GetType().Name, IntervalSeconds, BatchSize);

        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await EnqueueUnsyncedAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                Logger.LogInformation("{ServiceName} is shutting down", GetType().Name);
                break;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in {ServiceName}", GetType().Name);
            }

            await Task.Delay(TimeSpan.FromSeconds(IntervalSeconds), stoppingToken);
        }

        Logger.LogInformation("{ServiceName} stopped", GetType().Name);
    }

    /// <summary>
    /// Main enqueue logic - get unsynced records, serialize, create SyncQueue items
    /// </summary>
    protected async Task EnqueueUnsyncedAsync(CancellationToken ct)
    {
        using var scope = ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EdgeDbContext>();

        var unsynced = await GetUnsyncedRecordsAsync(dbContext, ct);
        if (unsynced.Count == 0) return;

        Logger.LogInformation("Enqueuing {Count} {EntityType} records to SyncQueue", 
            unsynced.Count, EntityTableName);

        var now = DateTime.UtcNow;
        var syncQueueItems = new List<SyncQueue>();

        foreach (var entity in unsynced)
        {
            // Subclass implements payload serialization
            var payload = SerializeEntity(entity);

            syncQueueItems.Add(new SyncQueue
            {
                TableName = EntityTableName,
                RecordKey = GetEntityId(entity).ToString(),
                ActionType = SyncActionType.CREATE,
                Payload = payload,
                Priority = SyncPriority.Critical,
                CreatedAt = now,
                RetryCount = 0,
                MaxRetries = 5,
                NextRetryAt = now
            });

            entity.IsSynced = true;
            entity.OriginNode = VesselImo;
            entity.UpdatedAt = now;
        }

        await dbContext.SyncQueue.AddRangeAsync(syncQueueItems, ct);
        await dbContext.SaveChangesAsync(ct);

        Logger.LogInformation("✅ Enqueued {Count} {EntityType} records to SyncQueue", 
            syncQueueItems.Count, EntityTableName);
    }

    /// <summary>
    /// Get unsynced records from database - implement in subclass
    /// Default: Get from DbSet<TEntity> where IsSynced = false
    /// </summary>
    protected virtual async Task<List<TEntity>> GetUnsyncedRecordsAsync(EdgeDbContext dbContext, CancellationToken ct)
    {
        var set = dbContext.Set<TEntity>();
        
        return await set
            .Where(e => !e.IsSynced)
            .OrderBy(e => e.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Serialize entity to JSON payload - implement in subclass
    /// Each entity type has different fields to serialize
    /// </summary>
    protected abstract string SerializeEntity(TEntity entity);

    /// <summary>
    /// Get entity ID - uses reflection to get the Id property
    /// Works for any entity type with an Id property
    /// </summary>
    protected virtual object GetEntityId(TEntity entity)
    {
        var idProperty = typeof(TEntity).GetProperty("Id");
        if (idProperty == null)
            throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} does not have an Id property");
        
        return idProperty.GetValue(entity) ?? throw new InvalidOperationException("Entity ID is null");
    }
}
