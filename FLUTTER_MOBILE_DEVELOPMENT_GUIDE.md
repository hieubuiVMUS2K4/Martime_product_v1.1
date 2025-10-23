# 📱 FLUTTER MOBILE APP - HƯỚNG DẪN PHÁT TRIỂN CHO THUYỀN VIÊN

## 🎯 Tổng Quan Hệ Thống

### Kiến Trúc Mobile App
```
┌─────────────────────────────────────────────────────────────┐
│                    FLUTTER MOBILE APP                        │
│              (Chạy trên điện thoại thuyền viên)             │
│                                                               │
│  [Login] → [Profile] → [My Tasks] → [Schedule] → [Reports]  │
│                                                               │
│  • Không có Database local                                   │
│  • Cache data (SharedPreferences/Hive)                      │
│  • Kết nối LAN với Edge Server                              │
│  • Offline-first với sync queue                             │
└───────────────────┬─────────────────────────────────────────┘
                    │
                    │ HTTP/REST API
                    │ LAN: 192.168.1.x:5001
                    │
        ┌───────────▼───────────┐
        │   EDGE SERVER API     │
        │   (Trên tàu)          │
        │   Port 5001           │
        └───────────┬───────────┘
                    │
        ┌───────────▼───────────┐
        │  PostgreSQL Database  │
        │  (Trên tàu)           │
        │  Port 5433            │
        └───────────────────────┘
```

---

## 📂 CẤU TRÚC DỰ ÁN FLUTTER

### Thư Mục Gốc
```
frontend-mobile/
├── lib/
│   ├── main.dart                      # Entry point
│   ├── app.dart                       # MaterialApp config
│   │
│   ├── core/                          # Core functionality
│   │   ├── constants/
│   │   │   ├── api_constants.dart     # API endpoints
│   │   │   ├── app_constants.dart     # App configs
│   │   │   └── cache_keys.dart        # Cache keys
│   │   ├── network/
│   │   │   ├── api_client.dart        # HTTP client
│   │   │   ├── api_interceptor.dart   # Request/response interceptor
│   │   │   └── network_info.dart      # Check connectivity
│   │   ├── cache/
│   │   │   ├── cache_manager.dart     # Cache operations
│   │   │   └── sync_queue.dart        # Offline sync queue
│   │   ├── auth/
│   │   │   ├── auth_manager.dart      # Authentication
│   │   │   └── token_storage.dart     # JWT token storage
│   │   └── utils/
│   │       ├── date_formatter.dart
│   │       ├── validators.dart
│   │       └── error_handler.dart
│   │
│   ├── data/                          # Data layer
│   │   ├── models/
│   │   │   ├── crew_member.dart       # Crew model
│   │   │   ├── maintenance_task.dart  # Task model
│   │   │   ├── schedule.dart          # Schedule model
│   │   │   └── sync_item.dart         # Sync queue item
│   │   ├── repositories/
│   │   │   ├── crew_repository.dart
│   │   │   ├── task_repository.dart
│   │   │   └── sync_repository.dart
│   │   └── data_sources/
│   │       ├── remote/
│   │       │   ├── crew_api.dart
│   │       │   ├── task_api.dart
│   │       │   └── auth_api.dart
│   │       └── local/
│   │           └── cache_data_source.dart
│   │
│   ├── domain/                        # Business logic
│   │   ├── entities/
│   │   │   ├── crew.dart
│   │   │   └── task.dart
│   │   └── usecases/
│   │       ├── login_usecase.dart
│   │       ├── get_my_tasks_usecase.dart
│   │       ├── complete_task_usecase.dart
│   │       └── sync_data_usecase.dart
│   │
│   ├── presentation/                  # UI layer
│   │   ├── screens/
│   │   │   ├── splash/
│   │   │   │   └── splash_screen.dart
│   │   │   ├── auth/
│   │   │   │   ├── login_screen.dart
│   │   │   │   └── forgot_password_screen.dart
│   │   │   ├── home/
│   │   │   │   └── home_screen.dart
│   │   │   ├── profile/
│   │   │   │   ├── profile_screen.dart
│   │   │   │   └── certificates_screen.dart
│   │   │   ├── tasks/
│   │   │   │   ├── task_list_screen.dart
│   │   │   │   ├── task_detail_screen.dart
│   │   │   │   └── complete_task_screen.dart
│   │   │   ├── schedule/
│   │   │   │   └── schedule_screen.dart
│   │   │   └── settings/
│   │   │       ├── settings_screen.dart
│   │   │       └── server_config_screen.dart
│   │   ├── widgets/
│   │   │   ├── common/
│   │   │   │   ├── loading_widget.dart
│   │   │   │   ├── error_widget.dart
│   │   │   │   └── empty_state_widget.dart
│   │   │   ├── task/
│   │   │   │   ├── task_card.dart
│   │   │   │   └── priority_badge.dart
│   │   │   └── certificate/
│   │   │       └── certificate_status_card.dart
│   │   └── providers/
│   │       ├── auth_provider.dart
│   │       ├── task_provider.dart
│   │       └── sync_provider.dart
│   │
│   └── routes/
│       └── app_router.dart            # Navigation routes
│
├── assets/
│   ├── images/
│   │   └── logo.png
│   └── icons/
│
├── test/
│   ├── unit/
│   ├── widget/
│   └── integration/
│
├── pubspec.yaml                       # Dependencies
├── README.md
└── analysis_options.yaml              # Linting rules
```

