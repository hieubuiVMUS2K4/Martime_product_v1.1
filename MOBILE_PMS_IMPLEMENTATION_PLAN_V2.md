# 📱 MOBILE PMS WORKFLOW - IMPLEMENTATION PLAN V2.0

> **Kế hoạch triển khai chi tiết cho Mobile App theo PMS Workflow Design chuẩn Maritime**
> 
> **Ngày tạo:** 14/12/2025  
> **Version:** 2.0 (Updated based on gap analysis)  
> **Trạng thái:** Ready for Implementation

---

## 🎯 TÓM TẮT EXECUTIVE

### Tình trạng hiện tại
- Mobile app hiện tại chỉ đáp ứng **~35%** yêu cầu PMS Workflow v2.0
- **VẤN ĐỀ NGHIÊM TRỌNG**: Mobile đang dùng `/complete` API → bỏ qua bước Approval của C/E
- Thiếu RECTIFY flow → Crew không biết task bị trả lại
- Thiếu Deferral system → Không thể xin hoãn task

### Mục tiêu
Triển khai đầy đủ PMS Workflow v2.0 trong **8 tuần** với 4 phases:
1. **Phase 1 (Week 1-2)**: Foundation - Model updates, API migration
2. **Phase 2 (Week 3-4)**: Core Workflow - Submit/RECTIFY flow
3. **Phase 3 (Week 5-6)**: Deferral System
4. **Phase 4 (Week 7-8)**: Advanced Features

---

## 📊 GAP ANALYSIS SUMMARY

### So sánh Mobile vs Backend

| Tính năng | Backend | Mobile hiện tại | Gap |
|-----------|---------|-----------------|-----|
| **Task Statuses** | 8 statuses | 5 statuses | ❌ Thiếu RECTIFY, PENDING_APPROVAL, hasPendingDeferral |
| **Workflow Actions** | 6 actions | 2 actions | ❌ Thiếu Submit, Deferral, Rectify, Photos |
| **Deferral System** | Full system | Không có | ❌ 0% |
| **Photo Validation** | requiredPhotos, photosUploaded | Không có | ❌ 0% |
| **Push Notifications** | 7 types | Không có | ❌ 0% |
| **Models/DTOs** | Full fields | ~60% fields | ⚠️ Thiếu 12 fields |

### Breaking Changes

🔴 **QUAN TRỌNG**: API endpoint thay đổi

```dart
// ❌ OLD (Wrong workflow)
POST /api/maintenance/tasks/{id}/complete
→ IN_PROGRESS → COMPLETED (bỏ qua approval)

// ✅ NEW (Correct workflow)
POST /api/tasks/{id}/submit
→ IN_PROGRESS → PENDING_APPROVAL → (C/E approve) → COMPLETED
```

---

## 🗂️ PHASE 1: FOUNDATION (Week 1-2) 🔴 CRITICAL

### Mục tiêu
Xây dựng nền tảng cho workflow mới: Models, APIs, UI cơ bản

### 1.1. Model Updates

#### File: `lib/data/models/maintenance_task.dart`

**Bổ sung 15 fields:**

