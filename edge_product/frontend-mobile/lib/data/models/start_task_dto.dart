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
