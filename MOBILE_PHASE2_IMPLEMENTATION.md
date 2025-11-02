# Mobile Phase 2 Implementation - Kanban Workflow & Checklist System

## 📋 Overview

Phase 2 đã hoàn tất việc cập nhật mobile app để hỗ trợ **Kanban workflow** và **TaskType checklist system** giống như frontend-edge.

## ✅ Completed Tasks

### 1. New Data Models Created

#### **task_type.dart**
- Model cho task templates (ENGINE_OIL_CHANGE, SAFETY_CHECK, etc.)
- Fields: id, typeCode, typeName, description, category, priority, estimatedDurationMinutes

#### **task_detail.dart**
- Model cho checklist items của mỗi TaskType
- Fields: id, taskTypeId, detailName, detailType (MEASUREMENT/CHECKLIST/INSPECTION)
- Support for: measurements (with min/max values), checklist items, inspections
- Flags: isMandatory, requiresPhoto

#### **maintenance_task_detail.dart**
- Model cho execution records (actual completion data)
- Fields: id, maintenanceTaskId, taskDetailId, measuredValue, checkResult, inspectionNotes, photoUrl
- Tracks completion status, completed by, completed at

#### **task_checklist_item.dart**
- Combined view: TaskDetail template + MaintenanceTaskDetail execution
- Provides easy access to both template and actual data

#### **task_progress.dart**
- Progress tracking model
- Fields: totalItems, completedItems, mandatoryItems, completedMandatoryItems
- Calculates completionPercentage
- `canComplete` flag indicates if all mandatory items are done

#### **complete_checklist_item_request.dart**
- Request DTO for completing checklist items
- Fields: measuredValue, checkResult, inspectionNotes, photoUrl, isCompleted

### 2. Updated Existing Models

#### **maintenance_task.dart**
- Added `taskTypeId?: int` - Links to TaskType template
- Added `taskTypeName?: string` - Display name of task type
- Added `hasTaskType` computed property - Check if task uses new system

### 3. API Client Updates

#### **task_api.dart**
Added 3 new endpoints:

```dart
// Get checklist with execution status
@GET('/api/maintenance/tasks/{taskId}/checklist')
Future<List<TaskChecklistItem>> getTaskChecklist(@Path('taskId') int taskId);

// Complete a checklist item
@POST('/api/maintenance/tasks/{taskId}/checklist/{detailId}/complete')
Future<void> completeChecklistItem(
  @Path('taskId') int taskId,
  @Path('detailId') int detailId,
  @Body() CompleteChecklistItemRequest request,
);

// Get task progress
@GET('/api/maintenance/tasks/{taskId}/progress')
Future<TaskProgress> getTaskProgress(@Path('taskId') int taskId);
```

### 4. Repository Layer Updates

#### **task_repository.dart**
Added 3 new methods with **offline-first** support:

**getTaskChecklist(taskId)**
- Fetches checklist items with execution status
- Caches per task: `task_checklist_{taskId}`
- Returns empty list for old tasks without TaskType
- Offline support with cache fallback

**completeChecklistItem()**
- Marks checklist item as completed
- Sends measurement/checklist/inspection data
- Invalidates checklist and progress cache
- Queues for offline sync if no connection

**getTaskProgress(taskId)**
- Calculates task completion percentage
- Returns mandatory vs total completion status
- Caches per task: `task_progress_{taskId}`
- Offline support with cache fallback

### 5. State Management Updates

#### **task_provider.dart**
Added new state variables:
- `_currentChecklist: List<TaskChecklistItem>` - Current task's checklist
- `_currentProgress: TaskProgress?` - Current task's progress

Added new methods:
- `fetchTaskChecklist(taskId)` - Load checklist for a task
- `completeChecklistItem(...)` - Complete an item and refresh
- `fetchTaskProgress(taskId)` - Load progress data
- `canCompleteTask()` - Check if all mandatory items done
- `clearCurrentChecklist()` - Clear state when leaving task detail

### 6. Sync Queue Support

#### **sync_item.dart**
- Added `SyncItemType.checklistComplete` enum value
- Supports offline completion of checklist items
- Will sync when connection restored

## 🏗️ Architecture

### Data Flow

```
UI (Screen/Widget)
    ↓
TaskProvider (State Management)
    ↓
TaskRepository (Business Logic + Offline)
    ↓
TaskApi (Retrofit HTTP Client)
    ↓
Backend API
```

### Offline Support

1. **Cache Strategy**
   - Checklist: `task_checklist_{taskId}` (1 hour expiry)
   - Progress: `task_progress_{taskId}` (1 hour expiry)
   - Tasks: `my_tasks` (1 hour expiry)

2. **Sync Queue**
   - Checklist completions queued offline
   - Auto-sync when connection restored
   - Retry mechanism with retry count

## 🎯 Kanban Workflow

### Status Flow

```
PENDING → IN_PROGRESS → COMPLETED
     ↓
  OVERDUE (if past due date)
```

### Task Lifecycle