```dart
class MaintenanceTask extends Equatable {
  // ... existing fields ...
  
  // 🔴 NEW FIELDS - PMS Workflow v2.0
  
  // === REJECTION TRACKING ===
  final String? rejectionReason;      // Lý do bị reject
  final int rejectionCount;           // Số lần bị reject (default: 0)
  final String? lastRejectedAt;       // ISO timestamp
  final String? lastRejectedBy;       // Crew ID hoặc name
  
  // === DEFERRAL TRACKING ===
  final bool hasPendingDeferral;      // Có deferral đang chờ không
  final int deferralCount;            // Số lần xin hoãn (default: 0)
  final String? lastDeferredAt;
  final String? lastDeferredBy;
  
  // === PHOTO REQUIREMENTS ⚡ CRITICAL ===
  final int requiredPhotos;           // Số ảnh yêu cầu (default: 0)
  final int photosUploaded;           // Số ảnh đã upload (default: 0)
  
  // === CMS FLAG ===
  final bool isCms;                   // Class Survey item (default: false)
  
  // === SUBMISSION TRACKING ===
  final String? submittedAt;          // ISO timestamp
  final String? submittedBy;          // Crew ID
  
  // === VERIFICATION TRACKING ===
  final String? verifiedAt;           // ISO timestamp
  final String? verifiedBy;           // C/E Crew ID
  final String? verificationResult;   // APPROVED, REJECTED
  
  // === COMPUTED PROPERTIES ===
  bool get isRectify => status == 'RECTIFY';
  bool get isPendingApproval => status == 'PENDING_APPROVAL';
  bool get canRequestDeferral => status == 'DUE' || status == 'OVERDUE';
  bool get canFixAndContinue => status == 'RECTIFY';
  bool get hasEnoughPhotos => photosUploaded >= requiredPhotos;
  bool get canSubmit => hasEnoughPhotos;  // + other validations
  
  const MaintenanceTask({
    // ... existing params ...
    this.rejectionReason,
    this.rejectionCount = 0,
    this.lastRejectedAt,
    this.lastRejectedBy,
    this.hasPendingDeferral = false,
    this.deferralCount = 0,
    this.lastDeferredAt,
    this.lastDeferredBy,
    this.requiredPhotos = 0,
    this.photosUploaded = 0,
    this.isCms = false,
    this.submittedAt,
    this.submittedBy,
    this.verifiedAt,
    this.verifiedBy,
    this.verificationResult,
  });
  
  factory MaintenanceTask.fromJson(Map<String, dynamic> json) {
    return MaintenanceTask(
      // ... existing mappings ...
      rejectionReason: json['rejectionReason'],
      rejectionCount: json['rejectionCount'] ?? 0,
      lastRejectedAt: json['lastRejectedAt'],
      lastRejectedBy: json['lastRejectedBy'],
      hasPendingDeferral: json['hasPendingDeferral'] ?? false,
      deferralCount: json['deferralCount'] ?? 0,
      lastDeferredAt: json['lastDeferredAt'],
      lastDeferredBy: json['lastDeferredBy'],
      requiredPhotos: json['requiredPhotos'] ?? 0,
      photosUploaded: json['photosUploaded'] ?? 0,
      isCms: json['isCms'] ?? false,
      submittedAt: json['submittedAt'],
      submittedBy: json['submittedBy'],
      verifiedAt: json['verifiedAt'],
      verifiedBy: json['verifiedBy'],
      verificationResult: json['verificationResult'],
    );
  }
  
  @override
  List<Object?> get props => [
    // ... existing props ...
    rejectionReason,
    rejectionCount,
    lastRejectedAt,
    lastRejectedBy,
    hasPendingDeferral,
    deferralCount,
    lastDeferredAt,
    lastDeferredBy,
    requiredPhotos,
    photosUploaded,
    isCms,
    submittedAt,
    submittedBy,
    verifiedAt,
    verifiedBy,
    verificationResult,
  ];
}
```

#### File: `lib/data/models/task_deferral_request.dart` (NEW)

```dart
import 'package:equatable/equatable.dart';

class TaskDeferralRequest extends Equatable {
  final String id;
  final String taskId;
  final String requestedBy;
  final String requestedAt;
  final String reason;
  final String currentDueDate;
  final String proposedDueDate;
  final int deferralDays;
  final String status;  // PENDING, APPROVED, REJECTED
  final String priority;
  final bool isCmsItem;
  final String? classPermissionLetter;
  
  // For OVERDUE deferrals
  final String? rootCause;
  final String? preventiveMeasures;
  final List<String>? attachments;
  
  // Review info
  final String? reviewedBy;
  final String? reviewedAt;
  final String? reviewNotes;
  
  const TaskDeferralRequest({
    required this.id,
    required this.taskId,
    required this.requestedBy,
    required this.requestedAt,
    required this.reason,
    required this.currentDueDate,
    required this.proposedDueDate,
    required this.deferralDays,
    required this.status,
    required this.priority,
    required this.isCmsItem,
    this.classPermissionLetter,
    this.rootCause,
    this.preventiveMeasures,
    this.attachments,
    this.reviewedBy,
    this.reviewedAt,
    this.reviewNotes,
  });
  
  factory TaskDeferralRequest.fromJson(Map<String, dynamic> json) {
    return TaskDeferralRequest(
      id: json['id'],
      taskId: json['taskId'],
      requestedBy: json['requestedBy'],
      requestedAt: json['requestedAt'],
      reason: json['reason'],
      currentDueDate: json['currentDueDate'],
      proposedDueDate: json['proposedDueDate'],
      deferralDays: json['deferralDays'],
      status: json['status'],
      priority: json['priority'],
      isCmsItem: json['isCmsItem'] ?? false,
      classPermissionLetter: json['classPermissionLetter'],
      rootCause: json['rootCause'],
      preventiveMeasures: json['preventiveMeasures'],
      attachments: json['attachments'] != null 
          ? List<String>.from(json['attachments']) 
          : null,
      reviewedBy: json['reviewedBy'],
      reviewedAt: json['reviewedAt'],
      reviewNotes: json['reviewNotes'],
    );
  }
  
  Map<String, dynamic> toJson() => {
    'id': id,
    'taskId': taskId,
    'requestedBy': requestedBy,
    'requestedAt': requestedAt,
    'reason': reason,
    'currentDueDate': currentDueDate,
    'proposedDueDate': proposedDueDate,
    'deferralDays': deferralDays,
    'status': status,
    'priority': priority,
    'isCmsItem': isCmsItem,
    if (classPermissionLetter != null) 'classPermissionLetter': classPermissionLetter,
    if (rootCause != null) 'rootCause': rootCause,
    if (preventiveMeasures != null) 'preventiveMeasures': preventiveMeasures,
    if (attachments != null) 'attachments': attachments,
    if (reviewedBy != null) 'reviewedBy': reviewedBy,
    if (reviewedAt != null) 'reviewedAt': reviewedAt,
    if (reviewNotes != null) 'reviewNotes': reviewNotes,
  };
  
  @override
  List<Object?> get props => [
    id, taskId, requestedBy, requestedAt, reason,
    currentDueDate, proposedDueDate, deferralDays, status, priority,
    isCmsItem, classPermissionLetter, rootCause, preventiveMeasures,
    attachments, reviewedBy, reviewedAt, reviewNotes,
  ];
}
```

