// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for Japanese (`ja`).
class AppLocalizationsJa extends AppLocalizations {
  AppLocalizationsJa([String locale = 'ja']) : super(locale);

  @override
  String get appName => '船員アプリ';

  @override
  String get login => 'ログイン';

  @override
  String get logout => 'ログアウト';

  @override
  String get username => 'ユーザー名';

  @override
  String get password => 'パスワード';

  @override
  String get pleaseEnterUsername => 'ユーザー名を入力してください';

  @override
  String get pleaseEnterPassword => 'パスワードを入力してください';

  @override
  String get loginFailed => 'ログインに失敗しました';

  @override
  String get loginSuccess => 'ログインに成功しました';

  @override
  String get dashboard => 'ダッシュボード';

  @override
  String get tasks => 'タスク';

  @override
  String get schedule => 'スケジュール';

  @override
  String get alarms => 'アラーム';

  @override
  String get settings => '設定';

  @override
  String get myTasks => 'マイタスク';

  @override
  String get taskDetails => 'タスク詳細';

  @override
  String get startTask => 'タスクを開始';

  @override
  String get completeTask => '完了';

  @override
  String get taskStatus => 'タスクステータス';

  @override
  String get taskPriority => '優先度';

  @override
  String get dueDate => '期限';

  @override
  String get estimatedTime => '予想時間';

  @override
  String get description => '説明';

  @override
  String get statusPending => '保留中';

  @override
  String get statusInProgress => '進行中';

  @override
  String get statusCompleted => '完了';

  @override
  String get statusOverdue => '期限切れ';

  @override
  String get priorityCritical => '緊急';

  @override
  String get priorityHigh => '高';

  @override
  String get priorityNormal => '通常';

  @override
  String get priorityLow => '低';

  @override
  String get account => 'アカウント';

  @override
  String get synchronization => '同期';

  @override
  String get serverConfiguration => 'サーバー設定';

  @override
  String get dataStorage => 'データストレージ';

  @override
  String get clearCache => 'キャッシュをクリア';

  @override
  String get removeAllCachedData => 'すべてのキャッシュデータを削除';

  @override
  String get about => 'について';

  @override
  String get version => 'バージョン';

  @override
  String get license => 'ライセンス';

  @override
  String get proprietary => '専有';

  @override
  String get syncStatus => '同期状態';

  @override
  String get offline => 'オフライン';

  @override
  String get online => 'オンライン';

  @override
  String itemsWaitingToSync(int count) {
    return '$count 項目が同期待ち';
  }

  @override
  String get syncNow => '今すぐ同期';

  @override
  String lastSyncAt(String time) {
    return '最終同期 $time';
  }

  @override
  String get serverUrl => 'サーバーURL';

  @override
  String get testConnection => '接続テスト';

  @override
  String get saveConfiguration => '設定を保存';

  @override
  String get connectionSuccessful => '接続成功！';

  @override
  String get connectionFailed => '接続失敗';

  @override
  String get pleaseEnterServerUrl => 'サーバーURLを入力してください';

  @override
  String get invalidUrlFormat => '無効なURL形式（http://またはhttps://で始まる必要があります）';

  @override
  String get clearCacheTitle => 'キャッシュをクリア';

  @override
  String get clearCacheMessage =>
      'すべてのキャッシュデータをクリアしてもよろしいですか？これによりすべてのオフラインデータが削除されます。';

  @override
  String get cacheClearedSuccess => 'キャッシュを正常にクリアしました';

  @override
  String get cancel => 'キャンセル';

  @override
  String get confirm => '確認';

  @override
  String get logoutTitle => 'ログアウト';

  @override
  String get logoutMessage => 'ログアウトしてもよろしいですか？';

  @override
  String get noTasksAvailable => '利用可能なタスクはありません';

  @override
  String get loadingTasks => 'タスクを読み込み中...';

  @override
  String get errorLoadingTasks => 'タスクの読み込みエラー';

  @override
  String get retry => '再試行';

  @override
  String get language => '言語';

  @override
  String get languageSettings => '言語設定';

  @override
  String get selectLanguage => '言語を選択';

