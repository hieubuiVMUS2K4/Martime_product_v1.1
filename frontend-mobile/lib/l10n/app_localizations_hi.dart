// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for Hindi (`hi`).
class AppLocalizationsHi extends AppLocalizations {
  AppLocalizationsHi([String locale = 'hi']) : super(locale);

  @override
  String get appName => 'Maritime Crew ऐप';

  @override
  String get login => 'लॉगिन';

  @override
  String get logout => 'लॉगआउट';

  @override
  String get username => 'यूज़रनेम';

  @override
  String get password => 'पासवर्ड';

  @override
  String get pleaseEnterUsername => 'कृपया यूज़रनेम दर्ज करें';

  @override
  String get pleaseEnterPassword => 'कृपया पासवर्ड दर्ज करें';

  @override
  String get loginFailed => 'लॉगिन विफल रहा';

  @override
  String get loginSuccess => 'लॉगिन सफल रहा';

  @override
  String get dashboard => 'डैशबोर्ड';

  @override
  String get tasks => 'कार्य';

  @override
  String get schedule => 'कार्यक्रम';

  @override
  String get alarms => 'अलार्म';

  @override
  String get settings => 'सेटिंग्स';

  @override
  String get myTasks => 'मेरे कार्य';

  @override
  String get taskDetails => 'कार्य विवरण';

  @override
  String get startTask => 'शुरू करें';

  @override
  String get completeTask => 'पूर्ण करें';

  @override
  String get taskStatus => 'स्थिति';

  @override
  String get taskPriority => 'प्राथमिकता';

  @override
  String get dueDate => 'नियत तारीख';

  @override
  String get estimatedTime => 'अनुमानित समय';

  @override
  String get description => 'विवरण';

  @override
  String get statusPending => 'लंबित';

  @override
  String get statusInProgress => 'प्रगति में';

  @override
  String get statusCompleted => 'पूर्ण';

  @override
  String get statusOverdue => 'समय सीमा समाप्त';

  @override
  String get priorityCritical => 'अत्यावश्यक';

  @override
  String get priorityHigh => 'उच्च';

  @override
  String get priorityNormal => 'सामान्य';

  @override
  String get priorityLow => 'निम्न';

  @override
  String get account => 'खाता';

  @override
  String get synchronization => 'समन्वयन';

  @override
  String get serverConfiguration => 'सर्वर कॉन्फ़िगरेशन';

  @override
  String get dataStorage => 'डेटा और स्टोरेज';

  @override
  String get clearCache => 'कैश साफ़ करें';

  @override
  String get removeAllCachedData => 'सभी कैश किए गए डेटा को हटाएं';

  @override
  String get about => 'के बारे में';

  @override
  String get version => 'संस्करण';

  @override
  String get license => 'लाइसेंस';

  @override
  String get proprietary => 'स्वामित्व';

  @override
  String get syncStatus => 'समन्वयन स्थिति';

  @override
  String get offline => 'ऑफ़लाइन';

  @override
  String get online => 'ऑनलाइन';

  @override
  String itemsWaitingToSync(int count) {
    return '$count आइटम समन्वयन के लिए प्रतीक्षारत';
  }

  @override
  String get syncNow => 'अभी समन्वयित करें';

  @override
  String lastSyncAt(String time) {
    return 'अंतिम समन्वयन $time पर';
  }

  @override
  String get serverUrl => 'सर्वर URL';

  @override
  String get testConnection => 'कनेक्शन परीक्षण करें';

  @override
  String get saveConfiguration => 'कॉन्फ़िगरेशन सहेजें';

  @override
  String get connectionSuccessful => 'कनेक्शन सफल रहा!';

  @override
  String get connectionFailed => 'कनेक्शन विफल रहा';

  @override
  String get pleaseEnterServerUrl => 'कृपया सर्वर URL दर्ज करें';

  @override
  String get invalidUrlFormat =>
      'अमान्य URL प्रारूप (http:// या https:// से शुरू होना चाहिए)';

  @override
  String get clearCacheTitle => 'कैश साफ़ करें';

  @override
  String get clearCacheMessage =>
      'क्या आप वाकई सभी कैश किए गए डेटा को साफ़ करना चाहते हैं? यह सभी ऑफ़लाइन डेटा को हटा देगा।';

  @override
  String get cacheClearedSuccess => 'कैश सफलतापूर्वक साफ़ किया गया';

  @override
  String get cancel => 'रद्द करें';

  @override
  String get confirm => 'पुष्टि करें';

  @override
  String get close => 'Close';

  @override
  String get logoutTitle => 'लॉगआउट';

  @override
  String get logoutMessage => 'क्या आप वाकई लॉगआउट करना चाहते हैं?';

  @override
  String get noTasksAvailable => 'कोई कार्य उपलब्ध नहीं है';

  @override
  String get loadingTasks => 'कार्य लोड हो रहे हैं...';

  @override
  String get errorLoadingTasks => 'कार्य लोड करने में त्रुटि';

  @override
  String get retry => 'पुनः प्रयास करें';

  @override
  String get language => 'भाषा';

  @override
  String get languageSettings => 'भाषा सेटिंग्स';

  @override
  String get selectLanguage => 'भाषा चुनें';

  @override
  String get languageChanged => 'भाषा सफलतापूर्वक बदली गई';

