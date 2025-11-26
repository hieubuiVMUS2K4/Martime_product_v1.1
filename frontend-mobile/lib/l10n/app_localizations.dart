import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:flutter/widgets.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:intl/intl.dart' as intl;

import 'app_localizations_en.dart';
import 'app_localizations_fil.dart';
import 'app_localizations_hi.dart';
import 'app_localizations_ja.dart';
import 'app_localizations_ko.dart';
import 'app_localizations_vi.dart';
import 'app_localizations_zh.dart';

// ignore_for_file: type=lint

/// Callers can lookup localized strings with an instance of AppLocalizations
/// returned by `AppLocalizations.of(context)`.
///
/// Applications need to include `AppLocalizations.delegate()` in their app's
/// `localizationDelegates` list, and the locales they support in the app's
/// `supportedLocales` list. For example:
///
/// ```dart
/// import 'l10n/app_localizations.dart';
///
/// return MaterialApp(
///   localizationsDelegates: AppLocalizations.localizationsDelegates,
///   supportedLocales: AppLocalizations.supportedLocales,
///   home: MyApplicationHome(),
/// );
/// ```
///
/// ## Update pubspec.yaml
///
/// Please make sure to update your pubspec.yaml to include the following
/// packages:
///
/// ```yaml
/// dependencies:
///   # Internationalization support.
///   flutter_localizations:
///     sdk: flutter
///   intl: any # Use the pinned version from flutter_localizations
///
///   # Rest of dependencies
/// ```
///
/// ## iOS Applications
///
/// iOS applications define key application metadata, including supported
/// locales, in an Info.plist file that is built into the application bundle.
/// To configure the locales supported by your app, you’ll need to edit this
/// file.
///
/// First, open your project’s ios/Runner.xcworkspace Xcode workspace file.
/// Then, in the Project Navigator, open the Info.plist file under the Runner
/// project’s Runner folder.
///
/// Next, select the Information Property List item, select Add Item from the
/// Editor menu, then select Localizations from the pop-up menu.
///
/// Select and expand the newly-created Localizations item then, for each
/// locale your application supports, add a new item and select the locale
/// you wish to add from the pop-up menu in the Value field. This list should
/// be consistent with the languages listed in the AppLocalizations.supportedLocales
/// property.
abstract class AppLocalizations {
  AppLocalizations(String locale)
      : localeName = intl.Intl.canonicalizedLocale(locale.toString());

  final String localeName;

  static AppLocalizations of(BuildContext context) {
    return Localizations.of<AppLocalizations>(context, AppLocalizations)!;
  }

  static const LocalizationsDelegate<AppLocalizations> delegate =
      _AppLocalizationsDelegate();

  /// A list of this localizations delegate along with the default localizations
  /// delegates.
  ///
  /// Returns a list of localizations delegates containing this delegate along with
  /// GlobalMaterialLocalizations.delegate, GlobalCupertinoLocalizations.delegate,
  /// and GlobalWidgetsLocalizations.delegate.
  ///
  /// Additional delegates can be added by appending to this list in
  /// MaterialApp. This list does not have to be used at all if a custom list
  /// of delegates is preferred or required.
  static const List<LocalizationsDelegate<dynamic>> localizationsDelegates =
      <LocalizationsDelegate<dynamic>>[
    delegate,
    GlobalMaterialLocalizations.delegate,
    GlobalCupertinoLocalizations.delegate,
    GlobalWidgetsLocalizations.delegate,
  ];

  /// A list of this localizations delegate's supported locales.
  static const List<Locale> supportedLocales = <Locale>[
    Locale('en'),
    Locale('fil'),
    Locale('hi'),
    Locale('ja'),
    Locale('ko'),
    Locale('vi'),
    Locale('zh')
  ];

  /// The application name
  ///
  /// In en, this message translates to:
  /// **'Maritime Crew App'**
  String get appName;

  /// No description provided for @login.
  ///
  /// In en, this message translates to:
  /// **'Login'**
  String get login;

  /// No description provided for @logout.
  ///
  /// In en, this message translates to:
  /// **'Logout'**
  String get logout;

  /// No description provided for @username.
  ///
  /// In en, this message translates to:
  /// **'Username'**
  String get username;

  /// No description provided for @password.
  ///
  /// In en, this message translates to:
  /// **'Password'**
  String get password;

  /// No description provided for @pleaseEnterUsername.
  ///
  /// In en, this message translates to:
  /// **'Please enter username'**
  String get pleaseEnterUsername;

  /// No description provided for @pleaseEnterPassword.
  ///
  /// In en, this message translates to:
  /// **'Please enter password'**
  String get pleaseEnterPassword;

  /// No description provided for @loginFailed.
  ///
  /// In en, this message translates to:
  /// **'Login failed'**
  String get loginFailed;

  /// No description provided for @loginSuccess.
  ///
  /// In en, this message translates to:
  /// **'Login successful'**
  String get loginSuccess;

  /// No description provided for @dashboard.
  ///
  /// In en, this message translates to:
  /// **'Dashboard'**
  String get dashboard;

  /// No description provided for @tasks.
  ///
  /// In en, this message translates to:
  /// **'Tasks'**
  String get tasks;

  /// No description provided for @schedule.
  ///
  /// In en, this message translates to:
  /// **'Schedule'**
  String get schedule;

  /// No description provided for @alarms.
  ///
  /// In en, this message translates to:
  /// **'Alarms'**
  String get alarms;

  /// No description provided for @settings.
  ///
  /// In en, this message translates to:
  /// **'Settings'**
  String get settings;

  /// No description provided for @myTasks.
  ///
  /// In en, this message translates to:
  /// **'My Tasks'**
  String get myTasks;

  /// No description provided for @taskDetails.
  ///
  /// In en, this message translates to:
  /// **'Task Details'**
  String get taskDetails;

  /// No description provided for @startTask.
  ///
  /// In en, this message translates to:
  /// **'Start Task'**
  String get startTask;

  /// No description provided for @completeTask.
  ///
  /// In en, this message translates to:
  /// **'Complete Task'**
  String get completeTask;

  /// No description provided for @taskStatus.
  ///
  /// In en, this message translates to:
  /// **'Task Status'**
  String get taskStatus;

  /// No description provided for @taskPriority.
  ///
  /// In en, this message translates to:
  /// **'Priority'**
  String get taskPriority;

