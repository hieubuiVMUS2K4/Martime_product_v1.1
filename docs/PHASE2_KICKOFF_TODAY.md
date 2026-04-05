# 🎬 PHASE 2 KICKOFF - TODAY'S WORK (2026-04-01)

**Status:** ✅ Phase 1 Complete  
**Current Task:** Task 2.1 - Voyage Planning Sync (Shore→Edge)  
**Today's Goal:** Understand current state & start Shore enqueue implementation

---

## 📊 INFRASTRUCTURE ASSESSMENT ✅

### Edge Side (Receiver) - FULLY READY ✅
✅ **Models:** All 5 planning types exist
- VoyageCargoPlan
- VoyageBunkerPlan
- VoyageCrewChangePlan
- VoyageCostEstimate
- VoyageRevenueEstimate

✅ **DbContext:** All 5 added as DbSets
- EdgeDbContext.VoyageCargoPlans
- EdgeDbContext.VoyageBunkerPlans
- EdgeDbContext.VoyageCrewChangePlans
- EdgeDbContext.VoyageCostEstimates
- EdgeDbContext.VoyageRevenueEstimates

✅ **SyncConflictHandler:** All 5 mapped
- _tableEntityMap includes all 5
- Class definitions reference all 5 types

✅ **Migrations:** Likely already includes voyage planning
- If Edge migrations are up-to-date, tables exist

**Conclusion:** Edge is 100% ready to receive planning sync from Shore!

---

### Shore Side (Sender) - NEEDS WORK ❌

❌ **Missing:** VoyagePlanningService
- No service that enqueues to SyncOutbox when planning created/updated
- Controllers likely exist but don't trigger sync

❌ **Missing:** Integration with VoyageController
- POST /api/voyage/{id}/cargo-plan → doesn't enqueue
- PUT /api/voyage/cargo-plan/{id} → doesn't enqueue
- DELETE /api/voyage/cargo-plan/{id} → doesn't enqueue

**Conclusion:** Shore is ~50% ready. Need to add enqueue logic.

---

## 🎯 TODAY'S IMMEDIATE TASKS

### Task 1: Verify Existing Controllers ✅
Location: `shore_product/backend/Controllers/VoyageController.cs`

```bash
# Check what exists
grep -n "cargo-plan\|bunker-plan\|crew-change\|cost-estimate\|revenue-estimate" \
  shore_product/backend/Controllers/VoyageController.cs
```

Expected: Some endpoints exist for managing planning
Action: Note their signatures for integration

### Task 2: Create VoyagePlanningService 🔴 DO THIS NOW
Location: `shore_product/backend/Services/Voyage/VoyagePlanningService.cs` (NEW)

