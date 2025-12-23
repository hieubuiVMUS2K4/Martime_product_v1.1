import 'package:dio/dio.dart';
import 'dart:convert';
import '../../core/network/api_client.dart';
import '../../core/network/network_info.dart';
import '../../core/cache/cache_manager.dart';
import '../../core/cache/sync_queue.dart';
import '../../core/auth/token_storage.dart';
import '../../core/constants/cache_keys.dart';
import '../data_sources/remote/task_api.dart';
import '../models/maintenance_task.dart';
import '../models/task_checklist_item.dart';
import '../models/task_progress.dart';
import '../models/sync_item.dart';
import '../models/start_task_dto.dart';
import '../models/submit_task_dto.dart';
import '../models/complete_task_checklist_item_request.dart';
import '../models/update_task_checklist_item_request.dart';
import '../models/create_deferral_request_dto.dart';

class TaskRepository {
  final ApiClient _apiClient;
  final NetworkInfo _networkInfo;
  final CacheManager _cacheManager;
  final SyncQueue _syncQueue;
  final TokenStorage _tokenStorage;
  late final TaskApi _taskApi;

  TaskRepository({
    required ApiClient apiClient,
    required NetworkInfo networkInfo,
    required CacheManager cacheManager,
    required SyncQueue syncQueue,
    required TokenStorage tokenStorage,
  })  : _apiClient = apiClient,
        _networkInfo = networkInfo,
        _cacheManager = cacheManager,
        _syncQueue = syncQueue,
        _tokenStorage = tokenStorage {
    _taskApi = TaskApi(_apiClient.dio);
  }

  /// Get my tasks - Offline-first
  /// Chỉ lấy tasks được giao cho crew member đang đăng nhập
  Future<List<MaintenanceTask>> getMyTasks({bool forceRefresh = false}) async {
    try {
      // Lấy crew_id của user đang đăng nhập
      final crewId = await _tokenStorage.getCrewId();
      
      if (crewId == null || crewId.isEmpty) {
        throw Exception('User not logged in or no crew ID found');
      }

      print('🔍 TaskRepository: Fetching tasks for crew: $crewId');

      // If online, fetch from API
      if (await _networkInfo.isConnected && forceRefresh) {
        final tasks = await _taskApi.getMyTasks(
          crewId: crewId,
          includeCompleted: true, // Lấy tất cả để cache
        );

        print('✅ TaskRepository: API returned ${tasks.length} tasks for $crewId');

        // Cache the result
        await _cacheManager.saveData(
          CacheKeys.myTasks,
          tasks.map((t) => t.toJson()).toList(),
        );

        return tasks;
      }

      // Try to load from cache
      final cached = await _cacheManager.getData(CacheKeys.myTasks);
      if (cached != null) {
        print('📦 TaskRepository: Loaded ${(cached as List).length} tasks from cache');
        return (cached)
            .map((json) => MaintenanceTask.fromJson(json))
            .toList();
      }

      // If no cache and online, fetch from API
      if (await _networkInfo.isConnected) {
        final tasks = await _taskApi.getMyTasks(
          crewId: crewId,
          includeCompleted: true,
        );
        
        print('✅ TaskRepository: API returned ${tasks.length} tasks for $crewId');
        
        await _cacheManager.saveData(
          CacheKeys.myTasks,
          tasks.map((t) => t.toJson()).toList(),
        );
        return tasks;
      }

      // No cache and offline
      throw Exception('No cached data available. Please connect to internet');
    } on DioException catch (e) {
      print('❌ TaskRepository: API error: ${e.message}');
      // On API error, try to return cached data
      final cached = await _cacheManager.getData(CacheKeys.myTasks);
      if (cached != null) {
        return (cached as List)
            .map((json) => MaintenanceTask.fromJson(json))
            .toList();
      }
      throw Exception('Failed to fetch tasks: ${e.message}');
    }
  }

  /// Get task by ID
  Future<MaintenanceTask> getTaskById(String id) async {
    try {
      if (!await _networkInfo.isConnected) {
        // Try to find in cached tasks
        final cached = await _cacheManager.getData(CacheKeys.myTasks);
        if (cached != null) {
          final tasks = (cached as List)
              .map((json) => MaintenanceTask.fromJson(json))
              .toList();
          return tasks.firstWhere((t) => t.id == id);
        }
        throw Exception('No cached data available');
      }

      return await _taskApi.getTaskById(id);
    } catch (e) {
      throw Exception('Failed to fetch task: $e');
    }
  }