  /// No description provided for @dueDate.
  ///
  /// In en, this message translates to:
  /// **'Due Date'**
  String get dueDate;

  /// No description provided for @estimatedTime.
  ///
  /// In en, this message translates to:
  /// **'Estimated Time'**
  String get estimatedTime;

  /// No description provided for @description.
  ///
  /// In en, this message translates to:
  /// **'Description'**
  String get description;

  /// No description provided for @statusPending.
  ///
  /// In en, this message translates to:
  /// **'Pending'**
  String get statusPending;

  /// No description provided for @statusInProgress.
  ///
  /// In en, this message translates to:
  /// **'In Progress'**
  String get statusInProgress;

  /// No description provided for @statusCompleted.
  ///
  /// In en, this message translates to:
  /// **'Completed'**
  String get statusCompleted;

  /// No description provided for @statusOverdue.
  ///
  /// In en, this message translates to:
  /// **'Overdue'**
  String get statusOverdue;

  /// No description provided for @priorityCritical.
  ///
  /// In en, this message translates to:
  /// **'Critical'**
  String get priorityCritical;

  /// No description provided for @priorityHigh.
  ///
  /// In en, this message translates to:
  /// **'High'**
  String get priorityHigh;

  /// No description provided for @priorityNormal.
  ///
  /// In en, this message translates to:
  /// **'Normal'**
  String get priorityNormal;

  /// No description provided for @priorityLow.
  ///
  /// In en, this message translates to:
  /// **'Low'**
  String get priorityLow;

  /// No description provided for @account.
  ///
  /// In en, this message translates to:
  /// **'Account'**
  String get account;

  /// No description provided for @synchronization.
  ///
  /// In en, this message translates to:
  /// **'Synchronization'**
  String get synchronization;

  /// No description provided for @serverConfiguration.
  ///
  /// In en, this message translates to:
  /// **'Server Configuration'**
  String get serverConfiguration;

  /// No description provided for @dataStorage.
  ///
  /// In en, this message translates to:
  /// **'Data & Storage'**
  String get dataStorage;

  /// No description provided for @clearCache.
  ///
  /// In en, this message translates to:
  /// **'Clear Cache'**
  String get clearCache;

  /// No description provided for @removeAllCachedData.
  ///
  /// In en, this message translates to:
  /// **'Remove all cached data'**
  String get removeAllCachedData;

  /// No description provided for @about.
  ///
  /// In en, this message translates to:
  /// **'About'**
  String get about;

  /// No description provided for @version.
  ///
  /// In en, this message translates to:
  /// **'Version'**
  String get version;

  /// No description provided for @license.
  ///
  /// In en, this message translates to:
  /// **'License'**
  String get license;

  /// No description provided for @proprietary.
  ///
  /// In en, this message translates to:
  /// **'Proprietary'**
  String get proprietary;

  /// No description provided for @syncStatus.
  ///
  /// In en, this message translates to:
  /// **'Sync Status'**
  String get syncStatus;

  /// No description provided for @offline.
  ///
  /// In en, this message translates to:
  /// **'Offline'**
  String get offline;

  /// No description provided for @online.
  ///
  /// In en, this message translates to:
  /// **'Online'**
  String get online;

  /// No description provided for @itemsWaitingToSync.
  ///
  /// In en, this message translates to:
  /// **'{count} items waiting to sync'**
  String itemsWaitingToSync(int count);

  /// No description provided for @syncNow.
  ///
  /// In en, this message translates to:
  /// **'Sync Now'**
  String get syncNow;

  /// No description provided for @lastSyncAt.
  ///
  /// In en, this message translates to:
  /// **'Last sync at {time}'**
  String lastSyncAt(String time);

  /// No description provided for @serverUrl.
  ///
  /// In en, this message translates to:
  /// **'Server URL'**
  String get serverUrl;

  /// No description provided for @testConnection.
  ///
  /// In en, this message translates to:
  /// **'Test Connection'**
  String get testConnection;

  /// No description provided for @saveConfiguration.
  ///
  /// In en, this message translates to:
  /// **'Save Configuration'**
  String get saveConfiguration;

  /// No description provided for @connectionSuccessful.
  ///
  /// In en, this message translates to:
  /// **'Connection successful!'**
  String get connectionSuccessful;

  /// No description provided for @connectionFailed.
  ///
  /// In en, this message translates to:
  /// **'Connection failed'**
  String get connectionFailed;

  /// No description provided for @pleaseEnterServerUrl.
  ///
  /// In en, this message translates to:
  /// **'Please enter server URL'**
  String get pleaseEnterServerUrl;

  /// No description provided for @invalidUrlFormat.
  ///
  /// In en, this message translates to:
  /// **'Invalid URL format (must start with http:// or https://)'**
  String get invalidUrlFormat;

  /// No description provided for @clearCacheTitle.
  ///
  /// In en, this message translates to:
  /// **'Clear Cache'**
  String get clearCacheTitle;

  /// No description provided for @clearCacheMessage.
  ///
  /// In en, this message translates to:
  /// **'Are you sure you want to clear all cached data? This will remove all offline data.'**
  String get clearCacheMessage;

  /// No description provided for @cacheClearedSuccess.
  ///
  /// In en, this message translates to:
  /// **'Cache cleared successfully'**
  String get cacheClearedSuccess;

  /// No description provided for @cancel.
  ///
  /// In en, this message translates to:
  /// **'Cancel'**
  String get cancel;

  /// No description provided for @confirm.
  ///
  /// In en, this message translates to:
  /// **'Confirm'**
  String get confirm;

  /// No description provided for @logoutTitle.
  ///
  /// In en, this message translates to:
  /// **'Logout'**
  String get logoutTitle;

  /// No description provided for @logoutMessage.
  ///
  /// In en, this message translates to:
  /// **'Are you sure you want to logout?'**
  String get logoutMessage;

  /// No description provided for @noTasksAvailable.
  ///
  /// In en, this message translates to:
  /// **'No tasks available'**
  String get noTasksAvailable;

  /// No description provided for @loadingTasks.
  ///
  /// In en, this message translates to:
  /// **'Loading tasks...'**
  String get loadingTasks;

  /// No description provided for @errorLoadingTasks.
  ///
  /// In en, this message translates to:
  /// **'Error loading tasks'**
  String get errorLoadingTasks;

  /// No description provided for @retry.
  ///
  /// In en, this message translates to:
  /// **'Retry'**
  String get retry;