---

## 📦 DEPENDENCIES (pubspec.yaml)

```yaml
name: maritime_crew_app
description: Mobile app for ship crew members to manage tasks and schedules
version: 1.0.0+1

environment:
  sdk: '>=3.0.0 <4.0.0'

dependencies:
  flutter:
    sdk: flutter

  # State Management
  provider: ^6.1.1                    # Simple state management
  flutter_riverpod: ^2.4.9           # Alternative: Riverpod

  # Network & API
  dio: ^5.4.0                         # HTTP client
  connectivity_plus: ^5.0.2           # Network connectivity
  retrofit: ^4.0.3                    # Type-safe REST client
  pretty_dio_logger: ^1.3.1           # API logging

  # Local Storage & Cache
  shared_preferences: ^2.2.2          # Simple key-value storage
  hive: ^2.2.3                        # Fast NoSQL database
  hive_flutter: ^1.1.0
  path_provider: ^2.1.1               # File system paths

  # Authentication & Security
  flutter_secure_storage: ^9.0.0     # Secure storage for tokens
  jwt_decoder: ^2.0.1                 # JWT parsing

  # UI Components
  cupertino_icons: ^1.0.6
  google_fonts: ^6.1.0                # Custom fonts
  flutter_svg: ^2.0.9                 # SVG support
  cached_network_image: ^3.3.0        # Image caching
  shimmer: ^3.0.0                     # Loading skeleton
  lottie: ^2.7.0                      # Animations

  # Date & Time
  intl: ^0.18.1                       # Internationalization
  timeago: ^3.6.0                     # Relative time

  # Forms & Validation
  flutter_form_builder: ^9.1.1
  form_builder_validators: ^9.1.0

  # QR Code Scanner (Optional - for equipment scanning)
  qr_code_scanner: ^1.0.1
  mobile_scanner: ^3.5.5

  # Utils
  get_it: ^7.6.4                      # Dependency injection
  logger: ^2.0.2                      # Logging
  uuid: ^4.2.2                        # UUID generation
  equatable: ^2.0.5                   # Value equality

dev_dependencies:
  flutter_test:
    sdk: flutter
  flutter_lints: ^3.0.1
  build_runner: ^2.4.7
  hive_generator: ^2.0.1
  retrofit_generator: ^8.0.4
  mockito: ^5.4.4                     # Testing

flutter:
  uses-material-design: true
  assets:
    - assets/images/
    - assets/icons/
```

---

## 🔧 CÁC FILE CHÍNH CẦN TẠO

### 1. **API Constants** (`lib/core/constants/api_constants.dart`)

```dart
class ApiConstants {
  // Base URL - Có thể thay đổi trong Settings
  static String baseUrl = 'http://192.168.1.100:5001'; // LAN IP của Edge Server
  
  // Auth Endpoints
  static const String login = '/api/auth/login';
  static const String logout = '/api/auth/logout';
  static const String refreshToken = '/api/auth/refresh';
  
  // Crew Endpoints
  static const String crewProfile = '/api/crew/me';
  static const String crewCertificates = '/api/crew/me/certificates';
  
  // Maintenance Task Endpoints
  static const String myTasks = '/api/maintenance/tasks/my-tasks';
  static const String taskDetail = '/api/maintenance/tasks/{id}';
  static const String completeTask = '/api/maintenance/tasks/{id}/complete';
  static const String startTask = '/api/maintenance/tasks/{id}/start';
  
  // Schedule Endpoints
  static const String mySchedule = '/api/schedule/my-schedule';
  static const String upcomingTasks = '/api/schedule/upcoming';
  
  // Sync Endpoints
  static const String syncStatus = '/api/sync/status';
  static const String syncData = '/api/sync/upload';
  
  // Timeouts
  static const int connectionTimeout = 30000; // 30 seconds
  static const int receiveTimeout = 30000;
  
  // Cache Duration
  static const Duration cacheDuration = Duration(hours: 1);
}
```

