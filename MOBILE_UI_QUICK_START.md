# Quick Start - Implementing Checklist UI

## 🎯 Mục tiêu

Tạo UI để hiển thị và hoàn thành checklist items cho maintenance tasks.

## 📦 Phase 2 đã hoàn tất

- ✅ Models: TaskDetail, MaintenanceTaskDetail, TaskChecklistItem, TaskProgress
- ✅ API client với 3 endpoints mới
- ✅ Repository methods với offline support
- ✅ TaskProvider với checklist state management

## 🚀 Phase 3 - UI Implementation

### Bước 1: Cập nhật TaskDetailScreen

File: `lib/presentation/screens/tasks/task_detail_screen.dart`

#### 1.1 Load checklist khi vào screen

```dart
@override
void initState() {
  super.initState();
  _loadTaskData();
}

Future<void> _loadTaskData() async {
  final provider = Provider.of<TaskProvider>(context, listen: false);
  
  // Load checklist if task has TaskType
  if (widget.task.hasTaskType) {
    try {
      await provider.fetchTaskChecklist(widget.task.id);
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Error loading checklist: $e')),
      );
    }
  }
}
```

#### 1.2 Thêm Accept Task button (cho PENDING tasks)

```dart
// Trong build method, sau phần thông tin task
if (task.isPending) {
  Padding(
    padding: const EdgeInsets.all(16.0),
    child: ElevatedButton.icon(
      onPressed: _isLoading ? null : _acceptTask,
      icon: Icon(Icons.play_arrow),
      label: Text('Accept Task'),
      style: ElevatedButton.styleFrom(
        minimumSize: Size(double.infinity, 50),
        backgroundColor: Colors.blue,
      ),
    ),
  );
}

Future<void> _acceptTask() async {
  setState(() => _isLoading = true);
  
  try {
    final provider = Provider.of<TaskProvider>(context, listen: false);
    await provider.startTask(task.id);
    
    setState(() => _isLoading = false);
    
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text('Task accepted! Status changed to In Progress'),
        backgroundColor: Colors.green,
      ),
    );
    
    // Reload checklist
    if (task.hasTaskType) {
      await provider.fetchTaskChecklist(task.id);
    }
  } catch (e) {
    setState(() => _isLoading = false);
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text('Error: $e'),
        backgroundColor: Colors.red,
      ),
    );
  }
}
```

#### 1.3 Hiển thị checklist (cho IN_PROGRESS tasks)

```dart
// Trong build method
if (task.isInProgress && task.hasTaskType) {
  Consumer<TaskProvider>(
    builder: (context, provider, child) {
      if (provider.currentChecklist.isEmpty) {
        return Center(
          child: Padding(
            padding: const EdgeInsets.all(32.0),
            child: Text('No checklist items'),
          ),
        );
      }
      
      return Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Progress section
          _buildProgressSection(provider.currentProgress),
          
          SizedBox(height: 16),
          
          // Checklist section
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16.0),
            child: Text(
              'Task Checklist',
              style: Theme.of(context).textTheme.titleLarge,
            ),
          ),
          
          SizedBox(height: 8),
          
          // Checklist items
          ListView.builder(
            shrinkWrap: true,
            physics: NeverScrollableScrollPhysics(),
            itemCount: provider.currentChecklist.length,
            itemBuilder: (context, index) {
              final item = provider.currentChecklist[index];
              return _buildChecklistItem(item);
            },
          ),
        ],
      );
    },
  );
}
```

#### 1.4 Progress widget

