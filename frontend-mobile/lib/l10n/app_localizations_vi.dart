// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for Vietnamese (`vi`).
class AppLocalizationsVi extends AppLocalizations {
  AppLocalizationsVi([String locale = 'vi']) : super(locale);

  @override
  String get appName => 'Ứng Dụng Thuyền Viên';

  @override
  String get login => 'Đăng nhập';

  @override
  String get logout => 'Đăng xuất';

  @override
  String get username => 'Tên đăng nhập';

  @override
  String get password => 'Mật khẩu';

  @override
  String get pleaseEnterUsername => 'Vui lòng nhập tên đăng nhập';

  @override
  String get pleaseEnterPassword => 'Vui lòng nhập mật khẩu';

  @override
  String get loginFailed => 'Đăng nhập thất bại';

  @override
  String get loginSuccess => 'Đăng nhập thành công';

  @override
  String get dashboard => 'Bảng điều khiển';

  @override
  String get tasks => 'Công việc';

  @override
  String get schedule => 'Lịch trình';

  @override
  String get alarms => 'Cảnh báo';

  @override
  String get settings => 'Cài đặt';

  @override
  String get myTasks => 'Công việc của tôi';

  @override
  String get taskDetails => 'Chi tiết công việc';

  @override
  String get startTask => 'Bắt đầu';

  @override
  String get completeTask => 'Hoàn thành';

  @override
  String get taskStatus => 'Trạng thái';

  @override
  String get taskPriority => 'Ưu tiên';

  @override
  String get dueDate => 'Hạn chót';

  @override
  String get estimatedTime => 'Thời gian dự kiến';

  @override
  String get description => 'Mô tả';

  @override
  String get statusPending => 'Đang chờ';

  @override
  String get statusInProgress => 'Đang thực hiện';

  @override
  String get statusCompleted => 'Đã hoàn thành';

  @override
  String get statusOverdue => 'Quá hạn';

  @override
  String get priorityCritical => 'Khẩn cấp';

  @override
  String get priorityHigh => 'Cao';

  @override
  String get priorityNormal => 'Bình thường';

  @override
  String get priorityLow => 'Thấp';

  @override
  String get account => 'Tài khoản';

  @override
  String get synchronization => 'Đồng bộ hóa';

  @override
  String get serverConfiguration => 'Cấu hình máy chủ';

  @override
  String get dataStorage => 'Dữ liệu & Lưu trữ';

  @override
  String get clearCache => 'Xóa bộ nhớ đệm';

  @override
  String get removeAllCachedData => 'Xóa tất cả dữ liệu đã lưu';

  @override
  String get about => 'Thông tin';

  @override
  String get version => 'Phiên bản';

  @override
  String get license => 'Giấy phép';

  @override
  String get proprietary => 'Độc quyền';

  @override
  String get syncStatus => 'Trạng thái đồng bộ';

  @override
  String get offline => 'Ngoại tuyến';

  @override
  String get online => 'Trực tuyến';

  @override
  String itemsWaitingToSync(int count) {
    return '$count mục chờ đồng bộ';
  }

  @override
  String get syncNow => 'Đồng bộ ngay';

  @override
  String lastSyncAt(String time) {
    return 'Đồng bộ lần cuối lúc $time';
  }

  @override
  String get serverUrl => 'Địa chỉ máy chủ';

  @override
  String get testConnection => 'Kiểm tra kết nối';

  @override
  String get saveConfiguration => 'Lưu cấu hình';

  @override
  String get connectionSuccessful => 'Kết nối thành công!';

  @override
  String get connectionFailed => 'Kết nối thất bại';

  @override
  String get pleaseEnterServerUrl => 'Vui lòng nhập địa chỉ máy chủ';

  @override
  String get invalidUrlFormat =>
      'Định dạng URL không hợp lệ (phải bắt đầu bằng http:// hoặc https://)';

  @override
  String get clearCacheTitle => 'Xóa bộ nhớ đệm';

