# 🚀 QUICK START - Maritime Crew App

## ⚡ Chạy App trong 3 bước

### 1. Install dependencies
```bash
cd d:\flutter_project\frontend-mobile
flutter pub get
```

### 2. Generate code
```bash
flutter pub run build_runner build --delete-conflicting-outputs
```

### 3. Run app
```bash
# Windows (nhanh nhất cho testing)
flutter run -d windows

# Android Emulator
flutter emulators --launch <emulator_name>
flutter run

# Android Device (USB debugging enabled)
flutter run -d <device_id>
```

---

## 🎮 Mock Data Quick Test

### Login Credentials:
```
Crew ID: CM001  →  John Smith (Chief Engineer)
Crew ID: CM002  →  David Wilson (2nd Engineer)
Crew ID: CM003  →  Mike Johnson (Electrician)
Password: Anything (not validated in mock mode)
```

### Mock Tasks Available: 5 tasks
- 3 PENDING
- 1 IN_PROGRESS
- 1 COMPLETED

---

## 🔄 Chuyển sang Backend thật (30 phút)

### Step 1: Update API URL
**File:** `lib/core/constants/api_constants.dart`
```dart
static String baseUrl = 'http://192.168.1.100:5001'; // Change to your IP
```

### Step 2: Update AuthProvider
**File:** `lib/presentation/providers/auth_provider.dart`

**Xóa dòng 56-77** (mock code) và thay bằng:
```dart
// Import at top
import '../../data/repositories/auth_repository.dart';
import '../../core/network/api_client.dart';

// Add to class
final AuthRepository _authRepository;

// Constructor
AuthProvider()
    : _authRepository = AuthRepository(
        apiClient: ApiClient(),
        tokenStorage: _tokenStorage,
      ),
      _tokenStorage = TokenStorage(),
      _cacheManager = CacheManager();

// In login method, replace mock code:
final response = await _authRepository.login(
  crewId: crewId,
  password: password,
);

_userId = response.userId;
_crewId = response.crewId;
_fullName = response.fullName;
_position = response.position;
```

### Step 3: Remove Mock Tasks
**File:** `lib/presentation/providers/task_provider.dart`

**Xóa dòng 48-52** (try-catch mock fallback):
```dart
// BEFORE:
try {
  _tasks = await _taskRepository.getMyTasks(forceRefresh: forceRefresh);
} catch (e) {
  print('API not available, using mock data: $e');
  _tasks = _generateMockTasks();
}

// AFTER:
_tasks = await _taskRepository.getMyTasks(forceRefresh: forceRefresh);
```

**Xóa method `_generateMockTasks()`** (dòng 59-170)

### Step 4: Test với Backend
```bash
flutter clean
flutter pub get
flutter run
```

**Test checklist:**
- [ ] Login với real credentials
- [ ] Fetch real tasks
- [ ] Start task → Check database
- [ ] Complete task → Check database
- [ ] Test offline mode → Check sync queue

---

## 🛠️ Useful Commands

### Development:
```bash
# Hot reload (trong app đang chạy)
Press 'r' in terminal

# Hot restart
Press 'R' in terminal

# Clear screen
Press 'c' in terminal

# Quit
Press 'q' in terminal
```

### Clean & Rebuild:
```bash
flutter clean
flutter pub get
flutter pub run build_runner build --delete-conflicting-outputs
flutter run
```

### Build APK:
```bash
# Debug APK (for testing)
flutter build apk --debug

# Release APK (for production)
flutter build apk --release

# APK location:
# build/app/outputs/flutter-apk/app-release.apk
```

### Check Errors:
```bash
flutter analyze

# Or in VS Code:
# View → Problems panel
```

---

## 📁 Important Files

### Config Files:
- `lib/core/constants/api_constants.dart` - API URL & endpoints
- `lib/app.dart` - Theme & navigation
- `pubspec.yaml` - Dependencies

### To Modify for Backend:
- `lib/presentation/providers/auth_provider.dart` - Login logic
- `lib/presentation/providers/task_provider.dart` - Remove mock tasks

### Documentation:
- `README.md` - Full project documentation
- `TESTING_GUIDE.md` - Complete testing scenarios
- `DEVELOPMENT_SUMMARY.md` - Architecture & features
- `NEXT_STEPS.md` - Detailed integration guide

---

## 🐛 Troubleshooting

### App won't build?
```bash
flutter doctor -v
flutter clean
flutter pub get
flutter pub run build_runner build --delete-conflicting-outputs
```

### No devices found?
```bash
# List emulators
flutter emulators

# Launch emulator
flutter emulators --launch <name>

# Or use Windows desktop
flutter run -d windows
```

### API connection fails?
1. Check Edge Server is running: `curl http://192.168.1.100:5001/api/health`
2. Verify IP in `api_constants.dart`
3. Check firewall allows port 5001
4. Test with Postman first

### Build runner errors?
```bash
flutter pub run build_runner clean
flutter pub run build_runner build --delete-conflicting-outputs
```

---

## 📊 Project Status

### ✅ Completed (100%):
- All 4 phases from guide
- 40+ files created
- 12 screens built
- Offline-first architecture
- Mock data for testing

### 🔴 Requires Backend:
- Real authentication
- Real task data
- Database synchronization
- Push notifications (future)

---

## 📖 Full Documentation

For detailed information:
- **Architecture**: `DEVELOPMENT_SUMMARY.md`
- **API Integration**: `NEXT_STEPS.md`
- **Testing**: `TESTING_GUIDE.md`
- **Setup**: `README.md`

---

**Need help? Check the docs or ask the development team! 🚢⚓**
