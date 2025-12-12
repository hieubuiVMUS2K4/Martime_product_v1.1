# Equipment Status Auto-Update Feature

## Overview
Automatically update Equipment Asset status when maintenance task status changes, reflecting real-time equipment availability.

## Business Logic

### Status Transitions

#### When Task → IN_PROGRESS
- **Equipment Status**: ACTIVE → **UNDER_MAINTENANCE**
- **Trigger**: PIC starts working on maintenance task
- **Scope**: 
  - Single equipment (legacy EquipmentId)
  - All equipment in group (EquipmentGroupId)

#### When Task → COMPLETED/CANCELLED
- **Equipment Status**: UNDER_MAINTENANCE → **ACTIVE**
- **Condition**: No other IN_PROGRESS tasks on same equipment
- **Smart Check**: Prevents restoring status if concurrent maintenance exists

## Implementation Details

### 1. Equipment Status Values
```csharp
public string Status { get; set; } = "ACTIVE";
// - ACTIVE: In operation
// - STANDBY: Spare/backup
// - UNDER_MAINTENANCE: Being serviced
// - DECOMMISSIONED: Retired
// - IN_STORAGE: Stored
```

### 2. Core Methods

#### `UpdateEquipmentStatusForTaskAsync(task, newStatus, oldStatus)`
**Purpose**: Update equipment status when task status changes

**Logic**:
```csharp
// Set UNDER_MAINTENANCE
if (newStatus == IN_PROGRESS && oldStatus != IN_PROGRESS)
    equipment.Status = "UNDER_MAINTENANCE";

// Restore ACTIVE (only if no other maintenance)
if (newStatus == COMPLETED/CANCELLED && oldStatus == IN_PROGRESS)
{
    if (!hasOtherActiveMaintenance)
        equipment.Status = "ACTIVE";
}
```

**Handles**:
- ✅ EquipmentGroup: Updates ALL members
- ✅ Legacy EquipmentId: Updates single asset by AssetCode
- ✅ Concurrent maintenance: Checks other IN_PROGRESS tasks
- ✅ Logging: Full audit trail

#### `ValidateEquipmentAvailabilityAsync(task)`
**Purpose**: Prevent starting new task if equipment under CRITICAL maintenance

**Returns**:
```csharp
(bool IsValid, string? Message)
```

**Logic**:
```csharp
if (equipment.Status == "UNDER_MAINTENANCE")
{
    var blockingTask = Find CRITICAL priority task;
    if (blockingTask exists)
        return (false, "Equipment under CRITICAL maintenance");
}
return (true, null);
```

**Business Rule**: CRITICAL tasks have priority - cannot start new maintenance until completed

### 3. Integration Point

**Endpoint**: `PATCH /api/maintenance/tasks/{id}/status`

**Enhanced Flow**:
```csharp
[HttpPatch("tasks/{id}/status")]
public async Task<IActionResult> UpdateTaskStatus(...)
{
    // 1. Validate status transition
    ValidateStatusTransition();
    
    // ✨ NEW: Validate equipment availability
    if (newStatus == IN_PROGRESS)
    {
        var check = await ValidateEquipmentAvailabilityAsync(task);
        if (!check.IsValid)
            return BadRequest(check.Message);
    }
    
    // 2. Update task status
    task.Status = newStatus;
    await _context.SaveChangesAsync();
    
    // ✨ NEW: Update equipment status
    await UpdateEquipmentStatusForTaskAsync(task, newStatus, oldStatus);
    
    return Ok();
}
```

## Use Cases

### Scenario 1: Single Equipment Maintenance
```
1. Task: "Main Engine Oil Change" (EquipmentId: "ME-01")
2. PIC starts → Task: IN_PROGRESS
   → Equipment ME-01: ACTIVE → UNDER_MAINTENANCE
3. PIC completes → Task: COMPLETED
   → Equipment ME-01: UNDER_MAINTENANCE → ACTIVE
```

### Scenario 2: Equipment Group Maintenance
```
1. Task: "All Generators Inspection" (EquipmentGroupId: "GEN-GROUP")
   Group Members: AE-01, AE-02, AE-03
2. PIC starts → Task: IN_PROGRESS
   → Equipment AE-01, AE-02, AE-03: ACTIVE → UNDER_MAINTENANCE
3. PIC completes → Task: COMPLETED
   → Equipment AE-01, AE-02, AE-03: UNDER_MAINTENANCE → ACTIVE
```

### Scenario 3: Concurrent Maintenance (Smart Handling)
```
1. Task A: "Generator Daily Check" → IN_PROGRESS
   → AE-01: ACTIVE → UNDER_MAINTENANCE
2. Task B: "Generator Filter Change" → IN_PROGRESS (on same AE-01)
   → AE-01: Remains UNDER_MAINTENANCE
3. Task A: COMPLETED
   → AE-01: Remains UNDER_MAINTENANCE (Task B still active)
4. Task B: COMPLETED
   → AE-01: UNDER_MAINTENANCE → ACTIVE (no other tasks)
```

### Scenario 4: CRITICAL Task Priority
```
1. Equipment: AE-01 (Status: UNDER_MAINTENANCE)
   Active Task: "Generator Major Overhaul" (CRITICAL priority)
2. Try to start: "Generator Oil Check" (NORMAL priority)
   → ❌ BLOCKED: "Equipment AE-01 is under CRITICAL maintenance (Task: GEN-OVERHAUL-001). Complete critical task first."
3. Complete critical task → AE-01: ACTIVE
4. Now can start: "Generator Oil Check" ✅
```

## Error Handling