### 2. **API Client** (`lib/core/network/api_client.dart`)

```dart
import 'package:dio/dio.dart';
import 'package:pretty_dio_logger/pretty_dio_logger.dart';
import '../constants/api_constants.dart';
import 'api_interceptor.dart';

class ApiClient {
  late Dio _dio;
  
  ApiClient() {
    _dio = Dio(
      BaseOptions(
        baseUrl: ApiConstants.baseUrl,
        connectTimeout: Duration(milliseconds: ApiConstants.connectionTimeout),
        receiveTimeout: Duration(milliseconds: ApiConstants.receiveTimeout),
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json',
        },
      ),
    );
    
    // Add interceptors
    _dio.interceptors.addAll([
      ApiInterceptor(), // Custom interceptor for auth
      PrettyDioLogger(
        requestHeader: true,
        requestBody: true,
        responseBody: true,
        responseHeader: false,
        error: true,
        compact: true,
      ),
    ]);
  }
  
  Dio get dio => _dio;
  
  // Update base URL (when user changes server settings)
  void updateBaseUrl(String newBaseUrl) {
    _dio.options.baseUrl = newBaseUrl;
    ApiConstants.baseUrl = newBaseUrl;
  }
}
```

### 3. **Auth Interceptor** (`lib/core/network/api_interceptor.dart`)

```dart
import 'package:dio/dio.dart';
import '../auth/token_storage.dart';

class ApiInterceptor extends Interceptor {
  final TokenStorage _tokenStorage = TokenStorage();
  
  @override
  void onRequest(RequestOptions options, RequestInterceptorHandler handler) async {
    // Add JWT token to header if exists
    final token = await _tokenStorage.getAccessToken();
    if (token != null) {
      options.headers['Authorization'] = 'Bearer $token';
    }
    
    super.onRequest(options, handler);
  }
  
  @override
  void onError(DioException err, ErrorErrorInterceptorHandler handler) async {
    // Handle 401 Unauthorized - Refresh token or logout
    if (err.response?.statusCode == 401) {
      // Try to refresh token
      final refreshed = await _refreshToken();
      if (refreshed) {
        // Retry the request
        final opts = err.requestOptions;
        final token = await _tokenStorage.getAccessToken();
        opts.headers['Authorization'] = 'Bearer $token';
        
        try {
          final response = await Dio().fetch(opts);
          return handler.resolve(response);
        } catch (e) {
          return handler.reject(err);
        }
      } else {
        // Logout user
        await _tokenStorage.clearTokens();
        // Navigate to login screen
      }
    }
    
    super.onError(err, handler);
  }
  
  Future<bool> _refreshToken() async {
    // Implement token refresh logic
    return false;
  }
}
```

### 4. **Token Storage** (`lib/core/auth/token_storage.dart`)

```dart
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class TokenStorage {
  final FlutterSecureStorage _storage = const FlutterSecureStorage();
  
  static const String _accessTokenKey = 'access_token';
  static const String _refreshTokenKey = 'refresh_token';
  static const String _userIdKey = 'user_id';
  static const String _crewIdKey = 'crew_id';
  
  // Save tokens after login
  Future<void> saveTokens({
    required String accessToken,
    required String refreshToken,
    required int userId,
    required String crewId,
  }) async {
    await Future.wait([
      _storage.write(key: _accessTokenKey, value: accessToken),
      _storage.write(key: _refreshTokenKey, value: refreshToken),
      _storage.write(key: _userIdKey, value: userId.toString()),
      _storage.write(key: _crewIdKey, value: crewId),
    ]);
  }
  
  Future<String?> getAccessToken() async {
    return await _storage.read(key: _accessTokenKey);
  }
  
  Future<String?> getRefreshToken() async {
    return await _storage.read(key: _refreshTokenKey);
  }
  
  Future<String?> getCrewId() async {
    return await _storage.read(key: _crewIdKey);
  }
  
  Future<void> clearTokens() async {
    await _storage.deleteAll();
  }
}
```

