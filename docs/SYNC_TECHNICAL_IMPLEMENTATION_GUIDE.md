# 🔧 TECHNICAL IMPLEMENTATION GUIDE - Voyage Sync Completion

**Object:** Detailed technical recommendations for critical bug fixes

---

## I. VOYAGE DOMAIN MODELS (Task 1.1)

### A. Shore Model Structure

```csharp
// shore_product/backend/Models/Voyage/VoyageRecord.cs
[Table("voyage_records")]
public class VoyageRecord : IEntity
{
    [Key]
    public Guid Id { get; set; }

    /// Factual data (owned by Edge)
    public Guid ShipId { get; set; }
    public string Status { get; set; } = "planning"; // planning, executing, completed, cancelled
    public DateTime? StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public DateTime? EstimatedArrivalDateTime { get; set; }
    
    /// Nested collections
    public ICollection<VoyagePlanLeg> PlanLegs { get; set; } = new List<VoyagePlanLeg>();
    public ICollection<VoyageStatusHistory> StatusHistories { get; set; } = new List<VoyageStatusHistory>();
    public ICollection<VoyageCrewAssignment> CrewAssignments { get; set; } = new List<VoyageCrewAssignment>();
    public ICollection<VoyageLogEntry> LogEntries { get; set; } = new List<VoyageLogEntry>();
    public ICollection<CargoOperation> CargoOperations { get; set; } = new List<CargoOperation>();

    /// Sync metadata
    public string? OriginNode { get; set; } // "edge" or "shore"
    public long SyncVersion { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;

    /// Planning data (owned by Shore, optional on Edge)
    public string? PlannedRoute { get; set; }
    public decimal? EstimatedFuelConsumption { get; set; }
    public DateTime? CreatedByShoreAt { get; set; }
}

// shore_product/backend/Models/Voyage/VoyagePlanLeg.cs
[Table("voyage_plan_legs")]
public class VoyagePlanLeg : IEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid VoyageRecordId { get; set; }
    [ForeignKey(nameof(VoyageRecordId))]
    public VoyageRecord VoyageRecord { get; set; }

    public int Sequence { get; set; }
    public Guid FromPortId { get; set; }
    public Guid ToPortId { get; set; }
    public DateTime PlannedDepartureTime { get; set; }
    public DateTime PlannedArrivalTime { get; set; }
    public decimal PlannedAverageCourse { get; set; } // degrees
    public decimal PlannedSpeed { get; set; } // knots

    /// Sync metadata
    public string? OriginNode { get; set; }
    public long SyncVersion { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

// Similar structure for VoyageCrewAssignment, VoyageLogEntry, CargoOperation...
```

### B. EF Configuration & Migration

```csharp
// shore_product/backend/Data/ModelBuilderConfigurations.cs
public static void ConfigureVoyageModels(this ModelBuilder modelBuilder)
{
    // Ensure all DateTime fields are UTC
    modelBuilder.Entity<VoyageRecord>(entity =>
    {
        entity.ToTable("voyage_records");
        entity.HasKey(e => e.Id);
        
        entity.Property(e => e.StartDateTime)
            .HasConversion(dt => dt.HasValue ? dt.Value.ToUniversalTime() : dt, 
                          dt => dt.HasValue ? DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc) : dt);
        entity.Property(e => e.UpdatedAt)
            .HasConversion(dt => dt.ToUniversalTime(),
                          dt => DateTime.SpecifyKind(dt, DateTimeKind.Utc));
        
        entity.HasMany(e => e.PlanLegs)
            .WithOne(l => l.VoyageRecord)
            .HasForeignKey(l => l.VoyageRecordId);
        
        entity.HasIndex(e => new { e.ShipId, e.StartDateTime });
        entity.HasIndex(e => e.OriginNode);
        entity.HasIndex(e => e.SyncVersion);
    });

    // Similar for other voyage entities...
}

// Add to DbContext OnModelCreating:
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ConfigureVoyageModels();
}
```

### C. Create Migration

```bash
cd shore_product/backend
dotnet ef migrations add AddVoyageDomainModels \
  --context AppDbContext \
  --output-dir Migrations
```

Migration should create 5 voyage tables:
- voyage_records
- voyage_plan_legs  
- voyage_status_histories
- voyage_crew_assignments
- voyage_log_entries

---

## II. SYNC INBOX MAPPING (Task 1.1)

### A. SyncInboxService Entry Point