  @override
  String get clearCacheMessage =>
      'Bạn có chắc chắn muốn xóa tất cả dữ liệu đã lưu? Điều này sẽ xóa tất cả dữ liệu ngoại tuyến.';

  @override
  String get cacheClearedSuccess => 'Đã xóa bộ nhớ đệm thành công';

  @override
  String get cancel => 'Hủy';

  @override
  String get confirm => 'Xác nhận';

  @override
  String get logoutTitle => 'Đăng xuất';

  @override
  String get logoutMessage => 'Bạn có chắc chắn muốn đăng xuất?';

  @override
  String get noTasksAvailable => 'Không có công việc';

  @override
  String get loadingTasks => 'Đang tải công việc...';

  @override
  String get errorLoadingTasks => 'Lỗi khi tải công việc';

  @override
  String get retry => 'Thử lại';

  @override
  String get language => 'Ngôn ngữ';

  @override
  String get languageSettings => 'Cài đặt ngôn ngữ';

  @override
  String get selectLanguage => 'Chọn ngôn ngữ';

  @override
  String get languageChanged => 'Đã thay đổi ngôn ngữ thành công';

  @override
  String get restartRequired =>
      'Vui lòng khởi động lại ứng dụng để áp dụng thay đổi ngôn ngữ';

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
  String get user => 'Người dùng';

  @override
  String get position => 'Vị trí';

  @override
  String get crewId => 'Mã thuyền viên';

  @override
  String get notAvailable => 'Không có';

  @override
  String get justNow => 'Vừa xong';

  @override
  String minutesAgo(int minutes) {
    return '$minutes phút trước';
  }

  @override
  String hoursAgo(int hours) {
    return '$hours giờ trước';
  }

  @override
  String get home => 'Trang chủ';

  @override
  String get profile => 'Hồ sơ';

  @override
  String get myCertificates => 'Chứng chỉ của tôi';

  @override
  String get myProfile => 'Hồ sơ của tôi';

  @override
  String get expired => 'HẾT HẠN';

  @override
  String get expiring => 'SẮP HẾT HẠN';

  @override
  String get safetyAlarms => 'Cảnh báo an toàn';

  @override
  String get alarmDetails => 'Chi tiết cảnh báo';

  @override
  String get alarmStatistics => 'Thống kê cảnh báo';

  @override
  String get alarmHistory => 'Lịch sử cảnh báo';

  @override
  String get acknowledge => 'Xác nhận';

  @override
  String get resolve => 'Giải quyết';

  @override
  String get alarmAcknowledged => 'Đã xác nhận cảnh báo';

  @override
  String get alarmResolved => 'Đã giải quyết cảnh báo';

  @override
  String get failedToAcknowledgeAlarm => 'Không thể xác nhận cảnh báo';

  @override
  String get failedToResolveAlarm => 'Không thể giải quyết cảnh báo';

  @override
  String get confirmResolution => 'Xác nhận giải quyết';

  @override
  String get areYouSureResolveAlarm =>
      'Bạn có chắc chắn muốn giải quyết cảnh báo này?';

  @override
  String get allSystemsNormal => 'Tất cả hệ thống hoạt động bình thường';

  @override
  String get generateSampleAlarms => 'Tạo cảnh báo mẫu';

  @override
  String get sampleAlarmsGenerated => 'Đã tạo cảnh báo mẫu';

  @override
  String get noDataAvailable => 'Không có dữ liệu';

  @override
  String get newWatchLog => 'Nhật ký trực ca mới';

  @override
  String get watchLogDetails => 'Chi tiết nhật ký trực ca';

  @override
  String get watchLogNotFound => 'Không tìm thấy nhật ký trực ca';

  @override
  String get addLogEntry => 'Thêm mục nhật ký';

  @override
  String get addNotableEvents =>
      'Thêm sự kiện hoặc quan sát đáng chú ý trong ca trực của bạn:';

  @override
  String get logEntrySaved =>
      'Đã lưu mục nhật ký. Đang chờ chữ ký của Thuyền trưởng.';