1. **Captain creates task** (frontend-edge)
   - Status: PENDING
   - Assigns to crew member
   - Optional: Selects TaskType (with checklist)

2. **Crew member accepts task** (mobile)
   - Call `startTask(taskId)`
   - Status: PENDING → IN_PROGRESS
   - Task appears in "In Progress" column on Kanban board

3. **Crew member works on checklist** (mobile)
   - View checklist items (measurements, checklists, inspections)
   - Complete items one by one
   - System tracks progress percentage
   - Mandatory items must be completed

4. **Crew member completes task** (mobile)
   - Only allowed if all mandatory checklist items done
   - Call `completeTask(taskId)`
   - Status: IN_PROGRESS → COMPLETED
   - Task appears in "Completed" column on Kanban board

## 📱 Next Steps: UI Implementation

### Phase 3 Tasks (Pending)

#### 1. Update TaskDetailScreen

Add checklist section:

```dart
// Check if task has TaskType checklist
if (task.hasTaskType) {
  // Show checklist widget
  TaskChecklistWidget(
    taskId: task.id,
    checklist: provider.currentChecklist,
    progress: provider.currentProgress,
    onItemTap: (item) => _showChecklistItemDialog(item),
  );
}

// Progress indicator
if (provider.currentProgress != null) {
  LinearProgressIndicator(
    value: provider.currentProgress!.completionPercentage / 100,
  );
  Text('${provider.currentProgress!.completedMandatoryItems}/'
       '${provider.currentProgress!.mandatoryItems} mandatory items completed');
}

// Complete button - only enabled if canComplete
ElevatedButton(
  onPressed: provider.canCompleteTask() 
    ? () => _completeTask()
    : null,
  child: Text('Complete Task'),
);
```

#### 2. Create TaskChecklistWidget

Display list of checklist items with status:

```dart
class TaskChecklistWidget extends StatelessWidget {
  final int taskId;
  final List<TaskChecklistItem> checklist;
  final TaskProgress? progress;
  final Function(TaskChecklistItem) onItemTap;
  
  Widget build(BuildContext context) {
    return Column(
      children: [
        // Progress bar
        if (progress != null)
          LinearProgressIndicator(
            value: progress.completionPercentage / 100,
          ),
        
        // Checklist items
        ListView.builder(
          itemCount: checklist.length,
          itemBuilder: (context, index) {
            final item = checklist[index];
            return ChecklistItemTile(
              item: item,
              onTap: () => onItemTap(item),
            );
          },
        ),
      ],
    );
  }
}
```

#### 3. Create ChecklistItemDialog

Dialog for completing individual items:

```dart
// For MEASUREMENT type
TextField(
  decoration: InputDecoration(
    labelText: 'Measured Value',
    suffixText: item.taskDetail.unit,
    hintText: 'Min: ${item.taskDetail.minValue}, Max: ${item.taskDetail.maxValue}',
  ),
  keyboardType: TextInputType.number,
);

// For CHECKLIST type
Row(
  children: [
    Text('Pass'),
    Radio(value: true, ...),
    Text('Fail'),
    Radio(value: false, ...),
  ],
);

// For INSPECTION type
TextField(
  decoration: InputDecoration(labelText: 'Inspection Notes'),
  maxLines: 3,
);

// Photo upload if required
if (item.requiresPhoto) {
  ImagePickerButton(
    onImagePicked: (file) => _uploadPhoto(file),
  );
}

// Save button
ElevatedButton(
  onPressed: () => _saveChecklistItem(),
  child: Text('Save'),
);
```

#### 4. Add "Accept Task" Button

In TaskDetailScreen for PENDING tasks:

```dart
if (task.isPending) {
  ElevatedButton(
    onPressed: () => _acceptTask(),
    child: Text('Accept Task'),
  );
}

void _acceptTask() async {
  try {
    await provider.startTask(task.id);
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text('Task accepted and moved to In Progress')),
    );
    
    // Load checklist if task has TaskType
    if (task.hasTaskType) {
      await provider.fetchTaskChecklist(task.id);
    }
  } catch (e) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text('Error: $e')),
    );
  }
}
```

#### 5. Implement Photo Upload

```dart
class PhotoUploadService {
  Future<String> uploadPhoto(File imageFile, int taskId, int detailId) async {
    // 1. Compress image
    final compressed = await compressImage(imageFile);
    
    // 2. Upload to server
    final formData = FormData.fromMap({
      'file': await MultipartFile.fromFile(compressed.path),
      'taskId': taskId,
      'detailId': detailId,
    });
    
    final response = await dio.post('/api/maintenance/upload-photo', data: formData);
    
    // 3. Return photo URL
    return response.data['photoUrl'];
  }
}
```

#### 6. Business Logic Validation