  @override
  String get languageChanged => '言語が正常に変更されました';

  @override
  String get restartRequired => '言語変更を適用するにはアプリを再起動してください';

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
  String get user => 'ユーザー';

  @override
  String get position => '役職';

  @override
  String get crewId => 'ID';

  @override
  String get notAvailable => '利用不可';

  @override
  String get justNow => 'たった今';

  @override
  String minutesAgo(int minutes) {
    return '$minutes分前';
  }

  @override
  String hoursAgo(int hours) {
    return '$hours時間前';
  }

  @override
  String get home => 'ホーム';

  @override
  String get profile => 'プロフィール';

  @override
  String get myCertificates => '私の資格証明書';

  @override
  String get myProfile => 'マイプロフィール';

  @override
  String get expired => '期限切れ';

  @override
  String get expiring => '期限切れ間近';

  @override
  String get safetyAlarms => '安全アラーム';

  @override
  String get alarmDetails => 'アラーム詳細';

  @override
  String get alarmStatistics => 'アラーム統計';

  @override
  String get alarmHistory => 'アラーム履歴';

  @override
  String get acknowledge => '確認';

  @override
  String get resolve => '解決';

  @override
  String get alarmAcknowledged => 'アラームを確認しました';

  @override
  String get alarmResolved => 'アラームを解決しました';

  @override
  String get failedToAcknowledgeAlarm => 'アラームの確認に失敗しました';

  @override
  String get failedToResolveAlarm => 'アラームの解決に失敗しました';

  @override
  String get confirmResolution => '解決の確認';

  @override
  String get areYouSureResolveAlarm => 'このアラームを解決してもよろしいですか？';

  @override
  String get allSystemsNormal => 'すべてのシステムが正常です';

  @override
  String get generateSampleAlarms => 'サンプルアラームを生成';

  @override
  String get sampleAlarmsGenerated => 'サンプルアラームが生成されました';

  @override
  String get noDataAvailable => '利用可能なデータがありません';

  @override
  String get newWatchLog => '新しい当直日誌';

  @override
  String get watchLogDetails => '当直日誌詳細';

  @override
  String get watchLogNotFound => '当直日誌が見つかりません';

  @override
  String get addLogEntry => 'ログエントリを追加';

  @override
  String get addNotableEvents => '当直中の注目すべき出来事や観察事項を追加してください：';

  @override
  String get logEntrySaved => 'ログエントリが保存されました。船長の署名待ちです。';

  @override
  String get watchLogCreatedSuccessfully => '当直日誌が正常に作成されました';

  @override
  String get watchDate => '当直日';

  @override
  String errorCompletingTask(String error) {
    return 'タスク完了エラー: $error';
  }

  @override
  String get pleaseEnterMeasuredValue => '測定値を入力してください！';

  @override
  String get invalidValue => '無効な値！';

  @override
  String valueTooLow(String min) {
    return '値が低すぎます！最小値: $min';
  }

  @override
  String valueTooHigh(String max) {
    return '値が高すぎます！最大値: $max';
  }

  @override
  String get pleaseEnterObservationNote => '観察メモを入力してください！';

  @override
  String get serverConfigurationSaved => 'サーバーURLが正常に保存されました！アプリを再起動してください。';

  @override
  String failedToSaveUrl(String error) {
    return 'URLの保存に失敗しました: $error';
  }

  @override
  String get resetToDefault => 'デフォルトにリセット';

  @override
  String loginFailedError(String error) {
    return 'ログインに失敗しました: $error';
  }

  @override
  String get serverSettings => 'サーバー設定';

  @override
  String get sync => '同期';

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
    return '更新に失敗しました: $error';
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
  String get taskOverdue => 'タスク期限切れ';

  @override
  String get thisTaskIsOverdue => 'このタスクは期限切れです。それでも開始しますか？';

  @override
  String get startAnyway => 'それでも開始';

  @override
  String get overdueTaskStarted =>
      'Overdue task started! Please complete ASAP.';

  @override
  String get taskStartedSuccessfully => 'Task started successfully!';

  @override
  String failedToStartTask(String error) {
    return 'タスクの開始に失敗しました: $error';
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