  /// No description provided for @language.
  ///
  /// In en, this message translates to:
  /// **'Language'**
  String get language;

  /// No description provided for @languageSettings.
  ///
  /// In en, this message translates to:
  /// **'Language Settings'**
  String get languageSettings;

  /// No description provided for @selectLanguage.
  ///
  /// In en, this message translates to:
  /// **'Select Language'**
  String get selectLanguage;

  /// No description provided for @languageChanged.
  ///
  /// In en, this message translates to:
  /// **'Language changed successfully'**
  String get languageChanged;

  /// No description provided for @restartRequired.
  ///
  /// In en, this message translates to:
  /// **'Please restart the app to apply language changes'**
  String get restartRequired;

  /// No description provided for @english.
  ///
  /// In en, this message translates to:
  /// **'English'**
  String get english;

  /// No description provided for @vietnamese.
  ///
  /// In en, this message translates to:
  /// **'Tiếng Việt'**
  String get vietnamese;

  /// No description provided for @filipino.
  ///
  /// In en, this message translates to:
  /// **'Filipino'**
  String get filipino;

  /// No description provided for @hindi.
  ///
  /// In en, this message translates to:
  /// **'हिंदी'**
  String get hindi;

  /// No description provided for @chinese.
  ///
  /// In en, this message translates to:
  /// **'简体中文'**
  String get chinese;

  /// No description provided for @japanese.
  ///
  /// In en, this message translates to:
  /// **'日本語'**
  String get japanese;

  /// No description provided for @korean.
  ///
  /// In en, this message translates to:
  /// **'한국어'**
  String get korean;

  /// No description provided for @user.
  ///
  /// In en, this message translates to:
  /// **'User'**
  String get user;

  /// No description provided for @position.
  ///
  /// In en, this message translates to:
  /// **'Position'**
  String get position;

  /// No description provided for @crewId.
  ///
  /// In en, this message translates to:
  /// **'Crew ID'**
  String get crewId;

  /// No description provided for @notAvailable.
  ///
  /// In en, this message translates to:
  /// **'N/A'**
  String get notAvailable;

  /// No description provided for @justNow.
  ///
  /// In en, this message translates to:
  /// **'Just now'**
  String get justNow;

  /// No description provided for @minutesAgo.
  ///
  /// In en, this message translates to:
  /// **'{minutes}m ago'**
  String minutesAgo(int minutes);

  /// No description provided for @hoursAgo.
  ///
  /// In en, this message translates to:
  /// **'{hours}h ago'**
  String hoursAgo(int hours);

  /// No description provided for @home.
  ///
  /// In en, this message translates to:
  /// **'Home'**
  String get home;

  /// No description provided for @profile.
  ///
  /// In en, this message translates to:
  /// **'Profile'**
  String get profile;

  /// No description provided for @myCertificates.
  ///
  /// In en, this message translates to:
  /// **'My Certificates'**
  String get myCertificates;

  /// No description provided for @myProfile.
  ///
  /// In en, this message translates to:
  /// **'My Profile'**
  String get myProfile;

  /// No description provided for @expired.
  ///
  /// In en, this message translates to:
  /// **'EXPIRED'**
  String get expired;

  /// No description provided for @expiring.
  ///
  /// In en, this message translates to:
  /// **'EXPIRING'**
  String get expiring;

  /// No description provided for @safetyAlarms.
  ///
  /// In en, this message translates to:
  /// **'Safety Alarms'**
  String get safetyAlarms;

  /// No description provided for @alarmDetails.
  ///
  /// In en, this message translates to:
  /// **'Alarm Details'**
  String get alarmDetails;

  /// No description provided for @alarmStatistics.
  ///
  /// In en, this message translates to:
  /// **'Alarm Statistics'**
  String get alarmStatistics;

  /// No description provided for @alarmHistory.
  ///
  /// In en, this message translates to:
  /// **'Alarm History'**
  String get alarmHistory;

  /// No description provided for @acknowledge.
  ///
  /// In en, this message translates to:
  /// **'Acknowledge'**
  String get acknowledge;

  /// No description provided for @resolve.
  ///
  /// In en, this message translates to:
  /// **'Resolve'**
  String get resolve;

  /// No description provided for @alarmAcknowledged.
  ///
  /// In en, this message translates to:
  /// **'Alarm acknowledged'**
  String get alarmAcknowledged;

  /// No description provided for @alarmResolved.
  ///
  /// In en, this message translates to:
  /// **'Alarm resolved'**
  String get alarmResolved;

  /// No description provided for @failedToAcknowledgeAlarm.
  ///
  /// In en, this message translates to:
  /// **'Failed to acknowledge alarm'**
  String get failedToAcknowledgeAlarm;

  /// No description provided for @failedToResolveAlarm.
  ///
  /// In en, this message translates to:
  /// **'Failed to resolve alarm'**
  String get failedToResolveAlarm;

  /// No description provided for @confirmResolution.
  ///
  /// In en, this message translates to:
  /// **'Confirm Resolution'**
  String get confirmResolution;

  /// No description provided for @areYouSureResolveAlarm.
  ///
  /// In en, this message translates to:
  /// **'Are you sure you want to resolve this alarm?'**
  String get areYouSureResolveAlarm;

  /// No description provided for @allSystemsNormal.
  ///
  /// In en, this message translates to:
  /// **'All systems normal'**
  String get allSystemsNormal;

  /// No description provided for @generateSampleAlarms.
  ///
  /// In en, this message translates to:
  /// **'Generate Sample Alarms'**
  String get generateSampleAlarms;

  /// No description provided for @sampleAlarmsGenerated.
  ///
  /// In en, this message translates to:
  /// **'Sample alarms generated'**
  String get sampleAlarmsGenerated;

  /// No description provided for @noDataAvailable.
  ///
  /// In en, this message translates to:
  /// **'No data available'**
  String get noDataAvailable;

  /// No description provided for @newWatchLog.
  ///
  /// In en, this message translates to:
  /// **'New Watch Log'**
  String get newWatchLog;

  /// No description provided for @watchLogDetails.
  ///
  /// In en, this message translates to:
  /// **'Watch Log Details'**
  String get watchLogDetails;

  /// No description provided for @watchLogNotFound.
  ///
  /// In en, this message translates to:
  /// **'Watch log not found'**
  String get watchLogNotFound;

  /// No description provided for @addLogEntry.
  ///
  /// In en, this message translates to:
  /// **'Add Log Entry'**
  String get addLogEntry;

