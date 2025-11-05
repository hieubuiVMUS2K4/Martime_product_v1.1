// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for English (`en`).
class AppLocalizationsEn extends AppLocalizations {
  AppLocalizationsEn([String locale = 'en']) : super(locale);

  @override
  String get appName => 'Maritime Crew App';

  @override
  String get login => 'Login';

  @override
  String get logout => 'Logout';

  @override
  String get username => 'Username';

  @override
  String get password => 'Password';

  @override
  String get pleaseEnterUsername => 'Please enter username';

  @override
  String get pleaseEnterPassword => 'Please enter password';

  @override
  String get loginFailed => 'Login failed';

  @override
  String get loginSuccess => 'Login successful';

  @override
  String get dashboard => 'Dashboard';

  @override
  String get tasks => 'Tasks';

  @override
  String get schedule => 'Schedule';

  @override
  String get alarms => 'Alarms';

  @override
  String get settings => 'Settings';

  @override
  String get myTasks => 'My Tasks';

  @override
  String get taskDetails => 'Task Details';

  @override
  String get startTask => 'Start Task';

  @override
  String get completeTask => 'Complete Task';

  @override
  String get taskStatus => 'Task Status';

  @override
  String get taskPriority => 'Priority';

  @override
  String get dueDate => 'Due Date';

  @override
  String get estimatedTime => 'Estimated Time';

  @override
  String get description => 'Description';

  @override
  String get statusPending => 'Pending';

  @override
  String get statusInProgress => 'In Progress';

  @override
  String get statusCompleted => 'Completed';

  @override
  String get statusOverdue => 'Overdue';

  @override
  String get priorityCritical => 'Critical';

  @override
  String get priorityHigh => 'High';

  @override
  String get priorityNormal => 'Normal';

  @override
  String get priorityLow => 'Low';

  @override
  String get account => 'Account';

  @override
  String get synchronization => 'Synchronization';

  @override
  String get serverConfiguration => 'Server Configuration';

  @override
  String get dataStorage => 'Data & Storage';

  @override
  String get clearCache => 'Clear Cache';

  @override
  String get removeAllCachedData => 'Remove all cached data';

  @override
  String get about => 'About';

  @override
  String get version => 'Version';

  @override
  String get license => 'License';

  @override
  String get proprietary => 'Proprietary';

  @override
  String get syncStatus => 'Sync Status';

  @override
  String get offline => 'Offline';

  @override
  String get online => 'Online';

  @override
  String itemsWaitingToSync(int count) {
    return '$count items waiting to sync';
  }

  @override
  String get syncNow => 'Sync Now';

  @override
  String lastSyncAt(String time) {
    return 'Last sync at $time';
  }

  @override
  String get serverUrl => 'Server URL';

  @override
  String get testConnection => 'Test Connection';

  @override
  String get saveConfiguration => 'Save Configuration';

  @override
  String get connectionSuccessful => 'Connection successful!';

  @override
  String get connectionFailed => 'Connection failed';

  @override
  String get pleaseEnterServerUrl => 'Please enter server URL';

  @override
  String get invalidUrlFormat =>
      'Invalid URL format (must start with http:// or https://)';

  @override
  String get clearCacheTitle => 'Clear Cache';

  @override
  String get clearCacheMessage =>
      'Are you sure you want to clear all cached data? This will remove all offline data.';

  @override
  String get cacheClearedSuccess => 'Cache cleared successfully';

  @override
  String get cancel => 'Cancel';

  @override
  String get confirm => 'Confirm';

  @override
  String get logoutTitle => 'Logout';

  @override
  String get logoutMessage => 'Are you sure you want to logout?';

  @override
  String get noTasksAvailable => 'No tasks available';

  @override
  String get loadingTasks => 'Loading tasks...';

  @override
  String get errorLoadingTasks => 'Error loading tasks';

  @override
  String get retry => 'Retry';

  @override
  String get language => 'Language';

  @override
  String get languageSettings => 'Language Settings';

