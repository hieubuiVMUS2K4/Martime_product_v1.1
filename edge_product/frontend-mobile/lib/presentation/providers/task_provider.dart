import 'package:flutter/material.dart';
import '../../data/models/maintenance_task.dart';
import '../../data/models/task_checklist_item.dart';
import '../../data/models/task_progress.dart';
import '../../data/models/create_deferral_request_dto.dart';
import '../../data/repositories/task_repository.dart';
import '../../core/network/network_info.dart';
import '../../core/di/service_locator.dart';

/// TaskProvider manages task state and operations.
/// Uses Service Locator for dependency injection to avoid creating
/// duplicate instances of repositories and services.
/// 
/// PERFORMANCE: Uses cached filtered lists to avoid repeated filtering
/// on every getter access.
class TaskProvider with ChangeNotifier {
  late final TaskRepository _taskRepository;
  late final NetworkInfo _networkInfo;
  
  List<MaintenanceTask> _tasks = [];
  bool _isLoading = false;
  String? _error;
  
  // PERFORMANCE: Cached filtered lists - invalidated on task list change
  List<MaintenanceTask>? _cachedNotStartedTasks;
  List<MaintenanceTask>? _cachedDueTasks;
  List<MaintenanceTask>? _cachedScheduledTasks;
  List<MaintenanceTask>? _cachedPendingTasks;
  List<MaintenanceTask>? _cachedInProgressTasks;
  List<MaintenanceTask>? _cachedRectifyTasks;
  List<MaintenanceTask>? _cachedPendingApprovalTasks;
  List<MaintenanceTask>? _cachedCompletedTasks;
  List<MaintenanceTask>? _cachedOverdueTasks;
  List<MaintenanceTask>? _cachedAllActiveTasks;
  
  // Checklist state for currently viewed task
  List<TaskChecklistItem> _currentChecklist = [];
  TaskProgress? _currentProgress;
  
  /// Use Service Locator for proper DI - avoids duplicate instances
  TaskProvider() {
    _networkInfo = sl<NetworkInfo>();
    _taskRepository = sl<TaskRepository>();
  }
  
  /// Constructor for testing with injected dependencies
  TaskProvider.withDependencies({
    required TaskRepository taskRepository,
    required NetworkInfo networkInfo,
  }) : _taskRepository = taskRepository,
       _networkInfo = networkInfo;
  
  List<MaintenanceTask> get tasks => _tasks;
  bool get isLoading => _isLoading;
  String? get error => _error;
  
  /// Check if device is currently online
  Future<bool> isOnline() async {
    return await _networkInfo.isConnected;
  }
  List<TaskChecklistItem> get currentChecklist => _currentChecklist;
  TaskProgress? get currentProgress => _currentProgress;
  
  /// PERFORMANCE: Invalidate all cached filtered lists
  void _invalidateFilterCaches() {
    _cachedNotStartedTasks = null;
    _cachedDueTasks = null;
    _cachedScheduledTasks = null;
    _cachedPendingTasks = null;
    _cachedInProgressTasks = null;
    _cachedRectifyTasks = null;
    _cachedPendingApprovalTasks = null;
    _cachedCompletedTasks = null;
    _cachedOverdueTasks = null;
    _cachedAllActiveTasks = null;
  }
  
  // Tab "Chưa bắt đầu" - UPCOMING + SCHEDULED + MISSING_* (CACHED)
  List<MaintenanceTask> get notStartedTasks {
    _cachedNotStartedTasks ??= _tasks.where((t) => t.isNotStarted).toList();
    return _cachedNotStartedTasks!;
  }
  
  // Tab "Tất cả" - All active (non-completed, non-cancelled) tasks (CACHED)
  List<MaintenanceTask> get allActiveTasks {
    _cachedAllActiveTasks ??= _tasks.where((t) => !t.isCompleted && !t.isCancelled).toList();
    return _cachedAllActiveTasks!;
  }
  
  // Tab "Đến hạn" - chỉ hiện tasks có status DUE (CACHED)
  List<MaintenanceTask> get dueTasks {
    _cachedDueTasks ??= _tasks.where((t) => t.isDue).toList();
    return _cachedDueTasks!;
  }
  
  // Scheduled tasks only (CACHED)
  List<MaintenanceTask> get scheduledTasks {
    _cachedScheduledTasks ??= _tasks.where((t) => t.isScheduled).toList();
    return _cachedScheduledTasks!;
  }
  