### 5. **Cache Manager** (`lib/core/cache/cache_manager.dart`)

```dart
import 'package:hive/hive.dart';
import 'dart:convert';

class CacheManager {
  static const String _cacheBox = 'cache_box';
  
  Future<void> init() async {
    await Hive.openBox(_cacheBox);
  }
  
  // Save data to cache
  Future<void> saveData(String key, dynamic data) async {
    final box = Hive.box(_cacheBox);
    await box.put(key, jsonEncode({
      'data': data,
      'timestamp': DateTime.now().millisecondsSinceEpoch,
    }));
  }
  
  // Get cached data
  Future<dynamic> getData(String key) async {
    final box = Hive.box(_cacheBox);
    final cached = box.get(key);
    
    if (cached == null) return null;
    
    final decoded = jsonDecode(cached);
    final timestamp = decoded['timestamp'] as int;
    final now = DateTime.now().millisecondsSinceEpoch;
    
    // Check if cache is expired (1 hour)
    if (now - timestamp > 3600000) {
      await box.delete(key);
      return null;
    }
    
    return decoded['data'];
  }
  
  // Clear all cache
  Future<void> clearCache() async {
    final box = Hive.box(_cacheBox);
    await box.clear();
  }
}
```

### 6. **Sync Queue** (`lib/core/cache/sync_queue.dart`)

```dart
import 'package:hive/hive.dart';
import '../network/network_info.dart';
import '../../data/models/sync_item.dart';

class SyncQueue {
  static const String _syncBox = 'sync_queue';
  final NetworkInfo _networkInfo;
  
  SyncQueue(this._networkInfo);
  
  // Add item to sync queue (when offline)
  Future<void> addToQueue(SyncItem item) async {
    final box = await Hive.openBox<SyncItem>(_syncBox);
    await box.add(item);
  }
  
  // Get all pending items
  Future<List<SyncItem>> getPendingItems() async {
    final box = await Hive.openBox<SyncItem>(_syncBox);
    return box.values.toList();
  }
  
  // Process sync queue when online
  Future<void> processSyncQueue() async {
    if (!await _networkInfo.isConnected) {
      return;
    }
    
    final box = await Hive.openBox<SyncItem>(_syncBox);
    final items = box.values.toList();
    
    for (var item in items) {
      try {
        // Send to server based on item type
        await _syncItemToServer(item);
        
        // Remove from queue after successful sync
        final key = box.keys.firstWhere((k) => box.get(k) == item);
        await box.delete(key);
      } catch (e) {
        // Keep in queue if sync fails
        print('Sync failed for item ${item.id}: $e');
      }
    }
  }
  
  Future<void> _syncItemToServer(SyncItem item) async {
    // Implement sync logic based on item type
    switch (item.type) {
      case SyncItemType.taskComplete:
        // Call complete task API
        break;
      case SyncItemType.taskStart:
        // Call start task API
        break;
      // Add more cases
    }
  }
  
  // Get queue size
  Future<int> getQueueSize() async {
    final box = await Hive.openBox<SyncItem>(_syncBox);
    return box.length;
  }
}
```

### 7. **Network Info** (`lib/core/network/network_info.dart`)

```dart
import 'package:connectivity_plus/connectivity_plus.dart';

class NetworkInfo {
  final Connectivity _connectivity = Connectivity();
  
  Future<bool> get isConnected async {
    final result = await _connectivity.checkConnectivity();
    return result != ConnectivityResult.none;
  }
  
  Stream<ConnectivityResult> get onConnectivityChanged {
    return _connectivity.onConnectivityChanged;
  }
}
```

---

## 🎨 CÁC SCREEN CHÍNH

### 1. **Login Screen** (`lib/presentation/screens/auth/login_screen.dart`)

**Chức năng:**
- Input: Crew ID + Password
- Lưu token vào secure storage
- Cache thông tin user
- Kiểm tra kết nối LAN với Edge Server

**UI Elements:**
- Logo Maritime
- TextField: Crew ID (VD: CM001)
- TextField: Password
- Button: Login
- Link: Forgot Password / Settings (Server Config)

### 2. **Home Screen** (`lib/presentation/screens/home/home_screen.dart`)

**Chức năng:**
- Dashboard tổng quan
- Quick stats: Tasks pending, overdue, completed today
- Recent activities
- Navigation to other screens

**UI Elements:**
- AppBar với avatar + tên crew
- Stats cards
- Quick action buttons
- Bottom Navigation Bar: Home, Tasks, Schedule, Profile