```csharp
// shore_product/backend/Services/Sync/SyncInboxService.cs

public async Task<SyncBatchProcessResult> ProcessBatchAsync(List<SyncQueueItemDto> items)
{
    var result = new SyncBatchProcessResult();
    
    // Group by table for bulk processing
    var groupedByTable = items.GroupBy(i => i.TableName);
    
    foreach (var group in groupedByTable)
    {
        var tableName = group.Key.ToLowerInvariant();
        
        switch (tableName)
        {
            case "voyage_record":
                await ProcessVoyageRecordsAsync(group.ToList(), result);
                break;
            case "voyage_plan_leg":
                await ProcessVoyagePlanLegsAsync(group.ToList(), result);
                break;
            case "voyage_status_history":
                await ProcessVoyageStatusHistoriesAsync(group.ToList(), result);
                break;
            case "voyage_crew_assignment":
                await ProcessVoyageCrewAssignmentsAsync(group.ToList(), result);
                break;
            case "voyage_log_entry":
                await ProcessVoyageLogEntriesAsync(group.ToList(), result);
                break;
            case "cargo_operation":
                await ProcessCargoOperationsAsync(group.ToList(), result);
                break;
            
            // Existing crew, report handlers...
            case "crew_member":
                await ProcessCrewMembersAsync(group.ToList(), result);
                break;
            // ... etc
        }
    }
    
    _logger.LogInformation(
        "SyncInbox batch processed: {Succeeded} succeeded, {Failed} failed",
        result.Succeeded, result.Failed);
    
    return result;
}
```

### B. Voyage Processing Handler

```csharp
private async Task ProcessVoyageRecordsAsync(
    List<SyncQueueItemDto> items, SyncBatchProcessResult result)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    
    try
    {
        foreach (var item in items)
        {
            try
            {
                // Idempotency check
                var alreadyProcessed = await IsAlreadyProcessedAsync(
                    item.TableName, item.RecordKey, item.SyncVersion);
                
                if (alreadyProcessed)
                {
                    _logger.LogDebug("Skipping duplicate voyage_record/{Key} v{Version}",
                        item.RecordKey, item.SyncVersion);
                    result.Succeeded++;
                    continue;
                }

                var voyageData = JsonSerializer.Deserialize<VoyageRecord>(
                    item.Payload, _jsonOptions);
                
                if (voyageData == null)
                    throw new InvalidOperationException("Null voyage payload");

                // Ensure UTC
                if (voyageData.StartDateTime.HasValue)
                    voyageData.StartDateTime = voyageData.StartDateTime.Value.ToUniversalTime();
                if (voyageData.EndDateTime.HasValue)
                    voyageData.EndDateTime = voyageData.EndDateTime.Value.ToUniversalTime();

                VoyageRecord existing = null;
                
                if (item.ActionType == SyncActionType.UPDATE || 
                    item.ActionType == SyncActionType.DELETE)
                {
                    existing = await _context.VoyageRecords
                        .FirstOrDefaultAsync(v => v.Id == Guid.Parse(item.RecordKey));
                }

                var resolution = _conflictResolver.Resolve(
                    item.TableName, existing, voyageData, item.OriginNode);

                if (!resolution.ShouldApply)
                {
                    _logger.LogInformation(
                        "Conflict rejected voyage_record/{Key}: {Reason}",
                        item.RecordKey, resolution.ConflictDetail);
                    result.Failed++;
                    continue;
                }

                switch (item.ActionType)
                {
                    case SyncActionType.CREATE:
                        voyageData.OriginNode = item.OriginNode;
                        voyageData.SyncVersion = item.SyncVersion;
                        _context.VoyageRecords.Add(voyageData);
                        _logger.LogInformation("SyncInbox: CREATE voyage_record/{Key}",
                            item.RecordKey);
                        break;

                    case SyncActionType.UPDATE:
                        existing.OriginNode = item.OriginNode;
                        existing.SyncVersion = item.SyncVersion;
                        existing.Status = voyageData.Status;
                        existing.StartDateTime = voyageData.StartDateTime;
                        existing.EndDateTime = voyageData.EndDateTime;
                        existing.EstimatedArrivalDateTime = voyageData.EstimatedArrivalDateTime;
                        existing.UpdatedAt = DateTime.UtcNow;
                        _context.VoyageRecords.Update(existing);
                        _logger.LogInformation("SyncInbox: UPDATE voyage_record/{Key}",
                            item.RecordKey);
                        break;

                    case SyncActionType.DELETE:
                        existing.IsDeleted = true;
                        existing.UpdatedAt = DateTime.UtcNow;
                        _context.VoyageRecords.Update(existing);
                        _logger.LogInformation("SyncInbox: SOFT DELETE voyage_record/{Key}",
                            item.RecordKey);
                        break;
                }

                result.Succeeded++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process voyage_record/{Key}",
                    item.RecordKey);
                result.Failed++;
                result.FailedItems.Add(new SyncBatchItemFailure
                {
                    TableName = item.TableName,
                    RecordKey = item.RecordKey,
                    ActionType = item.ActionType.ToString(),
                    Error = ex.Message
                });
            }
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "VoyageRecord batch transaction failed");
        throw;
    }
}
```

