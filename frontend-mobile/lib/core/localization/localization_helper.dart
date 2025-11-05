import 'package:flutter/material.dart';
import '../../l10n/app_localizations.dart';

/// Extension helper để dễ dàng access AppLocalizations
/// 
/// Usage:
/// ```dart
/// Text(context.l10n.taskDetails)
/// Text(context.l10n.statusPending)
/// ```
extension LocalizationExtension on BuildContext {
  AppLocalizations get l10n => AppLocalizations.of(this)!;
}

/// Helper functions cho status translation
class TaskStatusHelper {
  static String getStatusText(BuildContext context, String status) {
    final l10n = context.l10n;
    switch (status.toUpperCase()) {
      case 'PENDING':
        return l10n.statusPending;
      case 'IN_PROGRESS':
        return l10n.statusInProgress;
      case 'COMPLETED':
        return l10n.statusCompleted;
      case 'OVERDUE':
        return l10n.statusOverdue;
      default:
        return status;
    }
  }
}

/// Helper functions cho priority translation
class TaskPriorityHelper {
  static String getPriorityText(BuildContext context, String priority) {
    final l10n = context.l10n;
    switch (priority.toUpperCase()) {
      case 'CRITICAL':
        return l10n.priorityCritical;
      case 'HIGH':
        return l10n.priorityHigh;
      case 'NORMAL':
        return l10n.priorityNormal;
      case 'LOW':
        return l10n.priorityLow;
      default:
        return priority;
    }
  }
}

/// Example usage trong widget:
/// 
/// ```dart
/// class TaskCard extends StatelessWidget {
///   final String status;
///   final String priority;
///   
///   Widget build(BuildContext context) {
///     return Column(
///       children: [
///         Text(context.l10n.taskDetails),
///         Text(TaskStatusHelper.getStatusText(context, status)),
///         Text(TaskPriorityHelper.getPriorityText(context, priority)),
///       ],
///     );
///   }
/// }
/// ```
