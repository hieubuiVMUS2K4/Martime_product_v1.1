# Department-Based Task Assignment Implementation

## Overview
This document describes the implementation of department-based task assignment and approval workflow for the Maritime PMS system. This addresses the critical organizational structure of maritime vessels where work must respect departmental boundaries and hierarchical approval chains.

## Problem Statement
Previous implementation had fundamental design flaw:
- No departmental boundaries - system could assign ENGINE work to DECK crew
- No Person In Charge (PIC) concept for equipment groups
- No approval workflow for HIGH/CRITICAL tasks
- Violated maritime organizational principles and ISM Code requirements

## Maritime Organizational Structure

### Department Hierarchy

#### ENGINE Department
- **C/E (Chief Engineer)**: Department Head, Approver for all ENGINE work
- **2/E (Second Engineer)**: Work Planner - creates daily/weekly maintenance plans
- **3/E (Third Engineer)**: Senior executor
- **4/E (Fourth Engineer)**: Junior executor
- **E/O (Electrical Officer)**: Electrical systems specialist
- **Fitters/Oilers**: Equipment maintainers

#### DECK Department
- **C/O (Chief Officer)**: Department Head, Work Planner for DECK
- **2/O (Second Officer)**: Senior deck officer
- **3/O (Third Officer)**: Junior deck officer
- **Bosun**: Deck work supervisor
- **AB (Able Seaman)**: Experienced deck crew
- **OS (Ordinary Seaman)**: Junior deck crew

#### Other Departments
- **NAVIGATION**: Master, Officers
- **MANAGEMENT**: Master, C/E, C/O (leadership)
- **ELECTRICAL**: E/O, Electricians
- **CATERING**: Cook, Stewards

## Implementation Details

### 1. Database Changes

#### EquipmentGroup Model (EdgeModels.cs)
Added three new fields:
```csharp
public string? Department { get; set; }   // "ENGINE", "DECK", "NAVIGATION", etc.
public string? PicRole { get; set; }      // "2/E", "C/O", "E/O" - Person In Charge rank
public string? PicCrewId { get; set; }    // Specific crew override for PIC
```

**Migration**: `AddDepartmentAndPicToEquipmentGroup`

#### MaintenanceTask Model (EdgeModels.cs)
Added approval tracking fields:
```csharp
public string? ApprovedBy { get; set; }        // Crew ID who approved
public DateTime? ApprovedAt { get; set; }      // When task was approved
public string? RejectionReason { get; set; }   // If status is REJECTED
```

**Migration**: `AddApprovalFieldsToMaintenanceTask`

### 2. Constants (Department.cs)

#### Department Definitions
```csharp
public const string ENGINE = "ENGINE";      // Engine room equipment
public const string DECK = "DECK";          // Deck machinery and hull
public const string NAVIGATION = "NAVIGATION"; // Bridge equipment
public const string MANAGEMENT = "MANAGEMENT"; // Administrative
public const string ELECTRICAL = "ELECTRICAL"; // Electrical systems
public const string CATERING = "CATERING";  // Galley equipment
```

#### Extended Task Statuses
```csharp
public const string PENDING_APPROVAL = "PENDING_APPROVAL"; // Awaiting C/E/Master approval
public const string REJECTED = "REJECTED";                 // Task rejected, needs revision
```

### 3. Service Logic Updates

#### MaintenanceSchedulerService.cs - DetermineTaskAssignee()

**New 5-Tier Waterfall Logic** (Department-Aware):

0. **Department Filter**: Only consider crew from equipment group's department
1. **Group.PicCrewId**: Specific PIC override (highest priority)
2. **Group.PicRole**: Person In Charge rank for this equipment
3. **Schedule.AssignedToCrewId**: Specific crew override from schedule
4. **Schedule.AssignedToRole**: Role-based assignment from schedule
5. **Asset.DefaultExecutorRole**: Equipment-based default
6. **null**: Unassigned - Work Planner will assign manually

**Key Changes**:
```csharp
// Filter crew by department (strict boundary)
IQueryable<CrewMember> departmentCrewQuery = context.CrewMembers
    .Where(c => c.IsOnboard);

if (!string.IsNullOrWhiteSpace(group.Department))
{
    departmentCrewQuery = departmentCrewQuery.Where(c => c.Department == group.Department);
}
```

**Priority-Based Status**:
- HIGH/CRITICAL tasks → Status = `PENDING_APPROVAL` (requires approval)
- NORMAL/LOW tasks → Status = `PENDING` (ready for assignment)

### 4. API Endpoints

#### POST /api/maintenance/tasks/{id}/approve