### 3. **Profile Screen** (`lib/presentation/screens/profile/profile_screen.dart`)

**Chức năng:**
- Hiển thị thông tin cá nhân
- Xem certificates (STCW, Medical, Passport)
- Status badges (Valid, Expiring Soon, Expired)

**UI Elements:**
- Avatar + Crew info
- Personal details (readonly)
- Certificate status cards với màu sắc
- Navigation to Certificate details

### 4. **Task List Screen** (`lib/presentation/screens/tasks/task_list_screen.dart`)

**Chức năng:**
- Hiển thị tasks được assigned cho crew hiện tại
- Filter: All, Pending, In Progress, Overdue
- Pull to refresh
- Search by equipment name

**UI Elements:**
- Tabs: All | Pending | Overdue
- Task cards với priority badges
- Countdown timer (days until due)
- Click vào task → Task Detail

### 5. **Task Detail Screen** (`lib/presentation/screens/tasks/task_detail_screen.dart`)

**Chức năng:**
- Xem chi tiết maintenance task
- Start task (nếu pending)
- Complete task (nếu in progress)
- View equipment info
- View history

**UI Elements:**
- Equipment name + Task ID
- Priority & Status badges
- Due date với countdown
- Task description
- Interval info
- Last done info
- Action buttons: Start / Complete
- Notes section

### 6. **Complete Task Screen** (`lib/presentation/screens/tasks/complete_task_screen.dart`)

**Chức năng:**
- Form nhập thông tin hoàn thành task
- Offline support: Lưu vào sync queue nếu mất mạng
- Validation form

**UI Elements:**
- Form fields:
  - Running Hours (number)
  - Spare Parts Used (text)
  - Completion Notes (textarea)
  - Photo upload (optional)
- Button: Complete Task
- Offline indicator

### 7. **Schedule Screen** (`lib/presentation/screens/schedule/schedule_screen.dart`)

**Chức năng:**
- Calendar view của tasks
- Xem tasks theo timeline
- Filter by week/month

**UI Elements:**
- Calendar với dots trên ngày có task
- List tasks của ngày được chọn
- Timeline view
- Legend: Priority colors

### 8. **Settings Screen** (`lib/presentation/screens/settings/settings_screen.dart`)

**Chức năng:**
- Server configuration (Change LAN IP)
- Clear cache
- View sync queue status
- Logout

**UI Elements:**
- Server IP input
- Test connection button
- Cache size display
- Sync queue count
- Clear cache button
- Logout button

---

## 🔐 AUTHENTICATION FLOW

```dart
// lib/presentation/screens/auth/login_screen.dart

class LoginScreen extends StatefulWidget {
  @override
  _LoginScreenState createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  final _crewIdController = TextEditingController();
  final _passwordController = TextEditingController();
  bool _isLoading = false;
  
  Future<void> _login() async {
    if (!_formKey.currentState!.validate()) return;
    
    setState(() => _isLoading = true);
    
    try {
      // Call login API
      final response = await AuthApi().login(
        crewId: _crewIdController.text,
        password: _passwordController.text,
      );
      
      // Save tokens
      await TokenStorage().saveTokens(
        accessToken: response.accessToken,
        refreshToken: response.refreshToken,
        userId: response.userId,
        crewId: response.crewId,
      );
      
      // Cache user info
      await CacheManager().saveData('user_profile', response.user);
      
      // Navigate to home
      Navigator.pushReplacementNamed(context, '/home');
    } catch (e) {
      // Show error
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Login failed: ${e.toString()}')),
      );
    } finally {
      setState(() => _isLoading = false);
    }
  }
  
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(
        child: Padding(
          padding: EdgeInsets.all(24),
          child: Form(
            key: _formKey,
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                // Logo
                Image.asset('assets/images/logo.png', height: 120),
                SizedBox(height: 48),
                
                // Crew ID field
                TextFormField(
                  controller: _crewIdController,
                  decoration: InputDecoration(
                    labelText: 'Crew ID',
                    prefixIcon: Icon(Icons.person),
                    border: OutlineInputBorder(),
                  ),
                  validator: (value) {
                    if (value?.isEmpty ?? true) {
                      return 'Please enter your Crew ID';
                    }
                    return null;
                  },
                ),
                SizedBox(height: 16),
                
                // Password field
                TextFormField(
                  controller: _passwordController,
                  obscureText: true,
                  decoration: InputDecoration(
                    labelText: 'Password',
                    prefixIcon: Icon(Icons.lock),
                    border: OutlineInputBorder(),
                  ),
                  validator: (value) {
                    if (value?.isEmpty ?? true) {
                      return 'Please enter your password';
                    }
                    return null;
                  },
                ),
                SizedBox(height: 24),
                
                // Login button
                SizedBox(
                  width: double.infinity,
                  height: 48,
                  child: ElevatedButton(
                    onPressed: _isLoading ? null : _login,
                    child: _isLoading
                        ? CircularProgressIndicator(color: Colors.white)
                        : Text('Login'),
                  ),
                ),
                
                // Settings link
                TextButton(
                  onPressed: () {
                    Navigator.pushNamed(context, '/settings/server');
                  },
                  child: Text('Server Settings'),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
```

