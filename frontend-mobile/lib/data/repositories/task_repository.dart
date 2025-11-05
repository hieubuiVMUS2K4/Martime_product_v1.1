import 'package:dio/dio.dart';
import '../../core/network/api_client.dart';
import '../../core/network/network_info.dart';
import '../../core/cache/cache_manager.dart';
import '../../core/cache/sync_queue.dart';
import '../../core/auth/token_storage.dart';
import '../../core/constants/cache_keys.dart';
import '../data_sources/remote/task_api.dart';
import '../models/maintenance_task.dart';
import '../models/task_complete_request.dart';
import '../models/task_checklist_item.dart';
import '../models/task_progress.dart';
import '../models/complete_checklist_item_request.dart';
import '../models/sync_item.dart';

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
        return (cached as List)
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
  Future<MaintenanceTask> getTaskById(int id) async {
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

  /// Start task
  Future<MaintenanceTask> startTask(int taskId) async {
    try {
      if (!await _networkInfo.isConnected) {
        throw Exception('Cannot start task while offline');
      }

      final task = await _taskApi.startTask(taskId);

      // Update cache
      await _updateTaskInCache(task);

      return task;
    } on DioException catch (e) {
      throw Exception('Failed to start task: ${e.message}');
    }
  }

  /// Complete task - Offline-first with sync queue
  Future<void> completeTask({
    required int taskId,
    required String completedBy,
    required String completedByCrewId,
    double? runningHours,
    String? sparePartsUsed,
    String? notes,
    List<String>? photoUrls,
  }) async {
    final request = TaskCompleteRequest(
      completedBy: completedBy,
      completedByCrewId: completedByCrewId,
      completedAt: DateTime.now().toIso8601String(),
      runningHoursAtCompletion: runningHours,
      sparePartsUsed: sparePartsUsed,
      notes: notes,
      photoUrls: photoUrls,
    );

    try {
      if (await _networkInfo.isConnected) {
        // Online: Send immediately
        final task = await _taskApi.completeTask(taskId, request);

        // Update cache
        await _updateTaskInCache(task);
      } else {
        // Offline: Add to sync queue
        await _syncQueue.addToQueue(
          SyncItem(
            type: SyncItemType.taskComplete,
            data: {
              'taskId': taskId,
              ...request.toJson(),
            },
          ),
        );
      }
    } on DioException {
      // On error, add to sync queue
      await _syncQueue.addToQueue(
        SyncItem(
          type: SyncItemType.taskComplete,
          data: {
            'taskId': taskId,
            ...request.toJson(),
          },
        ),
      );
      throw Exception('Task saved offline. Will sync when online');
    }
  }

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
  Future<List<TaskChecklistItem>> getTaskChecklist(int taskId) async {
    try {
      // Check if task has TaskType
      final task = await getTaskById(taskId);
      if (!task.hasTaskType) {
        // Old task without TaskType - return empty list
        return [];
      }

      // Try cache first
      final cacheKey = 'task_checklist_$taskId';
      final cached = await _cacheManager.getData(cacheKey);
      
      if (cached != null && !await _networkInfo.isConnected) {
        print('📦 TaskRepository: Loaded checklist from cache for task $taskId');
        return (cached as List)
            .map((json) => TaskChecklistItem.fromJson(json))
            .toList();
      }

      // Fetch from API
      if (await _networkInfo.isConnected) {
        final checklist = await _taskApi.getTaskChecklist(taskId);
        
        print('✅ TaskRepository: API returned ${checklist.length} checklist items for task $taskId');
        
        // Cache the result
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
      final cacheKey = 'task_checklist_$taskId';
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
    required int taskId,
    required int detailId,
    String? measuredValue,
    bool? checkResult,
    String? inspectionNotes,
    String? photoUrl,
    required bool isCompleted, // Keep for offline sync compatibility
  }) async {
    // Parse measuredValue string to double if provided
    double? parsedMeasuredValue;
    if (measuredValue != null && measuredValue.trim().isNotEmpty) {
      parsedMeasuredValue = double.tryParse(measuredValue.trim());
    }

    final request = CompleteChecklistItemRequest(
      measuredValue: parsedMeasuredValue,
      checkResult: checkResult,
      notes: inspectionNotes, // Changed parameter name to match backend
      photoUrl: photoUrl,
      completedBy: 'Crew', // TODO: Get from auth state
    );

    try {
      if (await _networkInfo.isConnected) {
        // Online: Send immediately
        await _taskApi.completeChecklistItem(taskId, detailId, request);
        
        print('✅ TaskRepository: Completed checklist item $detailId for task $taskId');

        // Invalidate checklist cache to force refresh
        final cacheKey = 'task_checklist_$taskId';
        await _cacheManager.clearCache(cacheKey);
        
        // Invalidate progress cache
        final progressKey = 'task_progress_$taskId';
        await _cacheManager.clearCache(progressKey);
      } else {
        // Offline: Add to sync queue
        await _syncQueue.addToQueue(
          SyncItem(
            type: SyncItemType.checklistComplete,
            data: {
              'taskId': taskId,
              'detailId': detailId,
              ...request.toJson(),
            },
          ),
        );
        
        print('💾 TaskRepository: Queued checklist item $detailId for offline sync');
      }
    } on DioException catch (e) {
      print('❌ TaskRepository: Failed to complete checklist item: ${e.message}');
      
      // On error, add to sync queue
      await _syncQueue.addToQueue(
        SyncItem(
          type: SyncItemType.checklistComplete,
          data: {
            'taskId': taskId,
            'detailId': detailId,
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
}
