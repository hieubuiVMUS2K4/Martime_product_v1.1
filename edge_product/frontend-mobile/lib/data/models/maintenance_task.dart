import 'package:equatable/equatable.dart';
import 'package:flutter/material.dart';
import 'task_checklist_item.dart';

class MaintenanceTask extends Equatable {
  /// Workflow key (edge-services uses Guid/UUID)
  final String id;

  /// Human/task code key used by checklist endpoints (e.g. SCHED-..., MT-...)
  final String taskId;
  final String? equipmentId;
  final String? equipmentName;
  final String? equipmentGroupId;
  final String? equipmentGroupName;
  final String taskType;
  final String taskDescription;
  final double? intervalHours;
  final int? intervalDays;
  final String? lastDoneAt;
  final String? nextDueAt;
  final double? runningHoursAtLastDone;
  final String priority;
  final String status;
  final String? assignedTo;
  final String? assignedToCrewId;
  final String? assignedDepartment;
  final String? completedAt;
  final String? completedBy;
  final String? completedByCrewId;
  final String? notes;
  final String? requiredSpareParts; // From schedule config - what's needed
  final String? sparePartsUsed; // Filled by crew - what was actually used
  final double? runningHoursAtCompletion;
  final String? photoUrls;
  final bool isSynced;
  final String createdAt;
  final String? updatedAt;
  
  // New fields for TaskType integration
  final int? taskTypeId;
  final String? taskTypeName;

  // Checklist items from API
  final List<TaskChecklistItem> checklistItems;

  // === PMS WORKFLOW v2.0 (Phase 1 fields) ===

  // Rejection tracking
  final String? rejectionReason;
  final int rejectionCount;
  final String? lastRejectedAt;
  final String? lastRejectedBy;

  // Deferral tracking
  final bool hasPendingDeferral;
  final int deferralCount;
  final String? lastDeferredAt;
  final String? lastDeferredBy;

  // Photo requirements
  final int requiredPhotos;
  final int photosUploaded;

  // CMS flag
  final bool isCms;

  // Submission tracking
  final String? submittedAt;
  final String? submittedBy;

  // Verification tracking
  final String? verifiedAt;
  final String? verifiedBy;
  final String? verificationResult;
  
  // Optimized: Checklist summary for list view (without loading full items)
  final int checklistItemsCount;
  final int checklistCompletedCount;
  
  const MaintenanceTask({
    required this.id,
    required this.taskId,
    this.equipmentId,
    this.equipmentName,
    this.equipmentGroupId,
    this.equipmentGroupName,
    required this.taskType,
    required this.taskDescription,
    this.intervalHours,
    this.intervalDays,
    this.lastDoneAt,
    this.nextDueAt,
    this.runningHoursAtLastDone,
    required this.priority,
    required this.status,
    this.assignedTo,
    this.assignedToCrewId,
    this.assignedDepartment,
    this.completedAt,
    this.completedBy,
    this.completedByCrewId,
    this.notes,
    this.requiredSpareParts,
    this.sparePartsUsed,
    this.runningHoursAtCompletion,
    this.photoUrls,
    required this.isSynced,
    required this.createdAt,
    this.updatedAt,
    this.taskTypeId,
    this.taskTypeName,
    this.checklistItems = const [],

    // PMS workflow
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
    this.checklistItemsCount = 0,
    this.checklistCompletedCount = 0,
  });
  