  /// No description provided for @addNotableEvents.
  ///
  /// In en, this message translates to:
  /// **'Add notable events or observations during your watch:'**
  String get addNotableEvents;

  /// No description provided for @logEntrySaved.
  ///
  /// In en, this message translates to:
  /// **'Log entry saved. Waiting for Master signature.'**
  String get logEntrySaved;

  /// No description provided for @watchLogCreatedSuccessfully.
  ///
  /// In en, this message translates to:
  /// **'Watch log created successfully'**
  String get watchLogCreatedSuccessfully;

  /// No description provided for @watchDate.
  ///
  /// In en, this message translates to:
  /// **'Watch Date'**
  String get watchDate;

  /// No description provided for @errorCompletingTask.
  ///
  /// In en, this message translates to:
  /// **'Error completing task: {error}'**
  String errorCompletingTask(String error);

  /// No description provided for @pleaseEnterMeasuredValue.
  ///
  /// In en, this message translates to:
  /// **'Please enter measured value!'**
  String get pleaseEnterMeasuredValue;

  /// No description provided for @invalidValue.
  ///
  /// In en, this message translates to:
  /// **'Invalid value!'**
  String get invalidValue;

  /// No description provided for @valueTooLow.
  ///
  /// In en, this message translates to:
  /// **'Value too low! Minimum: {min}'**
  String valueTooLow(String min);

  /// No description provided for @valueTooHigh.
  ///
  /// In en, this message translates to:
  /// **'Value too high! Maximum: {max}'**
  String valueTooHigh(String max);

  /// No description provided for @pleaseEnterObservationNote.
  ///
  /// In en, this message translates to:
  /// **'Please enter observation note!'**
  String get pleaseEnterObservationNote;

  /// No description provided for @serverConfigurationSaved.
  ///
  /// In en, this message translates to:
  /// **'Server URL saved successfully! Please restart the app.'**
  String get serverConfigurationSaved;

  /// No description provided for @failedToSaveUrl.
  ///
  /// In en, this message translates to:
  /// **'Failed to save URL: {error}'**
  String failedToSaveUrl(String error);

  /// No description provided for @resetToDefault.
  ///
  /// In en, this message translates to:
  /// **'Reset to Default'**
  String get resetToDefault;

  /// No description provided for @loginFailedError.
  ///
  /// In en, this message translates to:
  /// **'Login failed: {error}'**
  String loginFailedError(String error);

  /// No description provided for @serverSettings.
  ///
  /// In en, this message translates to:
  /// **'Server Settings'**
  String get serverSettings;

  /// No description provided for @sync.
  ///
  /// In en, this message translates to:
  /// **'Sync'**
  String get sync;

  /// No description provided for @save.
  ///
  /// In en, this message translates to:
  /// **'Save'**
  String get save;

  /// No description provided for @taskOverview.
  ///
  /// In en, this message translates to:
  /// **'Task Overview'**
  String get taskOverview;

  /// No description provided for @quickAccess.
  ///
  /// In en, this message translates to:
  /// **'Quick Access'**
  String get quickAccess;

  /// No description provided for @crewMember.
  ///
  /// In en, this message translates to:
  /// **'Crew Member'**
  String get crewMember;

  /// No description provided for @urgentAttention.
  ///
  /// In en, this message translates to:
  /// **'Urgent Attention!'**
  String get urgentAttention;

  /// No description provided for @overdueTasksCount.
  ///
  /// In en, this message translates to:
  /// **'{count} overdue task(s)'**
  String overdueTasksCount(int count);

  /// No description provided for @itemsPending.
  ///
  /// In en, this message translates to:
  /// **'{count} item(s) pending'**
  String itemsPending(int count);

  /// No description provided for @watchSchedule.
  ///
  /// In en, this message translates to:
  /// **'Watch\nSchedule'**
  String get watchSchedule;

  /// No description provided for @goodMorning.
  ///
  /// In en, this message translates to:
  /// **'Good Morning'**
  String get goodMorning;

  /// No description provided for @goodAfternoon.
  ///
  /// In en, this message translates to:
  /// **'Good Afternoon'**
  String get goodAfternoon;

  /// No description provided for @goodEvening.
  ///
  /// In en, this message translates to:
  /// **'Good Evening'**
  String get goodEvening;

  /// No description provided for @goodNight.
  ///
  /// In en, this message translates to:
  /// **'Good Night'**
  String get goodNight;

  /// No description provided for @updateLogEntry.
  ///
  /// In en, this message translates to:
  /// **'Update Log Entry'**
  String get updateLogEntry;

  /// No description provided for @loadingWatchLog.
  ///
  /// In en, this message translates to:
  /// **'Loading watch log...'**
  String get loadingWatchLog;

  /// No description provided for @notableEvents.
  ///
  /// In en, this message translates to:
  /// **'Notable Events'**
  String get notableEvents;

  /// No description provided for @notableEventsHint.
  ///
  /// In en, this message translates to:
  /// **'Course alterations, ships sighted, weather changes, etc.'**
  String get notableEventsHint;

  /// No description provided for @onlyMasterCanSign.
  ///
  /// In en, this message translates to:
  /// **'Only the Master can sign and finalize watch logs.'**
  String get onlyMasterCanSign;

  /// No description provided for @needsAcknowledgment.
  ///
  /// In en, this message translates to:
  /// **'NEEDS ACKNOWLEDGMENT'**
  String get needsAcknowledgment;

  /// No description provided for @location.
  ///
  /// In en, this message translates to:
  /// **'Location'**
  String get location;

  /// No description provided for @alarmCode.
  ///
  /// In en, this message translates to:
  /// **'Alarm Code'**
  String get alarmCode;

  /// No description provided for @timestamp.
  ///
  /// In en, this message translates to:
  /// **'Timestamp'**
  String get timestamp;

  /// No description provided for @status.
  ///
  /// In en, this message translates to:
  /// **'Status'**
  String get status;

  /// No description provided for @acknowledgmentInfo.
  ///
  /// In en, this message translates to:
  /// **'Acknowledgment Info'**
  String get acknowledgmentInfo;

  /// No description provided for @acknowledgedBy.
  ///
  /// In en, this message translates to:
  /// **'Acknowledged By'**
  String get acknowledgedBy;

  /// No description provided for @acknowledgedAt.
  ///
  /// In en, this message translates to:
  /// **'Acknowledged At'**
  String get acknowledgedAt;