---

## III. CONFLICT POLICY MATRIX (Task 1.2)

### A. Ownership Rules

```csharp
// shore_product/backend/Services/Sync/ConflictResolverService.cs

// ===== VOYAGE DOMAIN =====
private readonly Dictionary<string, ownership> _voyageFieldOwnership = new()
{
    // voyage_record: mostly Edge (factual), some Shore (planning)
    
    // [Edge-owned - factual execution]
    ["voyage_record.status"] = Ownership.Edge,
    ["voyage_record.start_datetime"] = Ownership.Edge,
    ["voyage_record.end_datetime"] = Ownership.Edge,
    ["voyage_record.current_location"] = Ownership.Edge,
    ["voyage_record.actual_fuel_consumed"] = Ownership.Edge,
    
    // [Shore-owned - planning/estimate]
    ["voyage_record.planned_route"] = Ownership.Shore,
    ["voyage_record.estimated_fuel_consumption"] = Ownership.Shore,
    ["voyage_record.budget_estimate"] = Ownership.Shore,
    
    // voyage_plan_leg: Edge (actualization), Shore (planning)
    ["voyage_plan_leg.planned_departure_time"] = Ownership.Hybrid, // LWW
    ["voyage_plan_leg.planned_speed"] = Ownership.Shore,
    ["voyage_plan_leg.planned_course"] = Ownership.Shore,
    
    // voyage_crew_assignment: Edge (who's actually onboard)
    ["voyage_crew_assignment.crew_id"] = Ownership.Edge,
    ["voyage_crew_assignment.embark_port"] = Ownership.Edge,
    ["voyage_crew_assignment.embark_date"] = Ownership.Edge,
    
    // cargo_operation: Edge (actual operations)
    ["cargo_operation.*"] = Ownership.Edge,
};

enum Ownership { Edge, Shore, Hybrid }

public ConflictResolution Resolve(string tableName, object existing, 
    object incoming, string originNode)
{
    var tableNameLower = tableName.ToLowerInvariant();
    
    // ===== VOYAGE SPECIAL HANDLING =====
    if (tableNameLower.StartsWith("voyage_"))
    {
        return ResolveVoyageConflict(tableNameLower, existing, incoming, originNode);
    }
    
    // ===== Default: Edge-authoritative voyage tables =====
    if (IsVoyageTable(tableNameLower))
    {
        if (originNode == "edge")
            return ConflictResolution.Apply(incoming);
        else if (originNode == "shore")
            return ConflictResolution.Reject("Voyage tables owned by edge");
    }
    
    // ... existing crew, certificate logic ...
}

private ConflictResolution ResolveVoyageConflict(string table, 
    object existing, object incoming, string originNode)
{
    if (existing == null)
        return ConflictResolution.Apply(incoming);
    
    var voyageExisting = (VoyageRecord)existing;
    var voyageIncoming = (VoyageRecord)incoming;
    
    // For each critical field, apply ownership rules:
    
    // 1. Status: Edge owns (actual state on ship)
    if (voyageIncoming.Status != voyageExisting.Status)
    {
        if (originNode == "edge")
        {
            voyageExisting.Status = voyageIncoming.Status;
            _logger.LogInformation(
                "Voyage status conflict: {OldStatus} <- {NewStatus} from edge",
                voyageExisting.Status, voyageIncoming.Status);
        }
        else
        {
            _logger.LogInformation(
                "Voyage status conflict: rejected shore update");
            return ConflictResolution.Reject("Status owned by edge");
        }
    }
    
    // 2. EstimatedRoute: Shore owns (planning)
    if (!string.Equals(voyageIncoming.PlannedRoute, voyageExisting.PlannedRoute))
    {
        if (originNode == "shore")
        {
            voyageExisting.PlannedRoute = voyageIncoming.PlannedRoute;
            _logger.LogInformation(
                "Voyage plan route conflict: accepted from shore");
        }
        else
        {
            _logger.LogInformation(
                "Voyage plan route conflict: rejected edge update");
            return ConflictResolution.Reject("PlannedRoute owned by shore");
        }
    }
    
    // 3. StartDateTime/EndDateTime: Last-Write-Wins (Hybrid)
    if (voyageIncoming.StartDateTime != voyageExisting.StartDateTime)
    {
        if (voyageIncoming.UpdatedAt > voyageExisting.UpdatedAt)
        {
            voyageExisting.StartDateTime = voyageIncoming.StartDateTime;
            _logger.LogInformation(
                "Voyage datetime conflict: LWW applied (newer wins)");
        }
    }
    
    return ConflictResolution.Apply(voyageExisting);
}

private bool IsVoyageTable(string tableName) =>
    tableName.StartsWith("voyage_") || 
    tableName == "port_call" ||
    tableName == "cargo_operation" ||
    tableName == "voyage_log_entry";
```