#### File: `lib/data/models/submit_task_dto.dart` (NEW - replaces TaskCompleteRequest)

```dart
class SubmitTaskDto {
  /// IMPORTANT: Backend currently requires taskId in body even though it's also in the route.
  /// Type is UUID string.
  final String taskId;

  final String? notes;
  final String? sparePartsUsed;
  final List<String>? photoUrls;
  final double? completedRunningHours;
  
  const SubmitTaskDto({
    required this.taskId,
    this.notes,
    this.sparePartsUsed,
    this.photoUrls,
    this.completedRunningHours,
  });
  
  Map<String, dynamic> toJson() {
    final map = <String, dynamic>{
      'taskId': taskId,
    };
    if (notes != null) map['notes'] = notes;
    if (sparePartsUsed != null) map['sparePartsUsed'] = sparePartsUsed;
    if (photoUrls != null) map['photoUrls'] = photoUrls;
    if (completedRunningHours != null) map['completedRunningHours'] = completedRunningHours;
    return map;
  }
}
```

#### File: `lib/data/models/start_task_dto.dart` (NEW)

```dart
class StartTaskDto {
  /// Backend requires taskId in body (UUID string)
  final String taskId;
  final double? currentRunningHours;
  final String? notes;

  const StartTaskDto({
    required this.taskId,
    this.currentRunningHours,
    this.notes,
  });

  Map<String, dynamic> toJson() {
    final map = <String, dynamic>{
      'taskId': taskId,
    };
    if (currentRunningHours != null) map['currentRunningHours'] = currentRunningHours;
    if (notes != null) map['notes'] = notes;
    return map;
  }
}
```

#### File: `lib/data/models/create_deferral_request_dto.dart` (NEW)

```dart
class CreateDeferralRequestDto {
  final String taskId; // UUID
  final String reason;
  final String proposedDueDate; // ISO date-time string
  final String priority; // LOW, NORMAL, HIGH

  // OVERDUE-only (backend validation is strict)
  final String? rootCause;
  final String? preventiveMeasures;
  final List<String>? attachments; // URLs

  // CMS rule: required if deferralDays > 90
  final String? classPermissionLetter; // URL

  const CreateDeferralRequestDto({
    required this.taskId,
    required this.reason,
    required this.proposedDueDate,
    this.priority = 'NORMAL',
    this.rootCause,
    this.preventiveMeasures,
    this.attachments,
    this.classPermissionLetter,
  });

  Map<String, dynamic> toJson() => {
        'taskId': taskId,
        'reason': reason,
        'proposedDueDate': proposedDueDate,
        'priority': priority,
        if (rootCause != null) 'rootCause': rootCause,
        if (preventiveMeasures != null) 'preventiveMeasures': preventiveMeasures,
        if (attachments != null) 'attachments': attachments,
        if (classPermissionLetter != null) 'classPermissionLetter': classPermissionLetter,
      };
}
```

---

### 1.2. API Contract (Edge Backend - MUST MATCH)

> Backend `edge-services` uses **camelCase JSON** and **UUID (Guid) IDs**.
> Mobile MUST send headers:
>
> - `X-User-Id`: crewId (string)
> - `X-Device-Type`: `MOBILE`

**Task (Workflow v2.0)**

- `POST /api/tasks/{id}/start` (id = UUID)
  - Body: `StartTaskDto` (taskId required)
  - Allowed status: `DUE`, `OVERDUE`, `RECTIFY`
  - Reject if `hasPendingDeferral=true`

- `POST /api/tasks/{id}/submit` (id = UUID)
  - Body: `SubmitTaskDto` (taskId required)
  - Status: `IN_PROGRESS → PENDING_APPROVAL`
  - Reject if checklist not completed OR photosUploaded < requiredPhotos

- `GET /api/tasks/{id}/details` (id = UUID)
  - Returns `MaintenanceTaskDetailDto` including `statusHistory` + `pendingDeferral`

**Task lists (Crew)**

- `GET /api/maintenance/tasks/my-tasks?crewId={crewId}&includeCompleted={true|false}`
  - NOTE: if no `crewId/assignedTo` backend returns empty list