  @override
  String get watchLogCreatedSuccessfully => 'Đã tạo nhật ký trực ca thành công';

  @override
  String get watchDate => 'Ngày trực';

  @override
  String errorCompletingTask(String error) {
    return 'Lỗi khi hoàn thành công việc: $error';
  }

  @override
  String get pleaseEnterMeasuredValue => 'Vui lòng nhập giá trị đo!';

  @override
  String get invalidValue => 'Giá trị không hợp lệ!';

  @override
  String valueTooLow(String min) {
    return 'Giá trị quá thấp! Tối thiểu: $min';
  }

  @override
  String valueTooHigh(String max) {
    return 'Giá trị quá cao! Tối đa: $max';
  }

  @override
  String get pleaseEnterObservationNote => 'Vui lòng nhập ghi chú quan sát!';

  @override
  String get serverConfigurationSaved =>
      'Đã lưu địa chỉ máy chủ! Vui lòng khởi động lại ứng dụng.';

  @override
  String failedToSaveUrl(String error) {
    return 'Không thể lưu địa chỉ: $error';
  }

  @override
  String get resetToDefault => 'Đặt lại mặc định';

  @override
  String loginFailedError(String error) {
    return 'Đăng nhập thất bại: $error';
  }

  @override
  String get serverSettings => 'Cài đặt máy chủ';

  @override
  String get sync => 'Đồng bộ';

  @override
  String get save => 'Lưu';

  @override
  String get taskOverview => 'Tổng quan công việc';

  @override
  String get quickAccess => 'Truy cập nhanh';

  @override
  String get crewMember => 'Thành viên thuyền viên';

  @override
  String get urgentAttention => 'Cần chú ý gấp!';

  @override
  String overdueTasksCount(int count) {
    return '$count công việc quá hạn';
  }

  @override
  String itemsPending(int count) {
    return '$count mục đang chờ';
  }

  @override
  String get watchSchedule => 'Lịch\nTrực';

  @override
  String get goodMorning => 'Chào buổi sáng';

  @override
  String get goodAfternoon => 'Chào buổi chiều';

  @override
  String get goodEvening => 'Chào buổi tối';

  @override
  String get goodNight => 'Chúc ngủ ngon';

  @override
  String get updateLogEntry => 'Cập nhật mục nhật ký';

  @override
  String get loadingWatchLog => 'Đang tải nhật ký trực ca...';

  @override
  String get notableEvents => 'Sự kiện đáng chú ý';

  @override
  String get notableEventsHint =>
      'Thay đổi hướng đi, tàu gặp gỡ, thay đổi thời tiết, v.v.';

  @override
  String get onlyMasterCanSign =>
      'Chỉ Thuyền trưởng mới có thể ký và hoàn tất nhật ký trực ca.';

  @override
  String get needsAcknowledgment => 'CẦN XÁC NHẬN';

  @override
  String get location => 'Vị trí';

  @override
  String get alarmCode => 'Mã cảnh báo';

  @override
  String get timestamp => 'Thời gian';

  @override
  String get status => 'Trạng thái';

  @override
  String get acknowledgmentInfo => 'Thông tin xác nhận';

  @override
  String get acknowledgedBy => 'Được xác nhận bởi';

  @override
  String get acknowledgedAt => 'Xác nhận lúc';

  @override
  String get resolvedAt => 'Giải quyết lúc';

  @override
  String get error => 'Lỗi';

  @override
  String get show => 'Hiển thị';

  @override
  String daysCount(int count) {
    return '$count ngày';
  }

  @override
  String get noAlarmHistory => 'Không có lịch sử cảnh báo';

  @override
  String get resolved => 'Đã giải quyết';

  @override
  String get acknowledged => 'Đã xác nhận';

  @override
  String get viewCertificates => 'Xem chứng chỉ';

  @override
  String get loadingProfile => 'Đang tải hồ sơ...';

  @override
  String get failedToLoadProfile => 'Không thể tải hồ sơ';