  @override
  String get selectLanguage => 'Select Language';

  @override
  String get languageChanged => 'Language changed successfully';

  @override
  String get restartRequired =>
      'Please restart the app to apply language changes';

  @override
  String get english => 'English';

  @override
  String get vietnamese => 'Tiếng Việt';

  @override
  String get filipino => 'Filipino';

  @override
  String get hindi => 'हिंदी';

  @override
  String get chinese => '简体中文';

  @override
  String get japanese => '日本語';

  @override
  String get korean => '한국어';

  @override
  String get user => 'User';

  @override
  String get position => 'Position';

  @override
  String get crewId => 'Crew ID';

  @override
  String get notAvailable => 'N/A';

  @override
  String get justNow => 'Just now';

  @override
  String minutesAgo(int minutes) {
    return '${minutes}m ago';
  }

  @override
  String hoursAgo(int hours) {
    return '${hours}h ago';
  }

  @override
  String get home => 'Home';

  @override
  String get profile => 'Profile';

  @override
  String get myCertificates => 'My Certificates';

  @override
  String get myProfile => 'My Profile';

  @override
  String get expired => 'EXPIRED';

  @override
  String get expiring => 'EXPIRING';

  @override
  String get safetyAlarms => 'Safety Alarms';

  @override
  String get alarmDetails => 'Alarm Details';

  @override
  String get alarmStatistics => 'Alarm Statistics';

  @override
  String get alarmHistory => 'Alarm History';

  @override
  String get acknowledge => 'Acknowledge';

  @override
  String get resolve => 'Resolve';

  @override
  String get alarmAcknowledged => 'Alarm acknowledged';

  @override
  String get alarmResolved => 'Alarm resolved';

  @override
  String get failedToAcknowledgeAlarm => 'Failed to acknowledge alarm';

  @override
  String get failedToResolveAlarm => 'Failed to resolve alarm';

  @override
  String get confirmResolution => 'Confirm Resolution';

  @override
  String get areYouSureResolveAlarm =>
      'Are you sure you want to resolve this alarm?';

  @override
  String get allSystemsNormal => 'All systems normal';

  @override
  String get generateSampleAlarms => 'Generate Sample Alarms';

  @override
  String get sampleAlarmsGenerated => 'Sample alarms generated';

  @override
  String get noDataAvailable => 'No data available';

  @override
  String get newWatchLog => 'New Watch Log';

  @override
  String get watchLogDetails => 'Watch Log Details';

  @override
  String get watchLogNotFound => 'Watch log not found';

  @override
  String get addLogEntry => 'Add Log Entry';

  @override
  String get addNotableEvents =>
      'Add notable events or observations during your watch:';

  @override
  String get logEntrySaved => 'Log entry saved. Waiting for Master signature.';

  @override
  String get watchLogCreatedSuccessfully => 'Watch log created successfully';

  @override
  String get watchDate => 'Watch Date';

  @override
  String errorCompletingTask(String error) {
    return 'Error completing task: $error';
  }

  @override
  String get pleaseEnterMeasuredValue => 'Please enter measured value!';

  @override
  String get invalidValue => 'Invalid value!';

  @override
  String valueTooLow(String min) {
    return 'Value too low! Minimum: $min';
  }

  @override
  String valueTooHigh(String max) {
    return 'Value too high! Maximum: $max';
  }

  @override
  String get pleaseEnterObservationNote => 'Please enter observation note!';

  @override
  String get serverConfigurationSaved =>
      'Server URL saved successfully! Please restart the app.';

  @override
  String failedToSaveUrl(String error) {
    return 'Failed to save URL: $error';
  }

  @override
  String get resetToDefault => 'Reset to Default';

  @override
  String loginFailedError(String error) {
    return 'Login failed: $error';
  }

  @override
  String get serverSettings => 'Server Settings';

  @override
  String get sync => 'Sync';

  @override
  String get save => 'Save';

  @override
  String get taskOverview => 'Task Overview';

