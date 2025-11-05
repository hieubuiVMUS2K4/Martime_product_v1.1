// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for Filipino Pilipino (`fil`).
class AppLocalizationsFil extends AppLocalizations {
  AppLocalizationsFil([String locale = 'fil']) : super(locale);

  @override
  String get appName => 'Maritime Crew App';

  @override
  String get login => 'Mag-login';

  @override
  String get logout => 'Mag-logout';

  @override
  String get username => 'Username';

  @override
  String get password => 'Password';

  @override
  String get pleaseEnterUsername => 'Pakiusap ipasok ang username';

  @override
  String get pleaseEnterPassword => 'Pakiusap ipasok ang password';

  @override
  String get loginFailed => 'Hindi matagumpay ang pag-login';

  @override
  String get loginSuccess => 'Matagumpay na nag-login';

  @override
  String get dashboard => 'Dashboard';

  @override
  String get tasks => 'Mga Gawain';

  @override
  String get schedule => 'Iskedyul';

  @override
  String get alarms => 'Mga Alarma';

  @override
  String get settings => 'Mga Setting';

  @override
  String get myTasks => 'Aking Mga Gawain';

  @override
  String get taskDetails => 'Detalye ng Gawain';

  @override
  String get startTask => 'Simulan';

  @override
  String get completeTask => 'Tapusin';

  @override
  String get taskStatus => 'Katayuan';

  @override
  String get taskPriority => 'Priyoridad';

  @override
  String get dueDate => 'Petsa ng Deadline';

  @override
  String get estimatedTime => 'Tinatayang Oras';

  @override
  String get description => 'Paglalarawan';

  @override
  String get statusPending => 'Naghihintay';

  @override
  String get statusInProgress => 'Kasalukuyang Ginagawa';

  @override
  String get statusCompleted => 'Tapos na';

  @override
  String get statusOverdue => 'Lampas sa Takdang Oras';

  @override
  String get priorityCritical => 'Kritikal';

  @override
  String get priorityHigh => 'Mataas';

  @override
  String get priorityNormal => 'Normal';

  @override
  String get priorityLow => 'Mababa';

  @override
  String get account => 'Account';

  @override
  String get synchronization => 'Synchronization';

  @override
  String get serverConfiguration => 'Konpigurasyon ng Server';

  @override
  String get dataStorage => 'Data at Storage';

  @override
  String get clearCache => 'Burahin ang Cache';

  @override
  String get removeAllCachedData => 'Tanggalin ang lahat ng naka-cache na data';

  @override
  String get about => 'Tungkol sa';

  @override
  String get version => 'Bersyon';

  @override
  String get license => 'Lisensya';

  @override
  String get proprietary => 'Proprietary';

  @override
  String get syncStatus => 'Katayuan ng Sync';

  @override
  String get offline => 'Offline';

  @override
  String get online => 'Online';

  @override
  String itemsWaitingToSync(int count) {
    return '$count mga item na naghihintay na i-sync';
  }

  @override
  String get syncNow => 'I-sync Ngayon';

  @override
  String lastSyncAt(String time) {
    return 'Huling sync sa $time';
  }

  @override
  String get serverUrl => 'URL ng Server';

  @override
  String get testConnection => 'Subukan ang Koneksyon';

  @override
  String get saveConfiguration => 'I-save ang Konpigurasyon';

  @override
  String get connectionSuccessful => 'Matagumpay ang koneksyon!';

  @override
  String get connectionFailed => 'Hindi matagumpay ang koneksyon';

  @override
  String get pleaseEnterServerUrl => 'Pakiusap ipasok ang URL ng server';

  @override
  String get invalidUrlFormat =>
      'Hindi wastong format ng URL (dapat magsimula sa http:// o https://)';

  @override
  String get clearCacheTitle => 'Burahin ang Cache';

  @override
  String get clearCacheMessage =>
      'Sigurado ka bang gusto mong burahin ang lahat ng naka-cache na data? Ito ay magtanggal ng lahat ng offline data.';

  @override
  String get cacheClearedSuccess => 'Matagumpay na nabura ang cache';

  @override
  String get cancel => 'Kanselahin';

  @override
  String get confirm => 'Kumpirmahin';

  @override
  String get logoutTitle => 'Mag-logout';

  @override
  String get logoutMessage => 'Sigurado ka bang gusto mong mag-logout?';

  @override
  String get noTasksAvailable => 'Walang available na gawain';

  @override
  String get loadingTasks => 'Nilo-load ang mga gawain...';

  @override
  String get errorLoadingTasks => 'Error sa pag-load ng mga gawain';

  @override
  String get retry => 'Subukan Muli';

  @override
  String get language => 'Wika';

  @override
  String get languageSettings => 'Mga Setting ng Wika';

  @override
  String get selectLanguage => 'Pumili ng Wika';

  @override
  String get languageChanged => 'Matagumpay na nabago ang wika';

  @override
  String get restartRequired =>
      'Pakiusap i-restart ang app upang ilapat ang mga pagbabago sa wika';

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
  String get position => 'Posisyon';

  @override
  String get crewId => 'ID';

  @override
  String get notAvailable => 'Walang data';

  @override
  String get justNow => 'Ngayon lang';

  @override
  String minutesAgo(int minutes) {
    return '${minutes}m nakaraan';
  }

  @override
  String hoursAgo(int hours) {
    return '${hours}h nakaraan';
  }

  @override
  String get home => 'Home';

  @override
  String get profile => 'Profile';

  @override
  String get myCertificates => 'Aking Mga Sertipiko';

