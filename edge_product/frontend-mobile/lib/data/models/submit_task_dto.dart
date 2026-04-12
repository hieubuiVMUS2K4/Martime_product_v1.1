class SubmitTaskDto {
  /// IMPORTANT: Backend currently requires taskId in body even though it's also in the route.
  /// Type is UUID string.
  final String taskId;

  final String? notes;
  final String? sparePartsUsed;
  final List<String>? photoUrls;
  final double? completedRunningHours;
  /// Actual duration in minutes (optional — backend auto-calculates from startedAt if not provided)
  final int? actualDurationMinutes;

  const SubmitTaskDto({
    required this.taskId,
    this.notes,
    this.sparePartsUsed,
    this.photoUrls,
    this.completedRunningHours,
    this.actualDurationMinutes,
  });

  Map<String, dynamic> toJson() {
    final map = <String, dynamic>{
      'taskId': taskId,
    };
    if (notes != null) map['notes'] = notes;
    if (sparePartsUsed != null) map['sparePartsUsed'] = sparePartsUsed;
    if (photoUrls != null) map['photoUrls'] = photoUrls;
    if (completedRunningHours != null) map['completedRunningHours'] = completedRunningHours;
    if (actualDurationMinutes != null) map['actualDurationMinutes'] = actualDurationMinutes;
    return map;
  }
}