**Purpose**: Approve or reject HIGH/CRITICAL tasks (C/E or Master only)

**Request Body**:
```json
{
  "isApproved": true,           // true = approve, false = reject
  "rejectionReason": "...",     // Required if isApproved = false
  "approvedBy": "CREW002"       // Crew ID of approver (C/E or Master)
}
```

**Authorization Rules**:
- ENGINE department: C/E or Master only
- DECK department: C/O or Master only
- Other departments: Master only

**Status Transitions**:
- Approve: `PENDING_APPROVAL` → `PENDING` (ready for execution)
- Reject: `PENDING_APPROVAL` → `REJECTED` (needs revision)

**Response**:
```json
{
  "id": "guid",
  "taskId": "SCHED-...",
  "status": "PENDING",
  "approvedBy": "CREW002",
  "approvedAt": "2024-12-07T22:41:00Z",
  "message": "Task approved by C/E John Smith"
}
```

## Workflow Examples

### Example 1: Main Engine Oil Change (AUTO-ASSIGNED to 2/E)

**Equipment Group**: Main Engine System
- Department: `ENGINE`
- PicRole: `2/E`
- PicCrewId: `CREW004` (Second Engineer)

**Task Generation**:
1. Schedule triggers at NextDueDate
2. System calls `DetermineTaskAssignee()`
3. Filters crew: `WHERE Department = 'ENGINE' AND IsOnboard = true`
4. Finds PIC: `CREW004` (2/E) via `group.PicRole`
5. Priority: `HIGH` → Status: `PENDING_APPROVAL`
6. Auto-assigns to: `CREW004` (2/E)

**Approval Flow**:
1. C/E reviews task in Approval Dashboard
2. POST `/api/maintenance/tasks/{id}/approve` with `approvedBy: "CREW002"` (C/E)
3. Status changes: `PENDING_APPROVAL` → `PENDING`
4. 2/E can now reassign to Fitter if needed

### Example 2: Deck Painting (UNASSIGNED, Needs C/O Assignment)

**Equipment Group**: Main Deck
- Department: `DECK`
- PicRole: `null` (no specific PIC)
- PicCrewId: `null`

**Task Generation**:
1. Schedule triggers
2. System calls `DetermineTaskAssignee()`
3. Filters crew: `WHERE Department = 'DECK' AND IsOnboard = true`
4. No PIC → No specific role → Returns `null`
5. Priority: `NORMAL` → Status: `PENDING`
6. Task unassigned (C/O will assign to Bosun → AB)

**Assignment Flow**:
1. C/O opens Work Planning Dashboard
2. Sees unassigned DECK tasks
3. POST `/api/maintenance/tasks/{id}/assign` with `crewId: "CREW014"` (Bosun)
4. Bosun executes or delegates to AB crew

### Example 3: Cross-Department Assignment Prevention

**Scenario**: System tries to assign ENGINE work to DECK crew

**Old Behavior** (WRONG):
- Could assign Main Engine work to Bosun (DECK crew)
- Violated organizational boundaries

**New Behavior** (CORRECT):
```csharp
// Department filter ensures only ENGINE crew considered
departmentCrewQuery = departmentCrewQuery.Where(c => c.Department == "ENGINE");
// Bosun (Department = "DECK") is excluded from candidates
```

## User Roles and Dashboards

### Chief Engineer (C/E) - CREW002
**Primary Dashboard**: `/pms/approval-dashboard`
- View all tasks with Status = `PENDING_APPROVAL`
- Filter by Department = `ENGINE`
- Quick approve/reject buttons
- Shows task priority, equipment, assignee

### Second Engineer (2/E) - CREW004
**Primary Dashboard**: `/pms/work-planning`
- View all ENGINE department tasks (not just unassigned)
- Group by status: PENDING → IN_PROGRESS → COMPLETED
- Assign/reassign tasks to junior engineers and fitters
- This is THE key user for daily maintenance planning

### Chief Officer (C/O) - CREW003
**Primary Dashboard**: `/pms/work-planning` (DECK filter)
- View all DECK department tasks
- Assign work to Bosun → AB → OS
- Plan deck maintenance schedules

### Fitters/Oilers - CREW008-011
**Primary Dashboard**: Mobile app (frontend-mobile)
- View tasks assigned to them
- Start/complete tasks
- Upload photos, complete checklists

## Benefits

### 1. Organizational Compliance
- Respects maritime hierarchical structure
- Aligns with ISM Code Chapter 10 requirements
- Prevents cross-department assignment errors

### 2. Clear Responsibility
- PIC concept ensures someone owns each equipment group
- Department heads can see all their department's work
- Work Planners (2/E, C/O) have dedicated planning tools