  /// No description provided for @resolvedAt.
  ///
  /// In en, this message translates to:
  /// **'Resolved At'**
  String get resolvedAt;

  /// No description provided for @error.
  ///
  /// In en, this message translates to:
  /// **'Error'**
  String get error;

  /// No description provided for @show.
  ///
  /// In en, this message translates to:
  /// **'Show'**
  String get show;

  /// No description provided for @daysCount.
  ///
  /// In en, this message translates to:
  /// **'{count} days'**
  String daysCount(int count);

  /// No description provided for @noAlarmHistory.
  ///
  /// In en, this message translates to:
  /// **'No alarm history'**
  String get noAlarmHistory;

  /// No description provided for @resolved.
  ///
  /// In en, this message translates to:
  /// **'Resolved'**
  String get resolved;

  /// No description provided for @acknowledged.
  ///
  /// In en, this message translates to:
  /// **'Acknowledged'**
  String get acknowledged;

  /// No description provided for @viewCertificates.
  ///
  /// In en, this message translates to:
  /// **'View Certificates'**
  String get viewCertificates;

  /// No description provided for @loadingProfile.
  ///
  /// In en, this message translates to:
  /// **'Loading profile...'**
  String get loadingProfile;

  /// No description provided for @failedToLoadProfile.
  ///
  /// In en, this message translates to:
  /// **'Failed to load profile'**
  String get failedToLoadProfile;

  /// No description provided for @profileNotFound.
  ///
  /// In en, this message translates to:
  /// **'Profile not found'**
  String get profileNotFound;

  /// No description provided for @personalInformation.
  ///
  /// In en, this message translates to:
  /// **'Personal Information'**
  String get personalInformation;

  /// No description provided for @nationality.
  ///
  /// In en, this message translates to:
  /// **'Nationality'**
  String get nationality;

  /// No description provided for @dateOfBirth.
  ///
  /// In en, this message translates to:
  /// **'Date of Birth'**
  String get dateOfBirth;

  /// No description provided for @rank.
  ///
  /// In en, this message translates to:
  /// **'Rank'**
  String get rank;

  /// No description provided for @department.
  ///
  /// In en, this message translates to:
  /// **'Department'**
  String get department;

  /// No description provided for @contactInformation.
  ///
  /// In en, this message translates to:
  /// **'Contact Information'**
  String get contactInformation;

  /// No description provided for @email.
  ///
  /// In en, this message translates to:
  /// **'Email'**
  String get email;

  /// No description provided for @phone.
  ///
  /// In en, this message translates to:
  /// **'Phone'**
  String get phone;

  /// No description provided for @address.
  ///
  /// In en, this message translates to:
  /// **'Address'**
  String get address;

  /// No description provided for @emergencyContact.
  ///
  /// In en, this message translates to:
  /// **'Emergency Contact'**
  String get emergencyContact;

  /// No description provided for @contactInfo.
  ///
  /// In en, this message translates to:
  /// **'Contact Info'**
  String get contactInfo;

  /// No description provided for @documents.
  ///
  /// In en, this message translates to:
  /// **'Documents'**
  String get documents;

  /// No description provided for @passportNumber.
  ///
  /// In en, this message translates to:
  /// **'Passport Number'**
  String get passportNumber;

  /// No description provided for @passportExpiry.
  ///
  /// In en, this message translates to:
  /// **'Passport Expiry'**
  String get passportExpiry;

  /// No description provided for @seamanBookNumber.
  ///
  /// In en, this message translates to:
  /// **'Seaman Book Number'**
  String get seamanBookNumber;

  /// No description provided for @visaNumber.
  ///
  /// In en, this message translates to:
  /// **'Visa Number'**
  String get visaNumber;

  /// No description provided for @visaExpiry.
  ///
  /// In en, this message translates to:
  /// **'Visa Expiry'**
  String get visaExpiry;

  /// No description provided for @employment.
  ///
  /// In en, this message translates to:
  /// **'Employment'**
  String get employment;

  /// No description provided for @onboard.
  ///
  /// In en, this message translates to:
  /// **'ONBOARD'**
  String get onboard;

  /// No description provided for @offboard.
  ///
  /// In en, this message translates to:
  /// **'OFFBOARD'**
  String get offboard;

  /// No description provided for @joinDate.
  ///
  /// In en, this message translates to:
  /// **'Join Date'**
  String get joinDate;

  /// No description provided for @embarkDate.
  ///
  /// In en, this message translates to:
  /// **'Embark Date'**
  String get embarkDate;

  /// No description provided for @disembarkDate.
  ///
  /// In en, this message translates to:
  /// **'Disembark Date'**
  String get disembarkDate;

  /// No description provided for @contractEnd.
  ///
  /// In en, this message translates to:
  /// **'Contract End'**
  String get contractEnd;

  /// No description provided for @loadingCertificates.
  ///
  /// In en, this message translates to:
  /// **'Loading certificates...'**
  String get loadingCertificates;

  /// No description provided for @failedToLoadCertificates.
  ///
  /// In en, this message translates to:
  /// **'Failed to load certificates'**
  String get failedToLoadCertificates;

  /// No description provided for @noCertificateDataFound.
  ///
  /// In en, this message translates to:
  /// **'No certificate data found'**
  String get noCertificateDataFound;

  /// No description provided for @stcwCertificate.
  ///
  /// In en, this message translates to:
  /// **'STCW Certificate'**
  String get stcwCertificate;

  /// No description provided for @medicalCertificate.
  ///
  /// In en, this message translates to:
  /// **'Medical Certificate'**
  String get medicalCertificate;

  /// No description provided for @passport.
  ///
  /// In en, this message translates to:
  /// **'Passport'**
  String get passport;

  /// No description provided for @visa.
  ///
  /// In en, this message translates to:
  /// **'Visa'**
  String get visa;

  /// No description provided for @seamanBook.
  ///
  /// In en, this message translates to:
  /// **'Seaman Book'**
  String get seamanBook;

  /// No description provided for @certificatesExpired.
  ///
  /// In en, this message translates to:
  /// **'{count} certificate(s) have expired!'**
  String certificatesExpired(int count);

  /// No description provided for @certificatesExpiringSoon.
  ///
  /// In en, this message translates to:
  /// **'{count} certificate(s) expiring soon'**
  String certificatesExpiringSoon(int count);