**Deferrals**

- `POST /api/deferral-requests` (CreateDeferralRequestDto)
  - Allowed task status: `SCHEDULED`, `DUE`, `OVERDUE`
  - OVERDUE rules: reason >= 50 chars + attachments required + rootCause>=20 + preventiveMeasures>=20
  - CMS rule: if deferralDays > 90 then classPermissionLetter is required

- `GET /api/deferral-requests?status=PENDING&taskId={uuid}`
- `DELETE /api/deferral-requests/{id}` (cancel pending)

---

### 1.3. API Implementation (Mobile)

#### File: `lib/data/data_sources/remote/task_api.dart`

```dart
import 'package:dio/dio.dart';
import 'package:retrofit/retrofit.dart';
import '../../models/maintenance_task.dart';
import '../../models/start_task_dto.dart';
import '../../models/submit_task_dto.dart';
import '../../models/task_deferral_request.dart';
import '../../models/create_deferral_request_dto.dart';

part 'task_api.g.dart';

@RestApi()
abstract class TaskApi {
  factory TaskApi(Dio dio, {String baseUrl}) = _TaskApi;

  // === CREW TASK LIST (existing backend) ===
  @GET('/api/maintenance/tasks/my-tasks')
  Future<List<MaintenanceTask>> getMyTasks(
    @Query('crewId') String crewId,
    @Query('includeCompleted') bool includeCompleted,
  );

  // === TASK DETAILS ===
  /// Legacy raw entity endpoint (may not include names/statusHistory)
  @GET('/api/maintenance/tasks/{id}')
  Future<MaintenanceTask> getTaskById(@Path('id') String id); // UUID

  /// Recommended: full workflow info + statusHistory + pendingDeferral
  @GET('/api/tasks/{id}/details')
  Future<Map<String, dynamic>> getTaskDetails(@Path('id') String id); // UUID

  // === WORKFLOW v2.0 (MUST USE) ===
  /// Start task: DUE/OVERDUE/RECTIFY → IN_PROGRESS
  @POST('/api/tasks/{id}/start')
  Future<Map<String, dynamic>> startTask(
    @Path('id') String id,
    @Body() StartTaskDto dto,
  );

  /// Submit task for approval: IN_PROGRESS → PENDING_APPROVAL
  @POST('/api/tasks/{id}/submit')
  Future<Map<String, dynamic>> submitTask(
    @Path('id') String id,
    @Body() SubmitTaskDto dto,
  );

  // === DEFERRALS ===
  @POST('/api/deferral-requests')
  Future<Map<String, dynamic>> createDeferralRequest(
    @Body() CreateDeferralRequestDto dto,
  );

  @GET('/api/deferral-requests')
  Future<Map<String, dynamic>> getDeferralRequests(
    @Query('status') String? status,
    @Query('taskId') String? taskId,
    @Query('page') int? page,
    @Query('pageSize') int? pageSize,
  );

  @DELETE('/api/deferral-requests/{id}')
  Future<Map<String, dynamic>> cancelDeferralRequest(
    @Path('id') String id,
  );
}
```

#### File: `lib/data/repositories/task_repository.dart`

**Update `completeTask()` to use `/submit`:**

```dart
/// Submit task for approval (NEW - PMS Workflow v2.0)
/// Replaces old completeTask() which bypassed approval
Future<void> submitTask({
  required String taskId, // UUID string
  String? notes,
  String? sparePartsUsed,
  List<String>? photoUrls,
  double? runningHours,
}) async {
  final dto = SubmitTaskDto(
    taskId: taskId,
    notes: notes,
    sparePartsUsed: sparePartsUsed,
    photoUrls: photoUrls,
    completedRunningHours: runningHours,
  );

  try {
    if (await _networkInfo.isConnected) {
      // Online: Send immediately
      await _taskApi.submitTask(taskId, dto);

      // Update cache
      await _updateTaskInCache(task);
      
      print('✅ Task $taskId submitted for approval');
    } else {
      // Offline: Add to sync queue
      await _syncQueue.addToQueue(
        SyncItem(
          type: SyncItemType.taskSubmit,
          data: {
            'taskId': taskId,
            ...dto.toJson(),
          },
        ),
      );
      
      print('💾 Task $taskId queued for offline sync');
    }
  } on DioException catch (e) {
    print('❌ Failed to submit task: ${e.message}');
    
    // On error, add to sync queue
    await _syncQueue.addToQueue(
      SyncItem(
        type: SyncItemType.taskSubmit,
        data: {
          'taskId': taskId,
          ...dto.toJson(),
        },
      ),
    );
    
    throw Exception('Task saved offline. Will sync when online');
  }
}
```

---

### 1.4. Task List Screen Updates

#### File: `lib/presentation/screens/tasks/task_list_screen.dart`

**Add new status badges:**

