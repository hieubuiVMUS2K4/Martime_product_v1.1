import 'package:hive/hive.dart';
import '../network/network_info.dart';
import '../../data/models/sync_item.dart';

class SyncQueue {
  static const String _syncBox = 'sync_queue';
  final NetworkInfo _networkInfo;
  
  SyncQueue(this._networkInfo);
  
  // Initialize
  Future<void> init() async {
    await Hive.openBox<SyncItem>(_syncBox);
  }
  
  // Add item to sync queue (when offline)
  Future<void> addToQueue(SyncItem item) async {
    final box = Hive.box<SyncItem>(_syncBox);
    await box.add(item);
  }
  
  // Get all pending items
  Future<List<SyncItem>> getPendingItems() async {
    final box = Hive.box<SyncItem>(_syncBox);
    return box.values.toList();
  }
  
  // Process sync queue when online
  Future<void> processSyncQueue() async {
    if (!await _networkInfo.isConnected) {
      return;
    }
    
    final box = Hive.box<SyncItem>(_syncBox);
    final items = box.values.toList();
    
    for (var item in items) {
      try {
        // Send to server based on item type
        await _syncItemToServer(item);
        
        // Remove from queue after successful sync
        final key = box.keys.firstWhere((k) => box.get(k) == item);
        await box.delete(key);
      } catch (e) {
        // Keep in queue if sync fails
        print('Sync failed for item ${item.id}: $e');
        
        // Increment retry count
        item.retryCount++;
        await item.save();
      }
    }
  }
  
  Future<void> _syncItemToServer(SyncItem item) async {
    // TODO: Implement sync logic based on item type
    switch (item.type) {
      case SyncItemType.taskComplete:
        // Call complete task API
        break;
      case SyncItemType.taskStart:
        // Call start task API
        break;
      case SyncItemType.profileUpdate:
        // Call profile update API
        break;
    }
  }
  
  // Get queue size
  Future<int> getQueueSize() async {
    final box = Hive.box<SyncItem>(_syncBox);
    return box.length;
  }
  
  // Clear queue
  Future<void> clearQueue() async {
    final box = Hive.box<SyncItem>(_syncBox);
    await box.clear();
  }
  
  // Remove specific item
  Future<void> removeItem(String itemId) async {
    final box = Hive.box<SyncItem>(_syncBox);
    final key = box.keys.firstWhere(
      (k) => (box.get(k) as SyncItem).id == itemId,
    );
    await box.delete(key);
  }
}