  // Alias for backward compatibility with home_screen.dart (CACHED)
  List<MaintenanceTask> get pendingTasks {
    _cachedPendingTasks ??= _tasks.where((t) => t.isDue || t.isScheduled || t.isOverdueStatus).toList();
    return _cachedPendingTasks!;
  }
  
  // In progress tasks (CACHED)
  List<MaintenanceTask> get inProgressTasks {
    _cachedInProgressTasks ??= _tasks.where((t) => t.isInProgress).toList();
    return _cachedInProgressTasks!;
  }

  // Rectify tasks (CACHED)
  List<MaintenanceTask> get rectifyTasks {
    _cachedRectifyTasks ??= _tasks.where((t) => t.isRectify).toList();
    return _cachedRectifyTasks!;
  }

  // Pending approval tasks (CACHED)
  List<MaintenanceTask> get pendingApprovalTasks {
    _cachedPendingApprovalTasks ??= _tasks.where((t) => t.isPendingApproval).toList();
    return _cachedPendingApprovalTasks!;
  }
  
  // Completed tasks (CACHED)
  List<MaintenanceTask> get completedTasks {
    _cachedCompletedTasks ??= _tasks.where((t) => t.isCompleted).toList();
    return _cachedCompletedTasks!;
  }
  
  // Overdue tasks (CACHED)
  List<MaintenanceTask> get overdueTasks {
    _cachedOverdueTasks ??= _tasks.where((t) => t.isOverdueStatus || (t.isOverdue && !t.isCompleted)).toList();
    return _cachedOverdueTasks!;
  }
  
  // Fetch tasks from API/Cache - NO MOCK DATA
  // PERFORMANCE: Batched notifyListeners to reduce widget rebuilds
  Future<void> fetchMyTasks({bool forceRefresh = false}) async {
    _isLoading = true;
    _error = null;
    notifyListeners();
    
    print('🔍 TaskProvider: fetchMyTasks() called');
    
    try {
      print('📡 TaskProvider: Fetching from API...');
      _tasks = await _taskRepository.getMyTasks(forceRefresh: forceRefresh);
      
      // PERFORMANCE: Invalidate cached filtered lists after fetching new data
      _invalidateFilterCaches();
      
      print('✅ TaskProvider: API returned ${_tasks.length} tasks');
      
      // DEBUG: Print task statuses
      final statusCounts = <String, int>{};
      for (var task in _tasks) {
        statusCounts[task.status] = (statusCounts[task.status] ?? 0) + 1;
      }
      print('📊 Task Status Breakdown:');
      statusCounts.forEach((status, count) {
        print('   - $status: $count tasks');
      });
      print('📋 DUE tasks: ${dueTasks.length}');
      print('⏰ OVERDUE tasks: ${overdueTasks.length}');
    } catch (e) {
      print('❌ TaskProvider: Error in fetchMyTasks: $e');
      _error = e.toString();
      rethrow;
    } finally {
      // PERFORMANCE: Single notifyListeners at end regardless of success/failure
      _isLoading = false;
      notifyListeners();
      print('✅ TaskProvider: fetchMyTasks() completed with ${_tasks.length} tasks');
    }
  }
  
