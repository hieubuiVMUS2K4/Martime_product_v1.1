import 'package:hive/hive.dart';
import '../network/network_info.dart';
import '../../data/models/sync_item.dart';
import '../di/service_locator.dart';
import '../../data/data_sources/remote/task_api.dart';
import '../../data/models/submit_task_dto.dart';
import '../../data/models/start_task_dto.dart';
import '../../data/models/create_deferral_request_dto.dart';
import '../../data/models/update_task_checklist_item_request.dart';

class SyncQueue {
  static const String _syncBox = 'sync_queue';
  static const int _maxRetries = 5; // PERFORMANCE: Limit retries to avoid infinite loops
  final NetworkInfo _networkInfo;
  
  // PERFORMANCE: Cache TaskApi instance to avoid repeated lookups
  TaskApi? _taskApi;
  TaskApi get taskApi => _taskApi ??= sl<TaskApi>();
  
  // P0 FIX: Mutex flag to prevent race condition when processing queue
  bool _isProcessing = false;
  
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
  
  // Check if currently processing
  bool get isProcessing => _isProcessing;
  
  // PERFORMANCE: Process sync queue with optimized key management
  // P0 FIX: Added mutex to prevent race condition
  Future<void> processSyncQueue() async {
    // P0 FIX: Prevent concurrent processing - race condition protection
    if (_isProcessing) {
      print('⚠️ SyncQueue: Already processing, skipping duplicate call');
      return;
    }
    
    if (!await _networkInfo.isConnected) {
      return;
    }
    
    // Set mutex flag
    _isProcessing = true;
    
    try {
      final box = Hive.box<SyncItem>(_syncBox);
      
      // PERFORMANCE: Get keys and items together to avoid repeated lookups
      final keysToDelete = <dynamic>[];
      final itemsToRetry = <dynamic, SyncItem>{};
      
      // P0 FIX: Create a copy of keys to iterate safely
      final keysList = box.keys.toList();
      
      for (final key in keysList) {
        // P0 FIX: Re-check connection before each item (connectivity can change)
        if (!await _networkInfo.isConnected) {
          print('⚠️ SyncQueue: Lost connectivity, pausing sync');
          break;
        }
        
        final item = box.get(key);
        if (item == null) continue;
        
        // PERFORMANCE: Skip items that exceeded max retries
        if (item.retryCount >= _maxRetries) {
          print('⚠️ Max retries exceeded for ${item.id}, removing from queue');
          keysToDelete.add(key);
          continue;
        }
        
        try {
          await _syncItemToServer(item);
          keysToDelete.add(key); // Mark for deletion on success
          print('✅ SyncQueue: Successfully synced item ${item.id}');
        } catch (e) {
          print('❌ Sync failed for item ${item.id}: $e');
          item.retryCount++;
          itemsToRetry[key] = item;
        }
      }
      
      // PERFORMANCE: Batch delete successful items
      for (final key in keysToDelete) {
        await box.delete(key);
      }
      
      // PERFORMANCE: Batch save retry counts
      for (final entry in itemsToRetry.entries) {
        await entry.value.save();
      }
      
      print('📊 SyncQueue: Processed ${keysToDelete.length} items, ${itemsToRetry.length} failed');
    } finally {
      // P0 FIX: Always release mutex, even on error
      _isProcessing = false;
    }
  }
  
  Future<void> _syncItemToServer(SyncItem item) async {
    print('🔄 Syncing item ${item.id} of type ${item.type}');
    
    try {
      // NOTE: For sync items, call API directly to avoid re-adding to queue on error
      
      switch (item.type) {
        case SyncItemType.checklistComplete:
          // FIXED: Call API directly instead of repository to avoid infinite loop
          final data = item.data;
          final updateRequest = UpdateTaskChecklistItemRequest(
            isCompleted: data['isCompleted'] as bool?,
            completedBy: data['completedBy'] as String?,
            readingValue: data['readingValue']?.toDouble(),
            remarks: data['remarks'] as String?,
            isAbnormal: data['isAbnormal'] as bool?,
          );
          await taskApi.updateChecklistItem(
            data['taskCode'] as String,
            data['itemId'] as String,
            updateRequest,
          );
          print('✅ Synced checklist item: ${data['itemId']} (completed: ${data['isCompleted']})');
          break;
          
        case SyncItemType.sparePartsSync:
          // FIXED: Call API directly for spare parts
          final data = item.data;
          await taskApi.updateSparePartsUsed(
            data['taskCode'] as String,
            {'sparePartsUsed': data['sparePartsUsed']},
          );
          print('✅ Synced spare parts for task: ${data['taskCode']}');
          break;
          
        case SyncItemType.taskSubmit:
          // Sync task submission
          final submitData = item.data;
          await taskApi.submitTask(
            submitData['taskId'] as String,
            SubmitTaskDto(
              taskId: submitData['taskId'] as String,
              notes: submitData['notes'] as String?,
              sparePartsUsed: submitData['sparePartsUsed'] as String?,
              photoUrls: submitData['photoUrls'] != null
                  ? List<String>.from(submitData['photoUrls'] as List)
                  : null,
              completedRunningHours: submitData['completedRunningHours'] != null
                  ? (submitData['completedRunningHours'] as num).toDouble()
                  : null,
            ),
          );
          print('✅ Synced task submit: ${submitData['taskId']}');
          break;

        case SyncItemType.taskStart:
          // Sync task start
          final startData = item.data;
          await taskApi.startTask(
            startData['taskId'] as String,
            StartTaskDto(
              taskId: startData['taskId'] as String,
              currentRunningHours: startData['currentRunningHours'] != null
                  ? (startData['currentRunningHours'] as num).toDouble()
                  : null,
              notes: startData['notes'] as String?,
            ),
          );
          print('✅ Synced task start: ${startData['taskId']}');
          break;

        case SyncItemType.deferralCreate:
          // Sync deferral request creation
          final deferralData = item.data;
          await taskApi.createDeferralRequest(
            CreateDeferralRequestDto(
              taskId: deferralData['taskId'] as String,
              reason: deferralData['reason'] as String,
              proposedDueDate: deferralData['proposedDueDate'] as String,
              priority: deferralData['priority'] as String? ?? 'NORMAL',
              rootCause: deferralData['rootCause'] as String?,
              preventiveMeasures: deferralData['preventiveMeasures'] as String?,
              attachments: deferralData['attachments'] != null
                  ? List<String>.from(deferralData['attachments'] as List)
                  : null,
              classPermissionLetter: deferralData['classPermissionLetter'] as String?,
            ),
          );
          print('✅ Synced deferral create for task: ${deferralData['taskId']}');
          break;

        case SyncItemType.deferralCancel:
          // Sync deferral cancellation
          final cancelData = item.data;
          await taskApi.cancelDeferralRequest(cancelData['id'] as String);
          print('✅ Synced deferral cancel: ${cancelData['id']}');
          break;

        case SyncItemType.taskComplete:
        case SyncItemType.profileUpdate:
          // PERFORMANCE: These types are deprecated - mark as success to remove from queue
          print('⚠️ Removing deprecated sync type: ${item.type}');
          // No throw = success = will be deleted from queue
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
