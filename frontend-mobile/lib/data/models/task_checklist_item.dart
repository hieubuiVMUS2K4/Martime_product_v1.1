import 'package:equatable/equatable.dart';

/// Matches edge-services `TaskChecklistItem` (Models/EdgeModels.cs)
/// and responses from `TaskChecklistItemsController`.
class TaskChecklistItem extends Equatable {
  final String id; // Guid
  final String taskId; // task code string

  final String assetCode;
  final String assetName;
  final int sequenceOrder;

  final String? checkpointDescription;
  final bool requiresReading;
  final double? normalRangeMin;
  final double? normalRangeMax;
  final String? unit;

  final bool isCompleted;
  final String? completedAt;
  final String? completedBy;

  final double? readingValue;
  final String? remarks;
  final bool isAbnormal;

  const TaskChecklistItem({
    required this.id,
    required this.taskId,
    required this.assetCode,
    required this.assetName,
    required this.sequenceOrder,
    this.checkpointDescription,
    required this.requiresReading,
    this.normalRangeMin,
    this.normalRangeMax,
    this.unit,
    required this.isCompleted,
    this.completedAt,
    this.completedBy,
    this.readingValue,
    this.remarks,
    required this.isAbnormal,
  });

  factory TaskChecklistItem.fromJson(Map<String, dynamic> json) {
    return TaskChecklistItem(
      id: json['id']?.toString() ?? '',
      taskId: json['taskId']?.toString() ?? '',
      assetCode: json['assetCode']?.toString() ?? '',
      assetName: json['assetName']?.toString() ?? '',
      sequenceOrder: json['sequenceOrder'] ?? 0,
      checkpointDescription: json['checkpointDescription']?.toString(),
      requiresReading: json['requiresReading'] ?? false,
      normalRangeMin: (json['normalRangeMin'] as num?)?.toDouble(),
      normalRangeMax: (json['normalRangeMax'] as num?)?.toDouble(),
      unit: json['unit']?.toString(),
      isCompleted: json['isCompleted'] ?? false,
      completedAt: json['completedAt']?.toString(),
      completedBy: json['completedBy']?.toString(),
      readingValue: (json['readingValue'] as num?)?.toDouble(),
      remarks: json['remarks']?.toString(),
      isAbnormal: json['isAbnormal'] ?? false,
    );
  }

  Map<String, dynamic> toJson() => {
        'id': id,
        'taskId': taskId,
        'assetCode': assetCode,
        'assetName': assetName,
        'sequenceOrder': sequenceOrder,
        'checkpointDescription': checkpointDescription,
        'requiresReading': requiresReading,
        'normalRangeMin': normalRangeMin,
        'normalRangeMax': normalRangeMax,
        'unit': unit,
        'isCompleted': isCompleted,
        'completedAt': completedAt,
        'completedBy': completedBy,
        'readingValue': readingValue,
        'remarks': remarks,
        'isAbnormal': isAbnormal,
      };

  /// Convenience getters for range validation
  double? get minValue => normalRangeMin;
  double? get maxValue => normalRangeMax;
  
  /// Whether this checklist item is mandatory (always true for now, can be extended)
  bool get isMandatory => true;

  /// Create a copy with updated fields
  TaskChecklistItem copyWith({
    String? id,
    String? taskId,
    String? assetCode,
    String? assetName,
    int? sequenceOrder,
    String? checkpointDescription,
    bool? requiresReading,
    double? normalRangeMin,
    double? normalRangeMax,
    String? unit,
    bool? isCompleted,
    String? completedAt,
    String? completedBy,
    double? readingValue,
    String? remarks,
    bool? isAbnormal,
  }) {
    return TaskChecklistItem(
      id: id ?? this.id,
      taskId: taskId ?? this.taskId,
      assetCode: assetCode ?? this.assetCode,
      assetName: assetName ?? this.assetName,
      sequenceOrder: sequenceOrder ?? this.sequenceOrder,
      checkpointDescription: checkpointDescription ?? this.checkpointDescription,
      requiresReading: requiresReading ?? this.requiresReading,
      normalRangeMin: normalRangeMin ?? this.normalRangeMin,
      normalRangeMax: normalRangeMax ?? this.normalRangeMax,
      unit: unit ?? this.unit,
      isCompleted: isCompleted ?? this.isCompleted,
      completedAt: completedAt ?? this.completedAt,
      completedBy: completedBy ?? this.completedBy,
      readingValue: readingValue ?? this.readingValue,
      remarks: remarks ?? this.remarks,
      isAbnormal: isAbnormal ?? this.isAbnormal,
    );
  }

  @override
  List<Object?> get props => [
        id,
        taskId,
        assetCode,
        assetName,
        sequenceOrder,
        isCompleted,
        readingValue,
        isAbnormal,
      ];
}
