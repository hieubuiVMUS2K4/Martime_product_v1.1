import 'package:equatable/equatable.dart';

/// Task type template (e.g., ENGINE_OIL_CHANGE, SAFETY_CHECK)
class TaskType extends Equatable {
  final int id;
  final String typeCode;
  final String typeName;
  final String? description;
  final String category;
  final String priority;
  final int? estimatedDurationMinutes;
  final bool isActive;
  final String createdAt;
  final String? updatedAt;

  const TaskType({
    required this.id,
    required this.typeCode,
    required this.typeName,
    this.description,
    required this.category,
    required this.priority,
    this.estimatedDurationMinutes,
    required this.isActive,
    required this.createdAt,
    this.updatedAt,
  });

  factory TaskType.fromJson(Map<String, dynamic> json) {
    return TaskType(
      id: json['id'],
      typeCode: json['typeCode'],
      typeName: json['typeName'],
      description: json['description'],
      category: json['category'],
      priority: json['priority'],
      estimatedDurationMinutes: json['estimatedDurationMinutes'],
      isActive: json['isActive'] ?? true,
      createdAt: json['createdAt'],
      updatedAt: json['updatedAt'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'typeCode': typeCode,
      'typeName': typeName,
      'description': description,
      'category': category,
      'priority': priority,
      'estimatedDurationMinutes': estimatedDurationMinutes,
      'isActive': isActive,
      'createdAt': createdAt,
      'updatedAt': updatedAt,
    };
  }

  @override
  List<Object?> get props => [id, typeCode];
}