```dart
// Add to status badge builder
Widget _buildStatusBadge(MaintenanceTask task) {
  Color bgColor;
  Color textColor;
  String label;
  IconData icon;

  switch (task.status) {
    // ... existing cases ...
    
    case 'PENDING_APPROVAL':
      bgColor = Colors.amber.shade50;
      textColor = Colors.amber.shade700;
      label = 'Chờ nghiệm thu';
      icon = Icons.pending_actions;
      break;
      
    case 'RECTIFY':
      bgColor = Colors.orange.shade50;
      textColor = Colors.orange.shade700;
      label = 'Cần khắc phục';
      icon = Icons.build_circle;
      break;
      
    default:
      // ...
  }

  return Container(
    padding: EdgeInsets.symmetric(horizontal: 8, vertical: 4),
    decoration: BoxDecoration(
      color: bgColor,
      borderRadius: BorderRadius.circular(4),
    ),
    child: Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(icon, size: 12, color: textColor),
        SizedBox(width: 4),
        Text(label, style: TextStyle(color: textColor, fontSize: 11)),
      ],
    ),
  );
}

// Add indicators
Widget _buildTaskIndicators(MaintenanceTask task) {
  return Row(
    children: [
      // Pending deferral indicator
      if (task.hasPendingDeferral)
        Padding(
          padding: EdgeInsets.only(right: 4),
          child: Icon(Icons.schedule, size: 16, color: Colors.amber),
        ),
      
      // High rejection count warning
      if (task.rejectionCount >= 3)
        Padding(
          padding: EdgeInsets.only(right: 4),
          child: Badge(
            label: Text('${task.rejectionCount}x'),
            backgroundColor: Colors.red,
            child: Icon(Icons.warning, size: 16, color: Colors.red),
          ),
        ),
      
      // CMS badge
      if (task.isCms)
        Padding(
          padding: EdgeInsets.only(right: 4),
          child: Icon(Icons.verified, size: 16, color: Colors.blue),
        ),
    ],
  );
}
```

**Add filter tabs:**

```dart
// Add to TabBar
TabBar(
  tabs: [
    Tab(text: 'Đến hạn'),
    Tab(text: 'Quá hạn'),
    Tab(text: 'Đang làm'),
    Tab(text: 'Cần sửa'),  // NEW - RECTIFY
    Tab(text: 'Chờ duyệt'),  // NEW - PENDING_APPROVAL
    Tab(text: 'Hoàn thành'),
  ],
)

// Filter logic
List<MaintenanceTask> _filterTasks(String tabName) {
  switch (tabName) {
    case 'Cần sửa':
      return tasks.where((t) => t.status == 'RECTIFY').toList();
    case 'Chờ duyệt':
      return tasks.where((t) => t.status == 'PENDING_APPROVAL').toList();
    // ... other cases
  }
}
```

---

### 1.5. Task Detail Screen Updates

#### File: `lib/presentation/screens/tasks/task_detail_screen.dart`

**Add rejection info section:**

```dart
// Add after equipment info
if (widget.task.isRectify) ...[
  SizedBox(height: 16),
  _buildRejectionInfoCard(),
],

// Rejection info card
Widget _buildRejectionInfoCard() {
  return Card(
    color: Colors.orange.shade50,
    elevation: 2,
    shape: RoundedRectangleBorder(
      borderRadius: BorderRadius.circular(12),
      side: BorderSide(color: Colors.orange.shade200, width: 2),
    ),
    child: Padding(
      padding: EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(Icons.warning_amber_rounded, color: Colors.orange.shade700, size: 24),
              SizedBox(width: 8),
              Text(
                'TASK BỊ TRẢ LẠI',
                style: TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.bold,
                  color: Colors.orange.shade900,
                ),
              ),
            ],
          ),
          SizedBox(height: 12),
          Container(
            padding: EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(8),
              border: Border.all(color: Colors.orange.shade300),
            ),
            child: Text(
              widget.task.rejectionReason ?? 'No reason provided',
              style: TextStyle(fontSize: 14, height: 1.5),
            ),
          ),
          SizedBox(height: 12),
          _buildRejectionDetails(),
        ],
      ),
    ),
  );
}

Widget _buildRejectionDetails() {
  return Column(
    children: [
      _buildInfoRow('Từ chối bởi', widget.task.lastRejectedBy ?? 'Unknown'),
      _buildInfoRow('Thời gian', _formatDateTime(widget.task.lastRejectedAt)),
      _buildInfoRow('Số lần từ chối', '${widget.task.rejectionCount} lần'),
      
      if (widget.task.rejectionCount >= 3)
        Container(
          margin: EdgeInsets.only(top: 12),
          padding: EdgeInsets.all(12),
          decoration: BoxDecoration(
            color: Colors.red.shade50,
            borderRadius: BorderRadius.circular(8),
            border: Border.all(color: Colors.red.shade300),
          ),
          child: Row(
            children: [
              Icon(Icons.error_outline, color: Colors.red.shade700),
              SizedBox(width: 8),
              Expanded(
                child: Text(
                  'Nhiều lần bị từ chối. Vui lòng liên hệ cấp trên.',
                  style: TextStyle(color: Colors.red.shade900, fontSize: 13),
                ),
              ),
            ],
          ),
        ),
    ],
  );
}
```