### Non-Blocking Design
```csharp
catch (Exception ex)
{
    _logger.LogError(ex, "Error updating equipment status");
    // Don't throw - this is a secondary operation
    // Task status update should succeed even if equipment update fails
}
```

**Rationale**: Equipment status is auxiliary data. Task workflow is primary - shouldn't fail entire operation.

### Logging
- ✅ Info: Status changes
- ✅ Warning: Equipment not found
- ⚠️ Error: Update failures (but doesn't block)

## Database Impact

### Tables Modified
1. **EquipmentAssets**
   - Field: `Status`
   - Field: `UpdatedAt`
   - Field: `IsSynced` (for delta sync)

### Queries Added
```sql
-- Find equipment in group
SELECT * FROM EquipmentGroupMembers 
WHERE GroupId = ? 
JOIN EquipmentAssets ON AssetId = Id;

-- Check concurrent maintenance
SELECT COUNT(*) FROM MaintenanceTasks
WHERE (EquipmentGroupId = ? OR EquipmentId = ?)
  AND Status = 'IN_PROGRESS'
  AND Id != ?;

-- Find blocking CRITICAL task
SELECT * FROM MaintenanceTasks
WHERE EquipmentGroupId = ?
  AND Status = 'IN_PROGRESS'
  AND Priority = 'CRITICAL';
```

## Frontend Implications

### Equipment Assets Page
- Should display real-time status: ACTIVE, UNDER_MAINTENANCE, etc.
- Color coding:
  - 🟢 ACTIVE: Green
  - 🟡 UNDER_MAINTENANCE: Yellow/Orange
  - 🔴 DECOMMISSIONED: Red
  - ⚪ STANDBY: Gray

### Mobile App
- Sync equipment status via delta sync
- Show equipment availability before starting task
- Display warning if equipment already under maintenance

### Master Schedule / Gantt
- Visual indicator for equipment under maintenance
- Prevent scheduling new tasks on unavailable equipment

## Testing Scenarios

### Manual Test Cases

#### TC1: Single Equipment Status Update
1. Create task for equipment ME-01
2. Verify ME-01 status = ACTIVE
3. Start task (→ IN_PROGRESS)
4. Verify ME-01 status = UNDER_MAINTENANCE
5. Complete task
6. Verify ME-01 status = ACTIVE

#### TC2: Equipment Group Status Update
1. Create task for Equipment Group "Generators" (3 members)
2. Verify all 3 generators = ACTIVE
3. Start task
4. Verify all 3 generators = UNDER_MAINTENANCE
5. Complete task
6. Verify all 3 generators = ACTIVE

#### TC3: Concurrent Maintenance
1. Start Task A on ME-01 (Status → UNDER_MAINTENANCE)
2. Start Task B on ME-01 (Status remains UNDER_MAINTENANCE)
3. Complete Task A
4. Verify ME-01 STILL = UNDER_MAINTENANCE (Task B active)
5. Complete Task B
6. Verify ME-01 = ACTIVE

#### TC4: CRITICAL Task Blocking
1. Start CRITICAL task on AE-01
2. Verify AE-01 = UNDER_MAINTENANCE
3. Try to start NORMAL task on AE-01
4. Verify: Request BLOCKED with error message
5. Complete CRITICAL task
6. Retry NORMAL task
7. Verify: Request SUCCESS

#### TC5: Legacy EquipmentId Handling
1. Create task with old-style EquipmentId (string)
2. Start task
3. Verify equipment found by AssetCode and status updated

## Future Enhancements

### Planned
- [ ] Equipment downtime analytics (time spent UNDER_MAINTENANCE)
- [ ] Notification to crew when equipment becomes available
- [ ] Equipment status history log (audit trail)
- [ ] Integration with spare parts allocation (reserve parts when UNDER_MAINTENANCE)

### Considerations
- [ ] Add `MaintenanceStartedBy` to track who initiated
- [ ] Add `EstimatedCompletionTime` for better planning
- [ ] Location-based validation (can't maintain equipment if not accessible)
- [ ] Weather-based deferral (outdoor equipment in bad weather)

## Migration Notes

### Existing Data
- All existing EquipmentAssets have default Status = "ACTIVE"
- No migration needed - works with current schema
- Backward compatible with legacy EquipmentId tasks

### Deployment
1. ✅ Build backend (no DB migration required)
2. ✅ Deploy API
3. ⚠️ Monitor logs for any equipment not found warnings
4. ✅ Update frontend to display Status field

## API Documentation

### Updated Endpoint

**PATCH** `/api/maintenance/tasks/{id}/status`

**Request**:
```json
{
  "status": "IN_PROGRESS"
}
```

**Response** (Success):
```json
{
  "id": "guid",
  "taskId": "TASK-001",
  "status": "IN_PROGRESS",
  "previousStatus": "DUE",
  "message": "Status updated from DUE to IN_PROGRESS"
}
```

**Response** (Equipment Blocked):
```json
{
  "error": "Equipment not available",
  "message": "Equipment ME-01 is under CRITICAL maintenance (Task: OVERHAUL-001). Complete critical task first."
}
```

## Summary

This feature closes the loop between task management and equipment availability, ensuring:
- ✅ Real-time equipment status visibility
- ✅ Prevention of conflicting maintenance operations
- ✅ Automatic status restoration when work completed
- ✅ CRITICAL task priority enforcement
- ✅ Support for both individual and group-based maintenance
- ✅ Backward compatible with legacy system

**Status**: ✅ Implemented and Tested
**Version**: 1.0
**Last Updated**: December 12, 2025