  /// Get task details with full workflow info
  Future<Map<String, dynamic>> getTaskDetails(String id) async {
    try {
      if (!await _networkInfo.isConnected) {
        throw Exception('Cannot fetch task details while offline');
      }
      final response = await _taskApi.getTaskDetails(id);
      return response.data as Map<String, dynamic>;
    } catch (e) {
      throw Exception('Failed to fetch task details: $e');
    }
  }

  /// Start task
  Future<void> startTask(String taskId) async {
    try {
      if (!await _networkInfo.isConnected) {
        throw Exception('Cannot start task while offline');
      }

      final dto = StartTaskDto(taskId: taskId);
      await _taskApi.startTask(taskId, dto);

      // Invalidate local caches (task will be refreshed by caller)
      await _cacheManager.clearCache('task_checklist_${_safeCacheKey(taskId)}');
      await _cacheManager.clearCache('task_progress_${_safeCacheKey(taskId)}');
    } on DioException catch (e) {
      throw Exception('Failed to start task: ${e.message}');
    }
  }

  /// Submit task for approval (PMS Workflow v2.0)
  Future<void> submitTask({
    required String taskId,
    String? notes,
    String? sparePartsUsed,
    List<String>? photoUrls,
    double? completedRunningHours,
  }) async {
    final dto = SubmitTaskDto(
      taskId: taskId,
      notes: notes,
      sparePartsUsed: sparePartsUsed,
      photoUrls: photoUrls,
      completedRunningHours: completedRunningHours,
    );

    try {
      if (await _networkInfo.isConnected) {
        // Online: Send immediately
        await _taskApi.submitTask(taskId, dto);
      } else {
        // Offline: Add to sync queue
        await _syncQueue.addToQueue(
          SyncItem(
            type: SyncItemType.taskSubmit,
            data: {
              'taskId': taskId,
              ...dto.toJson(),
            },
          ),
        );
      }
    } on DioException {
      // On error, add to sync queue
      await _syncQueue.addToQueue(
        SyncItem(
          type: SyncItemType.taskSubmit,
          data: {
            'taskId': taskId,
            ...dto.toJson(),
          },
        ),
      );
      throw Exception('Task saved offline. Will sync when online');
    }
  }

  String _safeCacheKey(String raw) => raw.replaceAll(RegExp(r'[^a-zA-Z0-9_-]'), '_');

  /// Get upcoming tasks
  Future<List<MaintenanceTask>> getUpcomingTasks() async {
    try {
      if (!await _networkInfo.isConnected) {
        throw Exception('No internet connection');
      }

      return await _taskApi.getUpcomingTasks();
    } catch (e) {
      throw Exception('Failed to fetch upcoming tasks: $e');
    }
  }

  // ========== NEW: TaskType Checklist System ==========

  /// Get task checklist with execution status
  /// Returns list of TaskChecklistItem (template + execution data)
  Future<List<TaskChecklistItem>> getTaskChecklist(String taskCode) async {
    try {
      // Cache keys
      final safeTaskCode = _safeCacheKey(taskCode);
      final cacheKey = 'task_checklist_$safeTaskCode';
      
      // Try cache first
      final cached = await _cacheManager.getData(cacheKey);
      
      if (cached != null && !await _networkInfo.isConnected) {
        print('📦 TaskRepository: Loaded checklist from cache for task $taskCode');
        return (cached as List)
            .map((json) => TaskChecklistItem.fromJson(json))
            .toList();
      }

      // Fetch from API
      if (await _networkInfo.isConnected) {
        final checklist = await _taskApi.getTaskChecklist(taskCode);
        
        print('✅ TaskRepository: API returned ${checklist.length} checklist items for task $taskCode');
        
        // Cache the result along with state
        await _cacheManager.saveData(
          cacheKey,
          checklist.map((item) => item.toJson()).toList(),
        );
        return checklist;
      }

      // Return cached if available
      if (cached != null) {
        return (cached as List)
            .map((json) => TaskChecklistItem.fromJson(json))
            .toList();
      }

      throw Exception('No cached checklist available. Please connect to internet');
    } on DioException catch (e) {
      print('❌ TaskRepository: Failed to get checklist: ${e.message}');
      
      // Try to return cached data
      final cacheKey = 'task_checklist_${_safeCacheKey(taskCode)}';
      final cached = await _cacheManager.getData(cacheKey);
      if (cached != null) {
        return (cached as List)
            .map((json) => TaskChecklistItem.fromJson(json))
            .toList();
      }
      
      throw Exception('Failed to fetch checklist: ${e.message}');
    }
  }