**Add photo requirements indicator:**

```dart
// In checklist section
if (widget.task.requiredPhotos > 0)
  Container(
    padding: EdgeInsets.all(12),
    margin: EdgeInsets.only(bottom: 12),
    decoration: BoxDecoration(
      color: widget.task.hasEnoughPhotos 
          ? Colors.green.shade50 
          : Colors.orange.shade50,
      borderRadius: BorderRadius.circular(8),
      border: Border.all(
        color: widget.task.hasEnoughPhotos 
            ? Colors.green.shade300 
            : Colors.orange.shade300,
      ),
    ),
    child: Row(
      children: [
        Icon(
          widget.task.hasEnoughPhotos ? Icons.check_circle : Icons.photo_camera,
          color: widget.task.hasEnoughPhotos 
              ? Colors.green.shade700 
              : Colors.orange.shade700,
        ),
        SizedBox(width: 8),
        Text(
          '${widget.task.photosUploaded}/${widget.task.requiredPhotos} ảnh đã chụp',
          style: TextStyle(
            fontSize: 14,
            fontWeight: FontWeight.w600,
            color: widget.task.hasEnoughPhotos 
                ? Colors.green.shade900 
                : Colors.orange.shade900,
          ),
        ),
      ],
    ),
  ),
```

**Update action buttons:**

```dart
Widget _buildActionButtons() {
  if (widget.task.isRectify) {
    return ElevatedButton.icon(
      onPressed: () => _handleFixAndContinue(),
      icon: Icon(Icons.build_circle),
      label: Text('Sửa và tiếp tục'),
      style: ElevatedButton.styleFrom(
        backgroundColor: Colors.orange.shade600,
        minimumSize: Size(double.infinity, 48),
      ),
    );
  }
  
  if (widget.task.isPendingApproval) {
    return Container(
      padding: EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.amber.shade50,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: Colors.amber.shade300),
      ),
      child: Column(
        children: [
          Icon(Icons.pending_actions, size: 48, color: Colors.amber.shade700),
          SizedBox(height: 8),
          Text(
            'Chờ C/E nghiệm thu',
            style: TextStyle(
              fontSize: 16,
              fontWeight: FontWeight.bold,
              color: Colors.amber.shade900,
            ),
          ),
          SizedBox(height: 4),
          Text(
            'Báo cáo của bạn đang được xem xét',
            style: TextStyle(color: Colors.amber.shade700),
          ),
        ],
      ),
    );
  }
  
  // ... existing buttons for other statuses
}

void _handleFixAndContinue() {
  // Navigate to task execution screen
  Navigator.push(
    context,
    MaterialPageRoute(
      builder: (_) => TaskExecutionScreen(task: widget.task),
    ),
  );
}
```

---

### ✅ Phase 1 Deliverables

- [ ] MaintenanceTask model updated với 15 fields mới
- [ ] TaskDeferralRequest model created
- [ ] SubmitTaskDto model created (bao gồm `taskId` bắt buộc trong body)
- [ ] StartTaskDto model created (taskId bắt buộc trong body)
- [ ] CreateDeferralRequestDto model created
- [ ] task_api.dart migrated sang workflow v2.0: `/api/tasks/{id}/start`, `/api/tasks/{id}/submit`, `/api/tasks/{id}/details`
- [ ] task_repository.dart migrated: dùng UUID `id` thay vì int, stop dùng `/api/maintenance/tasks/{id}/complete`
- [ ] Task list hiển thị RECTIFY và PENDING_APPROVAL statuses
- [ ] Task detail hiển thị rejection info và photo requirements
- [ ] Action buttons theo status (Fix & Continue, Waiting for approval)

---

## 🔧 PHASE 2: CORE WORKFLOW (Week 3-4) 🔴 CRITICAL

### Mục tiêu
Triển khai đúng “Crew execution flow” theo backend Workflow v2.0:

`DUE/OVERDUE/RECTIFY → (Start) IN_PROGRESS → (Submit) PENDING_APPROVAL → (C/E Verify) COMPLETED / RECTIFY`

### 2.1. QUAN TRỌNG: Đúng kiểu ID (UUID) và đúng "taskId" (Task Code)

Backend đang dùng song song 2 key:

- `id` (UUID / Guid): dùng cho workflow endpoints `/api/tasks/{id}/...`.
- `taskId` (string code ví dụ `SCHED-...` hoặc `MT-...`): dùng cho checklist endpoints `/api/maintenance/tasks/{taskId}/checklist`.

✅ Mobile model phải có **cả hai**:

- `id` (String) → gọi start/submit/details.
- `taskId` (String) → gọi checklist.

### 2.2. Task Checklist Execution (bắt buộc để Submit pass validation)

Backend validation tại `POST /api/tasks/{id}/submit`:

- Nếu task có checklist items: **bắt buộc completed 100%**.
- Nếu `requiredPhotos > 0`: **bắt buộc photosUploaded >= requiredPhotos**.

#### Checklist API (Edge)

```http
GET  /api/maintenance/tasks/{taskId}/checklist
PUT  /api/maintenance/tasks/{taskId}/checklist/{itemId}
POST /api/maintenance/tasks/{taskId}/checklist/{itemId}/complete
GET  /api/maintenance/tasks/{taskId}/checklist/summary
```

#### Gợi ý triển khai mobile

- Khi mở Task Detail/Execution → gọi `GET checklist` theo `task.taskId`.
- Cho phép crew cập nhật từng item:
  - `isCompleted`
  - `readingValue` (nếu requiresReading)
  - `remarks`
  - `isAbnormal`
  - `completedBy` = crewId

**NOTE:** backend không ép validation readingValue trong controller hiện tại, nhưng mobile nên UI-guided để đủ dữ liệu.

### 2.3. Start Task đúng workflow (thay endpoint legacy)

#### Không dùng (legacy - thiếu deferral check/audit)

- `POST /api/maintenance/tasks/{id}/start` (legacy)

#### Dùng (workflow v2.0)

- `POST /api/tasks/{id}/start`
  - Headers: `X-User-Id`, `X-Device-Type=MOBILE`
  - Body: `StartTaskDto(taskId: id, currentRunningHours?, notes?)`
  - Allowed status: `DUE`, `OVERDUE`, `RECTIFY`
  - Block nếu `hasPendingDeferral=true`

#### UI Rules

- Hiển thị nút **Start** khi:
  - status ∈ {`DUE`,`OVERDUE`,`RECTIFY`}
  - `hasPendingDeferral == false`

Nếu `hasPendingDeferral==true`:

- Disable Start
- Hiển thị banner “Đang chờ duyệt hoãn (Deferral pending)”.

### 2.4. Submit Task đúng workflow (Crew hoàn thành → chờ nghiệm thu)

#### Dùng

- `POST /api/tasks/{id}/submit`
  - Headers: `X-User-Id`, `X-Device-Type=MOBILE`
  - Body: `SubmitTaskDto(taskId: id, notes?, sparePartsUsed?, photoUrls?, completedRunningHours?)`

#### UI Rules (trước khi gọi API)

- Bắt buộc checklist hoàn tất 100% nếu có checklist.
- Nếu `requiredPhotos>0`:
  - UI phải enforce chụp/upload đủ ảnh.
  - `photoUrls.length` dùng để backend set `photosUploaded`.

#### After Submit

- Chuyển status local thành `PENDING_APPROVAL`.
- Task Detail hiển thị panel “Chờ C/E nghiệm thu”.
- Disable mọi action liên quan execution.

### 2.5. Rectify Flow (khi C/E reject)

Backend khi reject:

- `PENDING_APPROVAL → RECTIFY`
- Fields set:
  - `rejectionReason`
  - `rejectionCount += 1`
  - `lastRejectedAt`, `lastRejectedBy`

#### Mobile behavior

- Task List phải làm nổi bật `RECTIFY`.
- Task Detail phải hiển thị “Task bị trả lại” + reason + rejectionCount.
- Nút hành động: **“Sửa và tiếp tục”** → dẫn tới Execution screen.

#### Technical

- Crew resume bằng cách gọi lại `POST /api/tasks/{id}/start` (status RECTIFY được phép start).
- Crew update checklist/ảnh → submit lại như bình thường.

### 2.6. Task Detail chuẩn (khuyến nghị dùng endpoint details)

Để có đủ dữ liệu workflow + status history:

- `GET /api/tasks/{id}/details`

Hiển thị:

- Status history (20 events gần nhất): fromStatus/toStatus, changedByName, changedAt, reason, notes, deviceType
- Pending deferral (nếu có): status, reason, proposedDueDate

### ✅ Phase 2 Deliverables

- [ ] Task execution flow: checklist CRUD/update theo checklist APIs
- [ ] Start task dùng `/api/tasks/{id}/start` + headers
- [ ] Submit task dùng `/api/tasks/{id}/submit` + validate checklist/photos
- [ ] Rectify UX: show rejection reason + allow restart + resubmit
- [ ] Task Detail dùng `/api/tasks/{id}/details` hiển thị statusHistory

---

## 🕒 PHASE 3: DEFERRAL SYSTEM (Week 5-6)

### Mục tiêu
Cho phép crew tạo/huỷ request xin hoãn task đúng quy tắc backend.

### 3.1. Khi nào được Request Deferral