  /// No description provided for @expiringSoon.
  ///
  /// In en, this message translates to:
  /// **'Expiring Soon'**
  String get expiringSoon;

  /// No description provided for @valid.
  ///
  /// In en, this message translates to:
  /// **'Valid'**
  String get valid;

  /// No description provided for @number.
  ///
  /// In en, this message translates to:
  /// **'Number'**
  String get number;

  /// No description provided for @issued.
  ///
  /// In en, this message translates to:
  /// **'Issued'**
  String get issued;

  /// No description provided for @expires.
  ///
  /// In en, this message translates to:
  /// **'Expires'**
  String get expires;

  /// No description provided for @daysRemaining.
  ///
  /// In en, this message translates to:
  /// **'{count} days remaining'**
  String daysRemaining(int count);

  /// No description provided for @maintenance.
  ///
  /// In en, this message translates to:
  /// **'Maintenance'**
  String get maintenance;

  /// No description provided for @loadingSchedule.
  ///
  /// In en, this message translates to:
  /// **'Loading schedule...'**
  String get loadingSchedule;

  /// No description provided for @upcomingDays.
  ///
  /// In en, this message translates to:
  /// **'Upcoming ({count} days)'**
  String upcomingDays(int count);

  /// No description provided for @thisWeek.
  ///
  /// In en, this message translates to:
  /// **'This Week'**
  String get thisWeek;

  /// No description provided for @thisMonth.
  ///
  /// In en, this message translates to:
  /// **'This Month'**
  String get thisMonth;

  /// No description provided for @allTasks.
  ///
  /// In en, this message translates to:
  /// **'All Tasks'**
  String get allTasks;

  /// No description provided for @total.
  ///
  /// In en, this message translates to:
  /// **'Total'**
  String get total;

  /// No description provided for @overdue.
  ///
  /// In en, this message translates to:
  /// **'Overdue'**
  String get overdue;

  /// No description provided for @dueSoon.
  ///
  /// In en, this message translates to:
  /// **'Due Soon'**
  String get dueSoon;

  /// No description provided for @noTasksScheduled.
  ///
  /// In en, this message translates to:
  /// **'No tasks scheduled'**
  String get noTasksScheduled;

  /// No description provided for @noMaintenanceTasksMatch.
  ///
  /// In en, this message translates to:
  /// **'No maintenance tasks match the selected filter'**
  String get noMaintenanceTasksMatch;

  /// No description provided for @signed.
  ///
  /// In en, this message translates to:
  /// **'Signed'**
  String get signed;

  /// No description provided for @unsigned.
  ///
  /// In en, this message translates to:
  /// **'Unsigned'**
  String get unsigned;

  /// No description provided for @personnel.
  ///
  /// In en, this message translates to:
  /// **'Personnel'**
  String get personnel;

  /// No description provided for @officerOnWatch.
  ///
  /// In en, this message translates to:
  /// **'Officer on Watch'**
  String get officerOnWatch;

  /// No description provided for @lookout.
  ///
  /// In en, this message translates to:
  /// **'Lookout'**
  String get lookout;

  /// No description provided for @weatherSeaConditions.
  ///
  /// In en, this message translates to:
  /// **'Weather & Sea Conditions'**
  String get weatherSeaConditions;

  /// No description provided for @weather.
  ///
  /// In en, this message translates to:
  /// **'Weather'**
  String get weather;

  /// No description provided for @seaState.
  ///
  /// In en, this message translates to:
  /// **'Sea State'**
  String get seaState;

  /// No description provided for @visibility.
  ///
  /// In en, this message translates to:
  /// **'Visibility'**
  String get visibility;

  /// No description provided for @navigationData.
  ///
  /// In en, this message translates to:
  /// **'Navigation Data'**
  String get navigationData;

  /// No description provided for @course.
  ///
  /// In en, this message translates to:
  /// **'Course'**
  String get course;

  /// No description provided for @speed.
  ///
  /// In en, this message translates to:
  /// **'Speed'**
  String get speed;

  /// No description provided for @distanceRun.
  ///
  /// In en, this message translates to:
  /// **'Distance Run'**
  String get distanceRun;

  /// No description provided for @shipPosition.
  ///
  /// In en, this message translates to:
  /// **'Position'**
  String get shipPosition;

  /// No description provided for @engineStatus.
  ///
  /// In en, this message translates to:
  /// **'Engine Status'**
  String get engineStatus;

  /// No description provided for @masterSignature.
  ///
  /// In en, this message translates to:
  /// **'Master Signature'**
  String get masterSignature;

  /// No description provided for @taskCompletedSuccessfully.
  ///
  /// In en, this message translates to:
  /// **'Task completed successfully!'**
  String get taskCompletedSuccessfully;

  /// No description provided for @taskSavedWillSync.
  ///
  /// In en, this message translates to:
  /// **'Task saved. Will sync when online.'**
  String get taskSavedWillSync;

  /// No description provided for @taskId.
  ///
  /// In en, this message translates to:
  /// **'Task ID'**
  String get taskId;

  /// No description provided for @offlineTaskWillSync.
  ///
  /// In en, this message translates to:
  /// **'You are offline. Task will be synced when connection is restored.'**
  String get offlineTaskWillSync;

  /// No description provided for @runningHoursRequired.
  ///
  /// In en, this message translates to:
  /// **'Running Hours *'**
  String get runningHoursRequired;

  /// No description provided for @enterCurrentRunningHours.
  ///
  /// In en, this message translates to:
  /// **'Enter current running hours'**
  String get enterCurrentRunningHours;

  /// No description provided for @hours.
  ///
  /// In en, this message translates to:
  /// **'hours'**
  String get hours;

  /// No description provided for @pleaseEnterRunningHours.
  ///
  /// In en, this message translates to:
  /// **'Please enter running hours'**
  String get pleaseEnterRunningHours;

  /// No description provided for @pleaseEnterValidNumber.
  ///
  /// In en, this message translates to:
  /// **'Please enter a valid number'**
  String get pleaseEnterValidNumber;

  /// No description provided for @runningHoursCannotBeLess.
  ///
  /// In en, this message translates to:
  /// **'Running hours cannot be less than last maintenance ({hours})'**
  String runningHoursCannotBeLess(double hours);

  /// No description provided for @sparePartsUsed.
  ///
  /// In en, this message translates to:
  /// **'Spare Parts Used'**
  String get sparePartsUsed;

