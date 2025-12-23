class AppConstants {
  // App Info
  static const String appName = 'Maritime Crew App';
  static const String appVersion = '1.0.0';
  
  // Cache Keys
  static const String userProfileKey = 'user_profile';
  static const String myTasksKey = 'my_tasks';
  static const String serverUrlKey = 'server_url';
  
  // Task Status
  static const String taskStatusPending = 'PENDING';
  static const String taskStatusInProgress = 'IN_PROGRESS';
  static const String taskStatusCompleted = 'COMPLETED';
  static const String taskStatusOverdue = 'OVERDUE';
  
  // Task Priority
  static const String priorityCritical = 'CRITICAL';
  static const String priorityHigh = 'HIGH';
  static const String priorityNormal = 'NORMAL';
  static const String priorityLow = 'LOW';
  
  // Task Type
  static const String taskTypeRunningHours = 'RUNNING_HOURS';
  static const String taskTypeCalendar = 'CALENDAR';
  static const String taskTypeCondition = 'CONDITION';
  
  // Date Formats
  static const String dateFormat = 'dd/MM/yyyy';
  static const String dateTimeFormat = 'dd/MM/yyyy HH:mm';
  static const String timeFormat = 'HH:mm';
}

/// Performance tuning constants
/// These values are optimized for battery life and user experience
class PerformanceConstants {
  PerformanceConstants._();
  
  /// Task list auto-refresh interval
  /// 30 seconds balances real-time updates with battery life
  /// Previously 5 seconds which was too aggressive
  static const Duration taskListRefreshInterval = Duration(seconds: 30);
  
  /// Minimum interval between manual refresh attempts
  static const Duration minManualRefreshInterval = Duration(seconds: 5);
  
  /// Quick API timeout for non-critical operations
  static const Duration quickApiTimeout = Duration(seconds: 3);
  
  /// Standard API timeout for normal operations
  static const Duration standardApiTimeout = Duration(seconds: 10);
  
  /// Extended timeout for file uploads
  static const Duration uploadTimeout = Duration(seconds: 60);
}

/// UI breakpoints and sizing constants
class UIBreakpoints {
  UIBreakpoints._();
  
  /// Width threshold for compact UI mode
  static const double smallScreen = 400;
  
  /// Width threshold for tablet layouts
  static const double mediumScreen = 600;
  
  /// Width threshold for desktop layouts
  static const double largeScreen = 900;
}

/// Animation durations
class AnimationDurations {
  AnimationDurations._();
  
  static const Duration fast = Duration(milliseconds: 150);
  static const Duration normal = Duration(milliseconds: 300);
  static const Duration slow = Duration(milliseconds: 500);
}
