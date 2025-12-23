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
class TaskProvider with ChangeNotifier {
  late final TaskRepository _taskRepository;
  late final NetworkInfo _networkInfo;
  
  List<MaintenanceTask> _tasks = [];
  bool _isLoading = false;
  String? _error;
  
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
  
  // Tab "Đến hạn" - chỉ hiện tasks có status DUE
  List<MaintenanceTask> get dueTasks =>
      _tasks.where((t) => t.isDue).toList();
  
  // Scheduled tasks only
  List<MaintenanceTask> get scheduledTasks =>
      _tasks.where((t) => t.isScheduled).toList();
  
  // Alias for backward compatibility with home_screen.dart
  List<MaintenanceTask> get pendingTasks =>
      _tasks.where((t) => t.isDue || t.isScheduled || t.isOverdueStatus).toList();
  
  List<MaintenanceTask> get inProgressTasks =>
      _tasks.where((t) => t.isInProgress).toList();

    List<MaintenanceTask> get rectifyTasks =>
      _tasks.where((t) => t.isRectify).toList();

    List<MaintenanceTask> get pendingApprovalTasks =>
      _tasks.where((t) => t.isPendingApproval).toList();
  
  List<MaintenanceTask> get completedTasks =>
      _tasks.where((t) => t.isCompleted).toList();
  
  List<MaintenanceTask> get overdueTasks =>
      _tasks.where((t) => t.isOverdueStatus || (t.isOverdue && !t.isCompleted)).toList();
  
  // Fetch tasks from API/Cache - NO MOCK DATA
  Future<void> fetchMyTasks({bool forceRefresh = false}) async {
    _isLoading = true;
    _error = null;
    notifyListeners();
    
    print('🔍 TaskProvider: fetchMyTasks() called');
    
    try {
      print('📡 TaskProvider: Fetching from API...');
      _tasks = await _taskRepository.getMyTasks(forceRefresh: forceRefresh);
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
      
      _isLoading = false;
      notifyListeners();
      print('✅ TaskProvider: fetchMyTasks() completed with ${_tasks.length} tasks');
    } catch (e) {
      print('❌ TaskProvider: Error in fetchMyTasks: $e');
      _error = e.toString();
      _isLoading = false;
      notifyListeners();
      rethrow; // Throw error instead of using mock data
    }
  }
  
  // Start task
  Future<void> startTask(String taskId) async {
    _isLoading = true;
    notifyListeners();
    
    try {
      await _taskRepository.startTask(taskId);
      // Refresh tasks after starting
      await fetchMyTasks(forceRefresh: true);
      _isLoading = false;
      notifyListeners();
    } catch (e) {
      _error = e.toString();
      _isLoading = false;
      notifyListeners();
      rethrow;
    }
  }
  
  // Submit task for approval
  Future<void> submitTask({
    required String taskId,
    double? runningHours,
    String? sparePartsUsed,
    String? notes,
    List<String>? photoUrls,
  }) async {
    _isLoading = true;
    notifyListeners();
    
    try {
      await _taskRepository.submitTask(
        taskId: taskId,
        completedRunningHours: runningHours,
        sparePartsUsed: sparePartsUsed,
        notes: notes,
        photoUrls: photoUrls,
      );
      // Refresh tasks after completing
      await fetchMyTasks(forceRefresh: true);
      _isLoading = false;
      notifyListeners();
    } catch (e) {
      _error = e.toString();
      _isLoading = false;
      notifyListeners();
      rethrow;
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
      
      _isLoading = false;
      notifyListeners();
    } catch (e) {
      print('❌ TaskProvider: Error fetching checklist: $e');
      _error = e.toString();
      _currentChecklist = [];
      _isLoading = false;
      notifyListeners();
      rethrow;
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

  Future<void> createDeferralRequest(CreateDeferralRequestDto dto) async {
    _isLoading = true;
    notifyListeners();
    try {
      await _taskRepository.createDeferralRequest(dto);
      // Refresh task to show pending deferral status
      await fetchMyTasks(forceRefresh: true);
      _isLoading = false;
      notifyListeners();
    } catch (e) {
      _error = e.toString();
      _isLoading = false;
      notifyListeners();
      rethrow;
    }
  }

  Future<void> cancelDeferralRequest(String id) async {
    _isLoading = true;
    notifyListeners();
    try {
      await _taskRepository.cancelDeferralRequest(id);
      // Refresh task to remove pending deferral status
      await fetchMyTasks(forceRefresh: true);
      _isLoading = false;
      notifyListeners();
    } catch (e) {
      _error = e.toString();
      _isLoading = false;
      notifyListeners();
      rethrow;
    }
  }

  Future<void> cancelPendingDeferralForTask(String taskId) async {
    _isLoading = true;
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
      
      _isLoading = false;
      notifyListeners();
    } catch (e) {
      _error = e.toString();
      _isLoading = false;
      notifyListeners();
      rethrow;
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
