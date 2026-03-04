import 'package:flutter/foundation.dart';
import 'package:dio/dio.dart';
import '../data/api/watchkeeping_api.dart';
import '../data/models/watchkeeping_log.dart';
import '../data/repositories/watchkeeping_repository.dart';

class WatchkeepingProvider with ChangeNotifier {
  final WatchkeepingApi _api;
  final WatchkeepingRepository _repository;

  List<WatchkeepingLog> _logs = [];
  List<WatchkeepingLog> get logs => _logs;

  bool _isLoading = false;
  bool get isLoading => _isLoading;

  String? _error;
  String? get error => _error;

  String _filterPeriod = 'all'; // all, 00-04, 04-08, 08-12, 12-16, 16-20, 20-24
  String get filterPeriod => _filterPeriod;

  String _filterType = 'all'; // all, NAVIGATION, ENGINE
  String get filterType => _filterType;

  WatchkeepingProvider({
    WatchkeepingApi? api,
    WatchkeepingRepository? repository,
  })  : _api = api ?? WatchkeepingApi(_createDio()),
        _repository = repository ?? WatchkeepingRepository();

  static Dio _createDio() {
    final dio = Dio(BaseOptions(
      baseUrl: 'http://localhost:5000',
      connectTimeout: const Duration(seconds: 5),
      receiveTimeout: const Duration(seconds: 3),
    ));
    return dio;
  }

  Future<void> fetchActiveLogs() async {
    _setLoading(true);
    try {
      _logs = await _api.getActiveWatchLogs();
      await _repository.saveAll(_logs);
      _error = null;
    } catch (e) {
      _error = 'Failed to load watch logs: $e';
      // Load from local storage on error
      _logs = await _repository.getAll();
    } finally {
      _setLoading(false);
    }
  }

  Future<void> fetchHistory(int days) async {
    _setLoading(true);
    try {
      _logs = await _api.getWatchHistory(days);
      await _repository.saveAll(_logs);
      _error = null;
    } catch (e) {
      _error = 'Failed to load watch history: $e';
      _logs = await _repository.getAll();
    } finally {
      _setLoading(false);
    }
  }

  Future<WatchkeepingLog?> getLogById(int id) async {
    try {
      return await _api.getWatchLogById(id);
    } catch (e) {
      _error = 'Failed to load watch log: $e';
      return _repository.getById(id);
    }
  }

  Future<bool> createLog(WatchkeepingLog log) async {
    try {
      final created = await _api.createWatchLog(log);
      await _repository.save(created);
      _logs.insert(0, created);
      notifyListeners();
      return true;
    } catch (e) {
      _error = 'Failed to create watch log: $e';
      // Save locally for sync later
      await _repository.save(log);
      _logs.insert(0, log);
      notifyListeners();
      return false;
    }
  }

  Future<bool> signLog(int id, String signature) async {
    try {
      final signed = await _api.signWatchLog(id, {'masterSignature': signature});
      await _repository.save(signed);
      
      final index = _logs.indexWhere((log) => log.id == id);
      if (index != -1) {
        _logs[index] = signed;
        notifyListeners();
      }
      return true;
    } catch (e) {
      _error = 'Failed to sign watch log: $e';
      return false;
    }
  }

  void setFilterPeriod(String period) {
    _filterPeriod = period;
    notifyListeners();
  }

  void setFilterType(String type) {
    _filterType = type;
    notifyListeners();
  }

  List<WatchkeepingLog> get filteredLogs {
    return _logs.where((log) {
      final periodMatch = _filterPeriod == 'all' || log.watchPeriod == _filterPeriod;
      final typeMatch = _filterType == 'all' || log.watchType == _filterType;
      return periodMatch && typeMatch;
    }).toList()
      ..sort((a, b) => b.watchDate.compareTo(a.watchDate));
  }

  Map<String, List<WatchkeepingLog>> get logsByDate {
    final Map<String, List<WatchkeepingLog>> grouped = {};
    
    for (var log in filteredLogs) {
      final dateKey = '${log.watchDate.year}-${log.watchDate.month.toString().padLeft(2, '0')}-${log.watchDate.day.toString().padLeft(2, '0')}';
      if (!grouped.containsKey(dateKey)) {
        grouped[dateKey] = [];
      }
      grouped[dateKey]!.add(log);
    }
    
    return grouped;
  }

  int get navigationLogsCount =>
      _logs.where((log) => log.watchType == 'NAVIGATION').length;

  int get engineLogsCount =>
      _logs.where((log) => log.watchType == 'ENGINE').length;

  int get unsignedLogsCount =>
      _logs.where((log) => !log.isSigned).length;

  void _setLoading(bool value) {
    _isLoading = value;
    notifyListeners();
  }

  Future<void> syncUnsyncedLogs() async {
    final unsynced = await _repository.getUnsynced();
    for (var log in unsynced) {
      try {
        await _api.createWatchLog(log);
        await _repository.markAsSynced(log.id!);
      } catch (e) {
        // Continue with next log
        continue;
      }
    }
  }
}
