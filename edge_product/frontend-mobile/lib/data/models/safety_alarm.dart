import 'package:json_annotation/json_annotation.dart';

part 'safety_alarm.g.dart';

@JsonSerializable()
class SafetyAlarm {
  final int id;
  final String alarmType;
  final String? alarmCode;
  final String severity;
  final String? location;
  final String? description;
  final DateTime timestamp;
  final bool isAcknowledged;
  final String? acknowledgedBy;
  final DateTime? acknowledgedAt;
  final bool isResolved;
  final DateTime? resolvedAt;
  final bool? isSynced;
  final DateTime? createdAt;
  final DateTime? updatedAt;

  SafetyAlarm({
    required this.id,
    required this.alarmType,
    this.alarmCode,
    required this.severity,
    this.location,
    this.description,
    required this.timestamp,
    required this.isAcknowledged,
    this.acknowledgedBy,
    this.acknowledgedAt,
    required this.isResolved,
    this.resolvedAt,
    this.isSynced,
    this.createdAt,
    this.updatedAt,
  });

  factory SafetyAlarm.fromJson(Map<String, dynamic> json) =>
      _$SafetyAlarmFromJson(json);

  Map<String, dynamic> toJson() => _$SafetyAlarmToJson(this);

  // Helper getters
  String get severityDisplay {
    switch (severity.toUpperCase()) {
      case 'CRITICAL':
        return 'Critical';
      case 'WARNING':
        return 'Warning';
      case 'INFO':
        return 'Info';
      default:
        return severity;
    }
  }

  String get typeDisplay {
    switch (alarmType.toUpperCase()) {
      case 'FIRE':
        return '🔥 Fire';
      case 'BILGE':
        return '💧 Bilge';
      case 'ENGINE':
        return '⚙️ Engine';
      case 'NAVIGATION':
        return '🧭 Navigation';
      case 'SAFETY':
        return '⚠️ Safety';
      case 'SECURITY':
        return '🔒 Security';
      default:
        return alarmType;
    }
  }

  String get locationDisplay {
    if (location == null) return 'Unknown';
    switch (location!.toUpperCase()) {
      case 'ENGINE_ROOM':
        return 'Engine Room';
      case 'BRIDGE':
        return 'Bridge';
      case 'DECK':
        return 'Deck';
      case 'GALLEY':
        return 'Galley';
      case 'MACHINERY_SPACE':
        return 'Machinery Space';
      case 'CARGO_HOLD':
        return 'Cargo Hold';
      default:
        return location!;
    }
  }

  String get statusDisplay {
    if (isResolved) return 'Resolved';
    if (isAcknowledged) return 'Acknowledged';
    return 'Unacknowledged';
  }

  Duration get timeSinceAlarm {
    return DateTime.now().difference(timestamp);
  }

  String get timeAgo {
    final duration = timeSinceAlarm;
    if (duration.inMinutes < 1) {
      return 'Just now';
    } else if (duration.inMinutes < 60) {
      return '${duration.inMinutes} min ago';
    } else if (duration.inHours < 24) {
      return '${duration.inHours} hr ago';
    } else {
      return '${duration.inDays} days ago';
    }
  }

  bool get isCritical => severity.toUpperCase() == 'CRITICAL';
  bool get isWarning => severity.toUpperCase() == 'WARNING';
  bool get isInfo => severity.toUpperCase() == 'INFO';

  SafetyAlarm copyWith({
    int? id,
    String? alarmType,
    String? alarmCode,
    String? severity,
    String? location,
    String? description,
    DateTime? timestamp,
    bool? isAcknowledged,
    String? acknowledgedBy,
    DateTime? acknowledgedAt,
    bool? isResolved,
    DateTime? resolvedAt,
    bool? isSynced,
    DateTime? createdAt,
    DateTime? updatedAt,
  }) {
    return SafetyAlarm(
      id: id ?? this.id,
      alarmType: alarmType ?? this.alarmType,
      alarmCode: alarmCode ?? this.alarmCode,
      severity: severity ?? this.severity,
      location: location ?? this.location,
      description: description ?? this.description,
      timestamp: timestamp ?? this.timestamp,
      isAcknowledged: isAcknowledged ?? this.isAcknowledged,
      acknowledgedBy: acknowledgedBy ?? this.acknowledgedBy,
      acknowledgedAt: acknowledgedAt ?? this.acknowledgedAt,
      isResolved: isResolved ?? this.isResolved,
      resolvedAt: resolvedAt ?? this.resolvedAt,
      isSynced: isSynced ?? this.isSynced,
      createdAt: createdAt ?? this.createdAt,
      updatedAt: updatedAt ?? this.updatedAt,
    );
  }
}

@JsonSerializable()
class AlarmStatistics {
  final int total;
  final int active;
  final int acknowledged;
  final int resolved;
  final List<SeverityCount> bySeverity;
  final List<TypeCount> byType;
  final List<LocationCount> byLocation;

  AlarmStatistics({
    required this.total,
    required this.active,
    required this.acknowledged,
    required this.resolved,
    required this.bySeverity,
    required this.byType,
    required this.byLocation,
  });

  factory AlarmStatistics.fromJson(Map<String, dynamic> json) =>
      _$AlarmStatisticsFromJson(json);

  Map<String, dynamic> toJson() => _$AlarmStatisticsToJson(this);
}

@JsonSerializable()
class SeverityCount {
  final String severity;
  final int count;

  SeverityCount({required this.severity, required this.count});

  factory SeverityCount.fromJson(Map<String, dynamic> json) =>
      _$SeverityCountFromJson(json);

  Map<String, dynamic> toJson() => _$SeverityCountToJson(this);
}

@JsonSerializable()
class TypeCount {
  final String type;
  final int count;

  TypeCount({required this.type, required this.count});

  factory TypeCount.fromJson(Map<String, dynamic> json) =>
      _$TypeCountFromJson(json);

  Map<String, dynamic> toJson() => _$TypeCountToJson(this);
}

@JsonSerializable()
class LocationCount {
  final String location;
  final int count;

  LocationCount({required this.location, required this.count});

  factory LocationCount.fromJson(Map<String, dynamic> json) =>
      _$LocationCountFromJson(json);

  Map<String, dynamic> toJson() => _$LocationCountToJson(this);
}
