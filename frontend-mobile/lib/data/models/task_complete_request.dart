class TaskCompleteRequest {
  final String completedBy;
  final String completedByCrewId;
  final String completedAt;
  final double? runningHoursAtCompletion;
  final String? sparePartsUsed;
  final String? notes;
  final List<String>? photoUrls;

  TaskCompleteRequest({
    required this.completedBy,
    required this.completedByCrewId,
    required this.completedAt,
    this.runningHoursAtCompletion,
    this.sparePartsUsed,
    this.notes,
    this.photoUrls,
  });

  Map<String, dynamic> toJson() {
    return {
      'completedBy': completedBy,
      'completedByCrewId': completedByCrewId,
      'completedAt': completedAt,
      'runningHoursAtCompletion': runningHoursAtCompletion,
      'sparePartsUsed': sparePartsUsed,
      'notes': notes,
      'photoUrls': photoUrls,
    };
  }
}
