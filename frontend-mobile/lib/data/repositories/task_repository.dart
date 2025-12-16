import 'package:dio/dio.dart';
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

  /// Update single task in cache
  Future<void> _updateTaskInCache(MaintenanceTask task) async {
    try {
      final cached = await _cacheManager.getData(CacheKeys.myTasks);
      if (cached != null) {
        final tasks = (cached as List)
            .map((json) => MaintenanceTask.fromJson(json))
            .toList();

        // Replace or add task
        final index = tasks.indexWhere((t) => t.id == task.id);
        if (index != -1) {
          tasks[index] = task;
        } else {
          tasks.add(task);
        }

        // Save back to cache
        await _cacheManager.saveData(
          CacheKeys.myTasks,
          tasks.map((t) => t.toJson()).toList(),
        );
      }
    } catch (e) {
      // Ignore cache update errors
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

  /// Complete a checklist item - Offline-first with sync queue
  Future<void> completeChecklistItem({
    required String taskCode,
    required String itemId,
    double? readingValue,
    String? remarks,
    bool isAbnormal = false,
  }) async {
    final crewId = await _tokenStorage.getCrewId();
    if (crewId == null || crewId.isEmpty) {
      throw Exception('No crew ID found');
    }

    final request = CompleteTaskChecklistItemRequest(
      completedBy: crewId,
      readingValue: readingValue,
      remarks: remarks,
      isAbnormal: isAbnormal,
    );

    try {
      if (await _networkInfo.isConnected) {
        // Online: Send immediately
        await _taskApi.completeChecklistItem(taskCode, itemId, request);

        print('✅ TaskRepository: Completed checklist item $itemId for task $taskCode');

        // Invalidate checklist cache to force refresh
        final cacheKey = 'task_checklist_${_safeCacheKey(taskCode)}';
        await _cacheManager.clearCache(cacheKey);
      } else {
        // Offline: Add to sync queue
        await _syncQueue.addToQueue(
          SyncItem(
            type: SyncItemType.checklistComplete,
            data: {
              'taskCode': taskCode,
              'itemId': itemId,
              ...request.toJson(),
            },
          ),
        );
        
        print('💾 TaskRepository: Queued checklist item $itemId for offline sync');
      }
    } on DioException catch (e) {
      print('❌ TaskRepository: Failed to complete checklist item: ${e.message}');
      
      // On error, add to sync queue
      await _syncQueue.addToQueue(
        SyncItem(
          type: SyncItemType.checklistComplete,
          data: {
            'taskCode': taskCode,
            'itemId': itemId,
            ...request.toJson(),
          },
        ),
      );
      
      throw Exception('Checklist item saved offline. Will sync when online');
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
      if (await _networkInfo.isConnected) {
        await _taskApi.createDeferralRequest(dto);
        print('✅ TaskRepository: Created deferral request for task ${dto.taskId}');
      } else {
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
      print('❌ TaskRepository: Failed to create deferral request: ${e.message}');
      // On error, add to sync queue
      await _syncQueue.addToQueue(
        SyncItem(
          type: SyncItemType.deferralCreate,
          data: dto.toJson(),
        ),
      );
      throw Exception('Deferral request saved offline. Will sync when online');
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
}