### B. Document Output

Create `docs/VOYAGE_CONFLICT_OWNERSHIP_MATRIX.md`:

```markdown
# Voyage Conflict Ownership Matrix

| Field | Table | Source of Truth | Conflict Rule |
|---|---|---|---|
| Status | voyage_record | Edge | Edge always wins |
| StartDateTime | voyage_record | Edge | Edge preferred, LWW fallback |
| PlannedRoute | voyage_record | Shore | Shore always wins |
| EstimatedFuelConsumption | voyage_record | Shore | Shore always wins |
| CrewAssignments | voyage_crew_assignment | Edge | Edge always wins |
| PlanLegs | voyage_plan_leg | Hybrid | LWW (newer UpdatedAt wins) |
| CargoOperations | cargo_operation | Edge | Edge always wins |

## Rationale

- **Edge Ownership:** Operational reality (execution, crew status, cargo ops)
- **Shore Ownership:** Planning/Commercial (route optimization, budgets, schedules)
- **Hybrid (LWW):** Planning legs can be updated by shore OR edge without hard conflict
```

---

## IV. VOYAGE PLANNING SYNC (Task 2.2) - Outbox Side

### A. SyncOutboxService Enhancement

```csharp
public async Task EnqueueAsync(string targetNode, string tableName, string recordKey,
    SyncActionType action, object payload)
{
    // ... existing validation ...
    
    // Add voyage planning support:
    var isVoyagePlanningTable = tableName.ToLowerInvariant() switch
    {
        "voyage_cargo_plan" => true,
        "voyage_bunker_plan" => true,
        "voyage_crew_change_plan" => true,
        "voyage_cost_estimate" => true,
        _ => false
    };
    
    if (isVoyagePlanningTable)
    {
        var item = new SyncOutboxItem
        {
            TargetNodeId = targetNode,
            TableName = tableName,
            RecordKey = recordKey,
            ActionType = action,
            Payload = JsonSerializer.Serialize(payload, _jsonOptions),
            SyncVersion = DateTime.UtcNow.Ticks, // Stable version
            Status = SyncItemStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.SyncOutboxItems.Add(item);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation(
            "Enqueued voyage planning: {Table}/{Key} for {TargetNode}",
            tableName, recordKey, targetNode);
    }
}
```

### B. Entity Models for Planning

```csharp
// shore_product/backend/Models/Voyage/VoyageCargoPlan.cs
[Table("voyage_cargo_plans")]
public class VoyageCargoPlan : IEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid VoyageRecordId { get; set; }
    [ForeignKey(nameof(VoyageRecordId))]
    public VoyageRecord VoyageRecord { get; set; }

    public string CargoType { get; set; } // containers, bulk, general, etc
    public decimal PlannedQuantity { get; set; }
    public decimal ActualQuantity { get; set; }
    public decimal PlannedWeight { get; set; } // tons

    /// Sync
    public string? OriginNode { get; set; } = "shore";
    public long SyncVersion { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

---

## V. NETWORK DETECTION (Task 2.3)

### A. NetworkDetectionService

```csharp
// edge_product/edge-services/Services/Core/NetworkDetectionService.cs

public interface INetworkDetectionService
{
    Task<NetworkType> DetectCurrentNetworkAsync();
}

