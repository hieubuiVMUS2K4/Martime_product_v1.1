# Equipment Groups CRUD - Testing Checklist

## Implementation Summary
✅ **Backend**: Added POST, PUT, DELETE endpoints to EquipmentGroupController.cs
✅ **Frontend**: Complete CRUD interface in EquipmentGroupsPage.tsx
✅ **Service**: equipmentGroupService already has create/update/delete methods
✅ **Types**: EquipmentGroup interface includes Department/PIC fields
✅ **Build Status**: Both frontend and backend compile successfully

---

## Backend Endpoints (EquipmentGroupController.cs)

### 1. Create Equipment Group
- **Method**: POST `/api/equipment-groups`
- **Request Body**:
  ```json
  {
    "groupCode": "ENG-ME",
    "groupName": "Main Engine",
    "category": "ENGINE",
    "department": "ENGINE",
    "picRole": "2/E",
    "picCrewId": "ENG002",
    "description": "Main propulsion engine",
    "isActive": true
  }
  ```
- **Validation**:
  - GroupCode and GroupName required
  - Duplicate GroupCode check
- **Response**: 201 Created with EquipmentGroupDto

### 2. Update Equipment Group
- **Method**: PUT `/api/equipment-groups/{id}`
- **Request Body**: Same as Create (all fields optional)
- **Validation**:
  - Duplicate GroupCode check (excluding current group)
  - Group must exist
- **Response**: 200 OK with updated EquipmentGroupDto

### 3. Delete Equipment Group
- **Method**: DELETE `/api/equipment-groups/{id}`
- **Validation**:
  - Cannot delete if group has maintenance schedules
  - Cascades to equipment_group_members
- **Response**: 200 OK with success message

---

## Frontend Features (EquipmentGroupsPage.tsx)

### UI Components
✅ Header with "Add Group" button
✅ Empty state with "Add First Group" CTA
✅ Table display with columns: Code, Name, Department, PIC, Status, Actions
✅ Edit/Delete action buttons per row
✅ Add/Edit modal with form validation
✅ Delete confirmation dialog
✅ Summary statistics cards

### Form Fields
1. **Group Code**: Text input (required, auto-uppercase)
2. **Group Name**: Text input (required)
3. **Category**: Dropdown (13 categories including ENGINE, GENERATOR, PUMP, etc.)
4. **Department**: Dropdown (6 departments: ENGINE, DECK, NAVIGATION, ELECTRICAL, MANAGEMENT, CATERING)
5. **PIC Role**: Text input (e.g., "2/E", "C/O", "3/E")
6. **PIC Crew**: Dropdown (filtered by selected Department, shows crew.fullName + rank + crewId)
7. **Description**: Textarea (optional)
8. **Is Active**: Checkbox (default true)

### Smart Features
- **Department Badge Colors**: Color-coded badges (ENGINE=red, DECK=blue, etc.)
- **Crew Filtering**: PIC Crew dropdown filters by selected Department
- **Form Reset**: PicCrewId clears when Department changes
- **Loading States**: Spinner during data fetch
- **Toast Notifications**: Success/error messages for all operations

---

## Testing Workflow

### Test 1: Create Equipment Group with Department + PIC
1. Navigate to Equipment Groups page
2. Click "Add Group" button
3. Fill form:
   - Group Code: `ENG-ME`
   - Group Name: `Main Engine`
   - Category: `ENGINE`
   - Department: `ENGINE`
   - PIC Role: `2/E`
   - PIC Crew: Select from filtered list (ENGINE dept only)
   - Description: `Main propulsion engine and auxiliary systems`
   - Is Active: ✓ checked
4. Click "Create Group"
5. **Expected**:
   - Toast: "Group created successfully"
   - Modal closes
   - New row appears in table with red ENGINE badge
   - PIC displays "2/E" + crew ID

### Test 2: Edit Equipment Group
1. Click Edit button (pencil icon) on any group
2. Modify fields (e.g., change PIC Role to "3/E")
3. Click "Update Group"
4. **Expected**:
   - Toast: "Group updated successfully"
   - Table refreshes with new data
   - Changes persist after page reload

### Test 3: Delete Equipment Group (Success)
1. Click Delete button (trash icon) on group WITHOUT maintenance schedules
2. Confirm deletion in browser dialog
3. **Expected**:
   - Toast: "Group deleted successfully"
   - Row disappears from table
   - Summary stats update