---

## 📊 DATA MODELS

### **Crew Member Model** (`lib/data/models/crew_member.dart`)

```dart
import 'package:json_annotation/json_annotation.dart';

part 'crew_member.g.dart';

@JsonSerializable()
class CrewMember {
  final int id;
  final String crewId;
  final String fullName;
  final String position;
  final String? rank;
  final String? department;
  final String? nationality;
  final String? dateOfBirth;
  final String? phoneNumber;
  final String? email;
  
  // Certificates
  final String? certificateNumber;
  final String? certificateIssue;
  final String? certificateExpiry;
  final String? medicalIssue;
  final String? medicalExpiry;
  final String? passportNumber;
  final String? passportExpiry;
  
  final bool isOnboard;
  final String createdAt;
  
  CrewMember({
    required this.id,
    required this.crewId,
    required this.fullName,
    required this.position,
    this.rank,
    this.department,
    this.nationality,
    this.dateOfBirth,
    this.phoneNumber,
    this.email,
    this.certificateNumber,
    this.certificateIssue,
    this.certificateExpiry,
    this.medicalIssue,
    this.medicalExpiry,
    this.passportNumber,
    this.passportExpiry,
    required this.isOnboard,
    required this.createdAt,
  });
  
  factory CrewMember.fromJson(Map<String, dynamic> json) =>
      _$CrewMemberFromJson(json);
  
  Map<String, dynamic> toJson() => _$CrewMemberToJson(this);
  
  // Certificate status checks
  bool get isCertificateExpiring {
    if (certificateExpiry == null) return false;
    final expiry = DateTime.parse(certificateExpiry!);
    final daysLeft = expiry.difference(DateTime.now()).inDays;
    return daysLeft <= 90 && daysLeft > 0;
  }
  
  bool get isCertificateExpired {
    if (certificateExpiry == null) return false;
    final expiry = DateTime.parse(certificateExpiry!);
    return expiry.isBefore(DateTime.now());
  }
}
```

### **Maintenance Task Model** (`lib/data/models/maintenance_task.dart`)

```dart
import 'package:json_annotation/json_annotation.dart';

part 'maintenance_task.g.dart';

@JsonSerializable()
class MaintenanceTask {
  final int id;
  final String taskId;
  final String equipmentId;
  final String equipmentName;
  final String taskType;
  final String taskDescription;
  final double? intervalHours;
  final int? intervalDays;
  final String? lastDoneAt;
  final String nextDueAt;
  final double? runningHoursAtLastDone;
  final String priority;
  final String status;
  final String? assignedTo;
  final String? completedAt;
  final String? completedBy;
  final String? notes;
  final String? sparePartsUsed;
  final bool isSynced;
  final String createdAt;
  
  MaintenanceTask({
    required this.id,
    required this.taskId,
    required this.equipmentId,
    required this.equipmentName,
    required this.taskType,
    required this.taskDescription,
    this.intervalHours,
    this.intervalDays,
    this.lastDoneAt,
    required this.nextDueAt,
    this.runningHoursAtLastDone,
    required this.priority,
    required this.status,
    this.assignedTo,
    this.completedAt,
    this.completedBy,
    this.notes,
    this.sparePartsUsed,
    required this.isSynced,
    required this.createdAt,
  });
  
  factory MaintenanceTask.fromJson(Map<String, dynamic> json) =>
      _$MaintenanceTaskFromJson(json);
  
  Map<String, dynamic> toJson() => _$MaintenanceTaskToJson(this);
  
  // Computed properties
  int get daysUntilDue {
    final due = DateTime.parse(nextDueAt);
    return due.difference(DateTime.now()).inDays;
  }
  
  bool get isOverdue => daysUntilDue < 0;
  bool get isDueSoon => daysUntilDue >= 0 && daysUntilDue <= 7;
  
  String get priorityColor {
    switch (priority) {
      case 'CRITICAL': return '#DC2626'; // Red
      case 'HIGH': return '#EA580C';    // Orange
      case 'NORMAL': return '#2563EB';  // Blue
      case 'LOW': return '#6B7280';     // Gray
      default: return '#6B7280';
    }
  }
}
```

