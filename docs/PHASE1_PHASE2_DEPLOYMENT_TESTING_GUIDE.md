# Phase 1 & Phase 2.1 - Deployment & Testing Guide

**Date:** April 1, 2026  
**Objective:** Complete final verification and prepare for production deployment

---

## 📋 PRE-DEPLOYMENT CHECKLIST

### 1. Code Quality Verification ✅
```bash
# Build verification
cd shore_product/backend
dotnet build
# Expected: Build succeeded with 0 errors (3 pre-existing warnings OK)

# Check for secrets  
grep -r "password\|secret\|token\|key" appsettings.json
# Expected: No actual values, only config keys

# Verify migrations
dotnet ef migrations list
# Expected: 20260401110009_AddVoyageDateTimeUtcConverters present
```

### 2. Database Preparation
```sql
-- Verify voyage planning tables exist
SELECT * FROM information_schema.tables 
WHERE table_name IN ('voyage_cargo_plan', 'voyage_bunker_plan', 
                     'voyage_crew_change_plan', 'voyage_cost_estimate', 
                     'voyage_revenue_estimate');

-- Verify sync infrastructure tables
SELECT * FROM information_schema.tables 
WHERE table_name IN ('sync_outbox', 'audit_logs', 'sync_node_trackers');
```

### 3. Service Registration Verification
**File:** `Program.cs`

```csharp
// Check these are present:
builder.Services.AddScoped<IVoyagePlanningService, VoyagePlanningService>();
builder.Services.AddScoped<ISyncOutboxService, SyncOutboxService>();
builder.Services.AddScoped<IVoyageService, VoyageService>();
```

### 4. Environment Variables
```bash
# Required for Phase 1
SECURITY_ENFORCE_HTTPS=true              # Production
SECURITY_REQUIRE_INTERNAL_ACCESS=true    # Production
INTERNAL_API_KEY=<strong-random-value>   # Min 32 chars

# Required for Phase 2+
SYNC_NETWORK_DETECTION_ENABLED=true      # Phase 2.2
SYNC_NONCE_REGISTRY_DB_BACKED=false      # Phase 2.3 (false until ready)
SYNC_DLQ_ENABLED=false                   # Phase 2.4 (false until ready)
```

---

## 🧪 LOCAL TESTING STEPS

### Step 1: Start Backend
```bash
cd shore_product/backend
dotnet run
# Expected output:
# info: Microsoft.AspNetCore.Hosting.Hosting: Application started
# info: Successfully registered 20 voyage planning endpoints
```

### Step 2: Create Test Voyage
```bash
# Via curl or Postman
POST http://localhost:5000/api/voyages
Content-Type: application/json
X-Internal-Api-Key: <internal-key>

{
  "vesselId": "00000000-0000-0000-0000-000000000001",
  "voyageNumber": "TEST-001",
  "departurePortCode": "SGP",
  "arrivalPortCode": "HKG",
  "departureAtUtc": "2026-04-05T12:00:00Z",
  "arrivalAtUtc": "2026-04-10T08:00:00Z",
  "status": "PLANNED"
}

# Expected response: 201 Created with voyage ID
# Save voyage ID for next steps: {voyageId}
```

### Step 3: Test Cargo Plan Creation
```bash
POST http://localhost:5000/api/voyages/{voyageId}/cargo-plans
Content-Type: application/json
X-Internal-Api-Key: <internal-key>

{
  "cargoType": "CONTAINERS",
  "plannedQuantity": 500,
  "unit": "TEU",
  "portCode": "HKG",
  "portName": "Hong Kong"
}

# Expected: 201 Created
# Response body includes plan ID, sync metadata (IsSynced=false, SyncVersion=0, OriginNode=SHORE)
```

### Step 4: Verify Sync Outbox Entry
```bash
# Via database query
SELECT * FROM sync_outbox 
WHERE table_name = 'voyage_cargo_plan' 
  AND action = 0  -- 0 = CREATE
  AND delivered_at IS NULL
ORDER BY created_at DESC
LIMIT 1;

# Expected: 1 row with:
# - target_node = "*" (broadcast to all Edge nodes)
# - payload contains full cargo plan entity
# - created_at shows recent timestamp
```

### Step 5: Test Plan Update
```bash
PUT http://localhost:5000/api/voyages/cargo-plans/{planId}
Content-Type: application/json
X-Internal-Api-Key: <internal-key>

{
  "cargoType": "GENERAL_CARGO",
  "plannedQuantity": 600
}

# Expected: 200 OK
# Response shows SyncVersion = 1
# New UPDATE action in sync_outbox
```

### Step 6: Test Plan Delete
```bash
DELETE http://localhost:5000/api/voyages/cargo-plans/{planId}
X-Internal-Api-Key: <internal-key>

# Expected: 204 No Content
# Plan no longer in voyage_cargo_plan table
# DELETE action in sync_outbox
```

### Step 7: Test Error Handling
```bash
# Create plan for non-existent voyage
POST http://localhost:5000/api/voyages/00000000-0000-0000-0000-000000000099/cargo-plans
X-Internal-Api-Key: <internal-key>

# Expected: 404 Not Found
# Response: { "message": "Voyage 00000000-0000-0000-0000-000000000099 not found" }
```

---

## 📊 MANUAL TEST RESULTS TEMPLATE

After running manual tests, record results:

