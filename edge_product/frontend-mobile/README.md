# 📱 Maritime Crew Mobile App

Mobile application cho thuyền viên quản lý maintenance tasks trên tàu.

## 🚀 Getting Started

### Prerequisites

- Flutter SDK (>= 3.0.0)
- Dart SDK (>= 3.0.0)
- Android Studio / VS Code
- Git

### Cài đặt Flutter

#### Windows:
```bash
# Download Flutter SDK từ https://flutter.dev
# Giải nén và add vào PATH
# Verify installation
flutter doctor
```

#### Kiểm tra requirements:
```bash
flutter doctor -v
```

## 📦 Installation

### 1. Clone repository (nếu có)
```bash
cd d:\flutter_project\frontend-mobile
```

### 2. Install dependencies
```bash
flutter pub get
```

### 3. Generate code (cho Hive adapters)
```bash
flutter pub run build_runner build --delete-conflicting-outputs
```

### 4. Run app

#### Trên Android Emulator:
```bash
flutter run
```

#### Trên thiết bị thực:
```bash
# Enable USB debugging trên điện thoại
# Connect điện thoại vào máy tính
flutter devices
flutter run -d <device_id>
```

## 📁 Project Structure

```
lib/
├── main.dart                          # Entry point
├── app.dart                           # MaterialApp configuration
│
├── core/                              # Core functionality
│   ├── constants/                     # Constants
│   │   ├── api_constants.dart        # API endpoints
│   │   ├── app_constants.dart        # App constants
│   │   └── cache_keys.dart           # Cache keys
│   ├── network/                       # Network layer
│   │   ├── api_client.dart           # HTTP client (Dio)
│   │   ├── api_interceptor.dart      # JWT interceptor
│   │   └── network_info.dart         # Connectivity check
│   ├── cache/                         # Cache & offline
│   │   ├── cache_manager.dart        # Cache operations
│   │   └── sync_queue.dart           # Offline sync queue
│   └── auth/                          # Authentication
│       └── token_storage.dart        # JWT token storage
│
├── data/                              # Data layer
│   ├── models/                        # Data models
│   │   ├── crew_member.dart          # Crew model
│   │   ├── maintenance_task.dart     # Task model
│   │   └── sync_item.dart            # Sync queue item
│   ├── repositories/                  # Repositories
│   └── data_sources/                  # Data sources
│       ├── remote/                    # API calls
│       └── local/                     # Local storage
│
└── presentation/                      # UI layer
    ├── screens/                       # Screens
    │   ├── auth/
    │   │   └── login_screen.dart     # Login screen
    │   ├── home/
    │   │   └── home_screen.dart      # Home/Dashboard
    │   ├── tasks/                     # Task screens
    │   ├── profile/                   # Profile screens
    │   └── settings/                  # Settings
    ├── widgets/                       # Reusable widgets
    │   ├── common/                    # Common widgets
    │   └── task/                      # Task-specific widgets
    └── providers/                     # State management
        ├── auth_provider.dart        # Auth state
        ├── task_provider.dart        # Task state
        └── sync_provider.dart        # Sync state
```

## 🔧 Configuration

### 1. Configure Edge Server URL

Mặc định app kết nối tới: `http://192.168.1.100:5001`

Để thay đổi, edit file `lib/core/constants/api_constants.dart`:

```dart
class ApiConstants {
  static String baseUrl = 'http://YOUR_EDGE_SERVER_IP:5001';
  // ...
}
```

Hoặc thay đổi trong Settings screen của app (coming soon).

### 2. Build cho Production

#### Android APK:
```bash
flutter build apk --release
```

APK sẽ được tạo tại: `build/app/outputs/flutter-apk/app-release.apk`

#### Android App Bundle (cho Google Play):
```bash
flutter build appbundle --release
```

## 🧪 Testing

### Run unit tests:
```bash
flutter test
```

### Run integration tests:
```bash
flutter test integration_test
```

## 📱 Tính năng chính