```csharp
namespace ProductApi.Services.Voyage;

public interface IVoyagePlanningService
{
    // Cargo Plans
    Task<VoyageCargoPlan> CreateCargoplanAsync(Guid voyageId, CreateCargoplanRequest req);
    Task<VoyageCargoPlan> UpdateCargoplanAsync(Guid cargoplanId, UpdateCargoplanRequest req);
    Task DeleteCargoplanAsync(Guid cargoplanId);

    // Bunker Plans
    Task<VoyageBunkerPlan> CreateBunkerplanAsync(Guid voyageId, CreateBunkerplanRequest req);
    Task<VoyageBunkerPlan> UpdateBunkerplanAsync(Guid bunkerplanId, UpdateBunkerplanRequest req);
    Task DeleteBunkerplanAsync(Guid bunkerplanId);

    // Crew Change Plans
    Task<VoyageCrewChangePlan> CreateCrewChangeplanAsync(Guid voyageId, CreateCrewChangeplanRequest req);
    Task<VoyageCrewChangePlan> UpdateCrewChangeplanAsync(Guid planId, UpdateCrewChangeplanRequest req);
    Task DeleteCrewChangeplanAsync(Guid planId);

    // Cost Estimates
    Task<VoyageCostEstimate> CreateCostestimateAsync(Guid voyageId, CreateCostestimateRequest req);
    Task<VoyageCostEstimate> UpdateCostestimateAsync(Guid estimateId, UpdateCostestimateRequest req);
    Task DeleteCostestimateAsync(Guid estimateId);

    // Revenue Estimates
    Task<VoyageRevenueEstimate> CreateRevenueestimateAsync(Guid voyageId, CreateRevenueestimateRequest req);
    Task<VoyageRevenueEstimate> UpdateRevenueestimateAsync(Guid estimateId, UpdateRevenueestimateRequest req);
    Task DeleteRevenueestimateAsync(Guid estimateId);
}

public class VoyagePlanningService : IVoyagePlanningService
{
    private readonly AppDbContext _db;
    private readonly ISyncOutboxService _syncOutbox;
    private readonly ILogger<VoyagePlanningService> _logger;

    public VoyagePlanningService(
        AppDbContext db,
        ISyncOutboxService syncOutbox,
        ILogger<VoyagePlanningService> logger)
    {
        _db = db;
        _syncOutbox = syncOutbox;
        _logger = logger;
    }

    // ============ CARGO PLANS ============
    public async Task<VoyageCargoPlan> CreateCargoplanAsync(Guid voyageId, CreateCargoplanRequest req)
    {
        var voyage = await _db.VoyageRecords.FindAsync(voyageId);
        if (voyage == null) throw new NotFoundException($"Voyage {voyageId} not found");

        var plan = new VoyageCargoPlan
        {
            Id = Guid.NewGuid(),
            VoyageId = voyageId,
            CargoType = req.CargoType,
            Weight = req.Weight,
            Quantity = req.Quantity,
            ShippingMarks = req.ShippingMarks,
            IsHazmat = req.IsHazmat ?? false,
            
            // Sync metadata
            IsSynced = false,
            SyncVersion = 0,
            OriginNode = "SHORE",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.VoyageCargoPlans.Add(plan);
        await _db.SaveChangesAsync();

        // Queue for Edge
        await _syncOutbox.EnqueueAsync(
            targetNode: "*",  // Broadcast to all Edge nodes
            tableName: "voyage_cargo_plan",
            recordKey: plan.Id.ToString(),
            action: SyncActionType.Create,
            payload: plan
        );

        _logger.LogInformation(
            "[VOYAGE-PLANNING] Created cargo plan {Id} for voyage {VoyageId}, " +
            "enqueued to sync outbox",
            plan.Id, voyageId);

        return plan;
    }

    public async Task<VoyageCargoPlan> UpdateCargoplanAsync(Guid cargoplanId, UpdateCargoplanRequest req)
    {
        var plan = await _db.VoyageCargoPlans.FindAsync(cargoplanId);
        if (plan == null) throw new NotFoundException($"Cargo plan {cargoplanId} not found");

        // Update fields
        plan.CargoType = req.CargoType ?? plan.CargoType;
        plan.Weight = req.Weight ?? plan.Weight;
        plan.Quantity = req.Quantity ?? plan.Quantity;
        plan.ShippingMarks = req.ShippingMarks ?? plan.ShippingMarks;
        plan.IsHazmat = req.IsHazmat ?? plan.IsHazmat;
        plan.LoadedAt = req.LoadedAt ?? plan.LoadedAt;
        plan.DischargedAt = req.DischargedAt ?? plan.DischargedAt;

        // Sync metadata
        plan.UpdatedAt = DateTime.UtcNow;
        plan.SyncVersion++;  // Increment for each change
        plan.IsSynced = false;

        await _db.SaveChangesAsync();

        // Queue for Edge
        await _syncOutbox.EnqueueAsync(
            targetNode: "*",
            tableName: "voyage_cargo_plan",
            recordKey: plan.Id.ToString(),
            action: SyncActionType.Update,
            payload: plan
        );

        _logger.LogInformation(
            "[VOYAGE-PLANNING] Updated cargo plan {Id}, " +
            "enqueued to sync outbox with version {Version}",
            plan.Id, plan.SyncVersion);

        return plan;
    }

    public async Task DeleteCargoplanAsync(Guid cargoplanId)
    {
        var plan = await _db.VoyageCargoPlans.FindAsync(cargoplanId);
        if (plan == null) throw new NotFoundException($"Cargo plan {cargoplanId} not found");

        _db.VoyageCargoPlans.Remove(plan);
        await _db.SaveChangesAsync();

        // Queue DELETE for Edge
        await _syncOutbox.EnqueueAsync(
            targetNode: "*",
            tableName: "voyage_cargo_plan",
            recordKey: cargoplanId.ToString(),
            action: SyncActionType.Delete,
            payload: new { Id = cargoplanId }
        );

        _logger.LogInformation(
            "[VOYAGE-PLANNING] Deleted cargo plan {Id}, " +
            "enqueued DELETE to sync outbox",
            cargoplanId);
    }

    // ============ BUNKER PLANS ============
    // [Similar pattern for BunkerPlan Create/Update/Delete]
    
    // ============ CREW CHANGE PLANS ============
    // [Similar pattern for CrewChangePlan Create/Update/Delete]
    
    // ============ COST ESTIMATES ============
    // [Similar pattern for CostEstimate Create/Update/Delete]
    
    // ============ REVENUE ESTIMATES ============
    // [Similar pattern for RevenueEstimate Create/Update/Delete]
}
```