  factory MaintenanceTask.fromJson(Map<String, dynamic> json) {
    return MaintenanceTask(
      id: json['id']?.toString() ?? '',
      taskId: json['taskId']?.toString() ?? '',
      equipmentId: json['equipmentId']?.toString(),
      equipmentName: json['equipmentName']?.toString(),
      equipmentGroupId: json['equipmentGroupId']?.toString(),
      equipmentGroupName: json['equipmentGroupName']?.toString(),
      taskType: json['taskType']?.toString() ?? 'UNKNOWN',
      taskDescription: json['taskDescription']?.toString() ?? '',
      intervalHours: json['intervalHours']?.toDouble(),
      intervalDays: json['intervalDays'],
      lastDoneAt: json['lastDoneAt']?.toString(),
      nextDueAt: json['nextDueAt']?.toString(),
      runningHoursAtLastDone: json['runningHoursAtLastDone']?.toDouble(),
      priority: json['priority']?.toString() ?? 'MEDIUM',
      status: json['status']?.toString() ?? 'PENDING',
      assignedTo: json['assignedTo']?.toString(),
      assignedToCrewId: json['assignedToCrewId']?.toString(),
      assignedDepartment: json['assignedDepartment']?.toString(),
      completedAt: json['completedAt']?.toString(),
      completedBy: json['completedBy']?.toString(),
      completedByCrewId: json['completedByCrewId']?.toString(),
      notes: json['notes']?.toString(),
      requiredSpareParts: json['requiredSpareParts']?.toString(),
      sparePartsUsed: json['sparePartsUsed']?.toString(),
      runningHoursAtCompletion: json['runningHoursAtCompletion']?.toDouble(),
      photoUrls: json['photoUrls']?.toString(),
      isSynced: json['isSynced'] ?? false,
      createdAt: json['createdAt']?.toString() ?? DateTime.now().toIso8601String(),
      updatedAt: json['updatedAt']?.toString(),
      taskTypeId: json['taskTypeId'],
      taskTypeName: json['taskTypeName']?.toString(),
      
      // Parse checklistItems array
      checklistItems: json['checklistItems'] != null
          ? (json['checklistItems'] as List)
              .map((item) => TaskChecklistItem.fromJson(item as Map<String, dynamic>))
              .toList()
          : [],

      // PMS workflow
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
      // Optimized checklist summary from API
      checklistItemsCount: json['checklistItemsCount'] ?? (json['checklistItems'] as List?)?.length ?? 0,
      checklistCompletedCount: json['checklistCompletedCount'] ?? (json['checklistItems'] as List?)?.where((i) => i['isCompleted'] == true).length ?? 0,
    );
  }
  
  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'taskId': taskId,
      'equipmentId': equipmentId,
      'equipmentName': equipmentName,
      'equipmentGroupId': equipmentGroupId,
      'equipmentGroupName': equipmentGroupName,
      'taskType': taskType,
      'taskDescription': taskDescription,
      'intervalHours': intervalHours,
      'intervalDays': intervalDays,
      'lastDoneAt': lastDoneAt,
      'nextDueAt': nextDueAt,
      'runningHoursAtLastDone': runningHoursAtLastDone,
      'priority': priority,
      'status': status,
      'assignedTo': assignedTo,
      'assignedToCrewId': assignedToCrewId,
      'assignedDepartment': assignedDepartment,
      'completedAt': completedAt,
      'completedBy': completedBy,
      'completedByCrewId': completedByCrewId,
      'notes': notes,
      'requiredSpareParts': requiredSpareParts,
      'sparePartsUsed': sparePartsUsed,
      'runningHoursAtCompletion': runningHoursAtCompletion,
      'photoUrls': photoUrls,
      'isSynced': isSynced,
      'createdAt': createdAt,
      'updatedAt': updatedAt,
      'taskTypeId': taskTypeId,
      'taskTypeName': taskTypeName,
      'checklistItems': checklistItems.map((item) => item.toJson()).toList(),

