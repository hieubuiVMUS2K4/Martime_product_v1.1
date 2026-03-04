import 'package:json_annotation/json_annotation.dart';

part 'watchkeeping_log.g.dart';

/// WatchkeepingLog model compliant with SOLAS Chapter V/28, STCW Convention, and MLC 2006
@JsonSerializable()
class WatchkeepingLog {
  final int? id;
  final DateTime watchDate;
  final String watchPeriod; // 00-04, 04-08, 08-12, 12-16, 16-20, 20-24
  final String watchType; // NAVIGATION, ENGINE
  final String officerOnWatch;
  final String? reliefOfficer; // Officer taking over watch
  final String? lookout;
  
  // STCW Rest Hours Compliance (Mandatory)
  final double workHours; // Hours worked this watch (default 4h)
  final double restHoursLast24h; // Minimum 10 hours in any 24-hour period
  final double restHoursLast7Days; // Minimum 77 hours in any 7-day period
  final bool restHoursCompliant; // Auto-calculated compliance
  final String? restHoursException; // If non-compliant, reason must be recorded
  
  // Weather & Navigation
  final String? weatherConditions;
  final String? seaState; // Douglas Sea Scale: Calm, Smooth, Slight, Moderate, Rough, Very Rough, High, Very High, Phenomenal
  final String? visibility; // Good (>5nm), Moderate (2-5nm), Poor (0.5-2nm), Fog (<0.5nm)
  final double? courseLogged; // Degrees true
  final double? speedLogged; // Knots
  final double? positionLat;
  final double? positionLon;
  final double? distanceRun; // Nautical miles during watch
  
  // Bridge Equipment Status
  final String? engineStatus;
  final bool radarOperational;
  final bool ecdisOperational;
  final bool aisOperational;
  final bool gyroOperational;
  final bool autopilotEngaged;
  final String? equipmentDefects; // Any navigation equipment failures
  
  // GMDSS Watch (SOLAS Chapter IV)
  final bool gmdssWatchMaintained;
  final String? navigationWarningsReceived; // NAVTEX, SafetyNET messages
  
  // Watch Events & Handover
  final String? notableEvents;
  final String? handoverNotes; // Notes for relieving officer (mandatory)
  final bool handoverChecklistCompleted;
  final DateTime? watchStartTime;
  final DateTime? watchEndTime;
  
  // Bridge Manning (STCW)
  final int bridgeManningLevel; // Number of persons on bridge
  final bool lookoutPosted; // Mandatory during hours of darkness
  
  // Fatigue Management (MLC 2006)
  final String? fatigueRiskLevel; // LOW, MEDIUM, HIGH
  final bool fatigueAssessmentDone;
  
  final String? masterSignature;
  final DateTime? signedAt;
  final bool isSynced;
  final DateTime? createdAt;

  WatchkeepingLog({
    this.id,
    required this.watchDate,
    required this.watchPeriod,
    required this.watchType,
    required this.officerOnWatch,
    this.reliefOfficer,
    this.lookout,
    this.workHours = 4.0,
    this.restHoursLast24h = 0.0,
    this.restHoursLast7Days = 0.0,
    this.restHoursCompliant = true,
    this.restHoursException,
    this.weatherConditions,
    this.seaState,
    this.visibility,
    this.courseLogged,
    this.speedLogged,
    this.positionLat,
    this.positionLon,
    this.distanceRun,
    this.engineStatus,
    this.radarOperational = true,
    this.ecdisOperational = true,
    this.aisOperational = true,
    this.gyroOperational = true,
    this.autopilotEngaged = false,
    this.equipmentDefects,
    this.gmdssWatchMaintained = true,
    this.navigationWarningsReceived,
    this.notableEvents,
    this.handoverNotes,
    this.handoverChecklistCompleted = false,
    this.watchStartTime,
    this.watchEndTime,
    this.bridgeManningLevel = 2,
    this.lookoutPosted = true,
    this.fatigueRiskLevel,
    this.fatigueAssessmentDone = false,
    this.masterSignature,
    this.signedAt,
    this.isSynced = false,
    this.createdAt,
  });

  factory WatchkeepingLog.fromJson(Map<String, dynamic> json) =>
      _$WatchkeepingLogFromJson(json);

  Map<String, dynamic> toJson() => _$WatchkeepingLogToJson(this);

  // STCW Compliance Checks
  bool get isRestHours24hCompliant => restHoursLast24h >= 10.0;
  bool get isRestHours7DaysCompliant => restHoursLast7Days >= 77.0;
  
  String get restHoursStatus {
    if (restHoursCompliant) {
      return '✅ Compliant';
    } else {
      return '⚠️ Non-compliant - Exception recorded';
    }
  }
  
  // Equipment Status Summary
  String get equipmentStatusSummary {
    List<String> issues = [];
    if (!radarOperational) issues.add('Radar');
    if (!ecdisOperational) issues.add('ECDIS');
    if (!aisOperational) issues.add('AIS');
    if (!gyroOperational) issues.add('Gyro');
    
    if (issues.isEmpty) {
      return '✅ All equipment operational';
    } else {
      return '⚠️ Defects: ${issues.join(', ')}';
    }
  }
  
  // Fatigue Risk Display
  String get fatigueRiskDisplay {
    switch (fatigueRiskLevel) {
      case 'LOW':
        return '🟢 Low Risk';
      case 'MEDIUM':
        return '🟡 Medium Risk';
      case 'HIGH':
        return '🔴 High Risk';
      default:
        return 'Not assessed';
    }
  }

  // Helper methods for display
  String get watchPeriodDisplay {
    switch (watchPeriod) {
      case '00-04':
        return '🌙 00:00 - 04:00 (Middle Watch)';
      case '04-08':
        return '🌅 04:00 - 08:00 (Morning Watch)';
      case '08-12':
        return '☀️ 08:00 - 12:00 (Forenoon Watch)';
      case '12-16':
        return '🌞 12:00 - 16:00 (Afternoon Watch)';
      case '16-20':
        return '🌆 16:00 - 20:00 (First Dog/Second Dog)';
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

  // Douglas Sea Scale display
  String get seaStateDisplay {
    if (seaState == null) return 'Not recorded';
    switch (seaState) {
      case 'Calm':
        return '0️⃣ Calm (Glassy)';
      case 'Smooth':
        return '1️⃣ Smooth (Rippled)';
      case 'Slight':
        return '2️⃣ Slight';
      case 'Moderate':
        return '3️⃣ Moderate';
      case 'Rough':
        return '4️⃣ Rough';
      case 'Very Rough':
        return '5️⃣ Very Rough';
      case 'High':
        return '6️⃣ High';
      case 'Very High':
        return '7️⃣ Very High';
      case 'Phenomenal':
        return '8️⃣ Phenomenal';
      default:
        return seaState!;
    }
  }

  String get visibilityDisplay {
    if (visibility == null) return 'Not recorded';
    switch (visibility) {
      case 'Good':
        return '👁️ Good (>5nm)';
      case 'Moderate':
        return '🌫️ Moderate (2-5nm)';
      case 'Poor':
        return '⚠️ Poor (0.5-2nm)';
      case 'Fog':
        return '🌁 Fog (<0.5nm)';
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
      
  bool get hasHandoverNotes =>
      handoverNotes != null && handoverNotes!.isNotEmpty;
}