### ✅ Đã implement:
- [x] Login authentication
- [x] JWT token storage (secure)
- [x] Home dashboard với stats
- [x] Offline-first architecture
- [x] Sync queue cho offline mode
- [x] Network connectivity check
- [x] Cache management

### 🚧 Đang phát triển:
- [ ] Task list screen
- [ ] Task detail screen
- [ ] Complete task screen
- [ ] Profile screen với certificates
- [ ] Schedule screen
- [ ] Settings screen
- [ ] Server URL configuration
- [ ] Photo upload
- [ ] Push notifications

## 🔐 Authentication

App sử dụng JWT authentication:

1. User login với Crew ID + Password
2. Server trả về Access Token & Refresh Token
3. Access Token được lưu trong Secure Storage
4. Mọi API request đều attach Access Token vào header
5. Khi token expire, tự động refresh hoặc logout

## 📶 Offline Mode

App hoạt động offline-first:

1. **Online**: Fetch data từ API → Cache vào local
2. **Offline**: Đọc data từ cache
3. **User actions**: Lưu vào sync queue
4. **Khi online trở lại**: Tự động sync queue lên server

## 🛠️ Technologies

- **Flutter**: UI framework
- **Dart**: Programming language
- **Dio**: HTTP client
- **Provider**: State management
- **Hive**: Local database (NoSQL)
- **Secure Storage**: JWT token storage
- **Connectivity Plus**: Network check
- **Google Fonts**: Custom fonts

## 📝 API Integration

App kết nối với Edge Server API:

### Endpoints:
- `POST /api/auth/login` - Login
- `POST /api/auth/refresh` - Refresh token
- `GET /api/crew/me` - Get my profile
- `GET /api/maintenance/tasks/my-tasks` - Get my tasks
- `POST /api/maintenance/tasks/{id}/complete` - Complete task
- `POST /api/maintenance/tasks/{id}/start` - Start task

Xem chi tiết API trong file `DATABASE_SCHEMA_FOR_MOBILE_SYNC.md`

---

## 🔄 Chuyển từ Mock Data sang Backend thật

### Hiện trạng:
App đang sử dụng **MOCK DATA** để test UI và flow. Các providers đã connect với repositories nhưng AuthProvider vẫn dùng mock login.

### Các file cần update khi có Backend:

#### 1️⃣ **AuthProvider** (`lib/presentation/providers/auth_provider.dart`)

**TRƯỚC (Mock - Dòng 56-67):**
```dart
// TODO: Call login API
// For now, just simulate success
await Future.delayed(const Duration(seconds: 2));

// Save tokens (mock data)
await _tokenStorage.saveTokens(
  accessToken: 'mock_access_token',
  refreshToken: 'mock_refresh_token',
  userId: 1,
  crewId: crewId,
  fullName: 'Crew Member',
  position: 'Engineer',
);
```

**SAU (Real API):**
```dart
// Import repository
import '../../data/repositories/auth_repository.dart';
import '../../core/network/api_client.dart';

// In AuthProvider class, add:
final AuthRepository _authRepository;

// Constructor:
AuthProvider()
    : _authRepository = AuthRepository(
        apiClient: ApiClient(),
        tokenStorage: TokenStorage(),
      );

// In login method:
final response = await _authRepository.login(
  crewId: crewId,
  password: password,
);

// response.userId, response.crewId, response.fullName đã tự động được save
_userId = response.userId;
_crewId = response.crewId;
_fullName = response.fullName;
_position = response.position;
```

#### 2️⃣ **TaskProvider** (`lib/presentation/providers/task_provider.dart`)

✅ **ĐÃ HOÀN THIỆN** - Đã connect với TaskRepository thật, sẵn sàng call API

#### 3️⃣ **CrewRepository** (`lib/data/repositories/crew_repository.dart`)

✅ **ĐÃ HOÀN THIỆN** - Profile screens có thể gọi `getMyProfile()` và `getMyCertificates()`