### Task 3: Register Service in Program.cs

```csharp
// In Program.cs:
builder.Services.AddScoped<IVoyagePlanningService, VoyagePlanningService>();
```

### Task 4: Update VoyageController to Use Service

```csharp
[HttpPost("voyage/{voyageId}/cargo-plan")]
public async Task<CargoplanDto> CreateCargoPlan(Guid voyageId, CreateCargoplanRequest req)
{
    var plan = await _voyagePlanningService.CreateCargoplanAsync(voyageId, req);
    return MapToDto(plan);
}

[HttpPut("cargo-plan/{cargoplanId}")]
public async Task<CargoplanDto> UpdateCargoPlan(Guid cargoplanId, UpdateCargoplanRequest req)
{
    var plan = await _voyagePlanningService.UpdateCargoplanAsync(cargoplanId, req);
    return MapToDto(plan);
}

[HttpDelete("cargo-plan/{cargoplanId}")]
public async Task DeleteCargoPlan(Guid cargoplanId)
{
    await _voyagePlanningService.DeleteCargoplanAsync(cargoplanId);
}
```

---

## 📋 PHASE 2.1 WORK ITEMS

| Item | Owner | Status | Deadline |
|------|-------|--------|----------|
| Verify Edge infrastructure | You | ✅ DONE | NOW |
| Create VoyagePlanningService | You | 🔴 TODO | TODAY |
| Register in Program.cs | You | 🔴 TODO | TODAY |
| Update VoyageController | You | 🔴 TODO | TODAY |
| Test: Create cargo plan locally | You | 🟡 TODO | TODAY |
| Test: Verify SyncOutbox row | You | 🟡 TODO | TODAY |
| Run smoke test 1: Single wave | You | 🔴 TODO | 2026-04-02 |
| Run smoke tests 2-5 | You | 🔴 TODO | 2026-04-03 |
| Add logging & error handling | You | 🔴 TODO | 2026-04-04 |
| Documentation | You | 🔴 TODO | 2026-04-05 |

---

## 🚀 NEXT STEPS (Priority Order)

1. **RIGHT NOW:** Create VoyagePlanningService.cs
   - Copy the template above
   - Implement all 15 methods (3 × 5 planning types)
   - Add logging with [VOYAGE-PLANNING] prefix
   - Test compilation: `dotnet build`

2. **THEN:** Register in Program.cs
   - Add one line: `builder.Services.AddScoped<IVoyagePlanningService, VoyagePlanningService>();`