  @override
  String get profileNotFound => 'Không tìm thấy hồ sơ';

  @override
  String get personalInformation => 'Thông tin cá nhân';

  @override
  String get nationality => 'Quốc tịch';

  @override
  String get dateOfBirth => 'Ngày sinh';

  @override
  String get rank => 'Cấp bậc';

  @override
  String get department => 'Bộ phận';

  @override
  String get contactInformation => 'Thông tin liên hệ';

  @override
  String get email => 'Email';

  @override
  String get phone => 'Điện thoại';

  @override
  String get address => 'Địa chỉ';

  @override
  String get emergencyContact => 'Liên hệ khẩn cấp';

  @override
  String get contactInfo => 'Thông tin liên hệ';

  @override
  String get documents => 'Tài liệu';

  @override
  String get passportNumber => 'Số hộ chiếu';

  @override
  String get passportExpiry => 'Hạn hộ chiếu';

  @override
  String get seamanBookNumber => 'Số sổ thủy thủ';

  @override
  String get visaNumber => 'Số visa';

  @override
  String get visaExpiry => 'Hạn visa';

  @override
  String get employment => 'Công việc';

  @override
  String get onboard => 'ĐANG TRÊN TÀU';

  @override
  String get offboard => 'ĐÃ RỜI TÀU';

  @override
  String get joinDate => 'Ngày gia nhập';

  @override
  String get embarkDate => 'Ngày lên tàu';

  @override
  String get disembarkDate => 'Ngày rời tàu';

  @override
  String get contractEnd => 'Kết thúc hợp đồng';

  @override
  String get loadingCertificates => 'Đang tải chứng chỉ...';

  @override
  String get failedToLoadCertificates => 'Không thể tải chứng chỉ';

  @override
  String get noCertificateDataFound => 'Không tìm thấy dữ liệu chứng chỉ';

  @override
  String get stcwCertificate => 'Chứng chỉ STCW';

  @override
  String get medicalCertificate => 'Chứng chỉ Y tế';

  @override
  String get passport => 'Hộ chiếu';

  @override
  String get visa => 'Visa';

  @override
  String get seamanBook => 'Sổ thủy thủ';

  @override
  String certificatesExpired(int count) {
    return '$count chứng chỉ đã hết hạn!';
  }

  @override
  String certificatesExpiringSoon(int count) {
    return '$count chứng chỉ sắp hết hạn';
  }

  @override
  String get expiringSoon => 'Sắp hết hạn';

  @override
  String get valid => 'Còn hiệu lực';

  @override
  String get number => 'Số';

  @override
  String get issued => 'Cấp ngày';

  @override
  String get expires => 'Hết hạn';

  @override
  String daysRemaining(int count) {
    return 'Còn $count ngày';
  }

  @override
  String get maintenance => 'Bảo trì';

  @override
  String get loadingSchedule => 'Đang tải lịch trình...';

  @override
  String upcomingDays(int count) {
    return 'Sắp tới ($count ngày)';
  }

  @override
  String get thisWeek => 'Tuần này';

  @override
  String get thisMonth => 'Tháng này';

  @override
  String get allTasks => 'Tất cả công việc';

  @override
  String get total => 'Tổng';

  @override
  String get overdue => 'Quá hạn';

  @override
  String get dueSoon => 'Sắp đến hạn';

  @override
  String get noTasksScheduled => 'Không có công việc được lên lịch';

  @override
  String get noMaintenanceTasksMatch =>
      'Không có công việc bảo trì nào phù hợp với bộ lọc đã chọn';

  @override
  String get signed => 'Đã ký';

  @override
  String get unsigned => 'Chưa ký';

  @override
  String get personnel => 'Nhân sự';

  @override
  String get officerOnWatch => 'Sĩ quan trực ca';

  @override
  String get lookout => 'Người quan sát';

  @override
  String get weatherSeaConditions => 'Điều kiện thời tiết & biển';

  @override
  String get weather => 'Thời tiết';

