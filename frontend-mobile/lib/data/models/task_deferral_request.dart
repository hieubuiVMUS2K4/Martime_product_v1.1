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
  final String status; // PENDING, APPROVED, REJECTED
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
      id: json['id']?.toString() ?? '',
      taskId: json['taskId']?.toString() ?? '',
      requestedBy: json['requestedBy']?.toString() ?? '',
      requestedAt: json['requestedAt']?.toString() ?? '',
      reason: json['reason']?.toString() ?? '',
      currentDueDate: json['currentDueDate']?.toString() ?? '',
      proposedDueDate: json['proposedDueDate']?.toString() ?? '',
      deferralDays: json['deferralDays'] ?? 0,
      status: json['status']?.toString() ?? '',
      priority: json['priority']?.toString() ?? 'NORMAL',
      isCmsItem: json['isCmsItem'] ?? false,
      classPermissionLetter: json['classPermissionLetter']?.toString(),
      rootCause: json['rootCause']?.toString(),
      preventiveMeasures: json['preventiveMeasures']?.toString(),
      attachments: json['attachments'] != null
          ? List<String>.from(json['attachments'])
          : null,
      reviewedBy: json['reviewedBy']?.toString(),
      reviewedAt: json['reviewedAt']?.toString(),
      reviewNotes: json['reviewNotes']?.toString(),
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
        id,
        taskId,
        status,
        requestedAt,
        proposedDueDate,
        reviewNotes,
      ];
}
