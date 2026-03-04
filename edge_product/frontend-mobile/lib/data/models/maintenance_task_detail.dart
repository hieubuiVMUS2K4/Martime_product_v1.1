import 'package:equatable/equatable.dart';

/// Execution record for a checklist item
class MaintenanceTaskDetail extends Equatable {
  final int? id;
  final int maintenanceTaskId;
  final int taskDetailId;
  final String? measuredValue;
  final bool? checkResult;
  final String? notes; // Changed from inspectionNotes to match backend
  final String? photoUrl;
  final bool isCompleted;
  final String? completedBy;
  final String? completedByCrewId;
  final String? completedAt;
  final String? createdAt;
  final String? updatedAt;

  const MaintenanceTaskDetail({
    this.id,
    required this.maintenanceTaskId,
    required this.taskDetailId,
    this.measuredValue,
    this.checkResult,
    this.notes,
    this.photoUrl,
    required this.isCompleted,
    this.completedBy,
    this.completedByCrewId,
    this.completedAt,
    this.createdAt,
    this.updatedAt,
  });

  factory MaintenanceTaskDetail.fromJson(Map<String, dynamic> json) {
    return MaintenanceTaskDetail(
      id: json['id'],
      maintenanceTaskId: json['maintenanceTaskId'],
      taskDetailId: json['taskDetailId'],
      // Backend sends double, convert to string for display
      measuredValue: json['measuredValue']?.toString(),
      checkResult: json['checkResult'],
      notes: json['notes'], // Backend uses 'notes' not 'inspectionNotes'
      photoUrl: json['photoUrl'],
      isCompleted: json['isCompleted'] ?? false,
      completedBy: json['completedBy'],
      completedByCrewId: json['completedByCrewId'],
      completedAt: json['completedAt'],
      createdAt: json['createdAt'],
      updatedAt: json['updatedAt'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      if (id != null) 'id': id,
      'maintenanceTaskId': maintenanceTaskId,
      'taskDetailId': taskDetailId,
      'measuredValue': measuredValue,
      'checkResult': checkResult,
      'notes': notes, // Backend uses 'notes' not 'inspectionNotes'
      'photoUrl': photoUrl,
      'isCompleted': isCompleted,
      'completedBy': completedBy,
      'completedByCrewId': completedByCrewId,
      'completedAt': completedAt,
      if (createdAt != null) 'createdAt': createdAt,
      if (updatedAt != null) 'updatedAt': updatedAt,
    };
  }

  @override
  List<Object?> get props => [id, maintenanceTaskId, taskDetailId];
}