  @override
  String get quickAccess => 'Quick Access';

  @override
  String get crewMember => 'Crew Member';

  @override
  String get urgentAttention => 'Urgent Attention!';

  @override
  String overdueTasksCount(int count) {
    return '$count overdue task(s)';
  }

  @override
  String itemsPending(int count) {
    return '$count item(s) pending';
  }

  @override
  String get watchSchedule => 'Watch\nSchedule';

  @override
  String get goodMorning => 'Good Morning';

  @override
  String get goodAfternoon => 'Good Afternoon';

  @override
  String get goodEvening => 'Good Evening';

  @override
  String get goodNight => 'Good Night';

  @override
  String get updateLogEntry => 'Update Log Entry';

  @override
  String get loadingWatchLog => 'Loading watch log...';

  @override
  String get notableEvents => 'Notable Events';

  @override
  String get notableEventsHint =>
      'Course alterations, ships sighted, weather changes, etc.';

  @override
  String get onlyMasterCanSign =>
      'Only the Master can sign and finalize watch logs.';

  @override
  String get needsAcknowledgment => 'NEEDS ACKNOWLEDGMENT';

  @override
  String get location => 'Location';

  @override
  String get alarmCode => 'Alarm Code';

  @override
  String get timestamp => 'Timestamp';

  @override
  String get status => 'Status';

  @override
  String get acknowledgmentInfo => 'Acknowledgment Info';

  @override
  String get acknowledgedBy => 'Acknowledged By';

  @override
  String get acknowledgedAt => 'Acknowledged At';

  @override
  String get resolvedAt => 'Resolved At';

  @override
  String get error => 'Error';

  @override
  String get show => 'Show';

  @override
  String daysCount(int count) {
    return '$count days';
  }

  @override
  String get noAlarmHistory => 'No alarm history';

  @override
  String get resolved => 'Resolved';

  @override
  String get acknowledged => 'Acknowledged';

  @override
  String get viewCertificates => 'View Certificates';

  @override
  String get loadingProfile => 'Loading profile...';

  @override
  String get failedToLoadProfile => 'Failed to load profile';

  @override
  String get profileNotFound => 'Profile not found';

  @override
  String get personalInformation => 'Personal Information';

  @override
  String get nationality => 'Nationality';

  @override
  String get dateOfBirth => 'Date of Birth';

  @override
  String get rank => 'Rank';

  @override
  String get department => 'Department';

  @override
  String get contactInformation => 'Contact Information';

  @override
  String get email => 'Email';

  @override
  String get phone => 'Phone';

  @override
  String get address => 'Address';

  @override
  String get emergencyContact => 'Emergency Contact';

  @override
  String get contactInfo => 'Contact Info';

  @override
  String get documents => 'Documents';

  @override
  String get passportNumber => 'Passport Number';

  @override
  String get passportExpiry => 'Passport Expiry';

  @override
  String get seamanBookNumber => 'Seaman Book Number';

  @override
  String get visaNumber => 'Visa Number';

  @override
  String get visaExpiry => 'Visa Expiry';

  @override
  String get employment => 'Employment';

  @override
  String get onboard => 'ONBOARD';

  @override
  String get offboard => 'OFFBOARD';

  @override
  String get joinDate => 'Join Date';

  @override
  String get embarkDate => 'Embark Date';

  @override
  String get disembarkDate => 'Disembark Date';

  @override
  String get contractEnd => 'Contract End';

  @override
  String get loadingCertificates => 'Loading certificates...';

  @override
  String get failedToLoadCertificates => 'Failed to load certificates';

  @override
  String get noCertificateDataFound => 'No certificate data found';

  @override
  String get stcwCertificate => 'STCW Certificate';

  @override
  String get medicalCertificate => 'Medical Certificate';

  @override
  String get passport => 'Passport';

  @override
  String get visa => 'Visa';

  @override
  String get seamanBook => 'Seaman Book';