### **Sync Item Model** (`lib/data/models/sync_item.dart`)

```dart
import 'package:hive/hive.dart';
import 'package:uuid/uuid.dart';

part 'sync_item.g.dart';

@HiveType(typeId: 0)
class SyncItem extends HiveObject {
  @HiveField(0)
  final String id;
  
  @HiveField(1)
  final SyncItemType type;
  
  @HiveField(2)
  final Map<String, dynamic> data;
  
  @HiveField(3)
  final DateTime createdAt;
  
  @HiveField(4)
  int retryCount;
  
  SyncItem({
    String? id,
    required this.type,
    required this.data,
    DateTime? createdAt,
    this.retryCount = 0,
  })  : id = id ?? const Uuid().v4(),
        createdAt = createdAt ?? DateTime.now();
}

@HiveType(typeId: 1)
enum SyncItemType {
  @HiveField(0)
  taskComplete,
  
  @HiveField(1)
  taskStart,
  
  @HiveField(2)
  profileUpdate,
}
```

---

## 🔄 OFFLINE-FIRST STRATEGY

### Cơ Chế Hoạt Động

1. **Online Mode** (Kết nối LAN):
   - Gọi API trực tiếp
   - Cache response để dùng offline
   - Hiển thị data real-time

2. **Offline Mode** (Mất kết nối):
   - Đọc data từ cache
   - User actions → Sync queue
   - Hiển thị offline indicator

3. **Sync Process** (Khi online trở lại):
   - Background service check connectivity
   - Process sync queue tự động
   - Retry failed items
   - Clear cache sau khi sync thành công

### Implementation

```dart
// lib/data/repositories/task_repository.dart

class TaskRepository {
  final TaskApi _api;
  final CacheManager _cache;
  final SyncQueue _syncQueue;
  final NetworkInfo _networkInfo;
  
  TaskRepository(this._api, this._cache, this._syncQueue, this._networkInfo);
  
  // Get my tasks
  Future<List<MaintenanceTask>> getMyTasks() async {
    try {
      if (await _networkInfo.isConnected) {
        // Online: Fetch from API
        final tasks = await _api.getMyTasks();
        
        // Cache for offline use
        await _cache.saveData('my_tasks', tasks);
        
        return tasks;
      } else {
        // Offline: Load from cache
        final cached = await _cache.getData('my_tasks');
        if (cached != null) {
          return (cached as List)
              .map((e) => MaintenanceTask.fromJson(e))
              .toList();
        }
        throw Exception('No cached data available');
      }
    } catch (e) {
      throw Exception('Failed to get tasks: $e');
    }
  }
  
  // Complete task
  Future<void> completeTask({
    required int taskId,
    required String completedBy,
    double? runningHours,
    String? sparePartsUsed,
    String? notes,
  }) async {
    final completeData = {
      'taskId': taskId,
      'completedBy': completedBy,
      'runningHours': runningHours,
      'sparePartsUsed': sparePartsUsed,
      'notes': notes,
      'completedAt': DateTime.now().toIso8601String(),
    };
    
    if (await _networkInfo.isConnected) {
      // Online: Send immediately
      await _api.completeTask(taskId, completeData);
    } else {
      // Offline: Add to sync queue
      await _syncQueue.addToQueue(
        SyncItem(
          type: SyncItemType.taskComplete,
          data: completeData,
        ),
      );
    }
  }
}
```

---

## 🚀 CÁC BƯỚC THỰC HIỆN

### Phase 1: Setup & Authentication (Week 1)
1. ✅ Tạo Flutter project
2. ✅ Setup dependencies
3. ✅ Tạo folder structure
4. ✅ Implement API Client & Interceptor
5. ✅ Implement Token Storage
6. ✅ Build Login Screen
7. ✅ Test authentication với Edge Server

### Phase 2: Core Features (Week 2-3)
8. ✅ Implement Data Models
9. ✅ Implement Repositories
10. ✅ Build Home Screen
11. ✅ Build Profile Screen
12. ✅ Build Task List Screen
13. ✅ Build Task Detail Screen
14. ✅ Test CRUD operations

