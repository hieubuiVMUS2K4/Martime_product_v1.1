import 'package:hive/hive.dart';
import '../network/network_info.dart';
import '../../data/models/sync_item.dart';
import '../di/service_locator.dart';
import '../../data/repositories/task_repository.dart';

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
    print('🔄 Syncing item ${item.id} of type ${item.type}');
    
    try {
      final taskRepository = sl<TaskRepository>();
      
      switch (item.type) {
        case SyncItemType.checklistComplete:
          // Sync checklist toggle (complete/uncomplete)
          final data = item.data;
          await taskRepository.completeChecklistItem(
            taskCode: data['taskCode'] as String,
            itemId: data['itemId'] as String,
            readingValue: data['readingValue']?.toDouble(),
            remarks: data['remarks'] as String?,
            isAbnormal: data['isAbnormal'] as bool? ?? false,
            isCompleted: data['isCompleted'] as bool?, // Support toggle
          );
          print('✅ Synced checklist item: ${data['itemId']} (completed: ${data['isCompleted']})');
          break;
          
        case SyncItemType.sparePartsSync:
          // Sync spare parts updates
          final data = item.data;
          await taskRepository.syncSparePartsUsed(
            taskCode: data['taskCode'] as String,
            sparePartsUsed: List<Map<String, dynamic>>.from(
              data['sparePartsUsed'] as List
            ),
          );
          print('✅ Synced spare parts for task: ${data['taskCode']}');
          break;
          
        case SyncItemType.taskComplete:
        case SyncItemType.taskStart:
        case SyncItemType.profileUpdate:
        case SyncItemType.taskSubmit:
        case SyncItemType.deferralCreate:
        case SyncItemType.deferralCancel:
          // TODO: Implement other sync types as needed
          print('📤 Placeholder sync for type: ${item.type}');
          break;
      }
    } catch (e) {
      print('❌ Sync failed for item ${item.id}: $e');
      rethrow; // Let the caller handle retry logic
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