```dart
Widget _buildProgressSection(TaskProgress? progress) {
  if (progress == null) return SizedBox.shrink();
  
  return Card(
    margin: EdgeInsets.all(16),
    child: Padding(
      padding: EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                'Progress',
                style: TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.bold,
                ),
              ),
              Text(
                '${progress.completionPercentage.toStringAsFixed(0)}%',
                style: TextStyle(
                  fontSize: 18,
                  fontWeight: FontWeight.bold,
                  color: progress.canComplete ? Colors.green : Colors.orange,
                ),
              ),
            ],
          ),
          
          SizedBox(height: 8),
          
          LinearProgressIndicator(
            value: progress.completionPercentage / 100,
            backgroundColor: Colors.grey[300],
            valueColor: AlwaysStoppedAnimation<Color>(
              progress.canComplete ? Colors.green : Colors.orange,
            ),
          ),
          
          SizedBox(height: 8),
          
          Text(
            '${progress.completedMandatoryItems}/${progress.mandatoryItems} mandatory items completed',
            style: TextStyle(
              fontSize: 12,
              color: Colors.grey[600],
            ),
          ),
          
          if (!progress.canComplete)
            Padding(
              padding: const EdgeInsets.only(top: 8.0),
              child: Row(
                children: [
                  Icon(Icons.info_outline, size: 16, color: Colors.orange),
                  SizedBox(width: 4),
                  Expanded(
                    child: Text(
                      'Complete all mandatory items to finish this task',
                      style: TextStyle(
                        fontSize: 12,
                        color: Colors.orange,
                      ),
                    ),
                  ),
                ],
              ),
            ),
        ],
      ),
    ),
  );
}
```

#### 1.5 Checklist item widget

```dart
Widget _buildChecklistItem(TaskChecklistItem item) {
  final detail = item.taskDetail;
  final isCompleted = item.isCompleted;
  
  return Card(
    margin: EdgeInsets.symmetric(horizontal: 16, vertical: 4),
    child: ListTile(
      leading: _buildChecklistIcon(detail, isCompleted),
      title: Row(
        children: [
          Expanded(
            child: Text(
              detail.detailName,
              style: TextStyle(
                fontWeight: FontWeight.w500,
                decoration: isCompleted ? TextDecoration.lineThrough : null,
              ),
            ),
          ),
          if (detail.isMandatory)
            Chip(
              label: Text('Required', style: TextStyle(fontSize: 10)),
              backgroundColor: Colors.red[100],
              padding: EdgeInsets.zero,
              materialTapTargetSize: MaterialTapTargetSize.shrinkWrap,
            ),
        ],
      ),
      subtitle: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          if (detail.description != null)
            Text(detail.description!),
          
          SizedBox(height: 4),
          
          _buildChecklistItemDetails(item),
        ],
      ),
      trailing: isCompleted 
        ? Icon(Icons.check_circle, color: Colors.green)
        : Icon(Icons.radio_button_unchecked, color: Colors.grey),
      onTap: isCompleted ? null : () => _showChecklistDialog(item),
    ),
  );
}

Widget _buildChecklistIcon(TaskDetail detail, bool isCompleted) {
  IconData icon;
  Color color = isCompleted ? Colors.green : Colors.blue;
  
  if (detail.isMeasurement) {
    icon = Icons.straighten;
  } else if (detail.isChecklist) {
    icon = Icons.check_box;
  } else {
    icon = Icons.search;
  }
  
  return CircleAvatar(
    backgroundColor: color.withOpacity(0.2),
    child: Icon(icon, color: color),
  );
}

Widget _buildChecklistItemDetails(TaskChecklistItem item) {
  final detail = item.taskDetail;
  final execution = item.executionDetail;
  
  if (execution == null || !item.isCompleted) {
    // Show what needs to be done
    List<String> requirements = [];
    
    if (detail.isMeasurement) {
      requirements.add('Measure ${detail.unit ?? 'value'}');
      if (detail.minValue != null || detail.maxValue != null) {
        requirements.add('Range: ${detail.minValue ?? '-'} to ${detail.maxValue ?? '-'}');
      }
    }
    
    if (detail.requiresPhoto) {
      requirements.add('📸 Photo required');
    }
    
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: requirements.map((r) => Text(
        r,
        style: TextStyle(fontSize: 12, color: Colors.grey[600]),
      )).toList(),
    );
  } else {
    // Show completed data
    List<Widget> completedInfo = [];
    
    if (execution.measuredValue != null) {
      completedInfo.add(Text(
        'Measured: ${execution.measuredValue} ${detail.unit ?? ''}',
        style: TextStyle(fontSize: 12, color: Colors.green[700]),
      ));
    }
    
    if (execution.checkResult != null) {
      completedInfo.add(Text(
        execution.checkResult! ? '✓ Pass' : '✗ Fail',
        style: TextStyle(
          fontSize: 12,
          color: execution.checkResult! ? Colors.green[700] : Colors.red[700],
        ),
      ));
    }
    
    if (execution.inspectionNotes != null) {
      completedInfo.add(Text(
        'Notes: ${execution.inspectionNotes}',
        style: TextStyle(fontSize: 12, color: Colors.grey[700]),
        maxLines: 2,
        overflow: TextOverflow.ellipsis,
      ));
    }
    
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: completedInfo,
    );
  }
}
```

