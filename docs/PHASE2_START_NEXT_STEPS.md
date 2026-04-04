# 🎬 PHASE 2 START - NEXT STEPS (ROADMAP)

**Time:** 2026-04-01  
**Status:** 🎬 READY TO CODE

---

## 📋 DOCUMENTS CREATED FOR REFERENCE

| Document | Purpose | Location |
|----------|---------|----------|
| **PHASE2_EXECUTIVE_SUMMARY.md** | High-level overview of all 4 tasks | docs/ |
| **PHASE2_COMPREHENSIVE_PLAN.md** | Detailed scope, risks, metrics | docs/ |
| **PHASE2_TASK_2_1_DETAILED_IMPLEMENTATION_GUIDE.md** | Step-by-step implementation | docs/ |
| **PHASE2_KICKOFF_TODAY.md** | What to do TODAY (this plan) | docs/ |

**Read in this order:**
1. This file (next 10 minutes)
2. PHASE2_KICKOFF_TODAY.md (detailed today's work)
3. PHASE2_TASK_2_1_DETAILED_IMPLEMENTATION_GUIDE.md (reference while coding)
4. PHASE2_COMPREHENSIVE_PLAN.md (for tasks 2.2, 2.3, 2.4)

---

## 🎯 WHAT WE ACCOMPLISHED TODAY

### ✅ Analysis Complete

**Edge Infrastructure:** FULLY READY ✅
- All 5 planning models: VoyageCargoPlan, VoyageBunkerPlan, VoyageCrewChangePlan, VoyageCostEstimate, VoyageRevenueEstimate
- All 5 DbSets in EdgeDbContext
- All 5 mapped in SyncConflictHandler
- **Conclusion:** Edge can receive planning sync RIGHT NOW

**Shore Infrastructure:** PARTIAL ⚠️
- Models exist ✅
- DbContext configured ✅
- SyncOutboxService ready ✅
- **Missing:** VoyagePlanningService (not created yet) ❌

**Secure Sync:** OPERATIONAL ✅
- HMAC-SHA256 signing working
- Request verification middleware active
- Node registry in database
- **Status:** All 4 endpoints signed and verified

---

## 🚀 YOUR TASK #1 (TODAY) - TASK 2.1 PHASE A

### CREATE: VoyagePlanningService.cs

**Location:** `shore_product/backend/Services/Voyage/VoyagePlanningService.cs` (NEW FILE)

**What to do:**
1. Copy the template from PHASE2_TASK_2_1_DETAILED_IMPLEMENTATION_GUIDE.md
2. Create the interface: `IVoyagePlanningService`
3. Implement the service class with 15 methods (3 actions × 5 plan types)
4. Key pattern:
   ```csharp
   Save to DB
   ↓
   Enqueue to SyncOutbox
   ↓
   Log with [VOYAGE-PLANNING] prefix
   ```

**Estimated time:** 1.5 hours

**Success criteria:**
- File compiles: `dotnet build` → 0 errors
- All 15 method signatures present
- Enqueue call for each Create/Update/Delete
- Logging in place

---

## 🚀 YOUR TASK #2 (TODAY) - TASK 2.1 PHASE B

### UPDATE: Program.cs

**Location:** `shore_product/backend/Program.cs`

**What to do:**
1. Find the services registration section (around line 35-80)
2. Add this single line:
   ```csharp
   builder.Services.AddScoped<IVoyagePlanningService, VoyagePlanningService>();
   ```
3. Place it near other Service registrations (Sync, Crew, etc.)

**Estimated time:** 5 minutes

**Success criteria:**
- Line added in correct location
- Correct namespace `ProductApi.Services.Voyage`
- Compilation passes

---

## 🚀 YOUR TASK #3 (TODAY) - TASK 2.1 PHASE C

### UPDATE: VoyageController.cs

**Location:** `shore_product/backend/Controllers/VoyageController.cs`

**What to do:**
1. Add dependency injection:
   ```csharp
   private readonly IVoyagePlanningService _voyagePlanningService;
   
   public VoyageController(
       AppDbContext db,
       IVoyagePlanningService voyagePlanningService)
   {
       _db = db;
       _voyagePlanningService = voyagePlanningService;
   }
   ```

2. Update existing endpoints (or create new ones):
   ```csharp
   [HttpPost("voyage/{voyageId}/cargo-plan")]
   public async Task<ActionResult<CargoplanDto>> CreateCargoPlan(
       Guid voyageId, CreateCargoplanRequest req)
   {
       var plan = await _voyagePlanningService.CreateCargoplanAsync(voyageId, req);
       return Ok(MapToDto(plan));
   }
   
   [HttpPut("cargo-plan/{cargoplanId}")]
   public async Task<ActionResult<CargoplanDto>> UpdateCargoPlan(
       Guid cargoplanId, UpdateCargoplanRequest req)
   {
       var plan = await _voyagePlanningService.UpdateCargoplanAsync(cargoplanId, req);
       return Ok(MapToDto(plan));
   }
   
   [HttpDelete("cargo-plan/{cargoplanId}")]
   public async Task<ActionResult> DeleteCargoPlan(Guid cargoplanId)
   {
       await _voyagePlanningService.DeleteCargoplanAsync(cargoplanId);
       return NoContent();
   }
   
   // Repeat for: BunkerPlan, CrewChangePlan, CostEstimate, RevenueEstimate
   // That's 5 plan types × 3 actions = 15 endpoints total
   ```

3. Add DTOs if they don't exist:
   - `CreateCargoplanRequest`, `UpdateCargoplanRequest`
   - `CargoplanDto`
   - Return types for all 5 types (similar pattern)

**Estimated time:** 1.5 hours

**Success criteria:**
- Service injected correctly
- 15 endpoints created (3 × 5)
- All endpoints call corresponding service methods
- Compilation passes

---

## ✅ YOUR TASK #4 (TODAY) - LOCAL TEST

### TEST: Create cargo plan locally

**What to do:**

1. **Start the backend:**
   ```bash
   cd f:\NCKH\Product\Martime_product_v1.1\shore_product\backend
   dotnet run
   # Wait for "Now listening on https://localhost:5001"
   ```

2. **Create a test cargo plan:**
   ```bash
   # First, create a voyage (or use existing)
   $voyageId = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"  # Use a real voyage ID
   
   Invoke-RestMethod `
     -Method Post `
     -Uri "https://localhost:5001/api/voyage/$voyageId/cargo-plan" `
     -ContentType "application/json" `
     -SkipCertificateCheck `
     -Body (ConvertTo-Json @{
       cargoType = "Container"
       weight = 500.5
       quantity = 10
       shippingMarks = "TEST-001"
       isHazmat = $false
     })
   ```

3. **Verify response:**
   - Should get 200 OK with plan ID
   - Should include: Id, VoyageId, CargoType, Weight, IsSynced, SyncVersion, OriginNode

**Estimated time:** 15 minutes

**Success criteria:**
- API returns 200 OK
- Response includes all expected fields
- No 500 errors in console

---

## ✅ YOUR TASK #5 (TODAY) - VERIFY OUTBOX

### TEST: Check SyncOutbox was populated

**What to do:**

1. **Query SyncOutbox via SQL:**
   ```sql
   -- Using psql or DBeaver connect to Shore database
   SELECT * FROM sync_outbox 
   WHERE table_name = 'voyage_cargo_plan' 
   AND delivered_at IS NULL
   ORDER BY created_at DESC
   LIMIT 1;
   ```

2. **Verify the row has:**
   - `target_node` = '*' (broadcast)
   - `table_name` = 'voyage_cargo_plan'
   - `action_type` = 'CREATE'
   - `delivered_at` = NULL (not yet delivered)
   - `payload` contains all plan fields (JSON)

**Estimated time:** 10 minutes

**Success criteria:**
- 1 row present in SyncOutbox
- All fields correct
- Payload is valid JSON with plan data

---

## 🏁 TODAY'S FINISH LINE

If you complete all 5 tasks above:
- ✅ VoyagePlanningService.cs created
- ✅ Program.cs registers the service
- ✅ VoyageController uses the service
- ✅ Local test confirms Create → API works
- ✅ SyncOutbox confirms Enqueue → Database works

**Total estimated time:** 4-5 hours (spread throughout day)

**What happens next (2026-04-02):**
- Start Edge-side testing (Pull → Sync)
- Run smoke tests 1-2
- Debug any issues

---

## 🎓 IMPORTANT GOTCHAS

### 1. DateTime must be UTC
```csharp
// ✅ CORRECT
plan.CreatedAt = DateTime.UtcNow;
plan.UpdatedAt = DateTime.UtcNow;

// ❌ WRONG
plan.CreatedAt = DateTime.Now;  // Local time!
```

### 2. OriginNode must be "SHORE" (case-sensitive)
```csharp
// ✅ CORRECT
plan.OriginNode = "SHORE";

// ❌ WRONG
plan.OriginNode = "Shore";  // Edge won't match!
```

### 3. targetNode for broadcast must be "*"
```csharp
// ✅ CORRECT
await _syncOutbox.EnqueueAsync(
    targetNode: "*",  // All Edge nodes
    tableName: "voyage_cargo_plan",
    ...
);

// ❌ WRONG
targetNode: "EDGE"  // Won't broadcast to all!
```

### 4. SyncVersion must increment on Update
```csharp
// ✅ CORRECT
plan.SyncVersion++;  // 0 → 1 → 2 → 3...

// ❌ WRONG
plan.SyncVersion = DateTime.UtcNow.Ticks;  // Edge idempotency breaks!
```

### 5. Enqueue happens AFTER SaveChanges
```csharp
// ✅ CORRECT
_db.VoyageCargoPlans.Add(plan);
await _db.SaveChangesAsync();  // DB saves first!

await _syncOutbox.EnqueueAsync(...);  // Then queue for sync

// ❌ WRONG (outbox queues before DB save)
await _syncOutbox.EnqueueAsync(...);
await _db.SaveChangesAsync();
```

---

## 📞 IF YOU GET STUCK

### Error: "Type 'IVoyagePlanningService' not found"
- Solution: Check Program.cs - did you add the registration line?
- Check: Using statement correct? `using ProductApi.Services.Voyage;`

### Error: "Cannot INSERT into sync_outbox; FK violation"
- Solution: Verify VoyageId exists in voyage_records table before create
- Test with known voyage ID

### Error: "Property... doesn't exist"
- Solution: Check field names match DB schema exactly (case-sensitive for JSON)
- Compare with existing VoyageCargoPlan model definition

### SyncOutbox row not created
- Solution: Verify SaveChangesAsync() call comes BEFORE EnqueueAsync()
- Check for exceptions in console (look for try-catch swallowing errors)

### Edge doesn't receive the plan (later test)
- Solution: Don't worry about this today
- Will test tomorrow in smoke tests
- Likely issue: SyncOutbox delivery, Edge pull, or conflict resolution (all tested next)

---

## 🎯 DEFINITION OF "DONE" (TODAY)

- [ ] VoyagePlanningService.cs created & compiles ✅
- [ ] Program.cs updated & compiles ✅
- [ ] VoyageController updated & compiles ✅
- [ ] Local test: Create cargo plan → 200 OK ✅
- [ ] SyncOutbox: 1 row appears after create ✅
- [ ] ZERO errors in `dotnet build` ✅
- [ ] All logging statements in place ✅

**If all checked:** You've completed Task 2.1 Phase A-C. Good work!

---

## 📅 NEXT STEPS (AFTER TODAY)

**2026-04-02:**
- Edge pulls the cargo plan from Shore
- Verify plan appears in edge_product database
- Run smoke tests 1-2

**2026-04-03:**
- Run remaining smoke tests 3-5
- Stress test concurrent updates
- Confirm no data loss

**2026-04-04:**
- Add comprehensive logging
- Document everything
- Prepare for Phase 2.2

**2026-04-05:**
- Kickoff Task 2.2 (Network Detection)

---

## ✨ SUMMARY

**What we did today:**
1. Analyzed Edge infrastructure → Ready ✅
2. Analyzed Shore infrastructure → Partial ⚠️
3. Created comprehensive documentation → 4 guides
4. Planned all 4 Phase 2 tasks → Detailed breakdown
5. Provided ready-to-code templates → Copy & paste ready

**What you do today:**
1. Code VoyagePlanningService (1.5 hrs)
2. Register in Program.cs (0.25 hrs)
3. Update VoyageController (1.5 hrs)
4. Local test: API call (0.25 hrs)
5. Verify SyncOutbox (0.25 hrs)

**Total effort:** ~4-5 hours

**Result:** Shore can enqueue planning to all Edge nodes ✅

---

**Status:** 🎬 READY TO START  
**Next Action:** Open VoyagePlanningService template & start coding  
**Questions?** See PHASE2_KICKOFF_TODAY.md for detailed guidance