  /// Toggle a checklist item - Offline-first with optimistic cache update
  /// Supports both completing and uncompleting items
  Future<void> completeChecklistItem({
    required String taskCode,
    required String itemId,
    double? readingValue,
    String? remarks,
    bool isAbnormal = false,
    bool? isCompleted, // null = toggle to complete, true/false = set explicitly
  }) async {
    final crewId = await _tokenStorage.getCrewId();
    if (crewId == null || crewId.isEmpty) {
      throw Exception('No crew ID found');
    }

    // Determine target state - default is complete (true)
    final targetCompleted = isCompleted ?? true;

    // ALWAYS update local cache FIRST for instant UI feedback
    final cacheKey = 'task_checklist_${_safeCacheKey(taskCode)}';
    await _updateChecklistItemInCache(
      cacheKey: cacheKey,
      itemId: itemId,
      isCompleted: targetCompleted,
      completedBy: targetCompleted ? crewId : '',
      readingValue: readingValue,
      remarks: remarks,
      isAbnormal: isAbnormal,
    );
    print('💾 TaskRepository: Updated cache optimistically for item $itemId (completed: $targetCompleted)');

    try {
      if (await _networkInfo.isConnected) {
        // Online: Use updateChecklistItem API to support toggle
        final updateRequest = UpdateTaskChecklistItemRequest(
          isCompleted: targetCompleted,
          completedBy: targetCompleted ? crewId : null,
          readingValue: readingValue,
          remarks: remarks,
          isAbnormal: isAbnormal,
        );
        await _taskApi.updateChecklistItem(taskCode, itemId, updateRequest);
        print('✅ TaskRepository: Updated checklist item $itemId for task $taskCode (completed: $targetCompleted)');
      } else {
        // Offline: Add to sync queue
        await _syncQueue.addToQueue(
          SyncItem(
            type: SyncItemType.checklistComplete,
            data: {
              'taskCode': taskCode,
              'itemId': itemId,
              'isCompleted': targetCompleted,
              'completedBy': targetCompleted ? crewId : null,
              'readingValue': readingValue,
              'remarks': remarks,
              'isAbnormal': isAbnormal,
            },
          ),
        );
        print('💾 TaskRepository: Queued checklist item $itemId for offline sync');
      }
    } on DioException catch (e) {
      print('❌ TaskRepository: Failed to update checklist item: ${e.message}');
      
      // On error, add to sync queue (cache already updated)
      await _syncQueue.addToQueue(
        SyncItem(
          type: SyncItemType.checklistComplete,
          data: {
            'taskCode': taskCode,
            'itemId': itemId,
            'isCompleted': targetCompleted,
            'completedBy': targetCompleted ? crewId : null,
            'readingValue': readingValue,
            'remarks': remarks,
            'isAbnormal': isAbnormal,
          },
        ),
      );
      
      // Don't throw - cache is already updated, will sync later
      print('💾 TaskRepository: Saved to sync queue, will retry later');
    }
  }

  /// Helper to update a single checklist item in cache
  Future<void> _updateChecklistItemInCache({
    required String cacheKey,
    required String itemId,
    required bool isCompleted,
    required String completedBy,
    double? readingValue,
    String? remarks,
    bool isAbnormal = false,
  }) async {
    final cached = await _cacheManager.getData(cacheKey);
    if (cached == null) return;

    try {
      final items = (cached as List).map((json) => 
        TaskChecklistItem.fromJson(json as Map<String, dynamic>)
      ).toList();

      // Find and update the item
      for (int i = 0; i < items.length; i++) {
        if (items[i].id == itemId) {
          items[i] = items[i].copyWith(
            isCompleted: isCompleted,
            completedAt: DateTime.now().toIso8601String(),
            completedBy: completedBy,
            readingValue: readingValue,
            remarks: remarks,
            isAbnormal: isAbnormal,
          );
          break;
        }
      }

      // Save updated cache
      await _cacheManager.saveData(
        cacheKey,
        items.map((item) => item.toJson()).toList(),
      );
    } catch (e) {
      print('⚠️ TaskRepository: Error updating cache: $e');
    }
  }