### Bước 2: Tạo Checklist Dialog

File: `lib/presentation/widgets/checklist_item_dialog.dart`

```dart
import 'package:flutter/material.dart';
import '../../data/models/task_checklist_item.dart';
import '../../data/models/task_detail.dart';

class ChecklistItemDialog extends StatefulWidget {
  final TaskChecklistItem item;
  final Function(String?, bool?, String?, String?) onComplete;
  
  const ChecklistItemDialog({
    Key? key,
    required this.item,
    required this.onComplete,
  }) : super(key: key);
  
  @override
  State<ChecklistItemDialog> createState() => _ChecklistItemDialogState();
}

class _ChecklistItemDialogState extends State<ChecklistItemDialog> {
  final _formKey = GlobalKey<FormState>();
  
  // Form fields
  final _measurementController = TextEditingController();
  bool? _checkResult;
  final _notesController = TextEditingController();
  String? _photoUrl;
  
  @override
  void dispose() {
    _measurementController.dispose();
    _notesController.dispose();
    super.dispose();
  }
  
  @override
  Widget build(BuildContext context) {
    final detail = widget.item.taskDetail;
    
    return AlertDialog(
      title: Text(detail.detailName),
      content: SingleChildScrollView(
        child: Form(
          key: _formKey,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Description
              if (detail.description != null)
                Padding(
                  padding: const EdgeInsets.only(bottom: 16.0),
                  child: Text(
                    detail.description!,
                    style: TextStyle(color: Colors.grey[600]),
                  ),
                ),
              
              // Measurement input
              if (detail.isMeasurement) _buildMeasurementInput(detail),
              
              // Checklist input
              if (detail.isChecklist) _buildChecklistInput(),
              
              // Inspection input
              if (detail.isInspection) _buildInspectionInput(),
              
              SizedBox(height: 16),
              
              // Photo upload
              if (detail.requiresPhoto) _buildPhotoUpload(),
            ],
          ),
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.pop(context),
          child: Text('Cancel'),
        ),
        ElevatedButton(
          onPressed: _handleSave,
          child: Text('Save'),
        ),
      ],
    );
  }
  
  Widget _buildMeasurementInput(TaskDetail detail) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        TextFormField(
          controller: _measurementController,
          keyboardType: TextInputType.numberWithOptions(decimal: true),
          decoration: InputDecoration(
            labelText: 'Measured Value',
            suffixText: detail.unit,
            hintText: detail.minValue != null && detail.maxValue != null
                ? '${detail.minValue} - ${detail.maxValue}'
                : null,
            border: OutlineInputBorder(),
          ),
          validator: (value) {
            if (value == null || value.isEmpty) {
              return 'Please enter a value';
            }
            
            final numValue = double.tryParse(value);
            if (numValue == null) {
              return 'Please enter a valid number';
            }
            
            if (detail.minValue != null && numValue < detail.minValue!) {
              return 'Value must be at least ${detail.minValue}';
            }
            
            if (detail.maxValue != null && numValue > detail.maxValue!) {
              return 'Value must be at most ${detail.maxValue}';
            }
            
            return null;
          },
        ),
        
        if (detail.acceptableCriteria != null)
          Padding(
            padding: const EdgeInsets.only(top: 8.0),
            child: Text(
              'Acceptable: ${detail.acceptableCriteria}',
              style: TextStyle(fontSize: 12, color: Colors.blue[700]),
            ),
          ),
      ],
    );
  }
  
  Widget _buildChecklistInput() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text('Result:', style: TextStyle(fontWeight: FontWeight.w500)),
        SizedBox(height: 8),
        Row(
          children: [
            Expanded(
              child: RadioListTile<bool>(
                title: Text('Pass'),
                value: true,
                groupValue: _checkResult,
                onChanged: (value) => setState(() => _checkResult = value),
                contentPadding: EdgeInsets.zero,
              ),
            ),
            Expanded(
              child: RadioListTile<bool>(
                title: Text('Fail'),
                value: false,
                groupValue: _checkResult,
                onChanged: (value) => setState(() => _checkResult = value),
                contentPadding: EdgeInsets.zero,
              ),
            ),
          ],
        ),
        
        if (_checkResult == false)
          Padding(
            padding: const EdgeInsets.only(top: 8.0),
            child: TextFormField(
              controller: _notesController,
              decoration: InputDecoration(
                labelText: 'Failure Notes (Required)',
                border: OutlineInputBorder(),
              ),
              maxLines: 3,
              validator: (value) {
                if (_checkResult == false && (value == null || value.isEmpty)) {
                  return 'Please explain why it failed';
                }
                return null;
              },
            ),
          ),
      ],
    );
  }
  
  Widget _buildInspectionInput() {
    return TextFormField(
      controller: _notesController,
      decoration: InputDecoration(
        labelText: 'Inspection Notes',
        hintText: 'Describe what you observed...',
        border: OutlineInputBorder(),
      ),
      maxLines: 4,
      validator: (value) {
        if (value == null || value.isEmpty) {
          return 'Please enter inspection notes';
        }
        return null;
      },
    );
  }
  
  Widget _buildPhotoUpload() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            Icon(Icons.camera_alt, color: Colors.red),
            SizedBox(width: 8),
            Text(
              'Photo Required',
              style: TextStyle(
                fontWeight: FontWeight.w500,
                color: Colors.red,
              ),
            ),
          ],
        ),
        
        SizedBox(height: 8),
        
        if (_photoUrl == null)
          OutlinedButton.icon(
            onPressed: _pickPhoto,
            icon: Icon(Icons.add_a_photo),
            label: Text('Take Photo'),
          )
        else
          Column(
            children: [
              // TODO: Show thumbnail
              Text('Photo uploaded ✓', style: TextStyle(color: Colors.green)),
              TextButton(
                onPressed: _pickPhoto,
                child: Text('Retake'),
              ),
            ],
          ),
      ],
    );
  }
  
  Future<void> _pickPhoto() async {
    // TODO: Implement photo picker
    // Use image_picker package
    // Upload to server
    // Get photoUrl
    
    setState(() {
      _photoUrl = 'https://example.com/photo.jpg'; // Placeholder
    });
  }
  
  void _handleSave() {
    if (!_formKey.currentState!.validate()) {
      return;
    }
    
    final detail = widget.item.taskDetail;
    
    // Validate photo if required
    if (detail.requiresPhoto && _photoUrl == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Photo is required for this item'),
          backgroundColor: Colors.red,
        ),
      );
      return;
    }
    
    // Validate checklist result selected
    if (detail.isChecklist && _checkResult == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Please select Pass or Fail'),
          backgroundColor: Colors.red,
        ),
      );
      return;
    }
    
    // Return data
    widget.onComplete(
      detail.isMeasurement ? _measurementController.text : null,
      detail.isChecklist ? _checkResult : null,
      _notesController.text.isNotEmpty ? _notesController.text : null,
      _photoUrl,
    );
    
    Navigator.pop(context);
  }
}
```

