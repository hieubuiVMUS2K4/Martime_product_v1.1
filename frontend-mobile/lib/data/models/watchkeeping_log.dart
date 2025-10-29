import 'package:json_annotation/json_annotation.dart';

part 'watchkeeping_log.g.dart';

@JsonSerializable()
class WatchkeepingLog {
  final int? id;
  final DateTime watchDate;
  final String watchPeriod; // 00-04, 04-08, 08-12, 12-16, 16-20, 20-24
  final String watchType; // NAVIGATION, ENGINE
  final String officerOnWatch;
  final String? lookout;
  final String? weatherConditions;
  final String? seaState; // Calm, Moderate, Rough, Very Rough
  final String? visibility; // Good, Moderate, Poor, Fog
  final double? courseLogged; // Degrees true
  final double? speedLogged; // Knots
  final double? positionLat;
  final double? positionLon;
  final double? distanceRun; // Nautical miles during watch
  final String? engineStatus;
  final String? notableEvents;
  final String? masterSignature;
  final bool isSynced;
  final DateTime? createdAt;

  WatchkeepingLog({
    this.id,
    required this.watchDate,
    required this.watchPeriod,
    required this.watchType,
    required this.officerOnWatch,
    this.lookout,
    this.weatherConditions,
    this.seaState,
    this.visibility,
    this.courseLogged,
    this.speedLogged,
    this.positionLat,
    this.positionLon,
    this.distanceRun,
    this.engineStatus,
    this.notableEvents,
    this.masterSignature,
    this.isSynced = false,
    this.createdAt,
  });

  factory WatchkeepingLog.fromJson(Map<String, dynamic> json) =>
      _$WatchkeepingLogFromJson(json);

  Map<String, dynamic> toJson() => _$WatchkeepingLogToJson(this);

  // Helper methods for display
  String get watchPeriodDisplay {
    switch (watchPeriod) {
      case '00-04':
        return '🌙 00:00 - 04:00 (Night Watch)';
      case '04-08':
        return '🌅 04:00 - 08:00 (Morning Watch)';
      case '08-12':
        return '☀️ 08:00 - 12:00 (Forenoon Watch)';
      case '12-16':
        return '🌞 12:00 - 16:00 (Afternoon Watch)';
      case '16-20':
        return '🌆 16:00 - 20:00 (Evening Watch)';
      case '20-24':
        return '🌃 20:00 - 24:00 (First Watch)';
      default:
        return watchPeriod;
    }
  }

  String get watchTypeDisplay {
    switch (watchType) {
      case 'NAVIGATION':
        return '🧭 Navigation Watch';
      case 'ENGINE':
        return '⚙️ Engine Watch';
      default:
        return watchType;
    }
  }

  String get seaStateDisplay {
    if (seaState == null) return 'Not recorded';
    switch (seaState) {
      case 'Calm':
        return '😌 Calm';
      case 'Moderate':
        return '🌊 Moderate';
      case 'Rough':
        return '⚠️ Rough';
      case 'Very Rough':
        return '🌪️ Very Rough';
      default:
        return seaState!;
    }
  }

  String get visibilityDisplay {
    if (visibility == null) return 'Not recorded';
    switch (visibility) {
      case 'Good':
        return '👁️ Good';
      case 'Moderate':
        return '🌫️ Moderate';
      case 'Poor':
        return '⚠️ Poor';
      case 'Fog':
        return '🌁 Fog';
      default:
        return visibility!;
    }
  }

  String get positionDisplay {
    if (positionLat == null || positionLon == null) {
      return 'Position not recorded';
    }
    final latDir = positionLat! >= 0 ? 'N' : 'S';
    final lonDir = positionLon! >= 0 ? 'E' : 'W';
    return '${positionLat!.abs().toStringAsFixed(4)}°$latDir, ${positionLon!.abs().toStringAsFixed(4)}°$lonDir';
  }

  bool get isSigned => masterSignature != null && masterSignature!.isNotEmpty;

  bool get hasNotableEvents =>
      notableEvents != null && notableEvents!.isNotEmpty;
}