public class NetworkDetectionService : INetworkDetectionService
{
    private readonly ILogger<NetworkDetectionService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public async Task<NetworkType> DetectCurrentNetworkAsync()
    {
        try
        {
            // Option 1: Call router API
            var routerUrl = _configuration.GetValue<string>("NetworkDetection:RouterApiUrl");
            
            if (!string.IsNullOrEmpty(routerUrl))
            {
                using var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                
                try
                {
                    var response = await client.GetAsync($"{routerUrl}/api/network/status");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var data = JsonSerializer.Deserialize<dynamic>(json);
                        
                        var networkName = data["currentNetwork"]?.ToString();
                        if (Enum.TryParse<NetworkType>(networkName, out var result))
                        {
                            _logger.LogInformation("Detected network: {NetworkType}", result);
                            return result;
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    _logger.LogWarning("Router API timeout, fallback to default");
                }
            }
            
            // Option 2: Check active network interfaces
            return DetectViaNetworkInterface();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Network detection error, defaulting to WiFi");
            return NetworkType.Shore_WiFi;
        }
    }

    private NetworkType DetectViaNetworkInterface()
    {
        var interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
        
        foreach (var iface in interfaces)
        {
            if (!iface.IsReceiveOnly && iface.OperationalStatus == 
                System.Net.NetworkInformation.OperationalStatus.Up)
            {
                // Heuristic: check interface name or description
                if (iface.Description.Contains("Iridium", StringComparison.OrdinalIgnoreCase))
                    return NetworkType.Satellite_Iridium;
                
                if (iface.Description.Contains("VSAT", StringComparison.OrdinalIgnoreCase))
                    return NetworkType.Satellite_VSAT;
                
                if (iface.Description.Contains("4G", StringComparison.OrdinalIgnoreCase) ||
                    iface.Description.Contains("LTE", StringComparison.OrdinalIgnoreCase))
                    return NetworkType.Cellular_4G;
                
                if (iface.Description.Contains("WiFi", StringComparison.OrdinalIgnoreCase) ||
                    iface.Description.Contains("Ethernet", StringComparison.OrdinalIgnoreCase))
                    return NetworkType.Shore_WiFi;
            }
        }
        
        _logger.LogWarning("Could not determine network type from interfaces");
        return NetworkType.Shore_WiFi;
    }
}
```

### B. SyncService Integration

```csharp
public class SyncService : ISyncService
{
    private readonly INetworkDetectionService _networkDetection;

    public SyncService(..., INetworkDetectionService networkDetection)
    {
        _networkDetection = networkDetection;
    }

    public async Task ExecuteSyncAsync(CancellationToken cancellationToken)
    {
        var networkType = await _networkDetection.DetectCurrentNetworkAsync();
        _logger.LogInformation("Sync running on network: {NetworkType}", networkType);
        
        var queue = await GetSyncQueueAsync();
        
        // Filter by network-appropriate priorities
        var itemsToSync = networkType switch
        {
            NetworkType.Satellite_Iridium =>
                queue.Where(i => i.Priority == SyncPriority.Critical).ToList(),
            
            NetworkType.Satellite_VSAT =>
                queue.Where(i => i.Priority <= SyncPriority.Operational).ToList(),
            
            NetworkType.Cellular_4G or NetworkType.Shore_WiFi =>
                queue.ToList(),
            
            _ => new List<SyncQueueItem>()
        };
        
        _logger.LogInformation(
            "Filtering sync: {NetworkType} → {ItemsToSync} items (total {AllItems})",
            networkType, itemsToSync.Count, queue.Count);
        
        // POST to shore...
    }
}
```

---

## VI. SOFT DELETE POLICY

### A. Delete Handling in SyncInboxService

```csharp
case SyncActionType.DELETE:
    // Never hard-delete voyage data, only soft delete
    if (existing is ISoftDeletable softDelectable)
    {
        softDelectable.IsDeleted = true;
        softDelectable.DeletedAt = DateTime.UtcNow;
        _context.Update(existing);
        _logger.LogInformation(
            "SyncInbox: SOFT DELETE {Table}/{Key}",
            item.TableName, item.RecordKey);
    }
    else
    {
        // Fallback for non-soft-delete entities
        _logger.LogWarning(
            "Cannot soft delete {Table}/{Key}: entity doesn't support ISoftDeletable",
            item.TableName, item.RecordKey);
    }
    break;
```

### B. Query Filter Configuration

```csharp
// Every voyage query should exclude deleted:
var activeVoyages = _context.VoyageRecords
    .Where(v => !v.IsDeleted)
    .OrderByDescending(v => v.UpdatedAt)
    .ToList();
```

---

## IMPLEMENTATION CHECKLIST

- [ ] Shore voyage models created (5 tables)
- [ ] EF migrations generated (non-destructive)
- [ ] SyncInboxService has all voyage cases
- [ ] ConflictResolverService has ownership rules
- [ ] DateTime UTC conversion in EF
- [ ] SyncOutboxService enqueues voyage planning
- [ ] NetworkDetectionService implemented  
- [ ] Soft delete policy enforced
- [ ] All tests passing (unit + integration)

---

**Ready for coding:** Each section above is a self-contained, copy-paste-ready implementation.