  @override
  String get seaState => 'Tình trạng biển';

  @override
  String get visibility => 'Tầm nhìn';

  @override
  String get navigationData => 'Dữ liệu hàng hải';

  @override
  String get course => 'Hướng đi';

  @override
  String get speed => 'Tốc độ';

  @override
  String get distanceRun => 'Quãng đường';

  @override
  String get shipPosition => 'Vị trí';

  @override
  String get engineStatus => 'Tình trạng máy';

  @override
  String get masterSignature => 'Chữ ký thuyền trưởng';

  @override
  String get taskCompletedSuccessfully => 'Hoàn thành công việc thành công!';

  @override
  String get taskSavedWillSync => 'Đã lưu công việc. Sẽ đồng bộ khi có mạng.';

  @override
  String get taskId => 'Mã công việc';

  @override
  String get offlineTaskWillSync =>
      'Bạn đang ngoại tuyến. Công việc sẽ được đồng bộ khi có kết nối.';

  @override
  String get runningHoursRequired => 'Giờ vận hành *';

  @override
  String get enterCurrentRunningHours => 'Nhập giờ vận hành hiện tại';

  @override
  String get hours => 'giờ';

  @override
  String get pleaseEnterRunningHours => 'Vui lòng nhập giờ vận hành';

  @override
  String get pleaseEnterValidNumber => 'Vui lòng nhập số hợp lệ';

  @override
  String runningHoursCannotBeLess(double hours) {
    return 'Giờ vận hành không thể nhỏ hơn lần bảo trì cuối ($hours)';
  }

  @override
  String get sparePartsUsed => 'Phụ tùng đã dùng';

  @override
  String get listSparePartsUsed => 'Liệt kê các phụ tùng đã sử dụng (tùy chọn)';

  @override
  String get notes => 'Ghi chú';

  @override
  String get addAdditionalNotes =>
      'Thêm ghi chú hoặc quan sát bổ sung (tùy chọn)';

  @override
  String get completingTask => 'Đang hoàn thành công việc...';

  @override
  String get savingForOfflineSync => 'Đang lưu để đồng bộ ngoại tuyến...';

  @override
  String get refreshTaskData => 'Làm mới dữ liệu công việc';

  @override
  String get taskDataRefreshed => '✅ Đã làm mới dữ liệu công việc';

  @override
  String failedToRefresh(String error) {
    return 'Làm mới thất bại: $error';
  }

  @override
  String overdueDaysPastDue(int days) {
    return '⚠️ QUÁ HẠN: $days ngày quá hạn';
  }

  @override
  String get maintenanceSchedule => 'Lịch bảo trì';

  @override
  String get type => 'Loại';

  @override
  String get interval => 'Chu kỳ';

  @override
  String runningHoursValue(int hours) {
    return '$hours giờ vận hành';
  }

  @override
  String daysValue(int days) {
    return '$days ngày';
  }

  @override
  String get lastDone => 'Lần cuối';

  @override
  String get nextDue => 'Kế tiếp';

  @override
  String get daysUntilDue => 'Số ngày đến hạn';

  @override
  String get runningHours => 'Giờ vận hành';

  @override
  String get atLastMaintenance => 'Tại lần bảo trì cuối';

  @override
  String hoursValue(int hours) {
    return '$hours giờ';
  }

  @override
  String get assignment => 'Phân công';

  @override
  String get assignedTo => 'Giao cho';

  @override
  String get taskChecklist => '📋 Checklist công việc';

  @override
  String get completionDetails => 'Chi tiết hoàn thành';

  @override
  String get completedBy => 'Người hoàn thành';

  @override
  String get completedAt => 'Hoàn thành lúc';

  @override
  String get spareParts => 'Phụ tùng';

  @override
  String get errorLoadingChecklist => 'Lỗi tải checklist';

  @override
  String get tryAgain => 'Thử lại';

  @override
  String get noChecklistYet => 'Chưa có checklist';

  @override
  String get thisTaskHasNoDetails => 'Công việc này chưa có chi tiết';