  @override
  String get restartRequired =>
      'भाषा परिवर्तन लागू करने के लिए कृपया ऐप को पुनः प्रारंभ करें';

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
  String get user => 'उपयोगकर्ता';

  @override
  String get position => 'पद';

  @override
  String get crewId => 'आईडी';

  @override
  String get notAvailable => 'उपलब्ध नहीं';

  @override
  String get justNow => 'अभी';

  @override
  String minutesAgo(int minutes) {
    return '$minutes मिनट पहले';
  }

  @override
  String hoursAgo(int hours) {
    return '$hours घंटे पहले';
  }

  @override
  String get home => 'होम';

  @override
  String get profile => 'प्रोफ़ाइल';

  @override
  String get myCertificates => 'मेरे प्रमाण पत्र';

  @override
  String get myProfile => 'मेरी प्रोफ़ाइल';

  @override
  String get expired => 'समाप्त';

  @override
  String get expiring => 'समाप्त होने वाला';

  @override
  String get safetyAlarms => 'सुरक्षा अलार्म';

  @override
  String get alarmDetails => 'अलार्म विवरण';

  @override
  String get alarmStatistics => 'अलार्म सांख्यिकी';

  @override
  String get alarmHistory => 'अलार्म इतिहास';

  @override
  String get acknowledge => 'पुष्टि करें';

  @override
  String get resolve => 'हल करें';

  @override
  String get alarmAcknowledged => 'अलार्म की पुष्टि की गई';

  @override
  String get alarmResolved => 'अलार्म हल हो गया';

  @override
  String get failedToAcknowledgeAlarm => 'अलार्म की पुष्टि विफल रही';

  @override
  String get failedToResolveAlarm => 'अलार्म हल करने में विफल';

  @override
  String get confirmResolution => 'समाधान की पुष्टि करें';

  @override
  String get areYouSureResolveAlarm =>
      'क्या आप वाकई इस अलार्म को हल करना चाहते हैं?';

  @override
  String get allSystemsNormal => 'सभी सिस्टम सामान्य हैं';

  @override
  String get generateSampleAlarms => 'नमूना अलार्म उत्पन्न करें';

  @override
  String get sampleAlarmsGenerated => 'नमूना अलार्म उत्पन्न हुए';

  @override
  String get noDataAvailable => 'कोई डेटा उपलब्ध नहीं है';

  @override
  String get newWatchLog => 'नई वॉच लॉग';

  @override
  String get watchLogDetails => 'वॉच लॉग विवरण';

  @override
  String get watchLogNotFound => 'वॉच लॉग नहीं मिला';

  @override
  String get addLogEntry => 'लॉग प्रविष्टि जोड़ें';

  @override
  String get addNotableEvents =>
      'अपनी वॉच के दौरान उल्लेखनीय घटनाओं या अवलोकनों को जोड़ें:';

  @override
  String get logEntrySaved =>
      'लॉग प्रविष्टि सहेजी गई। कप्तान के हस्ताक्षर की प्रतीक्षा में।';

  @override
  String get watchLogCreatedSuccessfully => 'वॉच लॉग सफलतापूर्वक बनाया गया';

  @override
  String get watchDate => 'वॉच तारीख';

  @override
  String errorCompletingTask(String error) {
    return 'कार्य पूर्ण करने में त्रुटि: $error';
  }

  @override
  String get pleaseEnterMeasuredValue => 'कृपया मापा गया मान दर्ज करें!';

  @override
  String get invalidValue => 'अमान्य मान!';

  @override
  String valueTooLow(String min) {
    return 'मान बहुत कम है! न्यूनतम: $min';
  }

  @override
  String valueTooHigh(String max) {
    return 'मान बहुत अधिक है! अधिकतम: $max';
  }

  @override
  String get pleaseEnterObservationNote => 'कृपया अवलोकन टिप्पणी दर्ज करें!';

  @override
  String get serverConfigurationSaved =>
      'सर्वर URL सफलतापूर्वक सहेजा गया! कृपया ऐप को पुनः प्रारंभ करें।';

  @override
  String failedToSaveUrl(String error) {
    return 'URL सहेजने में विफल: $error';
  }

  @override
  String get resetToDefault => 'डिफ़ॉल्ट पर रीसेट करें';

  @override
  String loginFailedError(String error) {
    return 'लॉगिन विफल रहा: $error';
  }

  @override
  String get serverSettings => 'सर्वर सेटिंग्स';

  @override
  String get sync => 'समन्वयित करें';

  @override
  String get save => 'सहेजें';

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
    return 'रीफ्रेश करने में विफल: $error';
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
  String get okPass => 'OK / उत्तीर्ण';

  @override
  String get ngFail => 'NG / अनुत्तीर्ण';

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
  String get taskOverdue => 'कार्य समय सीमा समाप्त';

  @override
  String get thisTaskIsOverdue =>
      'यह कार्य समय सीमा समाप्त हो चुका है। क्या आप फिर भी शुरू करना चाहते हैं?';

  @override
  String get startAnyway => 'फिर भी शुरू करें';

  @override
  String get overdueTaskStarted =>
      'Overdue task started! Please complete ASAP.';

  @override
  String get taskStartedSuccessfully => 'Task started successfully!';

  @override
  String failedToStartTask(String error) {
    return 'कार्य शुरू करने में विफल: $error';
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
