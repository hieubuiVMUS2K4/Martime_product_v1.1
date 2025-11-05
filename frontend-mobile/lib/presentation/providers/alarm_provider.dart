import 'package:flutter/foundation.dart';
import '../../data/models/safety_alarm.dart';
import '../../data/repositories/alarm_repository.dart';

class AlarmProvider with ChangeNotifier {
  final AlarmRepository _repository;

  AlarmProvider(this._repository);

  List<SafetyAlarm> _activeAlarms = [];
  List<SafetyAlarm> _alarmHistory = [];
  AlarmStatistics? _statistics;
  
  bool _isLoading = false;
  bool _isLoadingHistory = false;
  bool _isLoadingStats = false;
  String? _error;

  List<SafetyAlarm> get activeAlarms => _activeAlarms;
  List<SafetyAlarm> get alarmHistory => _alarmHistory;
  AlarmStatistics? get statistics => _statistics;
  bool get isLoading => _isLoading;
  bool get isLoadingHistory => _isLoadingHistory;
  bool get isLoadingStats => _isLoadingStats;
  String? get error => _error;

  int get criticalCount =>
      _activeAlarms.where((a) => a.isCritical && !a.isResolved).length;
  
  int get warningCount =>
      _activeAlarms.where((a) => a.isWarning && !a.isResolved).length;

  int get unacknowledgedCount =>
      _activeAlarms.where((a) => !a.isAcknowledged && !a.isResolved).length;

  Future<void> fetchActiveAlarms() async {
    _isLoading = true;
    _error = null;
    notifyListeners();

    try {
      _activeAlarms = await _repository.getActiveAlarms();
      // Sort by severity (critical first) then by timestamp (newest first)
      _activeAlarms.sort((a, b) {
        if (a.isCritical && !b.isCritical) return -1;
        if (!a.isCritical && b.isCritical) return 1;
        if (a.isWarning && !b.isWarning) return -1;
        if (!a.isWarning && b.isWarning) return 1;
        return b.timestamp.compareTo(a.timestamp);
      });
    } catch (e) {
      _error = e.toString();
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<void> fetchAlarmHistory({int days = 7}) async {
    _isLoadingHistory = true;
    _error = null;
    notifyListeners();

    try {
      _alarmHistory = await _repository.getAlarmHistory(days: days);
      _alarmHistory.sort((a, b) => b.timestamp.compareTo(a.timestamp));
    } catch (e) {
      _error = e.toString();
    } finally {
      _isLoadingHistory = false;
      notifyListeners();
    }
  }

  Future<void> fetchStatistics({int days = 30}) async {
    _isLoadingStats = true;
    _error = null;
    notifyListeners();

    try {
      _statistics = await _repository.getStatistics(days: days);
    } catch (e) {
      _error = e.toString();
    } finally {
      _isLoadingStats = false;
      notifyListeners();
    }
  }

  Future<bool> acknowledgeAlarm(int id, String acknowledgedBy) async {
    try {
      await _repository.acknowledgeAlarm(id, acknowledgedBy);
      
      // Update local state
      final index = _activeAlarms.indexWhere((a) => a.id == id);
      if (index != -1) {
        _activeAlarms[index] = _activeAlarms[index].copyWith(
          isAcknowledged: true,
          acknowledgedBy: acknowledgedBy,
          acknowledgedAt: DateTime.now(),
        );
        notifyListeners();
      }
      
      return true;
    } catch (e) {
      _error = e.toString();
      notifyListeners();
      return false;
    }
  }

  Future<bool> resolveAlarm(int id) async {
    try {
      await _repository.resolveAlarm(id);
      
      // Remove from active alarms
      _activeAlarms.removeWhere((a) => a.id == id);
      notifyListeners();
      
      return true;
    } catch (e) {
      _error = e.toString();
      notifyListeners();
      return false;
    }
  }

  Future<void> generateSampleAlarms() async {
    try {
      await _repository.generateSampleAlarms();
      await fetchActiveAlarms();
    } catch (e) {
      _error = e.toString();
      notifyListeners();
    }
  }

  void clearError() {
    _error = null;
    notifyListeners();
  }
}
