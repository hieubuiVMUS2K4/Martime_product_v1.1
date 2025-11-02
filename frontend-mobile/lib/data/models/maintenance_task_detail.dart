import 'package:equatable/equatable.dart';

/// Execution record for a checklist item
class MaintenanceTaskDetail extends Equatable {
  final int? id;
  final int maintenanceTaskId;
  final int taskDetailId;
  final String? measuredValue;
  final bool? checkResult;
  final String? inspectionNotes;
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
    this.inspectionNotes,
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
      measuredValue: json['measuredValue'],
      checkResult: json['checkResult'],
      inspectionNotes: json['inspectionNotes'],
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
      'inspectionNotes': inspectionNotes,
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