#### 4️⃣ **API Base URL** (`lib/core/constants/api_constants.dart`)

Update IP address của Edge Server:
```dart
static String baseUrl = 'http://192.168.1.100:5001'; // LAN IP trên tàu
```

### Checklist khi chuyển sang Backend:

- [ ] **1. Verify Backend đang chạy**
  ```bash
  # Test từ browser hoặc Postman
  curl http://192.168.1.100:5001/api/health
  ```

- [ ] **2. Update API Base URL**
  - Sửa `lib/core/constants/api_constants.dart`
  - Hoặc dùng Settings screen để config

- [ ] **3. Update AuthProvider**
  - Xóa mock code (dòng 56-67)
  - Thêm AuthRepository
  - Call `_authRepository.login()` thay vì mock

- [ ] **4. Test Login flow**
  - Run app: `flutter run -d windows`
  - Login với Crew ID thật từ database
  - Verify JWT token được lưu

- [ ] **5. Test Task flow**
  - Fetch my tasks → Verify data từ API
  - Start task → Check database updated
  - Complete task → Check completion recorded

- [ ] **6. Test Offline mode**
  - Disconnect WiFi
  - App vẫn hiển thị cached data
  - Complete task → Lưu vào sync queue
  - Reconnect → Auto sync

- [ ] **7. Test Certificate expiry warnings**
  - Profile → Certificates
  - Verify màu sắc: Red (expired), Orange (expiring), Green (valid)

### Quick Command để test:

```bash
# 1. Rebuild với clean state
flutter clean
flutter pub get
flutter pub run build_runner build --delete-conflicting-outputs

# 2. Run trên Windows (cho testing nhanh)
flutter run -d windows

# 3. Run trên Android emulator
flutter emulators --launch <emulator_id>
flutter run

# 4. Build APK để test trên điện thoại thật
flutter build apk --debug
# APK: build/app/outputs/flutter-apk/app-debug.apk
```

### Debugging Tips:

**1. Check API calls trong console:**
- Pretty Dio Logger sẽ log tất cả requests/responses
- Màu xanh = Success, Màu đỏ = Error

**2. Check cache:**
```dart
// In any screen, temporary debug:
final cache = await CacheManager().getData(CacheKeys.myTasks);
print('Cached tasks: $cache');
```

**3. Check sync queue:**
```dart
// In Settings screen:
final queueSize = await SyncQueue(NetworkInfo()).getQueueSize();
print('Pending sync items: $queueSize');
```

**4. Clear data for fresh start:**
- Settings → Clear Cache
- Hoặc: Uninstall app và reinstall

### Estimated time: 30 phút
- Update AuthProvider: 5 phút
- Test login: 5 phút  
- Test tasks flow: 10 phút
- Test offline mode: 10 phút

## 🐛 Troubleshooting

### Lỗi: "packages not found"
```bash
flutter pub get
```

### Lỗi: "build_runner errors"
```bash
flutter pub run build_runner build --delete-conflicting-outputs
```

### Lỗi: "No devices found"
```bash
# Kiểm tra Android Emulator
flutter emulators
flutter emulators --launch <emulator_id>

# Hoặc check USB debugging
adb devices
```

### Clean build
```bash
flutter clean
flutter pub get
flutter run
```

## 📖 Documentation

- [Flutter Docs](https://docs.flutter.dev)
- [Dart Docs](https://dart.dev/guides)
- [Provider Package](https://pub.dev/packages/provider)
- [Dio Package](https://pub.dev/packages/dio)
- [Hive Database](https://docs.hivedb.dev)

## 🤝 Development Workflow

1. Tạo feature branch
2. Implement feature
3. Test trên emulator/device
4. Commit với clear message
5. Merge vào main branch
6. Build release APK

## 📧 Support

Nếu gặp vấn đề, liên hệ development team.

## 📄 License

Copyright © 2024 Maritime Crew App

---

**🚢 Built for seafarers, by developers ⚓**