### 3. Risk Management
- HIGH/CRITICAL tasks require senior approval
- Approval chain documented (ApprovedBy + ApprovedAt)
- Rejected tasks tracked with reasons

### 4. Operational Efficiency
- Auto-assignment respects department boundaries
- PIC automatically assigned to their equipment
- Work Planners can focus on scheduling, not basic assignment

## Testing Recommendations

### 1. Department Filtering
- Create ENGINE group with Department="ENGINE", verify only ENGINE crew considered
- Create DECK group with Department="DECK", verify DECK crew isolation
- Test null department (should consider all onboard crew)

### 2. PIC Priority
- Set group.PicRole="2/E", verify CREW004 gets priority
- Set group.PicCrewId="CREW006" (E/O), verify specific crew override
- Test PIC not onboard, should fall back to next tier

### 3. Approval Workflow
- Create HIGH priority task → verify Status="PENDING_APPROVAL"
- Approve as C/E → verify Status="PENDING"
- Try approve as 2/E → verify rejection (not authorized)
- Reject task → verify Status="REJECTED" + RejectionReason

### 4. Status Transitions
- Test PENDING_APPROVAL → PENDING (approved)
- Test PENDING_APPROVAL → REJECTED (rejected)
- Test PENDING → IN_PROGRESS (assigned and started)
- Test IN_PROGRESS → COMPLETED (finished by crew)

## Next Steps

### Frontend Implementation (Pending)
1. **C/E Approval Dashboard** (`/pms/approval-dashboard`)
   - List tasks with Status="PENDING_APPROVAL"
   - Filter by Department="ENGINE"
   - Approve/Reject modal with reason input
   - Real-time count badge in navigation

2. **2/E Work Planning Dashboard** (`/pms/work-planning`)
   - List all ENGINE tasks grouped by status
   - Drag-and-drop reassignment
   - Calendar view for scheduled work
   - Equipment group overview with PIC assignments

3. **EquipmentGroup Form Updates** (`AddGroupModal.tsx`, `EditGroupModal.tsx`)
   - Department dropdown selector
   - PicRole dropdown (filtered by department)
   - PicCrewId dropdown (onboard crew matching department/role)
   - Helper text explaining PIC concept

### Database Seeding (Recommended)
1. Update existing equipment groups with Department and PicRole:
   ```sql
   -- Main Engine System
   UPDATE equipment_groups 
   SET department = 'ENGINE', pic_role = '2/E', pic_crew_id = 'CREW004'
   WHERE group_code = 'ENG-MAIN';
   
   -- Deck Machinery
   UPDATE equipment_groups 
   SET department = 'DECK', pic_role = 'C/O', pic_crew_id = 'CREW003'
   WHERE group_code LIKE 'DECK-%';
   ```

2. Create sample HIGH/CRITICAL tasks to test approval workflow

## Files Modified

### Backend
- `edge-services/Models/EdgeModels.cs`
  - EquipmentGroup: Added Department, PicRole, PicCrewId
  - MaintenanceTask: Added ApprovedBy, ApprovedAt, RejectionReason

- `edge-services/Models/Department.cs` (NEW)
  - Department constants
  - Extended task statuses

- `edge-services/Services/MaintenanceSchedulerService.cs`
  - Updated DetermineTaskAssignee() with department filtering
  - Added PIC priority logic
  - Priority-based status assignment

- `edge-services/Controllers/MaintenanceController.cs`
  - Added POST `/api/maintenance/tasks/{id}/approve` endpoint
  - Added ApproveTaskRequest DTO
  - Rank-based authorization validation

### Migrations
- `AddDepartmentAndPicToEquipmentGroup` - Applied ✅
- `AddApprovalFieldsToMaintenanceTask` - Applied ✅

### Frontend (Pending Implementation)
- `frontend-edge/src/pages/pms/ApprovalDashboardPage.tsx` (NEW)
- `frontend-edge/src/pages/pms/WorkPlanningPage.tsx` (NEW)
- `frontend-edge/src/components/pms/AddGroupModal.tsx` (UPDATE)
- `frontend-edge/src/components/pms/EditGroupModal.tsx` (UPDATE)

## Conclusion
This implementation provides the foundational organizational structure required for a maritime PMS system. It respects vessel hierarchy, prevents cross-department errors, implements proper approval chains, and provides role-based planning tools for 2/E and C/O positions.

**Status**: Backend complete, Frontend pending
**Date**: December 7, 2024
**Author**: GitHub Copilot + User (ANNIE GAS 09 domain expertise)
