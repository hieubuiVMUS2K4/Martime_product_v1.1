class UpdateTaskChecklistItemRequest {
  final bool? isCompleted;
  final double? readingValue;
  final String? remarks;
  final bool? isAbnormal;
  final String? completedBy;

  const UpdateTaskChecklistItemRequest({
    this.isCompleted,
    this.readingValue,
    this.remarks,
    this.isAbnormal,
    this.completedBy,
  });

  Map<String, dynamic> toJson() => {
        if (isCompleted != null) 'isCompleted': isCompleted,
        if (readingValue != null) 'readingValue': readingValue,
        if (remarks != null) 'remarks': remarks,
        if (isAbnormal != null) 'isAbnormal': isAbnormal,
        if (completedBy != null) 'completedBy': completedBy,
      };
}
