class CompleteTaskChecklistItemRequest {
  final String completedBy;
  final double? readingValue;
  final String? remarks;
  final bool isAbnormal;

  const CompleteTaskChecklistItemRequest({
    required this.completedBy,
    this.readingValue,
    this.remarks,
    this.isAbnormal = false,
  });

  Map<String, dynamic> toJson() => {
        'completedBy': completedBy,
        if (readingValue != null) 'readingValue': readingValue,
        if (remarks != null) 'remarks': remarks,
        'isAbnormal': isAbnormal,
      };
}
