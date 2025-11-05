// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for Korean (`ko`).
class AppLocalizationsKo extends AppLocalizations {
  AppLocalizationsKo([String locale = 'ko']) : super(locale);

  @override
  String get appName => '선원 앱';

  @override
  String get login => '로그인';

  @override
  String get logout => '로그아웃';

  @override
  String get username => '사용자 이름';

  @override
  String get password => '비밀번호';

  @override
  String get pleaseEnterUsername => '사용자 이름을 입력하세요';

  @override
  String get pleaseEnterPassword => '비밀번호를 입력하세요';

  @override
  String get loginFailed => '로그인 실패';

  @override
  String get loginSuccess => '로그인 성공';

  @override
  String get dashboard => '대시보드';

  @override
  String get tasks => '작업';

  @override
  String get schedule => '일정';

  @override
  String get alarms => '알람';

  @override
  String get settings => '설정';

  @override
  String get myTasks => '내 작업';

  @override
  String get taskDetails => '작업 세부정보';

  @override
  String get startTask => '작업 시작';

  @override
  String get completeTask => '완료';

  @override
  String get taskStatus => '작업 상태';

  @override
  String get taskPriority => '우선순위';

  @override
  String get dueDate => '마감일';

  @override
  String get estimatedTime => '예상 시간';

  @override
  String get description => '설명';

  @override
  String get statusPending => '대기 중';

  @override
  String get statusInProgress => '진행 중';

  @override
  String get statusCompleted => '완료됨';

  @override
  String get statusOverdue => '기한 초과';

  @override
  String get priorityCritical => '긴급';

  @override
  String get priorityHigh => '높음';

  @override
  String get priorityNormal => '보통';

  @override
  String get priorityLow => '낮음';

  @override
  String get account => '계정';

  @override
  String get synchronization => '동기화';

  @override
  String get serverConfiguration => '서버 구성';

  @override
  String get dataStorage => '데이터 저장소';

  @override
  String get clearCache => '캐시 지우기';

  @override
  String get removeAllCachedData => '모든 캐시 데이터 제거';

  @override
  String get about => '정보';

  @override
  String get version => '버전';

  @override
  String get license => '라이선스';

  @override
  String get proprietary => '독점';

  @override
  String get syncStatus => '동기화 상태';

  @override
  String get offline => '오프라인';

  @override
  String get online => '온라인';

  @override
  String itemsWaitingToSync(int count) {
    return '$count개 항목이 동기화 대기 중';
  }

  @override
  String get syncNow => '지금 동기화';

  @override
  String lastSyncAt(String time) {
    return '마지막 동기화 $time';
  }

  @override
  String get serverUrl => '서버 URL';

  @override
  String get testConnection => '연결 테스트';

  @override
  String get saveConfiguration => '구성 저장';

  @override
  String get connectionSuccessful => '연결 성공!';

  @override
  String get connectionFailed => '연결 실패';

  @override
  String get pleaseEnterServerUrl => '서버 URL을 입력하세요';

  @override
  String get invalidUrlFormat => '잘못된 URL 형식 (http:// 또는 https://로 시작해야 함)';

  @override
  String get clearCacheTitle => '캐시 지우기';

  @override
  String get clearCacheMessage =>
      '모든 캐시 데이터를 지우시겠습니까? 이렇게 하면 모든 오프라인 데이터가 제거됩니다.';

  @override
  String get cacheClearedSuccess => '캐시를 성공적으로 지웠습니다';

  @override
  String get cancel => '취소';

  @override
  String get confirm => '확인';

  @override
  String get logoutTitle => '로그아웃';

  @override
  String get logoutMessage => '로그아웃하시겠습니까?';

  @override
  String get noTasksAvailable => '사용 가능한 작업이 없습니다';

  @override
  String get loadingTasks => '작업 로드 중...';

  @override
  String get errorLoadingTasks => '작업 로드 오류';

  @override
  String get retry => '다시 시도';

  @override
  String get language => '언어';

  @override
  String get languageSettings => '언어 설정';

  @override
  String get selectLanguage => '언어 선택';

  @override
  String get languageChanged => '언어가 성공적으로 변경되었습니다';