### Bước 3: Sử dụng Dialog trong TaskDetailScreen

```dart
Future<void> _showChecklistDialog(TaskChecklistItem item) async {
  await showDialog(
    context: context,
    builder: (context) => ChecklistItemDialog(
      item: item,
      onComplete: (measuredValue, checkResult, notes, photoUrl) async {
        await _completeChecklistItem(
          item: item,
          measuredValue: measuredValue,
          checkResult: checkResult,
          notes: notes,
          photoUrl: photoUrl,
        );
      },
    ),
  );
}

Future<void> _completeChecklistItem({
  required TaskChecklistItem item,
  String? measuredValue,
  bool? checkResult,
  String? notes,
  String? photoUrl,
}) async {
  try {
    final provider = Provider.of<TaskProvider>(context, listen: false);
    
    await provider.completeChecklistItem(
      taskId: widget.task.id,
      detailId: item.taskDetail.id,
      measuredValue: measuredValue,
      checkResult: checkResult,
      inspectionNotes: notes,
      photoUrl: photoUrl,
      isCompleted: true,
    );
    
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text('Checklist item completed!'),
        backgroundColor: Colors.green,
      ),
    );
  } catch (e) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text('Error: $e'),
        backgroundColor: Colors.red,
      ),
    );
  }
}
```