  // Start task - PERFORMANCE: Batched notifyListeners
  Future<void> startTask(String taskId) async {
    _isLoading = true;
    _error = null;
    notifyListeners();
    
    try {
      await _taskRepository.startTask(taskId);
      // Refresh tasks after starting
      await fetchMyTasks(forceRefresh: true);
    } catch (e) {
      _error = e.toString();
      rethrow;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }
  
  // Submit task for approval - PERFORMANCE: Batched notifyListeners
  Future<void> submitTask({
    required String taskId,
    double? runningHours,
    String? sparePartsUsed,
    String? notes,
  }) async {
    _isLoading = true;
    _error = null;
    notifyListeners();
    
    try {
      await _taskRepository.submitTask(
        taskId: taskId,
        completedRunningHours: runningHours,
        sparePartsUsed: sparePartsUsed,
        notes: notes,
      );
      // Refresh tasks after completing
      await fetchMyTasks(forceRefresh: true);
    } catch (e) {
      _error = e.toString();
      rethrow;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<Map<String, dynamic>> fetchTaskDetails(String id) async {
    try {
      return await _taskRepository.getTaskDetails(id);
    } catch (e) {
      print('Error fetching task details: $e');
      rethrow;
    }
  }

  // ========== NEW: TaskType Checklist Methods ==========

  /// Fetch task checklist with execution status
  /// PERFORMANCE: Batched notifyListeners
  Future<void> fetchTaskChecklist(String taskCode) async {
    _isLoading = true;
    _error = null;
    notifyListeners();
    
    print('🔍 TaskProvider: fetchTaskChecklist() for task $taskCode');
    
    try {
      _currentChecklist = await _taskRepository.getTaskChecklist(taskCode);
      print('✅ TaskProvider: Loaded ${_currentChecklist.length} checklist items');
      
      // Also fetch progress
      // Legacy progress endpoint is not used for new checklist system.
    } catch (e) {
      print('❌ TaskProvider: Error fetching checklist: $e');
      _error = e.toString();
      _currentChecklist = [];
      rethrow;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  /// Complete/toggle a checklist item
  /// Updates local state immediately for instant UI feedback
  Future<void> completeChecklistItem({
    required String taskCode,
    required String itemId,
    double? readingValue,
    String? remarks,
    bool isAbnormal = false,
    bool? isCompleted, // null = complete, true/false = explicit state
  }) async {
    try {
      // Repository will update cache optimistically AND sync to server
      await _taskRepository.completeChecklistItem(
        taskCode: taskCode,
        itemId: itemId,
        readingValue: readingValue,
        remarks: remarks,
        isAbnormal: isAbnormal,
        isCompleted: isCompleted,
      );
      
      print('✅ TaskProvider: Updated checklist item $itemId (completed: $isCompleted)');
      
      // Don't refresh here - let the caller manage local state
      // This avoids unnecessary notifyListeners() calls
    } catch (e) {
      print('❌ TaskProvider: Error updating checklist item: $e');
      _error = e.toString();
      notifyListeners();
      rethrow;
    }
  }

  /// Fetch task progress
  Future<void> fetchTaskProgress(int taskId) async {
    try {
      _currentProgress = await _taskRepository.getTaskProgress(taskId);
      print('✅ TaskProvider: Progress ${_currentProgress?.completionPercentage.toStringAsFixed(1)}%');
      notifyListeners();
    } catch (e) {
      print('❌ TaskProvider: Error fetching progress: $e');
      _currentProgress = null;
    }
  }

  /// Check if task can be completed (all mandatory items done)
  bool canCompleteTask() {
    if (_currentProgress == null) return false;
    return _currentProgress!.canComplete;
  }

  /// Clear current checklist state
  void clearCurrentChecklist() {
    _currentChecklist = [];
    _currentProgress = null;
    notifyListeners();
  }

  // ========== DEFERRALS ==========

  /// PERFORMANCE: Batched notifyListeners
  Future<void> createDeferralRequest(CreateDeferralRequestDto dto) async {
    _isLoading = true;
    _error = null;
    notifyListeners();
    try {
      await _taskRepository.createDeferralRequest(dto);
      // Refresh task to show pending deferral status
      await fetchMyTasks(forceRefresh: true);
    } catch (e) {
      _error = e.toString();
      rethrow;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  /// PERFORMANCE: Batched notifyListeners
  Future<void> cancelDeferralRequest(String id) async {
    _isLoading = true;
    _error = null;
    notifyListeners();
    try {
      await _taskRepository.cancelDeferralRequest(id);
      // Refresh task to remove pending deferral status
      await fetchMyTasks(forceRefresh: true);
    } catch (e) {
      _error = e.toString();
      rethrow;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  /// PERFORMANCE: Batched notifyListeners
  Future<void> cancelPendingDeferralForTask(String taskId) async {
    _isLoading = true;
    _error = null;
    notifyListeners();
    try {
      // 1. Find the pending request
      final response = await _taskRepository.getDeferralRequests(
        status: 'PENDING',
        taskId: taskId,
        page: 1,
        pageSize: 1,
      );
      
      final items = response['items'] as List?;
      if (items == null || items.isEmpty) {
        throw Exception('DEFERRAL_NOT_FOUND');
      }
      
      final requestId = items[0]['id'];
      
      // 2. Cancel it
      await _taskRepository.cancelDeferralRequest(requestId);
      
      // 3. Refresh task
      await fetchMyTasks(forceRefresh: true);
    } catch (e) {
      _error = e.toString();
      rethrow;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  /// Fetch available materials from inventory for spare parts selection
  Future<List<Map<String, dynamic>>> fetchAvailableMaterials({String? search}) async {
    try {
      return await _taskRepository.getAvailableMaterials(
        search: search,
        onlyInStock: true,
      );
    } catch (e) {
      _error = e.toString();
      return [];
    }
  }
}
