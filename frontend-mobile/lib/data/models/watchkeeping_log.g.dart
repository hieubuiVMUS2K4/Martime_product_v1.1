// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'watchkeeping_log.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

WatchkeepingLog _$WatchkeepingLogFromJson(Map<String, dynamic> json) =>
    WatchkeepingLog(
      id: (json['id'] as num?)?.toInt(),
      watchDate: DateTime.parse(json['watchDate'] as String),
      watchPeriod: json['watchPeriod'] as String,
      watchType: json['watchType'] as String,
      officerOnWatch: json['officerOnWatch'] as String,
      lookout: json['lookout'] as String?,
      weatherConditions: json['weatherConditions'] as String?,
      seaState: json['seaState'] as String?,
      visibility: json['visibility'] as String?,
      courseLogged: (json['courseLogged'] as num?)?.toDouble(),
      speedLogged: (json['speedLogged'] as num?)?.toDouble(),
      positionLat: (json['positionLat'] as num?)?.toDouble(),
      positionLon: (json['positionLon'] as num?)?.toDouble(),
      distanceRun: (json['distanceRun'] as num?)?.toDouble(),
      engineStatus: json['engineStatus'] as String?,
      notableEvents: json['notableEvents'] as String?,
      masterSignature: json['masterSignature'] as String?,
      isSynced: json['isSynced'] as bool? ?? false,
      createdAt: json['createdAt'] == null
          ? null
          : DateTime.parse(json['createdAt'] as String),
    );

Map<String, dynamic> _$WatchkeepingLogToJson(WatchkeepingLog instance) =>
    <String, dynamic>{
      'id': instance.id,
      'watchDate': instance.watchDate.toIso8601String(),
      'watchPeriod': instance.watchPeriod,
      'watchType': instance.watchType,
      'officerOnWatch': instance.officerOnWatch,
      'lookout': instance.lookout,
      'weatherConditions': instance.weatherConditions,
      'seaState': instance.seaState,
      'visibility': instance.visibility,
      'courseLogged': instance.courseLogged,
      'speedLogged': instance.speedLogged,
      'positionLat': instance.positionLat,
      'positionLon': instance.positionLon,
      'distanceRun': instance.distanceRun,
      'engineStatus': instance.engineStatus,
      'notableEvents': instance.notableEvents,
      'masterSignature': instance.masterSignature,
      'isSynced': instance.isSynced,
      'createdAt': instance.createdAt?.toIso8601String(),
    };
