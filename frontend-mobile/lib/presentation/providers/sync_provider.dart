import 'package:flutter/material.dart';
import '../../core/cache/sync_queue.dart';
import '../../core/network/network_info.dart';

class SyncProvider with ChangeNotifier {
  final SyncQueue _syncQueue;
  final NetworkInfo _networkInfo;
  
  bool _isSyncing = false;
  int _queueSize = 0;
  bool _isOnline = false;
  DateTime? _lastSyncTime;
  
  SyncProvider()
      : _syncQueue = SyncQueue(NetworkInfo()),
        _networkInfo = NetworkInfo() {
    _init();
  }
  
  bool get isSyncing => _isSyncing;
  int get queueSize => _queueSize;
  bool get isOnline => _isOnline;
  DateTime? get lastSyncTime => _lastSyncTime;
  
  Future<void> _init() async {
    await _syncQueue.init();
    await _checkConnectivity();
    await _updateQueueSize();
    
    // Listen to connectivity changes
    _networkInfo.onConnectivityChanged.listen((result) {
      _checkConnectivity();
      if (_isOnline && _queueSize > 0) {
        syncQueue();
      }
    });
  }
  
  Future<void> _checkConnectivity() async {
    _isOnline = await _networkInfo.isConnected;
    notifyListeners();
  }
  
  Future<void> _updateQueueSize() async {
    _queueSize = await _syncQueue.getQueueSize();
    notifyListeners();
  }
  
  // Sync queue
  Future<void> syncQueue() async {
    if (_isSyncing || !_isOnline) return;
    
    _isSyncing = true;
    notifyListeners();
    
    try {
      await _syncQueue.processSyncQueue();
      await _updateQueueSize();
      _lastSyncTime = DateTime.now();
    } catch (e) {
      print('Sync error: $e');
    }
    
    _isSyncing = false;
    notifyListeners();
  }
}
