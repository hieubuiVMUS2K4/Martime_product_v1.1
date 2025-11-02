import 'dart:async';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../providers/task_provider.dart';
import '../../widgets/common/loading_widget.dart';
import '../../widgets/common/error_widget.dart';
import '../../widgets/common/empty_state_widget.dart';
import '../../widgets/task/task_card.dart';
import 'task_detail_screen.dart';

class TaskListScreen extends StatefulWidget {
  const TaskListScreen({super.key});

  @override
  State<TaskListScreen> createState() => _TaskListScreenState();
}

class _TaskListScreenState extends State<TaskListScreen>
    with SingleTickerProviderStateMixin, WidgetsBindingObserver {
  late TabController _tabController;
  String _searchQuery = '';
  final TextEditingController _searchController = TextEditingController();
  
  // Auto-refresh timer để sync với backend khi Captain giao task mới
  Timer? _refreshTimer;
  static const _refreshInterval = Duration(seconds: 5); // Reduced from 30s to 5s for faster sync

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 4, vsync: this);
    WidgetsBinding.instance.addObserver(this);
    
    // Fetch tasks on init
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _fetchAndStartTimer();
    });
  }

  @override
  void dispose() {
    _refreshTimer?.cancel();
    _tabController.dispose();
    _searchController.dispose();
    WidgetsBinding.instance.removeObserver(this);
    super.dispose();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    // Refresh khi app quay lại foreground
    if (state == AppLifecycleState.resumed) {
      Provider.of<TaskProvider>(context, listen: false).fetchMyTasks(forceRefresh: true);
    }
  }

  void _fetchAndStartTimer() {
    final taskProvider = Provider.of<TaskProvider>(context, listen: false);
    taskProvider.fetchMyTasks(forceRefresh: true); // Force API call on init
    
    // Auto-refresh every 5s to get task updates from Captain
    _refreshTimer?.cancel();
    _refreshTimer = Timer.periodic(_refreshInterval, (_) {
      if (mounted) {
        taskProvider.fetchMyTasks(forceRefresh: true); // Always force refresh from API
      }
    });
  }

  Widget _buildTab({
    required String label,
    required int count,
    required IconData icon,
    required bool isSmallScreen,
  }) {
    if (isSmallScreen) {
      // Compact mode for small screens
      return Tab(
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(icon, size: 18),
            const SizedBox(width: 4),
            Text(
              '($count)',
              style: const TextStyle(fontSize: 12, fontWeight: FontWeight.bold),
            ),
          ],
        ),
      );
    } else {
      // Full mode for larger screens
      return Tab(
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(icon, size: 20),
            const SizedBox(width: 8),
            Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  label,
                  style: const TextStyle(fontSize: 13),
                ),
                Text(
                  '($count)',
                  style: const TextStyle(
                    fontSize: 11,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ],
            ),
          ],
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final taskProvider = Provider.of<TaskProvider>(context);
    final screenWidth = MediaQuery.of(context).size.width;
    final isSmallScreen = screenWidth < 400;

    return Scaffold(
      appBar: AppBar(
        title: const Text('My Tasks'),
        bottom: PreferredSize(
          preferredSize: const Size.fromHeight(48),
          child: TabBar(
            controller: _tabController,
            isScrollable: true,
            tabAlignment: TabAlignment.start,
            labelPadding: EdgeInsets.symmetric(
              horizontal: isSmallScreen ? 12 : 16,
              vertical: 8,
            ),
            tabs: [
              _buildTab(
                label: 'Pending',
                count: taskProvider.pendingTasks.length,
                icon: Icons.pending_actions,
                isSmallScreen: isSmallScreen,
              ),
              _buildTab(
                label: 'In Progress',
                count: taskProvider.inProgressTasks.length,
                icon: Icons.play_arrow,
                isSmallScreen: isSmallScreen,
              ),
              _buildTab(
                label: 'Overdue',
                count: taskProvider.overdueTasks.length,
                icon: Icons.warning,
                isSmallScreen: isSmallScreen,
              ),
              _buildTab(
                label: 'Completed',
                count: taskProvider.completedTasks.length,
                icon: Icons.check_circle,
                isSmallScreen: isSmallScreen,
              ),
            ],
          ),
        ),
      ),
      body: Column(
        children: [
          // Search bar
          Padding(
            padding: EdgeInsets.symmetric(
              horizontal: screenWidth < 600 ? 12 : 16,
              vertical: 12,
            ),
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: 'Search by equipment name...',
                hintStyle: TextStyle(fontSize: isSmallScreen ? 13 : 14),
                prefixIcon: const Icon(Icons.search),
                suffixIcon: _searchQuery.isNotEmpty
                    ? IconButton(
                        icon: const Icon(Icons.clear),
                        onPressed: () {
                          _searchController.clear();
                          setState(() {
                            _searchQuery = '';
                          });
                        },
                      )
                    : null,
                contentPadding: EdgeInsets.symmetric(
                  horizontal: 16,
                  vertical: isSmallScreen ? 12 : 16,
                ),
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(12),
                ),
              ),
              onChanged: (value) {
                setState(() {
                  _searchQuery = value;
                });
              },
            ),
          ),

          // Task list
          Expanded(
            child: TabBarView(
              controller: _tabController,
              children: [
                _buildTaskList(taskProvider.pendingTasks, taskProvider, 'pending'),
                _buildTaskList(taskProvider.inProgressTasks, taskProvider, 'in_progress'),
                _buildTaskList(taskProvider.overdueTasks, taskProvider, 'overdue'),
                _buildTaskList(taskProvider.completedTasks, taskProvider, 'completed'),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildTaskList(List<dynamic> tasks, TaskProvider taskProvider, String tabType) {
    if (taskProvider.isLoading && tasks.isEmpty) {
      return const LoadingWidget(message: 'Loading tasks...');
    }

    if (taskProvider.error != null && tasks.isEmpty) {
      return ErrorDisplayWidget(
        message: taskProvider.error!,
        onRetry: () => taskProvider.fetchMyTasks(),
      );
    }

    // Filter by search query
    final filteredTasks = tasks.where((task) {
      return task.equipmentName
              .toLowerCase()
              .contains(_searchQuery.toLowerCase()) ||
          task.taskDescription
              .toLowerCase()
              .contains(_searchQuery.toLowerCase());
    }).toList();

    if (filteredTasks.isEmpty) {
      String emptyMessage = 'No tasks found';
      String emptySubtitle = 'You have no tasks in this category';
      
      if (_searchQuery.isEmpty) {
        switch (tabType) {
          case 'pending':
            emptyMessage = 'No pending tasks';
            emptySubtitle = 'All tasks have been started or completed';
            break;
          case 'in_progress':
            emptyMessage = 'No tasks in progress';
            emptySubtitle = 'Start a pending task to see it here';
            break;
          case 'overdue':
            emptyMessage = 'No overdue tasks';
            emptySubtitle = 'Great! All tasks are on schedule';
            break;
          case 'completed':
            emptyMessage = 'No completed tasks';
            emptySubtitle = 'Complete tasks will appear here';
            break;
        }
      } else {
        emptyMessage = 'No tasks match your search';
        emptySubtitle = 'Try a different search term';
      }
      
      return EmptyStateWidget(
        message: emptyMessage,
        subtitle: emptySubtitle,
        icon: _searchQuery.isEmpty ? Icons.task_alt : Icons.search_off,
      );
    }

    return RefreshIndicator(
      onRefresh: () => taskProvider.fetchMyTasks(),
      child: LayoutBuilder(
        builder: (context, constraints) {
          final isNarrow = constraints.maxWidth < 600;
          return ListView.builder(
            padding: EdgeInsets.symmetric(
              horizontal: isNarrow ? 12 : 16,
              vertical: 8,
            ),
            itemCount: filteredTasks.length,
            itemBuilder: (context, index) {
              final task = filteredTasks[index];
              return Padding(
                padding: EdgeInsets.only(bottom: isNarrow ? 8 : 12),
                child: TaskCard(
                  task: task,
                  onTap: () {
                    Navigator.push(
                      context,
                      MaterialPageRoute(
                        builder: (context) => TaskDetailScreen(task: task),
                      ),
                    );
                  },
                ),
              );
            },
          );
        },
      ),
    );
  }
}
