# 🎯 TASK 2.1: VOYAGE PLANNING SYNC (Shore→Edge) - DETAILED BREAKDOWN

**Status:** 🔴 CRITICAL / 🎬 STARTING NOW  
**Duration:** 4 days  
**Deadline:** 2026-04-05  

---

## 📊 CURRENT STATE ANALYSIS

### ✅ WHAT'S ALREADY DONE

#### Edge Side (Receiver) - READY ✅
- **SyncInboxService table mappings:** All 5 planning tables mapped
  - `voyage_cargo_plan` → `VoyageCargoPlan`
  - `voyage_bunker_plan` → `VoyageBunkerPlan`
  - `voyage_crew_change_plan` → `VoyageCrewChangePlan`
  - `voyage_cost_estimate` → `VoyageCostEstimate`
  - `voyage_revenue_estimate` → `VoyageRevenueEstimate`

- **CREATE on missing:** All 5 tables in `_createOnMissingUpdateTables`
  - Can auto-create missing rows from UPDATE action
  - Safe for first-time receipt

- **Models:** All 5 entities exist in Shore backend
  - Location: `Models/VoyageSyncModels.cs`
  - All have `ISyncableEntity` (IsSynced, SyncVersion, OriginNode)

#### Database & EF Core - READY ✅
- **Migrations:** Latest migration includes all planning tables
- **DbSet:** All 5 added to AppDbContext
- **UTC Converters:** All planning datetime fields have UTC conversion

### ❌ WHAT'S MISSING

#### Shore Side (Sender) - NEEDS IMPLEMENTATION ❌

**1. SyncOutboxService Integration**
- When Shore creates/updates VoyageCargoPlan → should enqueue to Edge
- When Shore creates/updates VoyageBunkerPlan → should enqueue to Edge
- etc. for all 5 planning types
- **Current:** No automatic enqueuing triggered
- **Needed:** Event handlers or repository layer that calls `EnqueueAsync()`

**2. VoyageCreateService/UpdateService**
- When voyage planning created/updated → trigger outbox
- **Pattern needed:**
  ```csharp
  var plan = new VoyageCargoPlan { ... };
  _db.VoyageCargoPlans.Add(plan);
  await _db.SaveChangesAsync();
  
  // NEW: Queue for Edge
  await _syncOutboxService.EnqueueAsync(
      targetNode: "EDGE", // or "*" to broadcast to all Edge nodes
      tableName: "voyage_cargo_plan",
      recordKey: plan.Id.ToString(),
      action: SyncActionType.Create,
      payload: plan
  );
  ```

**3. Conflict Resolution on Shore (optional but recommended)**
- If Edge and Shore both update VoyageCargoPlan simultaneously
- Need conflict resolution rule: **Shore owns planning**
- Currently: ConflictResolverService has plan rules
- **Needed:** Verify ConflictResolverService routes planning updates correctly (Phase 1 done)

#### Edge Side (Receiver) - PARTIALLY READY ⚠️

**1. Edge DTO/Models for Planning**
- Edge needs `VoyageCargoPlan`, etc. models to deserialize JSON
- **Current:** Unknown - need to verify
- **Needed:** If missing, create mirror models in Edge

**2. Edge SyncConflictHandler**
- Must accept inbound planning from Shore
- Must route to correct handler (already done per memory notes)
- **Current:** Supposedly working (per voyage_sync_gap_notes.md)
- **Needed:** Verify and test

**3. Edge Controllers (optional)**
- Crew/Ops can query voyage plans pulled from Shore
- **Current:** Probably missing
- **Needed:** GET /api/voyage/{voyageId}/cargo-plan, etc.

---

## 🔧 IMPLEMENTATION PLAN

### PHASE 2.1.A: Shore Outbox Integration (Day 1)

**Objective:** Make Shore enqueue planning when saved