### Phase 3: Offline Support (Week 4)
15. ✅ Implement Cache Manager
16. ✅ Implement Sync Queue
17. ✅ Implement Network Info
18. ✅ Build Complete Task Screen with offline support
19. ✅ Test offline scenarios

### Phase 4: Polish & Testing (Week 5)
20. ✅ Build Schedule Screen
21. ✅ Build Settings Screen
22. ✅ Add loading states & error handling
23. ✅ Add animations & transitions
24. ✅ Integration testing
25. ✅ Bug fixes & optimization

---

## 🎯 BACKEND ENDPOINTS CẦN BỔ SUNG

Bạn cần thêm các endpoints sau vào **Edge Services**:

### 1. **Authentication API**
```csharp
// POST /api/auth/login
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    // Validate Crew ID + Password
    // Generate JWT token
    // Return: accessToken, refreshToken, user info
}

// POST /api/auth/refresh
[HttpPost("refresh")]
public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
{
    // Validate refresh token
    // Generate new access token
}
```

### 2. **Crew API - My Profile**
```csharp
// GET /api/crew/me
[HttpGet("me")]
[Authorize]
public async Task<IActionResult> GetMyProfile()
{
    // Get crew info from JWT token
    // Return crew member details
}
```

### 3. **Maintenance API - My Tasks**
```csharp
// GET /api/maintenance/tasks/my-tasks
[HttpGet("tasks/my-tasks")]
[Authorize]
public async Task<IActionResult> GetMyTasks()
{
    // Get crew ID from JWT
    // Return tasks assigned to current crew
    var tasks = await _context.MaintenanceTasks
        .Where(t => t.AssignedTo == currentCrewName)
        .OrderBy(t => t.NextDueAt)
        .ToListAsync();
    
    return Ok(tasks);
}

// POST /api/maintenance/tasks/{id}/start
[HttpPost("tasks/{id}/start")]
[Authorize]
public async Task<IActionResult> StartTask(long id)
{
    // Update status to IN_PROGRESS
    // Log who started
}
```

---

## 📱 TESTING SCENARIOS

### 1. Login Test
- ✅ Login with valid credentials
- ✅ Login with invalid credentials
- ✅ Token persistence after app restart
- ✅ Auto-logout on token expiration

### 2. Task Management Test
- ✅ View assigned tasks
- ✅ Filter tasks by status
- ✅ Start task
- ✅ Complete task online
- ✅ Complete task offline → Sync when online

### 3. Offline Test
- ✅ Turn off WiFi
- ✅ Navigate app (read cached data)
- ✅ Complete task (add to queue)
- ✅ Turn on WiFi
- ✅ Verify auto-sync

### 4. Certificate Status Test
- ✅ View valid certificates (green)
- ✅ View expiring certificates (yellow)
- ✅ View expired certificates (red)

---

## 🔒 SECURITY CONSIDERATIONS

1. **Token Storage**: Sử dụng `flutter_secure_storage` cho JWT
2. **HTTPS**: Trong production, Edge Server phải dùng HTTPS
3. **Certificate Pinning**: Verify SSL certificate của server
4. **Input Validation**: Validate tất cả user input
5. **Sensitive Data**: Không log passwords hay tokens
6. **Session Timeout**: Auto-logout sau thời gian inactive

---

## 📚 TÀI LIỆU THAM KHẢO

- **Flutter Docs**: https://docs.flutter.dev
- **Dio Package**: https://pub.dev/packages/dio
- **Hive Database**: https://docs.hivedb.dev
- **Provider State Management**: https://pub.dev/packages/provider
- **JWT Handling**: https://pub.dev/packages/jwt_decoder

---

## 🎉 KẾT LUẬN

Với kiến trúc này, mobile app sẽ:
- ✅ **Offline-first**: Hoạt động mượt mà kể cả khi mất mạng
- ✅ **LAN Connectivity**: Kết nối nhanh với Edge Server trên tàu
- ✅ **Cache Smart**: Giảm băng thông, tăng tốc độ
- ✅ **Auto Sync**: Đồng bộ tự động khi online
- ✅ **Professional UI**: Giao diện chuyên nghiệp cho thuyền viên
- ✅ **Secure**: Bảo mật với JWT + Secure Storage
- ✅ **Maritime Focused**: Tập trung vào workflow hàng hải thực tế

**Bước tiếp theo**: Tạo Flutter project và bắt đầu implement từ Phase 1! 🚢⚓
