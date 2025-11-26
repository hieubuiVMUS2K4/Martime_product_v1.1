import 'package:flutter/material.dart';
import '../../../data/models/maintenance_task.dart';

class StatusBadge extends StatelessWidget {
  final MaintenanceTask task;
  final bool small;

  const StatusBadge({
    super.key,
    required this.task,
    this.small = false,
  });

  @override
  Widget build(BuildContext context) {
    Color color = task.statusColor;
    String text = task.statusText;
    IconData icon;

    if (task.isOverdue && !task.isCompleted) {
      icon = Icons.warning;
    } else if (task.isCompleted) {
      icon = Icons.check_circle;
    } else if (task.isInProgress) {
      icon = Icons.play_circle;
    } else {
      icon = Icons.pending;
    }

    return Container(
      padding: EdgeInsets.symmetric(
        horizontal: small ? 6 : 8,
        vertical: small ? 2 : 4,
      ),
      decoration: BoxDecoration(
        color: color.withOpacity(0.1),
        borderRadius: BorderRadius.circular(4),
        border: Border.all(color: color, width: 1),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: small ? 12 : 14, color: color),
          SizedBox(width: small ? 2 : 4),
          Flexible(
            child: Text(
              text,
              style: TextStyle(
                fontSize: small ? 10 : 11,
                fontWeight: FontWeight.bold,
                color: color,
              ),
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
            ),
          ),
        ],
      ),
    );
  }
}