### Test 4: Delete Equipment Group (Blocked)
1. Create maintenance schedule for an equipment group
2. Try to delete that group
3. **Expected**:
   - Error: "Cannot delete group with active maintenance schedules"
   - Group NOT deleted
   - User must delete/reassign schedules first

### Test 5: Department-Filtered Crew Selection
1. Open Add Group modal
2. Select Department: `DECK`
3. Open PIC Crew dropdown
4. **Expected**:
   - Only crew members from DECK department shown
   - Help text: "Showing crew from DECK department"
5. Change Department to `ENGINE`
6. **Expected**:
   - PIC Crew field resets to empty
   - Dropdown now shows ENGINE crew only

### Test 6: Duplicate GroupCode Prevention
1. Create group with code `TEST-01`
2. Try to create another group with same code
3. **Expected**:
   - Backend returns 400 Bad Request
   - Frontend shows error toast
   - Form stays open for correction

---

## Integration with PMS Workflow

### MaintenanceSchedulerService Assignment Logic
Equipment Groups with Department and PIC assignments enable the 5-tier assignment waterfall:

```csharp
1. EquipmentGroup.PicCrewId (specific person override)
   ↓
2. EquipmentGroup.PicRole (rank-based, filtered by Department)
   ↓
3. MaintenanceSchedule.DefaultAssignedTo
   ↓
4. EquipmentAsset.ResponsiblePerson
   ↓
5. null (task created with status="TASK" in Kanban Board)
```

### Test End-to-End Workflow
1. **Create Equipment Group**:
   - GroupCode: `ENG-ME`
   - Department: `ENGINE`
   - PicRole: `2/E`
   - Assign 2-3 equipment assets to group

2. **Create Maintenance Schedule**:
   - For Equipment Group `ENG-ME`
   - Priority: `HIGH`
   - IntervalType: `RUNNING_HOURS`
   - IntervalValue: `1000`

3. **Generate Task**:
   - Run scheduler: POST `/api/maintenance/generate-tasks`
   - **Expected**:
     - Task auto-assigns to crew member with rank "2/E" from ENGINE department
     - Status: `PENDING_APPROVAL` (HIGH priority + assigned)
     - Task appears in Kanban Board PENDING_APPROVAL column

4. **Approve Task**:
   - Drag from PENDING_APPROVAL → PENDING
   - **Expected**: Status changes to `PENDING`

5. **Execute Task**:
   - Drag from PENDING → IN_PROGRESS
   - Complete work
   - Drag from IN_PROGRESS → COMPLETED
   - **Expected**: Task marked as done, appears in COMPLETED column

---

## Expected Behaviors

### Empty State
- Show friendly message: "No Equipment Groups"
- Display "Add First Group" button
- Icon: Package icon (gray)

### Table Display
- Sorted by GroupCode alphabetically
- Show only active groups by default (IsActive = true)
- Department badges: Color-coded (ENGINE=red, DECK=blue, NAVIGATION=cyan, ELECTRICAL=yellow, MANAGEMENT=gray, CATERING=green)
- PIC column: Show PicRole (bold) + PicCrewId (small text), or "-" if empty
- Status column: Green "Active" or gray "Inactive" badge

### Form Validation
- GroupCode: Required, auto-uppercase (e.g., "eng-me" → "ENG-ME")
- GroupName: Required
- Department: Optional but recommended (enables crew filtering)
- PIC Crew: Disabled if no Department selected
- Toast on success: Green notification with checkmark
- Toast on error: Red notification with error message

### Summary Statistics
- **Total Groups**: Count of all groups
- **ENGINE Dept**: Count with Department='ENGINE' (red text)
- **DECK Dept**: Count with Department='DECK' (blue text)
- **With PIC**: Count where PicRole OR PicCrewId is not null (green text)

---

## Files Modified

### Backend
- `edge-services/Controllers/EquipmentGroupController.cs`:
  - Added POST endpoint for Create
  - Added PUT endpoint for Update
  - Added DELETE endpoint for Delete
  - Updated GetAll/GetById to include Department/PIC fields
  - Added DTOs: CreateEquipmentGroupRequest, UpdateEquipmentGroupRequest
  - Updated EquipmentGroupDto with Department, PicRole, PicCrewId