  @override
  String get restartRequired => '언어 변경을 적용하려면 앱을 다시 시작하세요';

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
  String get user => '사용자';

  @override
  String get position => '직위';

  @override
  String get crewId => 'ID';

  @override
  String get notAvailable => '사용 불가';

  @override
  String get justNow => '방금';

  @override
  String minutesAgo(int minutes) {
    return '$minutes분 전';
  }

  @override
  String hoursAgo(int hours) {
    return '$hours시간 전';
  }

  @override
  String get home => '홈';

  @override
  String get profile => '프로필';

  @override
  String get myCertificates => '내 자격증';

  @override
  String get myProfile => '내 프로필';

  @override
  String get expired => '만료됨';

  @override
  String get expiring => '만료 예정';

  @override
  String get safetyAlarms => '안전 알람';

  @override
  String get alarmDetails => '알람 세부정보';

  @override
  String get alarmStatistics => '알람 통계';

  @override
  String get alarmHistory => '알람 기록';

  @override
  String get acknowledge => '확인';

  @override
  String get resolve => '해결';

  @override
  String get alarmAcknowledged => '알람 확인됨';

  @override
  String get alarmResolved => '알람 해결됨';

  @override
  String get failedToAcknowledgeAlarm => '알람 확인 실패';

  @override
  String get failedToResolveAlarm => '알람 해결 실패';

  @override
  String get confirmResolution => '해결 확인';

  @override
  String get areYouSureResolveAlarm => '이 알람을 해결하시겠습니까?';

  @override
  String get allSystemsNormal => '모든 시스템 정상';

  @override
  String get generateSampleAlarms => '샘플 알람 생성';

  @override
  String get sampleAlarmsGenerated => '샘플 알람이 생성되었습니다';

  @override
  String get noDataAvailable => '사용 가능한 데이터 없음';

  @override
  String get newWatchLog => '새 당직 일지';

  @override
  String get watchLogDetails => '당직 일지 세부정보';

  @override
  String get watchLogNotFound => '당직 일지를 찾을 수 없습니다';

  @override
  String get addLogEntry => '일지 항목 추가';

  @override
  String get addNotableEvents => '당직 중 주목할 만한 사건이나 관찰 사항을 추가하세요:';

  @override
  String get logEntrySaved => '일지 항목이 저장되었습니다. 선장 서명 대기 중.';

  @override
  String get watchLogCreatedSuccessfully => '당직 일지가 성공적으로 생성되었습니다';

  @override
  String get watchDate => '당직 날짜';

  @override
  String errorCompletingTask(String error) {
    return '작업 완료 오류: $error';
  }

  @override
  String get pleaseEnterMeasuredValue => '측정 값을 입력하세요!';

  @override
  String get invalidValue => '잘못된 값!';

  @override
  String valueTooLow(String min) {
    return '값이 너무 낮습니다! 최소값: $min';
  }

  @override
  String valueTooHigh(String max) {
    return '값이 너무 높습니다! 최대값: $max';
  }

  @override
  String get pleaseEnterObservationNote => '관찰 메모를 입력하세요!';

  @override
  String get serverConfigurationSaved => '서버 URL이 성공적으로 저장되었습니다! 앱을 다시 시작하세요.';

  @override
  String failedToSaveUrl(String error) {
    return 'URL 저장 실패: $error';
  }

  @override
  String get resetToDefault => '기본값으로 재설정';

  @override
  String loginFailedError(String error) {
    return '로그인 실패: $error';
  }

  @override
  String get serverSettings => '서버 설정';

  @override
  String get sync => '동기화';

  @override
  String get save => '저장';

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
    return '새로고침 실패: $error';
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
  String get okPass => 'OK / 합격';

  @override
  String get ngFail => 'NG / 불합격';

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
  String get taskOverdue => '작업 기한 초과';

  @override
  String get thisTaskIsOverdue => '이 작업은 기한이 지났습니다. 그래도 시작하시겠습니까?';

  @override
  String get startAnyway => '그래도 시작';

  @override
  String get overdueTaskStarted =>
      'Overdue task started! Please complete ASAP.';

  @override
  String get taskStartedSuccessfully => 'Task started successfully!';

  @override
  String failedToStartTask(String error) {
    return '작업 시작 실패: $error';
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