      // PMS workflow
      'rejectionReason': rejectionReason,
      'rejectionCount': rejectionCount,
      'lastRejectedAt': lastRejectedAt,
      'lastRejectedBy': lastRejectedBy,
      'hasPendingDeferral': hasPendingDeferral,
      'deferralCount': deferralCount,
      'lastDeferredAt': lastDeferredAt,
      'lastDeferredBy': lastDeferredBy,
      'requiredPhotos': requiredPhotos,
      'photosUploaded': photosUploaded,
      'isCms': isCms,
      'submittedAt': submittedAt,
      'submittedBy': submittedBy,
      'verifiedAt': verifiedAt,
      'verifiedBy': verifiedBy,
      'verificationResult': verificationResult,
      'checklistItemsCount': checklistItemsCount,
      'checklistCompletedCount': checklistCompletedCount,
    };
  }
  
  // PERFORMANCE: Parse dates on demand - const class cannot have mutable cache fields
  // For hot paths, consider using a separate cache layer outside the model
  DateTime? get _dueDate {
    if (nextDueAt == null) return null;
    try {
      return DateTime.parse(nextDueAt!);
    } catch (e) {
      return null;
    }
  }
  
  // Date getters for UI - parse on access
  DateTime? get lastDoneAtDate {
    if (lastDoneAt == null) return null;
    try {
      return DateTime.parse(lastDoneAt!);
    } catch (e) {
      return null;
    }
  }
  
  DateTime? get completedAtDate {
    if (completedAt == null) return null;
    try {
      return DateTime.parse(completedAt!);
    } catch (e) {
      return null;
    }
  }
  
  DateTime? get nextDueAtDate => _dueDate;
  
  // PERFORMANCE: Pre-compute display string
  String get taskTypeDisplay => taskType.replaceAll('_', ' ');
  
  // Computed properties - handle nullable nextDueAt (OPTIMIZED)
  int get daysUntilDue {
    final due = _dueDate;
    if (due == null) return 0;
    return due.difference(DateTime.now()).inDays;
  }
  
  bool get isOverdue => daysUntilDue < 0;
  bool get isDueSoon => daysUntilDue >= 0 && daysUntilDue <= 7;
  // Workflow helpers
  bool get isRectify => status == 'RECTIFY';
  bool get isPendingApproval => status == 'PENDING_APPROVAL';
  bool get canRequestDeferral => status == 'DUE' || status == 'OVERDUE' || status == 'SCHEDULED' || hasMissingStatus;
  bool get canFixAndContinue => isRectify;
  bool get hasEnoughPhotos => photosUploaded >= requiredPhotos;

  bool get isScheduled => status == 'SCHEDULED';
  bool get isDue => status == 'DUE';
  bool get isOverdueStatus => status == 'OVERDUE';
  bool get isInProgress => status == 'IN_PROGRESS';
  bool get isCompleted => status == 'COMPLETED';
  bool get isCancelled => status == 'CANCELLED';
  
  // Missing status checks (tasks that need attention before they can proceed normally)
  bool get isMissingPic => status == 'MISSING_PIC';
  bool get isMissingChecklist => status == 'MISSING_CHECKLIST';
  bool get isMissingBoth => status == 'MISSING_BOTH';
  bool get hasMissingStatus => isMissingPic || isMissingChecklist || isMissingBoth;

  /// Start is allowed by backend for DUE/OVERDUE/RECTIFY/MISSING_*, but blocked if hasPendingDeferral
  /// Tasks with MISSING_* status can be started at any time (crew can self-assign)
  bool get canStart => (isDue || isOverdueStatus || isRectify || hasMissingStatus) && !hasPendingDeferral;
  
  /// Check if this task uses the new TaskType system
  bool get hasTaskType => taskTypeId != null;
  
  Color get priorityColor {
    switch (priority) {
      case 'CRITICAL':
        return Colors.red.shade700;
      case 'HIGH':
        return Colors.orange.shade700;
      case 'NORMAL':
        return Colors.blue.shade700;
      case 'LOW':
        return Colors.grey.shade600;
      default:
        return Colors.grey.shade600;
    }
  }
  
  Color get statusColor {
    if (isRectify) return Colors.orange.shade700;
    if (isPendingApproval) return Colors.amber.shade700;
    if (isOverdueStatus || (isOverdue && !isCompleted)) return Colors.red.shade700;
    if (isInProgress) return Colors.blue.shade700;
    if (isCompleted) return Colors.green.shade700;
    if (isDue || isScheduled) return Colors.grey.shade700;
    return Colors.grey.shade600;
  }
  
  String get statusText {
    return status;
  }

  /// Get display name - prefers equipment name, falls back to group name
  String get displayName {
    if (equipmentName != null && equipmentName!.isNotEmpty) {
      return equipmentName!;
    }
    if (equipmentGroupName != null && equipmentGroupName!.isNotEmpty) {
      return equipmentGroupName!;
    }
    return taskDescription;
  }

  /// Check if this task is for equipment group (not single equipment)
  bool get isGroupTask => equipmentGroupId != null && equipmentId == null;
  
  @override
  List<Object?> get props => [
    id, taskId, status, updatedAt,
    equipmentId, equipmentName,
    equipmentGroupId, equipmentGroupName,
    assignedTo, assignedDepartment,
    requiredSpareParts, sparePartsUsed,
    checklistItemsCount, checklistCompletedCount,
  ];
}