### Bước 4: Cập nhật Complete Task Button

```dart
// Trong TaskDetailScreen build method
if (task.isInProgress) {
  Consumer<TaskProvider>(
    builder: (context, provider, child) {
      final canComplete = provider.canCompleteTask();
      
      return Padding(
        padding: const EdgeInsets.all(16.0),
        child: ElevatedButton.icon(
          onPressed: canComplete ? _handleCompleteTask : null,
          icon: Icon(Icons.check_circle),
          label: Text('Complete Task'),
          style: ElevatedButton.styleFrom(
            minimumSize: Size(double.infinity, 50),
            backgroundColor: canComplete ? Colors.green : Colors.grey,
          ),
        ),
      );
    },
  );
}

Future<void> _handleCompleteTask() async {
  // Show confirmation
  final confirmed = await showDialog<bool>(
    context: context,
    builder: (context) => AlertDialog(
      title: Text('Complete Task'),
      content: Text('Are you sure you want to mark this task as completed?'),
      actions: [
        TextButton(
          onPressed: () => Navigator.pop(context, false),
          child: Text('Cancel'),
        ),
        ElevatedButton(
          onPressed: () => Navigator.pop(context, true),
          child: Text('Complete'),
        ),
      ],
    ),
  );
  
  if (confirmed != true) return;
  
  // Complete task
  try {
    final provider = Provider.of<TaskProvider>(context, listen: false);
    final auth = Provider.of<AuthProvider>(context, listen: false);
    
    await provider.completeTask(
      taskId: widget.task.id,
      completedBy: auth.currentUser?.fullName ?? 'Unknown',
      completedByCrewId: auth.currentUser?.crewId ?? '',
    );
    
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text('Task completed successfully!'),
        backgroundColor: Colors.green,
      ),
    );
    
    // Navigate back
    Navigator.pop(context);
  } catch (e) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text('Error completing task: $e'),
        backgroundColor: Colors.red,
      ),
    );
  }
}
```

## 📸 Photo Upload Implementation

### Thêm dependencies vào pubspec.yaml

```yaml
dependencies:
  image_picker: ^1.0.4
  image: ^4.1.3  # For compression
```

### Tạo PhotoUploadService

File: `lib/core/services/photo_upload_service.dart`

