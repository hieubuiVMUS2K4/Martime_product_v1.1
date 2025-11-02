import 'package:flutter/material.dart';
import '../../data/models/maintenance_task.dart';
import '../../data/models/task_checklist_item.dart';
import '../../data/models/task_progress.dart';
import '../../data/repositories/task_repository.dart';
import '../../core/network/api_client.dart';
import '../../core/network/network_info.dart';
import '../../core/cache/cache_manager.dart';
import '../../core/cache/sync_queue.dart';
import '../../core/auth/token_storage.dart';

class TaskProvider with ChangeNotifier {
  final TaskRepository _taskRepository;
  
  List<MaintenanceTask> _tasks = [];
  bool _isLoading = false;
  String? _error;
  
  // Checklist state for currently viewed task
  List<TaskChecklistItem> _currentChecklist = [];
  TaskProgress? _currentProgress;
  
  TaskProvider()
      : _taskRepository = TaskRepository(
          apiClient: ApiClient(),
          networkInfo: NetworkInfo(),
          cacheManager: CacheManager(),
          syncQueue: SyncQueue(NetworkInfo()),
          tokenStorage: TokenStorage(),
        );
  
  List<MaintenanceTask> get tasks => _tasks;
  bool get isLoading => _isLoading;
  String? get error => _error;
  List<TaskChecklistItem> get currentChecklist => _currentChecklist;
  TaskProgress? get currentProgress => _currentProgress;
  
  List<MaintenanceTask> get pendingTasks =>
      _tasks.where((t) => t.isPending).toList();
  
  List<MaintenanceTask> get inProgressTasks =>
      _tasks.where((t) => t.isInProgress).toList();
  
  List<MaintenanceTask> get completedTasks =>
      _tasks.where((t) => t.isCompleted).toList();
  
  List<MaintenanceTask> get overdueTasks =>
      _tasks.where((t) => t.isOverdue && !t.isCompleted).toList();
  
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
  Future<void> startTask(int taskId) async {
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
  
  // Complete task
  Future<void> completeTask({
    required int taskId,
    required String completedBy,
    required String completedByCrewId,
    double? runningHours,
    String? sparePartsUsed,
    String? notes,
  }) async {
    _isLoading = true;
    notifyListeners();
    
    try {
      await _taskRepository.completeTask(
        taskId: taskId,
        completedBy: completedBy,
        completedByCrewId: completedByCrewId,
        runningHours: runningHours,
        sparePartsUsed: sparePartsUsed,
        notes: notes,
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

  // ========== NEW: TaskType Checklist Methods ==========

  /// Fetch task checklist with execution status
  Future<void> fetchTaskChecklist(int taskId) async {
    _isLoading = true;
    _error = null;
    notifyListeners();
    
    print('🔍 TaskProvider: fetchTaskChecklist() for task $taskId');
    
    try {
      _currentChecklist = await _taskRepository.getTaskChecklist(taskId);
      print('✅ TaskProvider: Loaded ${_currentChecklist.length} checklist items');
      
      // Also fetch progress
      await fetchTaskProgress(taskId);
      
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

  /// Complete a checklist item
  Future<void> completeChecklistItem({
    required int taskId,
    required int detailId,
    String? measuredValue,
    bool? checkResult,
    String? inspectionNotes,
    String? photoUrl,
    required bool isCompleted,
  }) async {
    try {
      await _taskRepository.completeChecklistItem(
        taskId: taskId,
        detailId: detailId,
        measuredValue: measuredValue,
        checkResult: checkResult,
        inspectionNotes: inspectionNotes,
        photoUrl: photoUrl,
        isCompleted: isCompleted,
      );
      
      print('✅ TaskProvider: Completed checklist item $detailId');
      
      // Refresh checklist and progress
      await fetchTaskChecklist(taskId);
    } catch (e) {
      print('❌ TaskProvider: Error completing checklist item: $e');
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
}