  @override
  String certificatesExpired(int count) {
    return '$count certificate(s) have expired!';
  }

  @override
  String certificatesExpiringSoon(int count) {
    return '$count certificate(s) expiring soon';
  }

  @override
  String get expiringSoon => 'Expiring Soon';

  @override
  String get valid => 'Valid';

  @override
  String get number => 'Number';

  @override
  String get issued => 'Issued';

  @override
  String get expires => 'Expires';

  @override
  String daysRemaining(int count) {
    return '$count days remaining';
  }

  @override
  String get maintenance => 'Maintenance';

  @override
  String get loadingSchedule => 'Loading schedule...';

  @override
  String upcomingDays(int count) {
    return 'Upcoming ($count days)';
  }

  @override
  String get thisWeek => 'This Week';

  @override
  String get thisMonth => 'This Month';

  @override
  String get allTasks => 'All Tasks';

  @override
  String get total => 'Total';

  @override
  String get overdue => 'Overdue';

  @override
  String get dueSoon => 'Due Soon';

  @override
  String get noTasksScheduled => 'No tasks scheduled';

  @override
  String get noMaintenanceTasksMatch =>
      'No maintenance tasks match the selected filter';

  @override
  String get signed => 'Signed';

  @override
  String get unsigned => 'Unsigned';

  @override
  String get personnel => 'Personnel';

  @override
  String get officerOnWatch => 'Officer on Watch';

  @override
  String get lookout => 'Lookout';

  @override
  String get weatherSeaConditions => 'Weather & Sea Conditions';

  @override
  String get weather => 'Weather';

  @override
  String get seaState => 'Sea State';

  @override
  String get visibility => 'Visibility';

  @override
  String get navigationData => 'Navigation Data';

  @override
  String get course => 'Course';

  @override
  String get speed => 'Speed';

  @override
  String get distanceRun => 'Distance Run';

  @override
  String get shipPosition => 'Position';

  @override
  String get engineStatus => 'Engine Status';

  @override
  String get masterSignature => 'Master Signature';

  @override
  String get taskCompletedSuccessfully => 'Task completed successfully!';

  @override
  String get taskSavedWillSync => 'Task saved. Will sync when online.';

  @override
  String get taskId => 'Task ID';

  @override
  String get offlineTaskWillSync =>
      'You are offline. Task will be synced when connection is restored.';

  @override
  String get runningHoursRequired => 'Running Hours *';

  @override
  String get enterCurrentRunningHours => 'Enter current running hours';

  @override
  String get hours => 'hours';

  @override
  String get pleaseEnterRunningHours => 'Please enter running hours';

  @override
  String get pleaseEnterValidNumber => 'Please enter a valid number';

  @override
  String runningHoursCannotBeLess(double hours) {
    return 'Running hours cannot be less than last maintenance ($hours)';
  }

  @override
  String get sparePartsUsed => 'Spare Parts Used';

  @override
  String get listSparePartsUsed => 'List any spare parts used (optional)';

  @override
  String get notes => 'Notes';

  @override
  String get addAdditionalNotes =>
      'Add any additional notes or observations (optional)';

  @override
  String get completingTask => 'Completing task...';

  @override
  String get savingForOfflineSync => 'Saving for offline sync...';

  @override
  String get refreshTaskData => 'Refresh task data';

  @override
  String get taskDataRefreshed => '✅ Task data refreshed';

  @override
  String failedToRefresh(String error) {
    return 'Failed to refresh: $error';
  }

  @override
  String overdueDaysPastDue(int days) {
    return '⚠️ OVERDUE: $days days past due date';
  }

  @override
  String get maintenanceSchedule => 'Maintenance Schedule';

  @override
  String get type => 'Type';

  @override
  String get interval => 'Interval';

  @override
  String runningHoursValue(int hours) {
    return '$hours running hours';
  }

  @override
  String daysValue(int days) {
    return '$days days';
  }

  @override
  String get lastDone => 'Last Done';

  @override
  String get nextDue => 'Next Due';