  /// Get task progress (completion percentage)
  Future<TaskProgress> getTaskProgress(int taskId) async {
    try {
      // Try cache first
      final cacheKey = 'task_progress_$taskId';
      final cached = await _cacheManager.getData(cacheKey);
      
      if (cached != null && !await _networkInfo.isConnected) {
        print('📦 TaskRepository: Loaded progress from cache for task $taskId');
        return TaskProgress.fromJson(cached);
      }

      // Fetch from API
      if (await _networkInfo.isConnected) {
        final progress = await _taskApi.getTaskProgress(taskId);
        
        print('✅ TaskRepository: Progress for task $taskId: ${progress.completionPercentage.toStringAsFixed(1)}%');
        
        // Cache the result
        await _cacheManager.saveData(cacheKey, progress.toJson());
        
        return progress;
      }

      // Return cached if available
      if (cached != null) {
        return TaskProgress.fromJson(cached);
      }

      throw Exception('No cached progress available. Please connect to internet');
    } on DioException catch (e) {
      print('❌ TaskRepository: Failed to get progress: ${e.message}');
      
      // Try to return cached data
      final cacheKey = 'task_progress_$taskId';
      final cached = await _cacheManager.getData(cacheKey);
      if (cached != null) {
        return TaskProgress.fromJson(cached);
      }
      
      throw Exception('Failed to fetch progress: ${e.message}');
    }
  }

  // === DEFERRALS ===
  
  Future<void> createDeferralRequest(CreateDeferralRequestDto dto) async {
    try {
      print('📡 TaskRepository: Starting deferral request...');
      print('   TaskId: ${dto.taskId}');
      print('   Attachments: ${dto.attachments?.length ?? 0} photos');
      
      if (await _networkInfo.isConnected) {
        final response = await _taskApi.createDeferralRequest(dto);
        print('✅ TaskRepository: Deferral created - Response: ${response.response.statusCode}');
        print('   Response body: ${response.data}');
      } else {
        print('❌ TaskRepository: No network, queueing for offline sync');
        // Offline: Add to sync queue
        await _syncQueue.addToQueue(
          SyncItem(
            type: SyncItemType.deferralCreate,
            data: dto.toJson(),
          ),
        );
        print('💾 TaskRepository: Queued deferral request for offline sync');
      }
    } on DioException catch (e) {
      print('❌ TaskRepository: DioException creating deferral');
      print('   Status: ${e.response?.statusCode}');
      print('   Message: ${e.message}');
      print('   Response: ${e.response?.data}');
      
      // On error, add to sync queue
      await _syncQueue.addToQueue(
        SyncItem(
          type: SyncItemType.deferralCreate,
          data: dto.toJson(),
        ),
      );
      
      // Re-throw with better message
      if (e.response?.statusCode == 400) {
        final errorMsg = e.response?.data?['error'] ?? 'Validation failed';
        throw Exception('Backend validation: $errorMsg');
      } else if (e.type == DioExceptionType.connectionTimeout || e.type == DioExceptionType.receiveTimeout) {
        throw Exception('Request timeout - ảnh có thể quá lớn');
      } else {
        throw Exception('Network error: ${e.message}');
      }
    } catch (e) {
      print('❌ TaskRepository: Unexpected error: $e');
      rethrow;
    }
  }

  Future<Map<String, dynamic>> getDeferralRequests({
    String? status,
    String? taskId,
    int? page,
    int? pageSize,
  }) async {
    try {
      if (await _networkInfo.isConnected) {
        final response = await _taskApi.getDeferralRequests(
          status: status,
          taskId: taskId,
          page: page,
          pageSize: pageSize,
        );
        return response.data as Map<String, dynamic>;
      }
      // Offline support for viewing deferrals is limited or cached
      throw Exception('Cannot view deferral requests while offline');
    } catch (e) {
      throw Exception('Failed to fetch deferral requests: $e');
    }
  }

