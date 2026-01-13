import 'dart:async';
import 'package:flutter/material.dart';
import '../../core/cache/sync_queue.dart';
import '../../core/network/network_info.dart';
import '../../core/di/service_locator.dart';

class SyncProvider with ChangeNotifier {
  final SyncQueue _syncQueue;
  final NetworkInfo _networkInfo;
  
  bool _isSyncing = false;
  int _queueSize = 0;
  bool _isOnline = false;
  DateTime? _lastSyncTime;
  
  // PERFORMANCE: Track subscriptions to avoid memory leak
  StreamSubscription? _connectivitySubscription;
  Timer? _periodicSyncTimer;
  
  // Use service locator to avoid duplicate instances
  SyncProvider()
      : _syncQueue = sl<SyncQueue>(),
        _networkInfo = sl<NetworkInfo>() {
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
    _connectivitySubscription = _networkInfo.onConnectivityChanged.listen((result) async {
      // FIX: Await connectivity check before deciding to sync
      await _checkConnectivity();
      await _updateQueueSize();
      
      print('🌐 Connectivity changed: online=$_isOnline, queueSize=$_queueSize');
      
      if (_isOnline && _queueSize > 0) {
        print('📤 Auto-syncing ${_queueSize} pending items...');
        await syncQueue();
      }
    });
    
    // RELIABILITY: Periodic sync check every 30 seconds
    // This ensures sync happens even if connectivity events are missed
    _periodicSyncTimer = Timer.periodic(const Duration(seconds: 30), (timer) async {
      await _checkConnectivity();
      await _updateQueueSize();
      
      if (_isOnline && _queueSize > 0 && !_isSyncing) {
        print('⏰ Periodic sync: found $_queueSize pending items');
        await syncQueue();
      }
    });
    
    // Initial sync attempt if online and have items
    if (_isOnline && _queueSize > 0) {
      print('🚀 Initial sync: $_queueSize pending items');
      await syncQueue();
    }
  }
  
  @override
  void dispose() {
    // PERFORMANCE: Cancel subscriptions to avoid memory leak
    _connectivitySubscription?.cancel();
    _periodicSyncTimer?.cancel();
    super.dispose();
  }
  
  Future<void> _checkConnectivity() async {
    _isOnline = await _networkInfo.isConnected;
    notifyListeners();
  }
  
  Future<void> _updateQueueSize() async {
    _queueSize = await _syncQueue.getQueueSize();
    notifyListeners();
  }
  
  // Sync queue - can be called manually or automatically
  Future<void> syncQueue() async {
    if (_isSyncing || !_isOnline) return;
    
    _isSyncing = true;
    notifyListeners();
    
    try {
      print('📤 Starting sync of $_queueSize items...');
      await _syncQueue.processSyncQueue();
      await _updateQueueSize();
      _lastSyncTime = DateTime.now();
      print('✅ Sync complete. Remaining: $_queueSize items');
    } catch (e) {
      print('❌ Sync error: $e');
    }
    
    _isSyncing = false;
    notifyListeners();
  }
}