Backend cho phép tạo deferral khi task status ∈ {`SCHEDULED`, `DUE`, `OVERDUE`}.

Mobile rule khuyến nghị:

- Cho phép request khi status ∈ {`DUE`,`OVERDUE`,`SCHEDULED`}.
- Ẩn/disable nếu status ∈ {`IN_PROGRESS`,`PENDING_APPROVAL`,`RECTIFY`,`COMPLETED`,`CANCELLED`}.

### 3.2. Create Deferral Request

Endpoint:

- `POST /api/deferral-requests`
  - Headers: `X-User-Id`, `X-Device-Type=MOBILE`
  - Body: `CreateDeferralRequestDto`

#### Validation mapping (MUST MATCH backend)

**Common**

- `reason` min 20 chars (backend attribute) và thực tế:
  - OVERDUE: backend require reason >= 50 chars
- `proposedDueDate` phải > current `nextDueAt`

**OVERDUE deferral (status == OVERDUE)**

- `reason` >= 50 chars
- `attachments` bắt buộc có ít nhất 1 URL
- `rootCause` >= 20 chars
- `preventiveMeasures` >= 20 chars

**CMS rule**

- Nếu `task.isCms == true` và `(proposedDueDate - nextDueAt) > 90 days`:
  - `classPermissionLetter` bắt buộc (URL)

#### After create

- Backend sẽ set: `task.hasPendingDeferral = true`
- Mobile phải:
  - refresh task detail
  - disable Start (backend cũng sẽ block)
  - hiển thị pending indicator

### 3.3. View / Cancel pending deferral

Endpoint:

- `GET /api/deferral-requests?status=PENDING&taskId={uuid}`
  - Response dạng paging `{ items, totalCount, page, pageSize, totalPages }`

- `DELETE /api/deferral-requests/{id}`
  - Chỉ cancel được khi request còn `PENDING`

UI:

- Trong Task Detail, nếu `hasPendingDeferral==true`:
  - show card “Deferral đang chờ duyệt”
  - show reason + proposedDueDate + priority
  - nếu requester là current user → show nút “Huỷ yêu cầu”

### ✅ Phase 3 Deliverables

- [ ] UI tạo deferral (DUE/OVERDUE/SCHEDULED)
- [ ] Enforce validation OVERDUE + CMS rule trước khi submit
- [ ] Hiển thị pending deferral card + disable Start
- [ ] Cancel pending deferral

---

## 🚀 PHASE 4: ADVANCED FEATURES (Week 7-8)

### Mục tiêu
Hoàn thiện trải nghiệm vận hành: dashboard data, đồng bộ ổn định, hiển thị audit rõ ràng.

### 4.1. Dashboard endpoints (optional cho Mobile, có sẵn ở backend)

Nếu mobile có vai trò C/E hoặc cần overview:

- `GET /api/tasks/dashboard/summary`
  - pendingApprovalCount, pendingDeferralCount, rectifyTaskCount, overdueTaskCount...

- `GET /api/tasks/morning-briefing`
  - dueToday, pendingApproval, pendingDeferral, tasksInProgress...

### 4.2. Polling/Refresh strategy (thay push notifications)

Backend hiện chưa cung cấp push notification trong code.
Khuyến nghị mobile:

- Refresh Task List theo pull-to-refresh.
- Auto-refresh khi mở app hoặc resume app.
- Trên Task Detail: refresh sau Start/Submit/Deferral actions.

### 4.3. Offline considerations (nếu app có offline sync)

Nếu vẫn giữ sync queue:

- SyncItemType cần phân biệt:
  - `taskStart`
  - `taskSubmit`
  - `deferralCreate`
  - `deferralCancel`
  - `checklistItemUpdate`

**Cảnh báo:** các action workflow (start/submit/deferral) có validation server-side.
Khi offline → nên queue, nhưng khi online sync có thể fail; cần UI hiển thị “Sync failed” + retry.

### ✅ Phase 4 Deliverables

- [ ] Optional dashboard screens using available endpoints
- [ ] Stable refresh/polling strategy for status changes (PENDING_APPROVAL→COMPLETED/RECTIFY)
- [ ] Offline queue updated (nếu có offline mode)

---

## ✅ IMPLEMENTATION CHECKLIST (Go-Live Gate)

Trước khi release mobile:

- [ ] Không còn call `/api/maintenance/tasks/{id}/complete`
- [ ] Start luôn call `/api/tasks/{id}/start` (kèm headers + StartTaskDto)
- [ ] Submit luôn call `/api/tasks/{id}/submit` (kèm headers + SubmitTaskDto)
- [ ] Checklist update đúng endpoint `/api/maintenance/tasks/{taskId}/checklist/...` (taskId là string code)
- [ ] Deferral tạo/huỷ đúng rules (OVERDUE + CMS)

---

**Document Version:** 2.1  
**Last Updated:** 14/12/2025  
**Author:** PMS Development Team