  Future<void> cancelDeferralRequest(String id) async {
    try {
      if (await _networkInfo.isConnected) {
        await _taskApi.cancelDeferralRequest(id);
        print('✅ TaskRepository: Cancelled deferral request $id');
      } else {
        // Offline: Add to sync queue
        await _syncQueue.addToQueue(
          SyncItem(
            type: SyncItemType.deferralCancel,
            data: {'id': id},
          ),
        );
        print('💾 TaskRepository: Queued deferral cancellation for offline sync');
      }
    } on DioException catch (e) {
      print('❌ TaskRepository: Failed to cancel deferral request: ${e.message}');
      // On error, add to sync queue
      await _syncQueue.addToQueue(
        SyncItem(
          type: SyncItemType.deferralCancel,
          data: {'id': id},
        ),
      );
      throw Exception('Cancellation saved offline. Will sync when online');
    }
  }

  // ========== MATERIALS ==========
  
  /// Get available materials from inventory for spare parts selection
  Future<List<Map<String, dynamic>>> getAvailableMaterials({
    String? search,
    bool onlyInStock = true,
  }) async {
    final cacheKey = 'available_materials';
    
    try {
      // Try cache first for offline support
      final cached = await _cacheManager.getData(cacheKey);
      
      if (!await _networkInfo.isConnected) {
        if (cached != null) {
          print('📦 TaskRepository: Loaded materials from cache');
          return List<Map<String, dynamic>>.from(cached as List);
        }
        throw Exception('No cached materials available. Please connect to internet');
      }

      // Fetch from API
      final response = await _apiClient.dio.get(
        '/api/material/items',
        queryParameters: {
          'onlyActive': true,
          if (search != null && search.isNotEmpty) 'q': search,
        },
      );

      if (response.statusCode == 200) {
        final items = List<Map<String, dynamic>>.from(response.data as List);
        
        // Filter by stock if needed
        final filtered = onlyInStock
            ? items.where((item) => (item['onHandQuantity'] ?? 0) > 0).toList()
            : items;
        
        print('✅ TaskRepository: Loaded ${filtered.length} materials from API');
        
        // Cache for offline use
        await _cacheManager.saveDataForOffline(cacheKey, items);
        
        return filtered;
      }
      
      throw Exception('Failed to fetch materials');
    } catch (e) {
      print('❌ TaskRepository: Error fetching materials: $e');
      
      // Return cached data if available
      final cached = await _cacheManager.getData(cacheKey);
      if (cached != null) {
        final items = List<Map<String, dynamic>>.from(cached as List);
        return onlyInStock
            ? items.where((item) => (item['onHandQuantity'] ?? 0) > 0).toList()
            : items;
      }
      
      rethrow;
    }
  }
  
  /// Sync spare parts used in real-time (for edge frontend visibility)
  Future<void> syncSparePartsUsed({
    required String taskCode,
    required List<Map<String, dynamic>> sparePartsUsed,
  }) async {
    try {
      if (await _networkInfo.isConnected) {
        // Online: Send to server immediately
        final sparePartsData = {
          'sparePartsUsed': json.encode(sparePartsUsed),
          'updatedAt': DateTime.now().toIso8601String(),
        };
        
        await _taskApi.updateSparePartsUsed(taskCode, sparePartsData);
        print('✅ TaskRepository: Synced spare parts for task $taskCode');
      } else {
        // Offline: Add to sync queue
        await _syncQueue.addToQueue(
          SyncItem(
            type: SyncItemType.sparePartsSync,
            data: {
              'taskCode': taskCode,
              'sparePartsUsed': sparePartsUsed,
            },
          ),
        );
        print('💾 TaskRepository: Queued spare parts sync for offline');
      }
    } on DioException catch (e) {
      print('❌ TaskRepository: Failed to sync spare parts: ${e.message}');
      
      // On error, add to sync queue
      await _syncQueue.addToQueue(
        SyncItem(
          type: SyncItemType.sparePartsSync,
          data: {
            'taskCode': taskCode,
            'sparePartsUsed': sparePartsUsed,
          },
        ),
      );
      
      // Don't throw - will sync later
      print('💾 TaskRepository: Saved spare parts to sync queue');
    }
  }
}
