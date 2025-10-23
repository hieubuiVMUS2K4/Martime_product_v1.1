import 'package:equatable/equatable.dart';
import 'package:flutter/material.dart';

class MaintenanceTask extends Equatable {
  final int id;
  final String taskId;
  final String equipmentId;
  final String equipmentName;
  final String taskType;
  final String taskDescription;
  final double? intervalHours;
  final int? intervalDays;
  final String? lastDoneAt;
  final String nextDueAt;
  final double? runningHoursAtLastDone;
  final String priority;
  final String status;
  final String? assignedTo;
  final String? assignedToCrewId;
  final String? completedAt;
  final String? completedBy;
  final String? completedByCrewId;
  final String? notes;
  final String? sparePartsUsed;
  final double? runningHoursAtCompletion;
  final String? photoUrls;
  final bool isSynced;
  final String createdAt;
  final String? updatedAt;
  
  const MaintenanceTask({
    required this.id,
    required this.taskId,
    required this.equipmentId,
    required this.equipmentName,
    required this.taskType,
    required this.taskDescription,
    this.intervalHours,
    this.intervalDays,
    this.lastDoneAt,
    required this.nextDueAt,
    this.runningHoursAtLastDone,
    required this.priority,
    required this.status,
    this.assignedTo,
    this.assignedToCrewId,
    this.completedAt,
    this.completedBy,
    this.completedByCrewId,
    this.notes,
    this.sparePartsUsed,
    this.runningHoursAtCompletion,
    this.photoUrls,
    required this.isSynced,
    required this.createdAt,
    this.updatedAt,
  });
  
  factory MaintenanceTask.fromJson(Map<String, dynamic> json) {
    return MaintenanceTask(
      id: json['id'],
      taskId: json['taskId'],
      equipmentId: json['equipmentId'],
      equipmentName: json['equipmentName'],
      taskType: json['taskType'],
      taskDescription: json['taskDescription'],
      intervalHours: json['intervalHours']?.toDouble(),
      intervalDays: json['intervalDays'],
      lastDoneAt: json['lastDoneAt'],
      nextDueAt: json['nextDueAt'],
      runningHoursAtLastDone: json['runningHoursAtLastDone']?.toDouble(),
      priority: json['priority'],
      status: json['status'],
      assignedTo: json['assignedTo'],
      assignedToCrewId: json['assignedToCrewId'],
      completedAt: json['completedAt'],
      completedBy: json['completedBy'],
      completedByCrewId: json['completedByCrewId'],
      notes: json['notes'],
      sparePartsUsed: json['sparePartsUsed'],
      runningHoursAtCompletion: json['runningHoursAtCompletion']?.toDouble(),
      photoUrls: json['photoUrls'],
      isSynced: json['isSynced'] ?? false,
      createdAt: json['createdAt'],
      updatedAt: json['updatedAt'],
    );
  }
  
  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'taskId': taskId,
      'equipmentId': equipmentId,
      'equipmentName': equipmentName,
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
      'completedAt': completedAt,
      'completedBy': completedBy,
      'completedByCrewId': completedByCrewId,
      'notes': notes,
      'sparePartsUsed': sparePartsUsed,
      'runningHoursAtCompletion': runningHoursAtCompletion,
      'photoUrls': photoUrls,
      'isSynced': isSynced,
      'createdAt': createdAt,
      'updatedAt': updatedAt,
    };
  }
  
  // Computed properties
  int get daysUntilDue {
    final due = DateTime.parse(nextDueAt);
    return due.difference(DateTime.now()).inDays;
  }
  
  bool get isOverdue => daysUntilDue < 0;
  bool get isDueSoon => daysUntilDue >= 0 && daysUntilDue <= 7;
  bool get isPending => status == 'PENDING';
  bool get isInProgress => status == 'IN_PROGRESS';
  bool get isCompleted => status == 'COMPLETED';
  
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
    if (isOverdue) return Colors.red.shade700;
    if (isDueSoon) return Colors.orange.shade700;
    if (isCompleted) return Colors.green.shade700;
    if (isInProgress) return Colors.blue.shade700;
    return Colors.grey.shade600;
  }
  
  String get statusText {
    if (isOverdue && !isCompleted) return 'OVERDUE';
    return status;
  }
  
  @override
  List<Object?> get props => [id, taskId, status, updatedAt];
}
