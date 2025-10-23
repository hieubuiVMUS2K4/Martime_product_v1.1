import 'package:flutter/material.dart';
import '../../data/models/maintenance_task.dart';
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
  
  List<MaintenanceTask> get pendingTasks =>
      _tasks.where((t) => t.isPending).toList();
  
  List<MaintenanceTask> get inProgressTasks =>
      _tasks.where((t) => t.isInProgress).toList();
  
  List<MaintenanceTask> get completedTasks =>
      _tasks.where((t) => t.isCompleted).toList();
  
  List<MaintenanceTask> get overdueTasks =>
      _tasks.where((t) => t.isOverdue && !t.isCompleted).toList();
  
  // Fetch tasks from API/Cache
  Future<void> fetchMyTasks({bool forceRefresh = false}) async {
    _isLoading = true;
    _error = null;
    notifyListeners();
    
    print('🔍 TaskProvider: fetchMyTasks() called');
    
    try {
      // ============================================
      // Try to fetch from repository (will return empty if API not ready)
      // ============================================
      try {
        print('📡 TaskProvider: Trying to fetch from API...');
        _tasks = await _taskRepository.getMyTasks(forceRefresh: forceRefresh);
        print('✅ TaskProvider: API returned ${_tasks.length} tasks');
      } catch (e) {
        // If API fails, use mock data for testing UI
        print('⚠️ API not available, using mock data: $e');
        _tasks = _generateMockTasks();
        print('✅ TaskProvider: Generated ${_tasks.length} mock tasks');
      }
      
      _isLoading = false;
      notifyListeners();
      print('✅ TaskProvider: fetchMyTasks() completed with ${_tasks.length} tasks');
    } catch (e) {
      print('❌ TaskProvider: Error in fetchMyTasks: $e');
      _error = e.toString();
      _isLoading = false;
      notifyListeners();
    }
  }
  
  // Generate mock tasks for testing
  List<MaintenanceTask> _generateMockTasks() {
    final now = DateTime.now();
    return [
      MaintenanceTask(
        id: 1,
        taskId: 'TASK-001',
        equipmentId: 'EQ-ME-001',
        equipmentName: 'Main Engine',
        taskType: 'SCHEDULED',
        taskDescription: 'Check and clean fuel filters, inspect fuel lines',
        intervalHours: 500.0,
        intervalDays: null,
        lastDoneAt: now.subtract(const Duration(days: 45)).toIso8601String(),
        nextDueAt: now.add(const Duration(days: 5)).toIso8601String(),
        runningHoursAtLastDone: 1500.0,
        priority: 'HIGH',
        status: 'PENDING',
        assignedTo: 'CM001',
        completedAt: null,
        completedBy: null,
        notes: null,
        sparePartsUsed: null,
        isSynced: true,
        createdAt: now.subtract(const Duration(days: 90)).toIso8601String(),
      ),
      MaintenanceTask(
        id: 2,
        taskId: 'TASK-002',
        equipmentId: 'EQ-GE-001',
        equipmentName: 'Generator 1',
        taskType: 'SCHEDULED',
        taskDescription: 'Oil change and filter replacement',
        intervalHours: 250.0,
        intervalDays: null,
        lastDoneAt: now.subtract(const Duration(days: 3)).toIso8601String(),
        nextDueAt: now.subtract(const Duration(days: 2)).toIso8601String(),
        runningHoursAtLastDone: 800.0,
        priority: 'CRITICAL',
        status: 'PENDING',
        assignedTo: 'CM001',
        completedAt: null,
        completedBy: null,
        notes: null,
        sparePartsUsed: null,
        isSynced: true,
        createdAt: now.subtract(const Duration(days: 60)).toIso8601String(),
      ),
      MaintenanceTask(
        id: 3,
        taskId: 'TASK-003',
        equipmentId: 'EQ-PUMP-001',
        equipmentName: 'Cooling Water Pump',
        taskType: 'SCHEDULED',
        taskDescription: 'Lubricate bearings and check alignment',
        intervalHours: null,
        intervalDays: 30,
        lastDoneAt: now.subtract(const Duration(days: 20)).toIso8601String(),
        nextDueAt: now.add(const Duration(days: 10)).toIso8601String(),
        runningHoursAtLastDone: null,
        priority: 'NORMAL',
        status: 'IN_PROGRESS',
        assignedTo: 'CM001',
        completedAt: null,
        completedBy: null,
        notes: 'Started inspection',
        sparePartsUsed: null,
        isSynced: true,
        createdAt: now.subtract(const Duration(days: 50)).toIso8601String(),
      ),
      MaintenanceTask(
        id: 4,
        taskId: 'TASK-004',
        equipmentId: 'EQ-AC-001',
        equipmentName: 'Air Compressor',
        taskType: 'SCHEDULED',
        taskDescription: 'Clean air filters and check safety valves',
        intervalHours: null,
        intervalDays: 14,
        lastDoneAt: now.subtract(const Duration(days: 14)).toIso8601String(),
        nextDueAt: now.toIso8601String(),
        runningHoursAtLastDone: null,
        priority: 'HIGH',
        status: 'PENDING',
        assignedTo: 'CM001',
        completedAt: null,
        completedBy: null,
        notes: null,
        sparePartsUsed: null,
        isSynced: true,
        createdAt: now.subtract(const Duration(days: 40)).toIso8601String(),
      ),
      MaintenanceTask(
        id: 5,
        taskId: 'TASK-005',
        equipmentId: 'EQ-ME-001',
        equipmentName: 'Main Engine',
        taskType: 'SCHEDULED',
        taskDescription: 'Complete overhaul and inspection',
        intervalHours: 2000.0,
        intervalDays: null,
        lastDoneAt: now.subtract(const Duration(days: 180)).toIso8601String(),
        nextDueAt: now.subtract(const Duration(days: 1)).toIso8601String(),
        runningHoursAtLastDone: 500.0,
        priority: 'CRITICAL',
        status: 'COMPLETED',
        assignedTo: 'CM001',
        completedAt: now.subtract(const Duration(hours: 2)).toIso8601String(),
        completedBy: 'John Smith',
        notes: 'All checks passed. Replaced worn gaskets.',
        sparePartsUsed: 'Gasket set, Oil filter x2',
        isSynced: true,
        createdAt: now.subtract(const Duration(days: 200)).toIso8601String(),
      ),
    ];
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
}
