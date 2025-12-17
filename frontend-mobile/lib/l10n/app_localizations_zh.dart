// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for Chinese (`zh`).
class AppLocalizationsZh extends AppLocalizations {
  AppLocalizationsZh([String locale = 'zh']) : super(locale);

  @override
  String get appName => '海员应用程序';

  @override
  String get login => '登录';

  @override
  String get logout => '登出';

  @override
  String get username => '用户名';

  @override
  String get password => '密码';

  @override
  String get pleaseEnterUsername => '请输入用户名';

  @override
  String get pleaseEnterPassword => '请输入密码';

  @override
  String get loginFailed => '登录失败';

  @override
  String get loginSuccess => '登录成功';

  @override
  String get dashboard => '仪表板';

  @override
  String get tasks => '任务';

  @override
  String get schedule => '日程';

  @override
  String get alarms => '警报';

  @override
  String get settings => '设置';

  @override
  String get myTasks => '我的任务';

  @override
  String get taskDetails => '任务详情';

  @override
  String get startTask => '开始任务';

  @override
  String get completeTask => '完成';

  @override
  String get taskStatus => '任务状态';

  @override
  String get taskPriority => '优先级';

  @override
  String get dueDate => '截止日期';

  @override
  String get estimatedTime => '预计时间';

  @override
  String get description => '描述';

  @override
  String get statusPending => '待处理';

  @override
  String get statusInProgress => '进行中';

  @override
  String get statusCompleted => '已完成';

  @override
  String get statusOverdue => '逾期';

  @override
  String get priorityCritical => '紧急';

  @override
  String get priorityHigh => '高';

  @override
  String get priorityNormal => '正常';

  @override
  String get priorityLow => '低';

  @override
  String get account => '账户';

  @override
  String get synchronization => '同步';

  @override
  String get serverConfiguration => '服务器配置';

  @override
  String get dataStorage => '数据存储';

  @override
  String get clearCache => '清除缓存';

  @override
  String get removeAllCachedData => '删除所有缓存数据';

  @override
  String get about => '关于';

  @override
  String get version => '版本';

  @override
  String get license => '许可证';

  @override
  String get proprietary => '专有';

  @override
  String get syncStatus => '同步状态';

  @override
  String get offline => '离线';

  @override
  String get online => '在线';

  @override
  String itemsWaitingToSync(int count) {
    return '$count 项等待同步';
  }

  @override
  String get syncNow => '立即同步';

  @override
  String lastSyncAt(String time) {
    return '最后同步时间 $time';
  }

  @override
  String get serverUrl => '服务器地址';

  @override
  String get testConnection => '测试连接';

  @override
  String get saveConfiguration => '保存配置';

  @override
  String get connectionSuccessful => '连接成功！';

  @override
  String get connectionFailed => '连接失败';

  @override
  String get pleaseEnterServerUrl => '请输入服务器地址';

  @override
  String get invalidUrlFormat => '无效的URL格式（必须以http://或https://开头）';

  @override
  String get clearCacheTitle => '清除缓存';

  @override
  String get clearCacheMessage => '确定要清除所有缓存数据吗？这将删除所有离线数据。';

  @override
  String get cacheClearedSuccess => '缓存清除成功';

  @override
  String get cancel => '取消';

  @override
  String get confirm => '确认';

  @override
  String get close => 'Close';

  @override
  String get logoutTitle => '登出';

  @override
  String get logoutMessage => '确定要登出吗？';

  @override
  String get noTasksAvailable => '没有可用任务';

  @override
  String get loadingTasks => '加载任务中...';

  @override
  String get errorLoadingTasks => '加载任务错误';

  @override
  String get retry => '重试';

  @override
  String get language => '语言';

  @override
  String get languageSettings => '语言设置';

  @override
  String get selectLanguage => '选择语言';

  @override
  String get languageChanged => '语言更改成功';

  @override
  String get restartRequired => '请重新启动应用以应用语言更改';

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
  String get user => '用户';

  @override
  String get position => '职位';

  @override
  String get crewId => '编号';

  @override
  String get notAvailable => '不可用';

  @override
  String get justNow => '刚刚';