```dart
// Validate measurement values
bool validateMeasurement(TaskDetail detail, String value) {
  final numValue = double.tryParse(value);
  if (numValue == null) return false;
  
  if (detail.minValue != null && numValue < detail.minValue!) return false;
  if (detail.maxValue != null && numValue > detail.maxValue!) return false;
  
  return true;
}

// Check if mandatory photo provided
bool validatePhoto(TaskDetail detail, String? photoUrl) {
  if (detail.requiresPhoto && (photoUrl == null || photoUrl.isEmpty)) {
    return false;
  }
  return true;
}

// Validate before completing task
Future<bool> validateTaskCompletion(int taskId) async {
  final progress = await provider.fetchTaskProgress(taskId);
  
  if (!progress.canComplete) {
    showDialog(
      context: context,
      builder: (_) => AlertDialog(
        title: Text('Cannot Complete Task'),
        content: Text(
          'Please complete all ${progress.mandatoryItems} mandatory checklist items first.\n'
          'Progress: ${progress.completedMandatoryItems}/${progress.mandatoryItems}'
        ),
      ),
    );
    return false;
  }
  
  return true;
}
```

## 🔧 Testing Guide

### 1. Test Backward Compatibility

```dart
// Old tasks without TaskType should still work
final oldTask = MaintenanceTask(
  id: 1,
  taskTypeId: null, // No TaskType
  taskTypeName: null,
  // ... other fields
);

print(oldTask.hasTaskType); // Should be false

// Should return empty checklist
final checklist = await repository.getTaskChecklist(oldTask.id);
print(checklist.length); // Should be 0
```

### 2. Test Checklist Flow

```dart
// 1. Get task with TaskType
final task = await repository.getTaskById(21);
print('TaskType: ${task.taskTypeName}'); // e.g., "Engine Oil Change"

// 2. Get checklist
final checklist = await repository.getTaskChecklist(task.id);
print('Checklist items: ${checklist.length}');

// 3. Complete first item
await repository.completeChecklistItem(
  taskId: task.id,
  detailId: checklist[0].taskDetail.id,
  measuredValue: checklist[0].taskDetail.isMeasurement ? '50.5' : null,
  checkResult: checklist[0].taskDetail.isChecklist ? true : null,
  inspectionNotes: checklist[0].taskDetail.isInspection ? 'All good' : null,
  isCompleted: true,
);

// 4. Check progress
final progress = await repository.getTaskProgress(task.id);
print('Progress: ${progress.completionPercentage}%');
print('Can complete: ${progress.canComplete}');
```

### 3. Test Offline Support

```dart
// 1. Turn off WiFi
// 2. Complete a checklist item
try {
  await repository.completeChecklistItem(...);
} catch (e) {
  print(e); // Should say "saved offline, will sync when online"
}

// 3. Check sync queue
final queue = await syncQueue.getPendingItems();
print('Pending syncs: ${queue.length}');

// 4. Turn on WiFi
// 5. Trigger sync
await syncQueue.processPendingItems();

// 6. Verify item completed on server
final checklist = await repository.getTaskChecklist(taskId);
// Should show item as completed
```

## 📊 Database Schema Reference

### task_types
- id, type_code, type_name, description, category, priority, estimated_duration_minutes

### task_details
- id, task_type_id, detail_name, detail_type (MEASUREMENT/CHECKLIST/INSPECTION)
- is_mandatory, order_index, unit, min_value, max_value, acceptable_criteria
- requires_photo

### maintenance_tasks
- Added: task_type_id, task_type_name

### maintenance_task_details
- id, maintenance_task_id, task_detail_id
- measured_value, check_result, inspection_notes, photo_url
- is_completed, completed_by, completed_by_crew_id, completed_at

## 🎨 UI/UX Recommendations

1. **Color Coding**
   - 🔴 Mandatory items: Red indicator
   - ⚪ Optional items: Gray indicator
   - ✅ Completed items: Green checkmark
   - 📸 Photo required: Camera icon

2. **Progress Visualization**
   - Linear progress bar showing percentage
   - Text: "X/Y mandatory items completed"
   - Disable "Complete Task" button until all mandatory done

3. **Checklist Item Types**
   - 📏 MEASUREMENT: Show unit, min/max range, numeric input
   - ☑️ CHECKLIST: Show Pass/Fail radio buttons
   - 🔍 INSPECTION: Show multi-line text field for notes

4. **Photo Upload**
   - Show thumbnail of uploaded photo
   - Allow retake if needed
   - Compress before upload to save bandwidth

5. **Offline Indicators**
   - Show "Offline" badge if no connection
   - Show sync pending icon for queued items
   - Auto-refresh when connection restored

## 🔐 Security Notes

- All API calls require authentication token
- CrewId automatically extracted from token
- User can only see/complete their assigned tasks
- Photo uploads should be restricted to task assignee

## 📝 Summary

Phase 2 đã hoàn tất:
- ✅ 6 new models created
- ✅ 1 model updated (MaintenanceTask)
- ✅ 3 new API endpoints added
- ✅ Repository methods with offline support
- ✅ TaskProvider state management
- ✅ Sync queue for offline operations
- ✅ Retrofit code generated
- ✅ Build successful

**Ready for Phase 3: UI Implementation** 🚀