```markdown
### Local Testing Results - [DATE]

✅ Backend Builds Without Errors
- Build time: ___ seconds
- Warnings: ___ (pre-existing OK)
- Errors: 0 ✅

✅ Test Voyage Created Successfully
- Voyage ID: ________________
- Status: PLANNED
- Ports: SGP → HKG

✅ Cargo Plan Creation
- Plan ID: ________________
- Type: CONTAINERS
- Quantity: 500 TEU
- Sync Status: IsSynced=false ✓
- SyncVersion: 0 ✓
- OriginNode: SHORE ✓

✅ Sync Outbox Entry Created
- Action: CREATE ✓
- TargetNode: * (broadcast) ✓
- DeliveredAt: null ✓
- Payload: Valid JSON ✓

✅ Plan Update Works
- New SyncVersion: 1 ✓
- New Quantity: 600 ✓
- UPDATE action queued ✓

✅ Plan Delete Works
- Record removed ✓
- DELETE action queued ✓

✅ Error Handling
- Invalid voyageId → 404 ✓
- Missing required field → 400 ✓
- Invalid JWT/API key → 401/403 ✓

✅ All 5 Planning Types Work
- ✓ Cargo Plans
- ✓ Bunker Plans
- ✓ Crew Change Plans
- ✓ Cost Estimates
- ✓ Revenue Estimates

Tested by: ________________
Date: ________________
Issues found: (list any)
```

---

## 🚀 DEPLOYMENT STEPS

### Pre-Production Deployment

**1. Backup Production Database**
```bash
pg_dump -h <host> -U <user> -d <db> -F c -f backup_$(date +%Y%m%d_%H%M%S).dump
```

**2. Deploy Updated Backend**
```bash
# Build for production
cd shore_product/backend
dotnet build -c Release --no-restore

# Copy to production server
scp -r bin/Release/net8.0/* user@prod-server:/opt/shore-api/

# Restart API service
systemctl restart shore-api
```

**3. Run Database Migration**
```bash
# On production server
cd /opt/shore-api
dotnet ef database update --context AppDbContext
```

**4. Verify Migration Success**
```bash
# Check migration was applied
psql -h <host> -U <user> -d <db> -c "SELECT * FROM __EFMigrationsHistory ORDER BY MigrationId DESC LIMIT 1;"

# Expected: 20260401110009_AddVoyageDateTimeUtcConverters present
```

**5. Smoke Test in Production**
```bash
# Test health endpoint
curl -i https://prod-api.example.com/api/health
# Expected: 200 OK

# Test creating cargo plan (use real voyage ID)
curl -X POST https://prod-api.example.com/api/voyages/{voyageId}/cargo-plans \
  -H "Content-Type: application/json" \
  -H "X-Internal-Api-Key: <prod-internal-key>" \
  -d '{"cargoType":"CONTAINERS","plannedQuantity":500}'

# Expected: 201 Created
```

**6. Monitor Logs**
```bash
# Watch for errors
tail -f /var/log/shore-api/error.log

# Monitor sync_outbox population
watch 'psql -h <host> -U <user> -d <db> -c \
  "SELECT COUNT(*) FROM sync_outbox WHERE delivered_at IS NULL;"'
```

---

## ⚠️ ROLLBACK PROCEDURE

If issues found in production:

```bash
# 1. Revert to previous code
git checkout HEAD~1 shore_product/backend

# 2. Rebuild and redeploy older version
dotnet build -c Release --no-restore
# ... copy to production ...

# 3. If database migration caused issues, rollback:
cd /opt/shore-api
dotnet ef database update 20260331180000  # Previous migration ID

# 4. Restart service
systemctl restart shore-api

# 5. Verify rollback successful
curl https://prod-api.example.com/api/health
```

---

## 📈 MONITORING AFTER DEPLOYMENT

### 1. Sync Outbox Health
```sql
-- Check outbox queue depth (should trend toward 0)
SELECT COUNT(*) as pending_count 
FROM sync_outbox 
WHERE delivered_at IS NULL 
  AND created_at > NOW() - INTERVAL '1 hour';

-- Check delivery success rate
SELECT 
  COUNT(*) as total,
  COUNT(delivered_at) as delivered,
  ROUND(100.0 * COUNT(delivered_at) / COUNT(*), 2) as success_rate
FROM sync_outbox
WHERE created_at > NOW() - INTERVAL '24 hours';
```

### 2. Error Monitoring
```bash
# Watch for VoyagePlanningNotFoundException errors
grep -i "VoyagePlanningNotFoundException" /var/log/shore-api/error.log

# Watch for UTC conversion errors
grep -i "datetime\|utc" /var/log/shore-api/error.log | grep -i error
```

### 3. Performance Metrics
```sql
-- Check average planning creation time
SELECT 
  table_name,
  COUNT(*) as operations,
  AVG(EXTRACT(EPOCH FROM (delivered_at - created_at))) as avg_delivery_seconds
FROM sync_outbox
WHERE table_name LIKE 'voyage_%'
  AND created_at > NOW() - INTERVAL '24 hours'
GROUP BY table_name;
```

---

## 📞 ESCALATION CONTACTS

If issues arise:

| Issue | Contact | Action |
|-------|---------|--------|
| Build fails | DevOps Lead | Run dotnet build locally, check warnings |
| DB migration fails | DBA | Check PostgreSQL logs, verify Npgsql version |
| Sync not working | Sync Lead | Verify SyncOutboxService registered, check logs |
| API endpoints 404 | Backend Lead | Verify VoyagesController endpoints registered |
| UTC conversion errors | Database Lead | Check AppDbContext HasConversion registrations |

---

## ✅ FINAL SIGN-OFF

- [ ] Pre-deployment checklist completed
- [ ] Local testing passed (all 7 manual tests)
- [ ] Database backup created
- [ ] Migration verified
- [ ] Smoke tests in production passed
- [ ] Monitoring alerts configured
- [ ] Team notified of deployment
- [ ] Rollback procedure documented and tested

**Deployment Date:** _______________  
**Deployed By:** _______________  
**Verified By:** _______________
