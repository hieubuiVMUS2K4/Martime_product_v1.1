import '../data_sources/remote/alarm_api.dart';
import '../models/safety_alarm.dart';

class AlarmRepository {
  final AlarmApi _api;

  AlarmRepository(this._api);

  Future<List<SafetyAlarm>> getActiveAlarms() async {
    try {
      return await _api.getActiveAlarms();
    } catch (e) {
      throw Exception('Failed to fetch active alarms: $e');
    }
  }

  Future<List<SafetyAlarm>> getAlarmHistory({int days = 7}) async {
    try {
      return await _api.getAlarmHistory(days: days);
    } catch (e) {
      throw Exception('Failed to fetch alarm history: $e');
    }
  }

  Future<void> acknowledgeAlarm(int id, String acknowledgedBy) async {
    try {
      await _api.acknowledgeAlarm(id, {'acknowledgedBy': acknowledgedBy});
    } catch (e) {
      throw Exception('Failed to acknowledge alarm: $e');
    }
  }

  Future<void> resolveAlarm(int id) async {
    try {
      await _api.resolveAlarm(id);
    } catch (e) {
      throw Exception('Failed to resolve alarm: $e');
    }
  }

  Future<AlarmStatistics> getStatistics({int days = 30}) async {
    try {
      return await _api.getStatistics(days: days);
    } catch (e) {
      throw Exception('Failed to fetch alarm statistics: $e');
    }
  }

  Future<void> generateSampleAlarms() async {
    try {
      await _api.generateSampleAlarms();
    } catch (e) {
      throw Exception('Failed to generate sample alarms: $e');
    }
  }
}