#### Step 1: Create VoyagePlanningService
```csharp
// File: shore_product/backend/Services/Voyage/VoyagePlanningService.cs
public class VoyagePlanningService
{
    private readonly AppDbContext _db;
    private readonly ISyncOutboxService _syncOutbox;
    
    public async Task CreateCargoplanAsync(Guid voyageId, VoyageCargoPlan plan)
    {
        plan.VoyageId = voyageId;
        plan.IsSynced = false;
        plan.SyncVersion = 0;
        plan.OriginNode = "SHORE";
        
        _db.VoyageCargoPlans.Add(plan);
        await _db.SaveChangesAsync();
        
        // NEW: Queue for Edge
        await _syncOutbox.EnqueueAsync(
            targetNode: "*",  // Broadcast to all Edge nodes
            tableName: "voyage_cargo_plan",
            recordKey: plan.Id.ToString(),
            action: SyncActionType.Create,
            payload: plan
        );
    }
    
    public async Task UpdateCargoplanAsync(Guid cargoplanId, VoyageCargoPlan plan)
    {
        var existing = await _db.VoyageCargoPlans.FindAsync(cargoplanId);
        if (existing == null) throw new NotFoundException();
        
        // Update fields (map from plan DTO)
        existing.CargoType = plan.CargoType;
        existing.Weight = plan.Weight;
        existing.Quantity = plan.Quantity;
        existing.ShippingMarks = plan.ShippingMarks;
        existing.IsHarmat = plan.IsHarmat;
        existing.UpdatedAt = DateTime.UtcNow;
        existed.SyncVersion++; // Increment version
        
        await _db.SaveChangesAsync();
        
        // NEW: Queue for Edge
        await _syncOutbox.EnqueueAsync(
            targetNode: "*",
            tableName: "voyage_cargo_plan",
            recordKey: existing.Id.ToString(),
            action: SyncActionType.Update,
            payload: existing
        );
    }
    
    public async Task DeleteCargoplanAsync(Guid cargoplanId)
    {
        var existing = await _db.VoyageCargoPlans.FindAsync(cargoplanId);
        if (existing == null) throw new NotFoundException();
        
        _db.VoyageCargoPlans.Remove(existing);
        await _db.SaveChangesAsync();
        
        // NEW: Queue DELETE for Edge
        await _syncOutbox.EnqueueAsync(
            targetNode: "*",
            tableName: "voyage_cargo_plan",
            recordKey: cargoplanId.ToString(),
            action: SyncActionType.Delete,
            payload: new { Id = cargoplanId }  // Minimal payload for delete
        );
    }
}
```

Repeat for:
- VoyageBunkerPlan
- VoyageCrewChangePlan
- VoyageCostEstimate
- VoyageRevenueEstimate

#### Step 2: Update Voyage Controller
```csharp
[HttpPost("voyage/{voyageId}/cargo-plan")]
public async Task<CargoplanDto> CreateCargoplan(Guid voyageId, CreateCargoplanRequest req)
{
    var plan = new VoyageCargoPlan 
    { 
        CargoType = req.CargoType,
        Weight = req.Weight,
        // ... other fields
    };
    
    // Uses new VoyagePlanningService
    var result = await _voyagePlanningService.CreateCargoplanAsync(voyageId, plan);
    return MapToDto(result);
}

[HttpPut("cargo-plan/{cargoplanId}")]
public async Task<CargoplanDto> UpdateCargoplan(Guid cargoplanId, UpdateCargoplanRequest req)
{
    var plan = new VoyageCargoPlan 
    { 
        CargoType = req.CargoType,
        Weight = req.Weight,
        // ... other fields
    };
    
    var result = await _voyagePlanningService.UpdateCargoplanAsync(cargoplanId, plan);
    return MapToDto(result);
}

[HttpDelete("cargo-plan/{cargoplanId}")]
public async Task DeleteCargoplan(Guid cargoplanId)
{
    await _voyagePlanningService.DeleteCargoplanAsync(cargoplanId);
    return Ok();
}
```

**Deliverables:**
- ✅ VoyagePlanningService.cs created
- ✅ Enqueuing logic added (Create, Update, Delete)
- ✅ Voyage controller updated
- ✅ Test: Create cargo plan → verify SyncOutbox row created

---

### PHASE 2.1.B: Edge Reception & Storage (Day 2)

**Objective:** Edge receives and stores planning