  /// No description provided for @listSparePartsUsed.
  ///
  /// In en, this message translates to:
  /// **'List any spare parts used (optional)'**
  String get listSparePartsUsed;

  /// No description provided for @notes.
  ///
  /// In en, this message translates to:
  /// **'Notes'**
  String get notes;

  /// No description provided for @addAdditionalNotes.
  ///
  /// In en, this message translates to:
  /// **'Add any additional notes or observations (optional)'**
  String get addAdditionalNotes;

  /// No description provided for @completingTask.
  ///
  /// In en, this message translates to:
  /// **'Completing task...'**
  String get completingTask;

  /// No description provided for @savingForOfflineSync.
  ///
  /// In en, this message translates to:
  /// **'Saving for offline sync...'**
  String get savingForOfflineSync;

  /// No description provided for @refreshTaskData.
  ///
  /// In en, this message translates to:
  /// **'Refresh task data'**
  String get refreshTaskData;

  /// No description provided for @taskDataRefreshed.
  ///
  /// In en, this message translates to:
  /// **'✅ Task data refreshed'**
  String get taskDataRefreshed;

  /// No description provided for @failedToRefresh.
  ///
  /// In en, this message translates to:
  /// **'Failed to refresh: {error}'**
  String failedToRefresh(String error);

  /// No description provided for @overdueDaysPastDue.
  ///
  /// In en, this message translates to:
  /// **'⚠️ OVERDUE: {days} days past due date'**
  String overdueDaysPastDue(int days);

  /// No description provided for @maintenanceSchedule.
  ///
  /// In en, this message translates to:
  /// **'Maintenance Schedule'**
  String get maintenanceSchedule;

  /// No description provided for @type.
  ///
  /// In en, this message translates to:
  /// **'Type'**
  String get type;

  /// No description provided for @interval.
  ///
  /// In en, this message translates to:
  /// **'Interval'**
  String get interval;

  /// No description provided for @runningHoursValue.
  ///
  /// In en, this message translates to:
  /// **'{hours} running hours'**
  String runningHoursValue(int hours);

  /// No description provided for @daysValue.
  ///
  /// In en, this message translates to:
  /// **'{days} days'**
  String daysValue(int days);

  /// No description provided for @lastDone.
  ///
  /// In en, this message translates to:
  /// **'Last Done'**
  String get lastDone;

  /// No description provided for @nextDue.
  ///
  /// In en, this message translates to:
  /// **'Next Due'**
  String get nextDue;

  /// No description provided for @daysUntilDue.
  ///
  /// In en, this message translates to:
  /// **'Days Until Due'**
  String get daysUntilDue;

  /// No description provided for @runningHours.
  ///
  /// In en, this message translates to:
  /// **'Running Hours'**
  String get runningHours;

  /// No description provided for @atLastMaintenance.
  ///
  /// In en, this message translates to:
  /// **'At Last Maintenance'**
  String get atLastMaintenance;

  /// No description provided for @hoursValue.
  ///
  /// In en, this message translates to:
  /// **'{hours} hours'**
  String hoursValue(int hours);

  /// No description provided for @assignment.
  ///
  /// In en, this message translates to:
  /// **'Assignment'**
  String get assignment;

  /// No description provided for @assignedTo.
  ///
  /// In en, this message translates to:
  /// **'Assigned To'**
  String get assignedTo;

  /// No description provided for @taskChecklist.
  ///
  /// In en, this message translates to:
  /// **'📋 Task Checklist'**
  String get taskChecklist;

  /// No description provided for @completionDetails.
  ///
  /// In en, this message translates to:
  /// **'Completion Details'**
  String get completionDetails;

  /// No description provided for @completedBy.
  ///
  /// In en, this message translates to:
  /// **'Completed By'**
  String get completedBy;

  /// No description provided for @completedAt.
  ///
  /// In en, this message translates to:
  /// **'Completed At'**
  String get completedAt;

  /// No description provided for @spareParts.
  ///
  /// In en, this message translates to:
  /// **'Spare Parts'**
  String get spareParts;

  /// No description provided for @errorLoadingChecklist.
  ///
  /// In en, this message translates to:
  /// **'Error loading checklist'**
  String get errorLoadingChecklist;

  /// No description provided for @tryAgain.
  ///
  /// In en, this message translates to:
  /// **'Try Again'**
  String get tryAgain;

  /// No description provided for @noChecklistYet.
  ///
  /// In en, this message translates to:
  /// **'No checklist yet'**
  String get noChecklistYet;

  /// No description provided for @thisTaskHasNoDetails.
  ///
  /// In en, this message translates to:
  /// **'This task has no details'**
  String get thisTaskHasNoDetails;

  /// No description provided for @progressCount.
  ///
  /// In en, this message translates to:
  /// **'Progress: {completed}/{total}'**
  String progressCount(int completed, int total);

  /// No description provided for @mandatory.
  ///
  /// In en, this message translates to:
  /// **'MANDATORY'**
  String get mandatory;

  /// No description provided for @unitLabel.
  ///
  /// In en, this message translates to:
  /// **'Unit: {unit}'**
  String unitLabel(String unit);

  /// No description provided for @measurement.
  ///
  /// In en, this message translates to:
  /// **'Measurement'**
  String get measurement;

  /// No description provided for @checklist.
  ///
  /// In en, this message translates to:
  /// **'Checklist'**
  String get checklist;

  /// No description provided for @inspection.
  ///
  /// In en, this message translates to:
  /// **'Inspection'**
  String get inspection;

  /// No description provided for @completed.
  ///
  /// In en, this message translates to:
  /// **'Completed'**
  String get completed;

  /// No description provided for @measuredValueWithUnit.
  ///
  /// In en, this message translates to:
  /// **'Measured value: {value} {unit}'**
  String measuredValueWithUnit(String value, String unit);

  /// No description provided for @checkResult.
  ///
  /// In en, this message translates to:
  /// **'Check Result:'**
  String get checkResult;

  /// No description provided for @okPass.
  ///
  /// In en, this message translates to:
  /// **'OK / Pass'**
  String get okPass;

  /// No description provided for @ngFail.
  ///
  /// In en, this message translates to:
  /// **'NG / Fail'**
  String get ngFail;

  /// No description provided for @measuredValue.
  ///
  /// In en, this message translates to:
  /// **'Measured Value:'**
  String get measuredValue;

  /// No description provided for @enterValue.
  ///
  /// In en, this message translates to:
  /// **'Enter value...'**
  String get enterValue;

