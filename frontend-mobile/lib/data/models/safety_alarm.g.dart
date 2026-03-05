// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'safety_alarm.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

SafetyAlarm _$SafetyAlarmFromJson(Map<String, dynamic> json) => SafetyAlarm(
      id: (json['id'] as num).toInt(),
      alarmType: json['alarmType'] as String,
      alarmCode: json['alarmCode'] as String?,
      severity: json['severity'] as String,
      location: json['location'] as String?,
      description: json['description'] as String?,
      timestamp: DateTime.parse(json['timestamp'] as String),
      isAcknowledged: json['isAcknowledged'] as bool,
      acknowledgedBy: json['acknowledgedBy'] as String?,
      acknowledgedAt: json['acknowledgedAt'] == null
          ? null
          : DateTime.parse(json['acknowledgedAt'] as String),
      isResolved: json['isResolved'] as bool,
      resolvedAt: json['resolvedAt'] == null
          ? null
          : DateTime.parse(json['resolvedAt'] as String),
      isSynced: json['isSynced'] as bool?,
      createdAt: json['createdAt'] == null
          ? null
          : DateTime.parse(json['createdAt'] as String),
      updatedAt: json['updatedAt'] == null
          ? null
          : DateTime.parse(json['updatedAt'] as String),
    );

Map<String, dynamic> _$SafetyAlarmToJson(SafetyAlarm instance) =>
    <String, dynamic>{
      'id': instance.id,
      'alarmType': instance.alarmType,
      'alarmCode': instance.alarmCode,
      'severity': instance.severity,
      'location': instance.location,
      'description': instance.description,
      'timestamp': instance.timestamp.toIso8601String(),
      'isAcknowledged': instance.isAcknowledged,
      'acknowledgedBy': instance.acknowledgedBy,
      'acknowledgedAt': instance.acknowledgedAt?.toIso8601String(),
      'isResolved': instance.isResolved,
      'resolvedAt': instance.resolvedAt?.toIso8601String(),
      'isSynced': instance.isSynced,
      'createdAt': instance.createdAt?.toIso8601String(),
      'updatedAt': instance.updatedAt?.toIso8601String(),
    };

AlarmStatistics _$AlarmStatisticsFromJson(Map<String, dynamic> json) =>
    AlarmStatistics(
      total: (json['total'] as num).toInt(),
      active: (json['active'] as num).toInt(),
      acknowledged: (json['acknowledged'] as num).toInt(),
      resolved: (json['resolved'] as num).toInt(),
      bySeverity: (json['bySeverity'] as List<dynamic>)
          .map((e) => SeverityCount.fromJson(e as Map<String, dynamic>))
          .toList(),
      byType: (json['byType'] as List<dynamic>)
          .map((e) => TypeCount.fromJson(e as Map<String, dynamic>))
          .toList(),
      byLocation: (json['byLocation'] as List<dynamic>)
          .map((e) => LocationCount.fromJson(e as Map<String, dynamic>))
          .toList(),
    );

Map<String, dynamic> _$AlarmStatisticsToJson(AlarmStatistics instance) =>
    <String, dynamic>{
      'total': instance.total,
      'active': instance.active,
      'acknowledged': instance.acknowledged,
      'resolved': instance.resolved,
      'bySeverity': instance.bySeverity,
      'byType': instance.byType,
      'byLocation': instance.byLocation,
    };

SeverityCount _$SeverityCountFromJson(Map<String, dynamic> json) =>
    SeverityCount(
      severity: json['severity'] as String,
      count: (json['count'] as num).toInt(),
    );

Map<String, dynamic> _$SeverityCountToJson(SeverityCount instance) =>
    <String, dynamic>{
      'severity': instance.severity,
      'count': instance.count,
    };

TypeCount _$TypeCountFromJson(Map<String, dynamic> json) => TypeCount(
      type: json['type'] as String,
      count: (json['count'] as num).toInt(),
    );

Map<String, dynamic> _$TypeCountToJson(TypeCount instance) => <String, dynamic>{
      'type': instance.type,
      'count': instance.count,
    };

LocationCount _$LocationCountFromJson(Map<String, dynamic> json) =>
    LocationCount(
      location: json['location'] as String,
      count: (json['count'] as num).toInt(),
    );

Map<String, dynamic> _$LocationCountToJson(LocationCount instance) =>
    <String, dynamic>{
      'location': instance.location,
      'count': instance.count,
    };
