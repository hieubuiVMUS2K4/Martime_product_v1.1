import 'package:equatable/equatable.dart';

/// Task progress information
class TaskProgress extends Equatable {
  final int maintenanceTaskId;
  final int totalItems;
  final int completedItems;
  final int mandatoryItems;
  final int completedMandatoryItems;
  final double completionPercentage;
  final bool canComplete;

  const TaskProgress({
    required this.maintenanceTaskId,
    required this.totalItems,
    required this.completedItems,
    required this.mandatoryItems,
    required this.completedMandatoryItems,
    required this.completionPercentage,
    required this.canComplete,
  });

  factory TaskProgress.fromJson(Map<String, dynamic> json) {
    return TaskProgress(
      maintenanceTaskId: json['maintenanceTaskId'],
      totalItems: json['totalItems'] ?? 0,
      completedItems: json['completedItems'] ?? 0,
      mandatoryItems: json['mandatoryItems'] ?? 0,
      completedMandatoryItems: json['completedMandatoryItems'] ?? 0,
      completionPercentage: (json['completionPercentage'] ?? 0.0).toDouble(),
      canComplete: json['canComplete'] ?? false,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'maintenanceTaskId': maintenanceTaskId,
      'totalItems': totalItems,
      'completedItems': completedItems,
      'mandatoryItems': mandatoryItems,
      'completedMandatoryItems': completedMandatoryItems,
      'completionPercentage': completionPercentage,
      'canComplete': canComplete,
    };
  }

  @override
  List<Object?> get props => [
        maintenanceTaskId,
        completedItems,
        completedMandatoryItems,
      ];
}