  /// No description provided for @limitRange.
  ///
  /// In en, this message translates to:
  /// **'Limit: {min} - {max} {unit}'**
  String limitRange(String min, String max, String unit);

  /// No description provided for @observationNotes.
  ///
  /// In en, this message translates to:
  /// **'Observation Notes:'**
  String get observationNotes;

  /// No description provided for @enterDetailedNotes.
  ///
  /// In en, this message translates to:
  /// **'Enter detailed notes...'**
  String get enterDetailedNotes;

  /// No description provided for @notesOptional.
  ///
  /// In en, this message translates to:
  /// **'Notes (optional):'**
  String get notesOptional;

  /// No description provided for @addNotesIfNeeded.
  ///
  /// In en, this message translates to:
  /// **'Add notes if needed...'**
  String get addNotesIfNeeded;

  /// No description provided for @alreadyCompletedCanUpdate.
  ///
  /// In en, this message translates to:
  /// **'Already completed. You can update it.'**
  String get alreadyCompletedCanUpdate;

  /// No description provided for @tapToComplete.
  ///
  /// In en, this message translates to:
  /// **'👆 Tap to complete'**
  String get tapToComplete;

  /// No description provided for @savedItem.
  ///
  /// In en, this message translates to:
  /// **'✅ Saved: {item}'**
  String savedItem(String item);

  /// No description provided for @errorMessage.
  ///
  /// In en, this message translates to:
  /// **'❌ Error: {error}'**
  String errorMessage(String error);

  /// No description provided for @update.
  ///
  /// In en, this message translates to:
  /// **'Update'**
  String get update;

  /// No description provided for @complete.
  ///
  /// In en, this message translates to:
  /// **'Complete'**
  String get complete;

  /// No description provided for @taskOverdue.
  ///
  /// In en, this message translates to:
  /// **'Task Overdue'**
  String get taskOverdue;

  /// No description provided for @thisTaskIsOverdue.
  ///
  /// In en, this message translates to:
  /// **'This task is overdue. Starting it now will help catch up on maintenance, but please complete it as soon as possible.'**
  String get thisTaskIsOverdue;

  /// No description provided for @startAnyway.
  ///
  /// In en, this message translates to:
  /// **'Start Anyway'**
  String get startAnyway;

  /// No description provided for @overdueTaskStarted.
  ///
  /// In en, this message translates to:
  /// **'Overdue task started! Please complete ASAP.'**
  String get overdueTaskStarted;

  /// No description provided for @taskStartedSuccessfully.
  ///
  /// In en, this message translates to:
  /// **'Task started successfully!'**
  String get taskStartedSuccessfully;

  /// No description provided for @failedToStartTask.
  ///
  /// In en, this message translates to:
  /// **'Failed to start task: {error}'**
  String failedToStartTask(String error);

  /// No description provided for @searchByEquipmentName.
  ///
  /// In en, this message translates to:
  /// **'Search by equipment name...'**
  String get searchByEquipmentName;

  /// No description provided for @noTasksFound.
  ///
  /// In en, this message translates to:
  /// **'No tasks found'**
  String get noTasksFound;

  /// No description provided for @noTasksInCategory.
  ///
  /// In en, this message translates to:
  /// **'You have no tasks in this category'**
  String get noTasksInCategory;

  /// No description provided for @noPendingTasks.
  ///
  /// In en, this message translates to:
  /// **'No pending tasks'**
  String get noPendingTasks;

  /// No description provided for @allTasksStartedOrCompleted.
  ///
  /// In en, this message translates to:
  /// **'All tasks have been started or completed'**
  String get allTasksStartedOrCompleted;

  /// No description provided for @noTasksInProgress.
  ///
  /// In en, this message translates to:
  /// **'No tasks in progress'**
  String get noTasksInProgress;

  /// No description provided for @startPendingTaskToSeeHere.
  ///
  /// In en, this message translates to:
  /// **'Start a pending task to see it here'**
  String get startPendingTaskToSeeHere;

  /// No description provided for @noOverdueTasks.
  ///
  /// In en, this message translates to:
  /// **'No overdue tasks'**
  String get noOverdueTasks;

  /// No description provided for @allTasksOnSchedule.
  ///
  /// In en, this message translates to:
  /// **'Great! All tasks are on schedule'**
  String get allTasksOnSchedule;

  /// No description provided for @noCompletedTasks.
  ///
  /// In en, this message translates to:
  /// **'No completed tasks'**
  String get noCompletedTasks;

  /// No description provided for @completedTasksAppearHere.
  ///
  /// In en, this message translates to:
  /// **'Complete tasks will appear here'**
  String get completedTasksAppearHere;

  /// No description provided for @noTasksMatchSearch.
  ///
  /// In en, this message translates to:
  /// **'No tasks match your search'**
  String get noTasksMatchSearch;

  /// No description provided for @tryDifferentSearchTerm.
  ///
  /// In en, this message translates to:
  /// **'Try a different search term'**
  String get tryDifferentSearchTerm;
}

class _AppLocalizationsDelegate
    extends LocalizationsDelegate<AppLocalizations> {
  const _AppLocalizationsDelegate();

  @override
  Future<AppLocalizations> load(Locale locale) {
    return SynchronousFuture<AppLocalizations>(lookupAppLocalizations(locale));
  }

  @override
  bool isSupported(Locale locale) => <String>[
        'en',
        'fil',
        'hi',
        'ja',
        'ko',
        'vi',
        'zh'
      ].contains(locale.languageCode);

  @override
  bool shouldReload(_AppLocalizationsDelegate old) => false;
}

AppLocalizations lookupAppLocalizations(Locale locale) {
  // Lookup logic when only language code is specified.
  switch (locale.languageCode) {
    case 'en':
      return AppLocalizationsEn();
    case 'fil':
      return AppLocalizationsFil();
    case 'hi':
      return AppLocalizationsHi();
    case 'ja':
      return AppLocalizationsJa();
    case 'ko':
      return AppLocalizationsKo();
    case 'vi':
      return AppLocalizationsVi();
    case 'zh':
      return AppLocalizationsZh();
  }

  throw FlutterError(
      'AppLocalizations.delegate failed to load unsupported locale "$locale". This is likely '
      'an issue with the localizations generation tool. Please file an issue '
      'on GitHub with a reproducible sample app and the gen-l10n configuration '
      'that was used.');
}
