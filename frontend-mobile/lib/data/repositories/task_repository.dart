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
  Future<List<MaintenanceTask>> getMyTasks({bool forceRefresh = false}) async {
    try {
      // If online, fetch from API
      if (await _networkInfo.isConnected && forceRefresh) {
        final tasks = await _taskApi.getMyTasks();

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
        return (cached as List)
            .map((json) => MaintenanceTask.fromJson(json))
            .toList();
      }

      // If no cache and online, fetch from API
      if (await _networkInfo.isConnected) {
        final tasks = await _taskApi.getMyTasks();
        await _cacheManager.saveData(
          CacheKeys.myTasks,
          tasks.map((t) => t.toJson()).toList(),
        );
        return tasks;
      }

      // No cache and offline
      throw Exception('No cached data available. Please connect to internet');
    } on DioException catch (e) {
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
}