#### Step 1: Verify Edge Models Exist
```csharp
// File: edge_product/edge-services/Models/VoyageSync/VoyageCargoPlan.cs
public class VoyageCargoPlan : ISyncableEntity
{
    public Guid Id { get; set; }
    public Guid VoyageId { get; set; }
    public string CargoType { get; set; }
    public double Weight { get; set; }
    public int Quantity { get; set; }
    public string ShippingMarks { get; set; }
    public bool IsHazmat { get; set; }
    public DateTime? LoadedAt { get; set; }
    public DateTime? DischargedAt { get; set; }
    
    // ISyncableEntity
    public bool IsSynced { get; set; }
    public long SyncVersion { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string OriginNode { get; set; }
    
    public virtual Voyage Voyage { get; set; }
}
```

If missing on Edge, create them.

#### Step 2: Verify Edge DbContext
```csharp
// edge-services/Data/EdgeDbContext.cs
public DbSet<VoyageCargoPlan> VoyageCargoPlans { get; set; }
public DbSet<VoyageBunkerPlan> VoyageBunkerPlans { get; set; }
public DbSet<VoyageCrewChangePlan> VoyageCrewChangePlans { get; set; }
public DbSet<VoyageCostEstimate> VoyageCostEstimates { get; set; }
public DbSet<VoyageRevenueEstimate> VoyageRevenueEstimates { get; set; }
```

If missing, add them.

#### Step 3: Verify SyncConflictHandler
```csharp
// edge-services/Services/Sync/SyncConflictHandler.cs
public async Task<ResolveResult> ResolveAsync(string tableName, object local, object remote)
{
    if (tableName.StartsWith("voyage_"))
    {
        // Route to voyage conflict resolution
        return await ResolveVoyageAsync(tableName, local, remote);
    }
    // ... other domains
}

private async Task<ResolveResult> ResolveVoyageAsync(string tableName, object local, object remote)
{
    // Since Shore owns planning: always prefer remote (Shore)
    // Update local with remote values
    var properties = tableName switch
    {
        "voyage_cargo_plan" => GetCargoplanProperties(local, remote),
        "voyage_bunker_plan" => GetBunkerplanProperties(local, remote),
        // ... etc
    };
    
    foreach (var (localVal, remoteVal) in properties)
    {
        localVal = remoteVal;  // Shore wins
    }
    
    return new ResolveResult { Apply(local), Origin = "SHORE" };
}
```

**Deliverables:**
- ✅ Edge models created (if missing)
- ✅ Edge DbContext updated (if missing)
- ✅ SyncConflictHandler routes planning to Shore ownership  resolution
- ✅ Test: Edge receives planning → verify in voyage_cargo_plan table

---

### PHASE 2.1.C: End-to-End Integration Testing (Day 3)

#### Smoke Test 1: Single Create Wave
```bash
# On Shore
POST /api/voyage/{voyageId}/cargo-plan
{
  cargoType: "Container",
  weight: 500.5,
  quantity: 10,
  shippingMarks: "DEMO-001",
  isHazmat: false
}

# Verify Shore
SELECT * FROM sync_outbox WHERE table_name = 'voyage_cargo_plan' 
  AND delivered_at IS NULL;
# Expect: 1 row with status PENDING

# On Edge (pull)
GET /api/sync/pull??since=<timestamp>

# Verify Edge received
SELECT * FROM voyage_cargo_plan WHERE cargo_type = 'Container';
# Expect: 1 row with IsSynced = false initially, then true after confirm

# Edge ack
POST /api/sync/acknowledge
{ itemIds: [<outbox-id>] }

# Verify Shore
SELECT * FROM sync_outbox WHERE table_name = 'voyage_cargo_plan' 
  AND id = <outbox-id>;
# Expect: delivered_at is NOT NULL
```

#### Smoke Test 2: Multiple Waves
- Create 5 cargo plans with different voyage IDs
- Pull each → verify all on Edge
- Ack all → verify Shore cleared

#### Smoke Test 3: Update Wave
- Update cargo plan weight on Shore
- Pull on Edge → verify weight updated
- Ack → verify delivered

#### Smoke Test 4: Delete Wave
- Delete cargo plan on Shore
- View SyncOutbox → expect DELETE action
- Pull on Edge → verify Edge marked as deleted or removed

#### Smoke Test 5: Concurrent Updates (stress test)
- Create planning on Shore
- Update planning on Shore (10x)
- Simultaneously on Edge, try to update same planning (optional conflict scenario)
- Verify no data loss, last-write-wins correctly applied

