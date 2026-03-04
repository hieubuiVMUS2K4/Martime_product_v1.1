import 'package:hive/hive.dart';
import '../models/watchkeeping_log.dart';

class WatchkeepingRepository {
  static const String _boxName = 'watchkeeping_logs';
  Box<Map>? _box;

  Future<void> _ensureBox() async {
    _box ??= await Hive.openBox<Map>(_boxName);
  }

  Future<void> save(WatchkeepingLog log) async {
    await _ensureBox();
    await _box!.put(log.id, log.toJson());
  }

  Future<void> saveAll(List<WatchkeepingLog> logs) async {
    await _ensureBox();
    final Map<dynamic, Map<String, dynamic>> entries = {};
    for (var log in logs) {
      if (log.id != null) {
        entries[log.id] = log.toJson();
      }
    }
    await _box!.putAll(entries);
  }

  Future<List<WatchkeepingLog>> getAll() async {
    await _ensureBox();
    return _box!.values
        .map((json) => WatchkeepingLog.fromJson(Map<String, dynamic>.from(json)))
        .toList();
  }

  Future<WatchkeepingLog?> getById(int id) async {
    await _ensureBox();
    final json = _box!.get(id);
    if (json == null) return null;
    return WatchkeepingLog.fromJson(Map<String, dynamic>.from(json));
  }

  Future<List<WatchkeepingLog>> getUnsynced() async {
    await _ensureBox();
    return _box!.values
        .map((json) => WatchkeepingLog.fromJson(Map<String, dynamic>.from(json)))
        .where((log) => !log.isSynced)
        .toList();
  }

  Future<void> markAsSynced(int id) async {
    await _ensureBox();
    final json = _box!.get(id);
    if (json != null) {
      final log = WatchkeepingLog.fromJson(Map<String, dynamic>.from(json));
      final updated = WatchkeepingLog(
        id: log.id,
        watchDate: log.watchDate,
        watchPeriod: log.watchPeriod,
        watchType: log.watchType,
        officerOnWatch: log.officerOnWatch,
        lookout: log.lookout,
        weatherConditions: log.weatherConditions,
        seaState: log.seaState,
        visibility: log.visibility,
        courseLogged: log.courseLogged,
        speedLogged: log.speedLogged,
        positionLat: log.positionLat,
        positionLon: log.positionLon,
        distanceRun: log.distanceRun,
        engineStatus: log.engineStatus,
        notableEvents: log.notableEvents,
        masterSignature: log.masterSignature,
        isSynced: true,
        createdAt: log.createdAt,
      );
      await _box!.put(id, updated.toJson());
    }
  }

  Future<void> clear() async {
    await _ensureBox();
    await _box!.clear();
  }
}
