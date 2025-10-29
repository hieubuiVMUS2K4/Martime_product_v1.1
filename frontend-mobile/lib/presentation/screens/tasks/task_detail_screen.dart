import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import '../../../data/models/maintenance_task.dart';
import '../../providers/task_provider.dart';
import '../../widgets/task/priority_badge.dart';
import '../../widgets/task/status_badge.dart';
import 'complete_task_screen.dart';

class TaskDetailScreen extends StatelessWidget {
  final MaintenanceTask task;

  const TaskDetailScreen({
    super.key,
    required this.task,
  });

  @override
  Widget build(BuildContext context) {
    final taskProvider = Provider.of<TaskProvider>(context);
    final dateFormat = DateFormat('dd MMM yyyy');

    return Scaffold(
      appBar: AppBar(
        title: const Text('Task Details'),
      ),
      body: SingleChildScrollView(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Overdue Warning Banner
            if (task.isOverdue && !task.isCompleted)
              Container(
                width: double.infinity,
                color: Colors.red.shade700,
                padding: const EdgeInsets.symmetric(vertical: 12, horizontal: 16),
                child: Row(
                  children: [
                    const Icon(Icons.warning, color: Colors.white),
                    const SizedBox(width: 12),
                    Expanded(
                      child: Text(
                        '⚠️ OVERDUE: ${task.daysUntilDue.abs()} days past due date',
                        style: const TextStyle(
                          color: Colors.white,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            
            // Header Card
            Card(
              margin: const EdgeInsets.all(16),
              child: Padding(
                padding: const EdgeInsets.all(20),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Equipment Name
                    Text(
                      task.equipmentName,
                      style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                            fontWeight: FontWeight.bold,
                          ),
                    ),
                    const SizedBox(height: 8),

                    // Task ID
                    Row(
                      children: [
                        Icon(Icons.tag, size: 16, color: Colors.grey.shade600),
                        const SizedBox(width: 4),
                        Text(
                          'Task ID: ${task.taskId}',
                          style: TextStyle(color: Colors.grey.shade600),
                        ),
                      ],
                    ),
                    const SizedBox(height: 12),

                    // Badges
                    Row(
                      children: [
                        PriorityBadge(priority: task.priority),
                        const SizedBox(width: 8),
                        StatusBadge(task: task),
                      ],
                    ),
                  ],
                ),
              ),
            ),

            // Task Description
            _buildSection(
              context,
              title: 'Description',
              icon: Icons.description,
              child: Text(
                task.taskDescription,
                style: Theme.of(context).textTheme.bodyLarge,
              ),
            ),

            // Task Type & Interval
            _buildSection(
              context,
              title: 'Maintenance Schedule',
              icon: Icons.schedule,
              child: Column(
                children: [
                  _buildInfoRow(
                    'Type',
                    task.taskType.replaceAll('_', ' '),
                  ),
                  if (task.intervalHours != null)
                    _buildInfoRow(
                      'Interval',
                      '${task.intervalHours} running hours',
                    ),
                  if (task.intervalDays != null)
                    _buildInfoRow(
                      'Interval',
                      '${task.intervalDays} days',
                    ),
                ],
              ),
            ),

            // Due Date Info
            _buildSection(
              context,
              title: 'Schedule',
              icon: Icons.calendar_today,
              child: Column(
                children: [
                  if (task.lastDoneAt != null)
                    _buildInfoRow(
                      'Last Done',
                      dateFormat.format(DateTime.parse(task.lastDoneAt!)),
                    ),
                  _buildInfoRow(
                    'Next Due',
                    dateFormat.format(DateTime.parse(task.nextDueAt)),
                  ),
                  _buildInfoRow(
                    'Days Until Due',
                    '${task.daysUntilDue} days',
                    valueColor: task.isOverdue
                        ? Colors.red.shade700
                        : task.isDueSoon
                            ? Colors.orange.shade700
                            : null,
                  ),
                ],
              ),
            ),

            // Running Hours
            if (task.runningHoursAtLastDone != null)
              _buildSection(
                context,
                title: 'Running Hours',
                icon: Icons.access_time,
                child: Column(
                  children: [
                    _buildInfoRow(
                      'At Last Maintenance',
                      '${task.runningHoursAtLastDone} hours',
                    ),
                  ],
                ),
              ),

            // Assignment
            if (task.assignedTo != null)
              _buildSection(
                context,
                title: 'Assignment',
                icon: Icons.person,
                child: Column(
                  children: [
                    _buildInfoRow('Assigned To', task.assignedTo!),
                  ],
                ),
              ),

            // Completion Info
            if (task.isCompleted)
              _buildSection(
                context,
                title: 'Completion Details',
                icon: Icons.check_circle,
                child: Column(
                  children: [
                    if (task.completedBy != null)
                      _buildInfoRow('Completed By', task.completedBy!),
                    if (task.completedAt != null)
                      _buildInfoRow(
                        'Completed At',
                        dateFormat.format(DateTime.parse(task.completedAt!)),
                      ),
                    if (task.runningHoursAtCompletion != null)
                      _buildInfoRow(
                        'Running Hours',
                        '${task.runningHoursAtCompletion} hours',
                      ),
                    if (task.sparePartsUsed != null)
                      _buildInfoRow('Spare Parts', task.sparePartsUsed!),
                    if (task.notes != null) ...[
                      const SizedBox(height: 8),
                      const Text(
                        'Notes:',
                        style: TextStyle(fontWeight: FontWeight.bold),
                      ),
                      const SizedBox(height: 4),
                      Text(task.notes!),
                    ],
                  ],
                ),
              ),

            const SizedBox(height: 100), // Space for FAB
          ],
        ),
      ),
      floatingActionButton: _buildActionButton(context, taskProvider),
    );
  }

  Widget _buildSection(
    BuildContext context, {
    required String title,
    required IconData icon,
    required Widget child,
  }) {
    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Icon(icon, size: 20, color: Theme.of(context).colorScheme.primary),
                const SizedBox(width: 8),
                Text(
                  title,
                  style: Theme.of(context).textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.bold,
                      ),
                ),
              ],
            ),
            const SizedBox(height: 12),
            child,
          ],
        ),
      ),
    );
  }

  Widget _buildInfoRow(String label, String value, {Color? valueColor}) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 140,
            child: Text(
              label,
              style: TextStyle(
                color: Colors.grey.shade600,
                fontWeight: FontWeight.w500,
              ),
            ),
          ),
          Expanded(
            child: Text(
              value,
              style: TextStyle(
                fontWeight: FontWeight.w600,
                color: valueColor,
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget? _buildActionButton(BuildContext context, TaskProvider taskProvider) {
    if (task.isCompleted) {
      return null; // No action for completed tasks
    }

    if (task.isPending) {
      return FloatingActionButton.extended(
        onPressed: taskProvider.isLoading
            ? null
            : () async {
                // Show warning if task is overdue
                if (task.isOverdue) {
                  final confirmed = await showDialog<bool>(
                    context: context,
                    builder: (context) => AlertDialog(
                      icon: const Icon(Icons.warning, color: Colors.red, size: 48),
                      title: const Text('Task Overdue'),
                      content: Text(
                        'This task is ${task.daysUntilDue.abs()} days overdue!\n\n'
                        'Due date: ${task.nextDueAt}\n\n'
                        'Do you still want to start this task?',
                      ),
                      actions: [
                        TextButton(
                          onPressed: () => Navigator.pop(context, false),
                          child: const Text('Cancel'),
                        ),
                        ElevatedButton(
                          onPressed: () => Navigator.pop(context, true),
                          style: ElevatedButton.styleFrom(
                            backgroundColor: Colors.red,
                          ),
                          child: const Text('Start Anyway'),
                        ),
                      ],
                    ),
                  );
                  
                  if (confirmed != true) return;
                }
                
                try {
                  await taskProvider.startTask(task.id);
                  if (context.mounted) {
                    ScaffoldMessenger.of(context).showSnackBar(
                      SnackBar(
                        content: Text(
                          task.isOverdue 
                            ? 'Overdue task started! Please complete ASAP.'
                            : 'Task started successfully!',
                        ),
                        backgroundColor: task.isOverdue ? Colors.orange : null,
                      ),
                    );
                    Navigator.pop(context);
                  }
                } catch (e) {
                  if (context.mounted) {
                    ScaffoldMessenger.of(context).showSnackBar(
                      SnackBar(
                        content: Text('Failed to start task: $e'),
                        backgroundColor: Colors.red,
                      ),
                    );
                  }
                }
              },
        icon: const Icon(Icons.play_arrow),
        label: Text(task.isOverdue ? 'Start Overdue Task' : 'Start Task'),
        backgroundColor: task.isOverdue ? Colors.red : null,
      );
    }

    if (task.isInProgress) {
      return FloatingActionButton.extended(
        onPressed: () {
          Navigator.push(
            context,
            MaterialPageRoute(
              builder: (context) => CompleteTaskScreen(task: task),
            ),
          );
        },
        icon: const Icon(Icons.check),
        label: const Text('Complete Task'),
        backgroundColor: Colors.green,
      );
    }

    return null;
  }
}