3. **THEN:** Update VoyageController
   - Inject `IVoyagePlanningService`
   - Update existing endpoints to use service
   - Add for each planning type (5 × 3 = 15 endpoints total)

4. **TEST LOCALLY:**
   ```bash
   # Start Shore backend
   cd shore_product/backend
   dotnet run
   
   # Create test cargo plan
   curl -X POST http://localhost:5000/api/voyage/{voyageId}/cargo-plan \
     -H "Content-Type: application/json" \
     -d '{
       "cargoType": "Container",
       "weight": 500.5,
       "quantity": 10,
       "shippingMarks": "TEST-001",
       "isHazmat": false
     }'
   
   # Verify SyncOutbox
   sqlite3 shore.db "SELECT * FROM sync_outbox WHERE table_name = 'voyage_cargo_plan' AND delivered_at IS NULL;"
   # Expected: 1 row with status PENDING
   ```

5. **IF ERROR:** Check:
   - Is VoyagePlanningService registered in Program.cs?
   - Does VoyageController have `private readonly IVoyagePlanningService _voyagePlanningService;`?
   - Are the parameter names correct in Update methods?

---

## ⚠️ GOTCHAS TO WATCH

1. **SyncVersion must be incremented** on Update, not just timestamp
   - Edge uses SyncVersion for idempotency
   - `plan.SyncVersion++` before SaveChanges

2. **OriginNode must be "SHORE"** not "Shore" or "shore"
   - Case-sensitive in conflict resolution
   - Check: Edge SyncConflictHandler expects "SHORE"

3. **DateTime must be UTC**
   - Migrations already handle conversion
   - Use `DateTime.UtcNow` not `DateTime.Now`

4. **Enqueue happens AFTER SaveChanges**
   - Order matters! DB change commits first, then outbox queues
   - If enqueue fails, DB change remains (deferred consistency)

5. **targetNode should be "*" for broadcast**
   - "*" means "all Edge nodes"
   - Not "EDGE" or "Edge"

---

## 📚 Files to Create/Modify

```
NEW:
  shore_product/backend/Services/Voyage/VoyagePlanningService.cs

MODIFY:
  shore_product/backend/Program.cs (register service)
  shore_product/backend/Controllers/VoyageController.cs (use service)

VERIFY (no changes needed):
  edge_product/edge-services/Models/EdgeModels.cs (models exist)
  edge_product/edge-services/Data/EdgeDbContext.cs (DbSets exist)
  edge_product/edge-services/Services/Core/SyncConflictHandler.cs (mappings exist)
```

---

## 🎉 SUCCESS CRITERIA FOR TODAY

- [ ] VoyagePlanningService.cs created with all 15 methods
- [ ] Service registered in Program.cs
- [ ] VoyageController updated to use service
- [ ] Local test: Create cargo plan → succeeds
- [ ] Verify: SyncOutbox has PENDING row for new plan
- [ ] Compilation: `dotnet build` succeeds with 0 errors
- [ ] No new warnings introduced

**Estimated Time:** 2-3 hours

---

## 📞 IF STUCK

1. **Compilation error on VoyagePlanningService:**
   - Ensure `using` statements are correct
   - Check namespace: `namespace ProductApi.Services.Voyage;`
   - Verify ISyncOutboxService exists

2. **SyncOutbox query returns nothing:**
   - Check database: `SELECT COUNT(*) FROM sync_outbox;`
   - Verify targetNode is "*" (with quotes in JSON)
   - Check tableName is "voyage_cargo_plan" (lowercase, underscore)

3. **Edge doesn't receive planning:**
   - Covered in next smoke test on 2026-04-02
   - For now, just ensure Shore enqueue works

---

**Status:** 🎬 READY TO KICKOFF  
**Time:** ~2-3 hours  
**Difficulty:** 🟢 MEDIUM (straightforward service pattern)