  @override
  String minutesAgo(int minutes) {
    return '$minutes分钟前';
  }

  @override
  String hoursAgo(int hours) {
    return '$hours小时前';
  }

  @override
  String get home => '主页';

  @override
  String get profile => '个人资料';

  @override
  String get myCertificates => '我的证书';

  @override
  String get myProfile => '我的个人资料';

  @override
  String get expired => '已过期';

  @override
  String get expiring => '即将过期';

  @override
  String get safetyAlarms => '安全警报';

  @override
  String get alarmDetails => '警报详情';

  @override
  String get alarmStatistics => '警报统计';

  @override
  String get alarmHistory => '警报历史';

  @override
  String get acknowledge => '确认';

  @override
  String get resolve => '解决';

  @override
  String get alarmAcknowledged => '警报已确认';

  @override
  String get alarmResolved => '警报已解决';

  @override
  String get failedToAcknowledgeAlarm => '确认警报失败';

  @override
  String get failedToResolveAlarm => '解决警报失败';

  @override
  String get confirmResolution => '确认解决';

  @override
  String get areYouSureResolveAlarm => '确定要解决此警报吗？';

  @override
  String get allSystemsNormal => '所有系统正常';

  @override
  String get generateSampleAlarms => '生成示例警报';

  @override
  String get sampleAlarmsGenerated => '示例警报已生成';

  @override
  String get noDataAvailable => '无可用数据';

  @override
  String get newWatchLog => '新值班日志';

  @override
  String get watchLogDetails => '值班日志详情';

  @override
  String get watchLogNotFound => '未找到值班日志';

  @override
  String get addLogEntry => '添加日志条目';

  @override
  String get addNotableEvents => '添加值班期间的重要事件或观察：';

  @override
  String get logEntrySaved => '日志条目已保存。等待船长签名。';

  @override
  String get watchLogCreatedSuccessfully => '值班日志创建成功';

  @override
  String get watchDate => '值班日期';

  @override
  String errorCompletingTask(String error) {
    return '完成任务错误: $error';
  }

  @override
  String get pleaseEnterMeasuredValue => '请输入测量值！';

  @override
  String get invalidValue => '无效值！';

  @override
  String valueTooLow(String min) {
    return '值太低！最小值: $min';
  }

  @override
  String valueTooHigh(String max) {
    return '值太高！最大值: $max';
  }

  @override
  String get pleaseEnterObservationNote => '请输入观察备注！';

  @override
  String get serverConfigurationSaved => '服务器地址保存成功！请重启应用。';

  @override
  String failedToSaveUrl(String error) {
    return '保存地址失败: $error';
  }

  @override
  String get resetToDefault => '重置为默认值';

  @override
  String loginFailedError(String error) {
    return '登录失败: $error';
  }

  @override
  String get serverSettings => '服务器设置';

  @override
  String get sync => '同步';

  @override
  String get save => '保存';

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
    return '刷新失败: $error';
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
  String get okPass => 'OK / 合格';

  @override
  String get ngFail => 'NG / 不合格';

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
  String get tapToComplete => '👆 Tap to complete';

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
  String get taskOverdue => '任务逾期';

  @override
  String get thisTaskIsOverdue => '此任务已逾期。是否仍要开始？';

  @override
  String get startAnyway => '仍然开始';

  @override
  String get overdueTaskStarted =>
      'Overdue task started! Please complete ASAP.';

  @override
  String get taskStartedSuccessfully => 'Task started successfully!';

  @override
  String failedToStartTask(String error) {
    return '启动任务失败: $error';
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

  @override
  String get statusHistory => 'Status History';

  @override
  String get pendingApproval => 'Pending Approval';

  @override
  String get rectify => 'Rectify Required';

  @override
  String get waitingForApproval => 'Waiting for C/E approval';

  @override
  String get taskRejected => 'Task Rejected';

  @override
  String get rejectedBy => 'Rejected by';

  @override
  String get rejectionCount => 'Rejection count';

  @override
  String get fixAndContinue => 'Fix & Continue';

  @override
  String get deferralPending => 'Deferral Pending';

  @override
  String get deferralRequest => 'Request Deferral';

  @override
  String get cancelDeferral => 'Cancel Deferral';
}
