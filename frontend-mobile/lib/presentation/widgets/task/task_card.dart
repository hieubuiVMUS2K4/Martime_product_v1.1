import 'package:flutter/material.dart';
import '../../../data/models/maintenance_task.dart';
import 'priority_badge.dart';
import 'status_badge.dart';

class TaskCard extends StatelessWidget {
  final MaintenanceTask task;
  final VoidCallback onTap;

  const TaskCard({
    super.key,
    required this.task,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(12),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Header: Equipment name + badges
              Row(
                children: [
                  Expanded(
                    child: Text(
                      task.equipmentName,
                      style: Theme.of(context).textTheme.titleMedium?.copyWith(
                            fontWeight: FontWeight.bold,
                          ),
                    ),
                  ),
                  PriorityBadge(priority: task.priority),
                ],
              ),
              const SizedBox(height: 8),

              // Task ID
              Text(
                'Task ID: ${task.taskId}',
                style: Theme.of(context).textTheme.bodySmall?.copyWith(
                      color: Colors.grey.shade600,
                    ),
              ),
              const SizedBox(height: 4),

              // Task description (preview)
              Text(
                task.taskDescription,
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
                style: Theme.of(context).textTheme.bodyMedium,
              ),
              const SizedBox(height: 12),

              // Footer: Status + Due date
              Row(
                children: [
                  StatusBadge(task: task),
                  const Spacer(),
                  _buildDueDate(context),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildDueDate(BuildContext context) {
    final daysUntilDue = task.daysUntilDue;
    final isOverdue = task.isOverdue;
    final isDueSoon = task.isDueSoon;

    Color color;
    IconData icon;
    String text;

    if (isOverdue) {
      color = Colors.red.shade700;
      icon = Icons.warning;
      text = '${daysUntilDue.abs()} days overdue';
    } else if (isDueSoon) {
      color = Colors.orange.shade700;
      icon = Icons.schedule;
      text = '$daysUntilDue days left';
    } else {
      color = Colors.grey.shade600;
      icon = Icons.calendar_today;
      text = '$daysUntilDue days';
    }

    return Row(
      children: [
        Icon(icon, size: 16, color: color),
        const SizedBox(width: 4),
        Text(
          text,
          style: TextStyle(
            fontSize: 12,
            color: color,
            fontWeight: isOverdue || isDueSoon ? FontWeight.bold : FontWeight.normal,
          ),
        ),
      ],
    );
  }
}