**Deliverables:**
- ✅ Test script (PowerShell or Bash)
- ✅ All 5 smoke tests pass with 0 data loss
- ✅ Latency measured: <500ms Shore→Edge via LAN

---

### PHASE 2.1.D: Hardening & Documentation (Day 4)

#### 1. Add Audit Logging
```csharp
_logger.LogInformation("[VOYAGE-PLANNING] Created {PlanType} for voyage {VoyageId}, "
    + "enqueued to sync outbox for {TargetNode}", 
    "cargo_plan", voyageId, targetNode);
```

#### 2. Add Error Handling
- What if SyncOutboxService.EnqueueAsync fails?
  - Rollback the create/update?
  - Or accept that outbox is queued separately and can be retried?
- Recommendation: Accept async outbox queueing (deferred consistency)

#### 3. Performance Optimization
- Batch enqueuing: if user creates 10 cargo plans at once,  should we batch the outbox inserts?
- Answer: Yes, use `EnqueueBatchAsync()` if available

#### 4. Documentation
- Planning sync sequence diagram
- SyncOutbox retry logic (for Phase 2.4)
- Edge conflict resolution for planning
- Operational guide: How to manually replay planning sync if failed

**Deliverables:**
- ✅ TASK_2_1_IMPLEMENTATION_GUIDE.md
- ✅ Logging configured for [VOYAGE-PLANNING] prefix
- ✅ Error handling documented
- ✅ Performance baseline recorded

---

## 📋 CHECKLIST - TASK 2.1 COMPLETION

### Shore Side
- [ ] VoyagePlanningService created (Create/Update/Delete for all 5 types)
- [ ] Voyage controller updated
- [ ] SyncOutboxService integration tested
- [ ] Audit logging added with [VOYAGE-PLANNING] prefix
- [ ] Error handling added

### Edge Side
- [ ] Models exist for all 5 planning types
- [ ] DbContext includes all 5 DbSets
- [ ] SyncConflictHandler routes planning correctly
- [ ] UTC converters applied (if model has DateTime fields)

### Testing
- [ ] Smoke Test 1: Single create wave PASSED
- [ ] Smoke Test 2: Multiple waves PASSED
- [ ] Smoke Test 3: Update wave PASSED
- [ ] Smoke Test 4: Delete wave PASSED
- [ ] Smoke Test 5: Concurrent updates PASSED
- [ ] Latency baseline: <500ms LAN

### Documentation
- [ ] Implementation guide created
- [ ] Database diagram updated (design page)
- [ ] Runbook for manual replay
- [ ] Troubleshooting guide

---

## ⚠️ RISKS & MITIGATION

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|-----------|
| Edge doesn't have planning models | MEDIUM | BLOCKER | Check today, create if missing |
| SyncConflictHandler doesn't route | LOW | BLOCKER | Verify before coding |
| Outbox enqueue fails silently |  LOW | MODERATE | Add transaction wrapping, logging |
| Concurrent planning updates corrupt data | LOW | HIGH | Test Case 5 thoroughly |
| Planning sync creates zombie records | LOW | MODERATE | Add DELETE sync items |

---

## 🎬 NEXT ACTIONS

**TODAY (2026-04-01):**
1. ✅ Verify Edge has planning models (or create them)
2. ✅ Check Edge DbContext & SyncConflictHandler status
3. ✅ Confirm SyncOutboxService will work with planning entities
4. 📅 Start Code: VoyagePlanningService

**TOMORROW (2026-04-02):**
1. Complete VoyagePlanningService for all 5 plan types
2. Update Voyage controllers
3. Verify Shore enqueuing works

**2026-04-03:**
1. Verify/create Edge models
2. Run smoke tests 1-2
3. Fix any 404 or deserialization errors

**2026-04-04:**
1. Complete smoke tests 3-5
2. Add audit logging & error handling
3. Record latency baseline

**2026-04-05:**
1. Document everything
2. Prepare for Phase 2.2 kickoff
3. Code review & merge to feature/tinht

---

**Task Owner:** [You]  
**Estimated Completion:** 2026-04-05  
**Status:** 🎬 READY TO START
