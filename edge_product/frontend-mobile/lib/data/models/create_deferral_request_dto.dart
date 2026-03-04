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