```dart
import 'dart:io';
import 'package:image_picker/image_picker.dart';
import 'package:image/image.dart' as img;
import 'package:dio/dio.dart';
import 'package:path_provider/path_provider.dart';

class PhotoUploadService {
  final Dio _dio;
  final ImagePicker _picker = ImagePicker();
  
  PhotoUploadService(this._dio);
  
  Future<String?> pickAndUploadPhoto({
    required int taskId,
    required int detailId,
  }) async {
    try {
      // Pick image
      final XFile? image = await _picker.pickImage(
        source: ImageSource.camera,
        maxWidth: 1920,
        maxHeight: 1080,
        imageQuality: 85,
      );
      
      if (image == null) return null;
      
      // Compress image
      final compressed = await _compressImage(File(image.path));
      
      // Upload to server
      final formData = FormData.fromMap({
        'file': await MultipartFile.fromFile(
          compressed.path,
          filename: 'task_${taskId}_detail_${detailId}_${DateTime.now().millisecondsSinceEpoch}.jpg',
        ),
        'taskId': taskId,
        'detailId': detailId,
      });
      
      final response = await _dio.post(
        '/api/maintenance/upload-photo',
        data: formData,
      );
      
      return response.data['photoUrl'];
    } catch (e) {
      print('Error uploading photo: $e');
      rethrow;
    }
  }
  
  Future<File> _compressImage(File file) async {
    // Read image
    final bytes = await file.readAsBytes();
    final image = img.decodeImage(bytes);
    
    if (image == null) return file;
    
    // Resize if too large
    final resized = img.copyResize(
      image,
      width: image.width > 1920 ? 1920 : image.width,
      height: image.height > 1080 ? 1080 : image.height,
    );
    
    // Compress to JPEG
    final compressed = img.encodeJpg(resized, quality: 85);
    
    // Save to temp file
    final tempDir = await getTemporaryDirectory();
    final tempFile = File('${tempDir.path}/compressed_${DateTime.now().millisecondsSinceEpoch}.jpg');
    await tempFile.writeAsBytes(compressed);
    
    return tempFile;
  }
}
```

## ✅ Testing Checklist

- [ ] Accept task (PENDING → IN_PROGRESS)
- [ ] Load checklist items
- [ ] Complete measurement item with value
- [ ] Complete checklist item (Pass/Fail)
- [ ] Complete inspection item with notes
- [ ] Upload photo for item requiring photo
- [ ] Progress bar updates after each item
- [ ] Cannot complete task until all mandatory items done
- [ ] Complete task (IN_PROGRESS → COMPLETED)
- [ ] Offline mode - checklist items queued
- [ ] Online mode - sync queued items

## 🎨 UI Preview

```
┌─────────────────────────────────┐
│ Task Detail                  [X]│
├─────────────────────────────────┤
│ Engine Oil Change               │
│ Equipment: Main Engine          │
│ Status: IN PROGRESS             │
│                                 │
│ ┌─────────────────────────────┐ │
│ │ Progress          75%       │ │
│ │ ████████████░░░░            │ │
│ │ 3/4 mandatory items done    │ │
│ └─────────────────────────────┘ │
│                                 │
│ Task Checklist                  │
│                                 │
│ ┌─ [✓] Oil Level Measurement ─┐│
│ │   Required                   ││
│ │   Measured: 4.5 L            ││
│ └─────────────────────────────┘│
│                                 │
│ ┌─ [✓] Oil Filter Check ───────┐│
│ │   Required                   ││
│ │   ✓ Pass                     ││
│ └─────────────────────────────┘│
│                                 │
│ ┌─ [✓] Oil Color Inspection ──┐│
│ │   Required                   ││
│ │   Notes: Clean, amber color  ││
│ └─────────────────────────────┘│
│                                 │
│ ┌─ [ ] Clean Work Area ────────┐│
│ │   Optional                   ││
│ │   Take photo when done       ││
│ └─────────────────────────────┘│
│                                 │
│ [    Complete Task    ]         │
└─────────────────────────────────┘
```

## 🚀 Next Steps

1. Implement TaskDetailScreen updates
2. Create ChecklistItemDialog
3. Add PhotoUploadService
4. Test complete workflow
5. Handle edge cases (offline, errors, etc.)

Good luck! 🎉