  @override
  String get myProfile => 'Aking Profile';

  @override
  String get expired => 'NATAPOS NA';

  @override
  String get expiring => 'MALAPIT NANG MATAPOS';

  @override
  String get safetyAlarms => 'Mga Alarma ng Kaligtasan';

  @override
  String get alarmDetails => 'Detalye ng Alarma';

  @override
  String get alarmStatistics => 'Istatistika ng Alarma';

  @override
  String get alarmHistory => 'Kasaysayan ng Alarma';

  @override
  String get acknowledge => 'Kilalanin';

  @override
  String get resolve => 'Lutasin';

  @override
  String get alarmAcknowledged => 'Nakilala ang alarma';

  @override
  String get alarmResolved => 'Nalutas ang alarma';

  @override
  String get failedToAcknowledgeAlarm => 'Hindi makilala ang alarma';

  @override
  String get failedToResolveAlarm => 'Hindi malutas ang alarma';

  @override
  String get confirmResolution => 'Kumpirmahin ang Paglutas';

  @override
  String get areYouSureResolveAlarm =>
      'Sigurado ka bang gusto mong lutasin ang alarmang ito?';

  @override
  String get allSystemsNormal => 'Lahat ng sistema ay normal';

  @override
  String get generateSampleAlarms => 'Bumuo ng Sample Alarms';

  @override
  String get sampleAlarmsGenerated => 'Nabuo ang sample alarms';

  @override
  String get noDataAvailable => 'Walang available na data';

  @override
  String get newWatchLog => 'Bagong Watch Log';

  @override
  String get watchLogDetails => 'Detalye ng Watch Log';

  @override
  String get watchLogNotFound => 'Hindi natagpuan ang watch log';

  @override
  String get addLogEntry => 'Magdagdag ng Log Entry';

  @override
  String get addNotableEvents =>
      'Magdagdag ng mga kapansin-pansing kaganapan o obserbasyon sa iyong pagbantay:';

  @override
  String get logEntrySaved =>
      'Na-save ang log entry. Naghihintay ng pirma ng Master.';

  @override
  String get watchLogCreatedSuccessfully =>
      'Matagumpay na nalikha ang watch log';

  @override
  String get watchDate => 'Petsa ng Pagbabantay';

  @override
  String errorCompletingTask(String error) {
    return 'Error sa pagtatapos ng gawain: $error';
  }

  @override
  String get pleaseEnterMeasuredValue =>
      'Pakiusap ipasok ang nasukat na halaga!';

  @override
  String get invalidValue => 'Hindi wastong halaga!';

  @override
  String valueTooLow(String min) {
    return 'Masyadong mababa ang halaga! Minimum: $min';
  }

  @override
  String valueTooHigh(String max) {
    return 'Masyadong mataas ang halaga! Maximum: $max';
  }

  @override
  String get pleaseEnterObservationNote => 'Pakiusap ipasok ang obserbasyon!';

  @override
  String get serverConfigurationSaved =>
      'Matagumpay na na-save ang Server URL! Pakiusap i-restart ang app.';

  @override
  String failedToSaveUrl(String error) {
    return 'Hindi ma-save ang URL: $error';
  }

  @override
  String get resetToDefault => 'I-reset sa Default';

  @override
  String loginFailedError(String error) {
    return 'Hindi matagumpay ang pag-login: $error';
  }

  @override
  String get serverSettings => 'Mga Setting ng Server';

  @override
  String get sync => 'I-sync';

  @override
  String get save => 'I-save';

  @override
  String get taskOverview => 'Pangkalahatang-ideya ng Gawain';

  @override
  String get quickAccess => 'Mabilis na Access';

  @override
  String get crewMember => 'Miyembro ng Tripulante';

  @override
  String get urgentAttention => 'Kagyat na Pansin!';

  @override
  String overdueTasksCount(int count) {
    return '$count lampas sa takdang panahon';
  }

  @override
  String itemsPending(int count) {
    return '$count item(s) nakabinbin';
  }

  @override
  String get watchSchedule => 'Iskedyul ng\nPagbantay';

  @override
  String get goodMorning => 'Magandang Umaga';

  @override
  String get goodAfternoon => 'Magandang Hapon';

  @override
  String get goodEvening => 'Magandang Gabi';

  @override
  String get goodNight => 'Magandang Gabi';

  @override
  String get updateLogEntry => 'I-update ang Log Entry';

  @override
  String get loadingWatchLog => 'Naglo-load ng watch log...';

  @override
  String get notableEvents => 'Mga Kapansin-pansing Kaganapan';

  @override
  String get notableEventsHint =>
      'Mga pagbabago sa kurso, mga barkong nakita, pagbabago ng panahon, atbp.';

  @override
  String get onlyMasterCanSign =>
      'Ang Master lamang ang maaaring pumirma at tapusin ang mga watch log.';

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
    return 'Hindi ma-refresh: $error';
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
  String get okPass => 'OK / Pumasa';

  @override
  String get ngFail => 'NG / Bumagsak';

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
  String get taskOverdue => 'Lampas sa Takdang Oras ang Gawain';

  @override
  String get thisTaskIsOverdue =>
      'Ang gawaing ito ay lampas na sa takdang oras. Gusto mo pa rin bang simulan ito?';

  @override
  String get startAnyway => 'Simulan Pa Rin';

  @override
  String get overdueTaskStarted =>
      'Overdue task started! Please complete ASAP.';

  @override
  String get taskStartedSuccessfully => 'Task started successfully!';

  @override
  String failedToStartTask(String error) {
    return 'Hindi masimulan ang gawain: $error';
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
