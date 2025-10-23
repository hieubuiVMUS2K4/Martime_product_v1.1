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
    with SingleTickerProviderStateMixin {
  late TabController _tabController;
  String _searchQuery = '';
  final TextEditingController _searchController = TextEditingController();

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 4, vsync: this);
    
    // Fetch tasks on init
    WidgetsBinding.instance.addPostFrameCallback((_) {
      Provider.of<TaskProvider>(context, listen: false).fetchMyTasks();
    });
  }

  @override
  void dispose() {
    _tabController.dispose();
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final taskProvider = Provider.of<TaskProvider>(context);

    return Scaffold(
      appBar: AppBar(
        title: const Text('My Tasks'),
        bottom: TabBar(
          controller: _tabController,
          tabs: [
            Tab(
              text: 'All (${taskProvider.tasks.length})',
              icon: const Icon(Icons.list),
            ),
            Tab(
              text: 'Pending (${taskProvider.pendingTasks.length})',
              icon: const Icon(Icons.pending_actions),
            ),
            Tab(
              text: 'Overdue (${taskProvider.overdueTasks.length})',
              icon: const Icon(Icons.warning),
            ),
            Tab(
              text: 'Completed (${taskProvider.completedTasks.length})',
              icon: const Icon(Icons.check_circle),
            ),
          ],
        ),
      ),
      body: Column(
        children: [
          // Search bar
          Padding(
            padding: const EdgeInsets.all(16),
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: 'Search by equipment name...',
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
                _buildTaskList(taskProvider.tasks, taskProvider),
                _buildTaskList(taskProvider.pendingTasks, taskProvider),
                _buildTaskList(taskProvider.overdueTasks, taskProvider),
                _buildTaskList(taskProvider.completedTasks, taskProvider),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildTaskList(List<dynamic> tasks, TaskProvider taskProvider) {
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
      return EmptyStateWidget(
        message: _searchQuery.isEmpty
            ? 'No tasks found'
            : 'No tasks match your search',
        subtitle: _searchQuery.isEmpty
            ? 'You have no tasks in this category'
            : 'Try a different search term',
        icon: _searchQuery.isEmpty ? Icons.task_alt : Icons.search_off,
      );
    }

    return RefreshIndicator(
      onRefresh: () => taskProvider.fetchMyTasks(),
      child: ListView.builder(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        itemCount: filteredTasks.length,
        itemBuilder: (context, index) {
          final task = filteredTasks[index];
          return TaskCard(
            task: task,
            onTap: () {
              Navigator.push(
                context,
                MaterialPageRoute(
                  builder: (context) => TaskDetailScreen(task: task),
                ),
              );
            },
          );
        },
      ),
    );
  }
}
