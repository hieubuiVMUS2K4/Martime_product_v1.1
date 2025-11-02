import 'package:equatable/equatable.dart';
import 'task_detail.dart';
import 'maintenance_task_detail.dart';

/// Combined view of TaskDetail template + execution status
class TaskChecklistItem extends Equatable {
  final TaskDetail taskDetail;
  final MaintenanceTaskDetail? executionDetail;

  const TaskChecklistItem({
    required this.taskDetail,
    this.executionDetail,
  });

  factory TaskChecklistItem.fromJson(Map<String, dynamic> json) {
    return TaskChecklistItem(
      taskDetail: TaskDetail.fromJson(json['taskDetail']),
      executionDetail: json['executionDetail'] != null
          ? MaintenanceTaskDetail.fromJson(json['executionDetail'])
          : null,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'taskDetail': taskDetail.toJson(),
      'executionDetail': executionDetail?.toJson(),
    };
  }

  bool get isCompleted => executionDetail?.isCompleted ?? false;
  bool get isMandatory => taskDetail.isMandatory;
  bool get requiresPhoto => taskDetail.requiresPhoto;

  @override
  List<Object?> get props => [taskDetail, executionDetail];
}