  @override
  String get daysUntilDue => 'Days Until Due';

  @override
  String get runningHours => 'Running Hours';

  @override
  String get atLastMaintenance => 'At Last Maintenance';

  @override
  String hoursValue(int hours) {
    return '$hours hours';
  }

  @override
  String get assignment => 'Assignment';

  @override
  String get assignedTo => 'Assigned To';

  @override
  String get taskChecklist => '📋 Task Checklist';

  @override
  String get completionDetails => 'Completion Details';

  @override
  String get completedBy => 'Completed By';

  @override
  String get completedAt => 'Completed At';

  @override
  String get spareParts => 'Spare Parts';

  @override
  String get errorLoadingChecklist => 'Error loading checklist';

  @override
  String get tryAgain => 'Try Again';

  @override
  String get noChecklistYet => 'No checklist yet';

  @override
  String get thisTaskHasNoDetails => 'This task has no details';

  @override
  String progressCount(int completed, int total) {
    return 'Progress: $completed/$total';
  }

  @override
  String get mandatory => 'MANDATORY';

  @override
  String unitLabel(String unit) {
    return 'Unit: $unit';
  }

  @override
  String get measurement => 'Measurement';

  @override
  String get checklist => 'Checklist';

  @override
  String get inspection => 'Inspection';

  @override
  String get completed => 'Completed';

  @override
  String measuredValueWithUnit(String value, String unit) {
    return 'Measured value: $value $unit';
  }

  @override
  String get checkResult => 'Check Result:';

  @override
  String get okPass => 'OK / Pass';

  @override
  String get ngFail => 'NG / Fail';

  @override
  String get measuredValue => 'Measured Value:';

  @override
  String get enterValue => 'Enter value...';

  @override
  String limitRange(String min, String max, String unit) {
    return 'Limit: $min - $max $unit';
  }

  @override
  String get observationNotes => 'Observation Notes:';

  @override
  String get enterDetailedNotes => 'Enter detailed notes...';

  @override
  String get notesOptional => 'Notes (optional):';

  @override
  String get addNotesIfNeeded => 'Add notes if needed...';

  @override
  String get alreadyCompletedCanUpdate =>
      'Already completed. You can update it.';

  @override
  String savedItem(String item) {
    return '✅ Saved: $item';
  }

  @override
  String errorMessage(String error) {
    return '❌ Error: $error';
  }

  @override
  String get update => 'Update';

  @override
  String get complete => 'Complete';

  @override
  String get taskOverdue => 'Task Overdue';

  @override
  String get thisTaskIsOverdue =>
      'This task is overdue. Starting it now will help catch up on maintenance, but please complete it as soon as possible.';

  @override
  String get startAnyway => 'Start Anyway';

  @override
  String get overdueTaskStarted =>
      'Overdue task started! Please complete ASAP.';

  @override
  String get taskStartedSuccessfully => 'Task started successfully!';

  @override
  String failedToStartTask(String error) {
    return 'Failed to start task: $error';
  }

  @override
  String get searchByEquipmentName => 'Search by equipment name...';

  @override
  String get noTasksFound => 'No tasks found';

  @override
  String get noTasksInCategory => 'You have no tasks in this category';

  @override
  String get noPendingTasks => 'No pending tasks';

  @override
  String get allTasksStartedOrCompleted =>
      'All tasks have been started or completed';

  @override
  String get noTasksInProgress => 'No tasks in progress';

  @override
  String get startPendingTaskToSeeHere => 'Start a pending task to see it here';

  @override
  String get noOverdueTasks => 'No overdue tasks';

  @override
  String get allTasksOnSchedule => 'Great! All tasks are on schedule';

  @override
  String get noCompletedTasks => 'No completed tasks';

  @override
  String get completedTasksAppearHere => 'Complete tasks will appear here';

  @override
  String get noTasksMatchSearch => 'No tasks match your search';

  @override
  String get tryDifferentSearchTerm => 'Try a different search term';
}