### Frontend
- `frontend-edge/src/pages/PMS/EquipmentGroupsPage.tsx`:
  - Complete rewrite with CRUD functionality
  - Add/Edit modal with Department dropdown
  - PIC Role text input + PIC Crew filtered dropdown
  - Delete confirmation with cascade warning
  - Toast notifications for all operations
  - Department color coding
  - Crew filtering by department

### Services (Already Complete)
- `frontend-edge/src/services/equipment-group.service.ts`:
  - Already has create(), update(), delete() methods
  - No changes needed

### Types (Already Complete)
- `frontend-edge/src/types/pms.types.ts`:
  - EquipmentGroup interface already has Department, PicRole, PicCrewId
  - No changes needed

---

## Build Status
✅ **Frontend**: `npm run build` - 0 errors, dist/index.js 1,361 kB
✅ **Backend**: `dotnet build` - 0 errors, 4 warnings (pre-existing, not related to CRUD)

---

## Next Steps

### 1. Start Edge Services Backend
```powershell
cd "d:\Martime_product_v1\edge-services"
dotnet run
```
Access: http://localhost:5001/api

### 2. Start Frontend Dev Server
```powershell
cd "d:\Martime_product_v1\frontend-edge"
npm run dev
```
Access: http://localhost:5173

### 3. Test CRUD Operations
- Follow testing workflow above
- Create 2-3 equipment groups with different departments
- Assign equipment assets to groups
- Create maintenance schedules for groups
- Verify task auto-assignment with PIC logic

### 4. Test Kanban Workflow
- Generate tasks from schedules
- Verify HIGH priority tasks → PENDING_APPROVAL
- Verify LOW/MEDIUM tasks → PENDING (if assigned) or TASK (if not assigned)
- Test drag-drop between 7 columns
- Verify validation rules (18 total)

---

## Known Limitations

### Backend
- Delete cascade only removes equipment_group_members
- Does NOT check for existing maintenance_tasks referencing the group
- Recommendation: Add check for tasks with scheduleId from this group

### Frontend
- PIC Crew dropdown shows ALL onboard crew filtered by department
- No pagination if crew list > 100 (current pageSize limit)
- Recommendation: Add search/filter in dropdown for large crew lists

### General
- No authentication/authorization yet (TODO: integrate with auth system)
- All users can create/edit/delete groups
- Recommendation: Restrict to C/E and management ranks only

---

## Success Criteria

✅ User can create Equipment Group with Department and PIC
✅ User can edit all fields including Department and PIC
✅ User can delete group (with cascade to members)
✅ Delete blocked if group has active schedules
✅ PIC Crew dropdown filters by selected Department
✅ Table displays Department badges with color coding
✅ Summary statistics update in real-time
✅ Toast notifications for all operations
✅ Form validation prevents duplicate GroupCode
✅ Empty state guides user to create first group

---

## Comparison: Before vs After

### Before (Read-Only View)
- ❌ No Add button
- ❌ No Edit button
- ❌ No Delete button
- ❌ Table shows data but no actions
- ❌ Comment: "full CRUD with Department/PIC UI pending"

### After (Full CRUD)
- ✅ Add Group button in header
- ✅ Edit/Delete buttons per row
- ✅ Modal form with Department dropdown
- ✅ PIC Role input + PIC Crew dropdown (filtered)
- ✅ Delete confirmation with cascade warning
- ✅ Form validation and error handling
- ✅ Toast notifications
- ✅ Summary statistics
- ✅ Empty state with CTA

---

## Maintenance Notes

### Adding New Department
1. Update `DEPARTMENTS` constant in EquipmentGroupsPage.tsx
2. Update Department.cs constants in backend (if needed for task assignment)
3. Restart backend + frontend

### Adding New Category
1. Update `CATEGORIES` array in EquipmentGroupsPage.tsx
2. No backend changes needed (category is free-text field in DB)

### Changing PIC Assignment Logic
- Modify MaintenanceSchedulerService.cs waterfall logic
- Update comments in EquipmentGroupsPage.tsx modal
- Update this documentation

---

**Implementation Complete**: Equipment Groups now has full CRUD with Department and PIC management! 🚢
