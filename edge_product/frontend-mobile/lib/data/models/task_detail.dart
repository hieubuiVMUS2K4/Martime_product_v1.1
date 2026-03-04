import 'package:equatable/equatable.dart';

/// Checklist item template for a task type
class TaskDetail extends Equatable {
  final int id;
  final int taskTypeId;
  final String detailName;
  final String detailType; // MEASUREMENT, CHECKLIST, INSPECTION
  final String? description;
  final bool isMandatory;
  final int orderIndex;
  final String? unit;
  final double? minValue;
  final double? maxValue;
  final String? acceptableCriteria;
  final bool requiresPhoto;
  final String createdAt;
  final String? updatedAt;

  const TaskDetail({
    required this.id,
    required this.taskTypeId,
    required this.detailName,
    required this.detailType,
    this.description,
    required this.isMandatory,
    required this.orderIndex,
    this.unit,
    this.minValue,
    this.maxValue,
    this.acceptableCriteria,
    required this.requiresPhoto,
    required this.createdAt,
    this.updatedAt,
  });

  factory TaskDetail.fromJson(Map<String, dynamic> json) {
    return TaskDetail(
      id: json['id'] as int,
      taskTypeId: json['taskTypeId'] as int? ?? 0,
      detailName: json['detailName'] as String,
      detailType: json['detailType'] as String,
      description: json['description'] as String?,
      isMandatory: json['isMandatory'] as bool? ?? false,
      orderIndex: json['orderIndex'] as int? ?? 0,
      unit: json['unit'] as String?,
      minValue: (json['minValue'] as num?)?.toDouble(),
      maxValue: (json['maxValue'] as num?)?.toDouble(),
      acceptableCriteria: json['acceptableCriteria'] as String?,
      requiresPhoto: json['requiresPhoto'] as bool? ?? false,
      createdAt: json['createdAt'] as String? ?? DateTime.now().toIso8601String(),
      updatedAt: json['updatedAt'] as String?,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'taskTypeId': taskTypeId,
      'detailName': detailName,
      'detailType': detailType,
      'description': description,
      'isMandatory': isMandatory,
      'orderIndex': orderIndex,
      'unit': unit,
      'minValue': minValue,
      'maxValue': maxValue,
      'acceptableCriteria': acceptableCriteria,
      'requiresPhoto': requiresPhoto,
      'createdAt': createdAt,
      'updatedAt': updatedAt,
    };
  }

  bool get isMeasurement => detailType == 'MEASUREMENT';
  bool get isChecklist => detailType == 'CHECKLIST';
  bool get isInspection => detailType == 'INSPECTION';

  @override
  List<Object?> get props => [id, taskTypeId, detailName];
}