  @override
  String progressCount(int completed, int total) {
    return 'Tiến độ: $completed/$total';
  }

  @override
  String get mandatory => 'BẮT BUỘC';

  @override
  String unitLabel(String unit) {
    return 'Đơn vị: $unit';
  }

  @override
  String get measurement => 'Đo đạc';

  @override
  String get checklist => 'Kiểm tra';

  @override
  String get inspection => 'Quan sát';

  @override
  String get completed => 'Đã hoàn thành';

  @override
  String measuredValueWithUnit(String value, String unit) {
    return 'Giá trị đo: $value $unit';
  }

  @override
  String get checkResult => 'Kết quả kiểm tra:';

  @override
  String get okPass => 'OK / Đạt';

  @override
  String get ngFail => 'NG / Không đạt';

  @override
  String get measuredValue => 'Giá trị đo được:';

  @override
  String get enterValue => 'Nhập giá trị...';

  @override
  String limitRange(String min, String max, String unit) {
    return 'Giới hạn: $min - $max $unit';
  }

  @override
  String get observationNotes => 'Ghi chú quan sát:';

  @override
  String get enterDetailedNotes => 'Nhập ghi chú chi tiết...';

  @override
  String get notesOptional => 'Ghi chú (tùy chọn):';

  @override
  String get addNotesIfNeeded => 'Thêm ghi chú nếu cần...';

  @override
  String get alreadyCompletedCanUpdate =>
      'Đã hoàn thành trước đó. Bạn có thể cập nhật lại.';

  @override
  String get tapToComplete => '👆 Chạm để hoàn thành';

  @override
  String savedItem(String item) {
    return '✅ Đã lưu: $item';
  }

  @override
  String errorMessage(String error) {
    return '❌ Lỗi: $error';
  }

  @override
  String get update => 'Cập nhật';

  @override
  String get complete => 'Hoàn thành';

  @override
  String get taskOverdue => 'Công việc quá hạn';

  @override
  String get thisTaskIsOverdue =>
      'Công việc này đã quá hạn. Bắt đầu ngay sẽ giúp bắt kịp công việc bảo trì, nhưng vui lòng hoàn thành càng sớm càng tốt.';

  @override
  String get startAnyway => 'Vẫn bắt đầu';

  @override
  String get overdueTaskStarted =>
      'Đã bắt đầu công việc quá hạn! Vui lòng hoàn thành càng sớm càng tốt.';

  @override
  String get taskStartedSuccessfully => 'Đã bắt đầu công việc thành công!';

  @override
  String failedToStartTask(String error) {
    return 'Không thể bắt đầu công việc: $error';
  }

  @override
  String get searchByEquipmentName => 'Tìm kiếm theo tên thiết bị...';

  @override
  String get noTasksFound => 'Không tìm thấy công việc';

  @override
  String get noTasksInCategory =>
      'Bạn không có công việc nào trong danh mục này';

  @override
  String get noPendingTasks => 'Không có công việc đang chờ';

  @override
  String get allTasksStartedOrCompleted =>
      'Tất cả công việc đã được bắt đầu hoặc hoàn thành';

  @override
  String get noTasksInProgress => 'Không có công việc đang thực hiện';

  @override
  String get startPendingTaskToSeeHere =>
      'Bắt đầu công việc đang chờ để xem ở đây';

  @override
  String get noOverdueTasks => 'Không có công việc quá hạn';

  @override
  String get allTasksOnSchedule => 'Tuyệt vời! Tất cả công việc đúng tiến độ';

  @override
  String get noCompletedTasks => 'Không có công việc đã hoàn thành';

  @override
  String get completedTasksAppearHere =>
      'Công việc đã hoàn thành sẽ hiển thị ở đây';

  @override
  String get noTasksMatchSearch => 'Không có công việc nào khớp với tìm kiếm';

  @override
  String get tryDifferentSearchTerm => 'Thử từ khóa tìm kiếm khác';
}
