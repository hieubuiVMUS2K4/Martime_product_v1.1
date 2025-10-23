# 🚀 Next Steps - Flutter Mobile App

## Immediate Actions Required

### 1️⃣ Generate Retrofit Code (CRITICAL)
```bash
cd d:\flutter_project\frontend-mobile
flutter pub run build_runner build --delete-conflicting-outputs
```

**What this does**: Generates `.g.dart` files for all Retrofit API interfaces
**Time needed**: 30 seconds
**Why**: App won't compile without these generated files

---

### 2️⃣ Test the App
```bash
flutter run -d windows
```

Or if you have Android emulator:
```bash
flutter run
```

**Expected Result**: App should launch and show login screen

---

## Before Connecting to Backend

### Update Server URL
Edit `lib/core/constants/api_constants.dart`:
```dart
static String baseUrl = 'http://YOUR_BACKEND_IP:5001';
```

Or use the Settings screen in the app to configure dynamically.

---

## Connect Providers to Repositories

Currently, providers use mock data. To connect to real backend:

### Update `lib/presentation/providers/task_provider.dart`

Replace the mock implementation with:

```dart
import 'package:flutter/material.dart';
import '../../data/models/maintenance_task.dart';
import '../../data/repositories/task_repository.dart';
import '../../core/network/api_client.dart';
import '../../core/network/network_info.dart';
import '../../core/cache/cache_manager.dart';
import '../../core/auth/token_storage.dart';

class TaskProvider with ChangeNotifier {
  final TaskRepository _taskRepository;
  
  List<MaintenanceTask> _tasks = [];
  bool _isLoading = false;
  String? _error;
  
  TaskProvider()
      : _taskRepository = TaskRepository(
          apiClient: ApiClient(),
          networkInfo: NetworkInfo(),
          cacheManager: CacheManager(),
          tokenStorage: TokenStorage(),
        );
  
  List<MaintenanceTask> get tasks => _tasks;
  bool get isLoading => _isLoading;
  String? get error => _error;
  
  List<MaintenanceTask> get pendingTasks =>
      _tasks.where((t) => t.isPending).toList();
  
  List<MaintenanceTask> get inProgressTasks =>
      _tasks.where((t) => t.isInProgress).toList();
  
  List<MaintenanceTask> get completedTasks =>
      _tasks.where((t) => t.isCompleted).toList();
  
  List<MaintenanceTask> get overdueTasks =>
      _tasks.where((t) => t.isOverdue && !t.isCompleted).toList();
  
  // Fetch tasks from API/Cache
  Future<void> fetchMyTasks({bool forceRefresh = false}) async {
    _isLoading = true;
    _error = null;
    notifyListeners();
    
    try {
      _tasks = await _taskRepository.getMyTasks(forceRefresh: forceRefresh);
      _isLoading = false;
      notifyListeners();
    } catch (e) {
      _error = e.toString();
      _isLoading = false;
      notifyListeners();
    }
  }
  
  // Start task
  Future<void> startTask(int taskId) async {
    _isLoading = true;
    notifyListeners();
    
    try {
      await _taskRepository.startTask(taskId);
      // Refresh tasks after starting
      await fetchMyTasks(forceRefresh: true);
      _isLoading = false;
      notifyListeners();
    } catch (e) {
      _error = e.toString();
      _isLoading = false;
      notifyListeners();
      rethrow;
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
    _isLoading = true;
    notifyListeners();
    
    try {
      await _taskRepository.completeTask(
        taskId: taskId,
        completedBy: completedBy,
        runningHours: runningHours,
        sparePartsUsed: sparePartsUsed,
        notes: notes,
      );
      // Refresh tasks after completing
      await fetchMyTasks(forceRefresh: true);
      _isLoading = false;
      notifyListeners();
    } catch (e) {
      _error = e.toString();
      _isLoading = false;
      notifyListeners();
      rethrow;
    }
  }
}
```

### Update `lib/presentation/providers/auth_provider.dart`

Replace with:

```dart
import 'package:flutter/material.dart';
import '../../core/auth/token_storage.dart';
import '../../core/cache/cache_manager.dart';
import '../../data/repositories/auth_repository.dart';
import '../../core/network/api_client.dart';
import '../../core/network/network_info.dart';

class AuthProvider with ChangeNotifier {
  final TokenStorage _tokenStorage = TokenStorage();
  final CacheManager _cacheManager = CacheManager();
  late final AuthRepository _authRepository;
  
  bool _isLoggedIn = false;
  bool _isLoading = false;
  String? _error;
  String? _crewId;
  String? _fullName;
  String? _position;
  
  AuthProvider() {
    _authRepository = AuthRepository(
      apiClient: ApiClient(),
      networkInfo: NetworkInfo(),
      tokenStorage: _tokenStorage,
    );
  }
  
  bool get isLoggedIn => _isLoggedIn;
  bool get isLoading => _isLoading;
  String? get error => _error;
  String? get crewId => _crewId;
  String? get fullName => _fullName;
  String? get position => _position;
  
  // Check if user is logged in
  Future<void> checkLoginStatus() async {
    _isLoading = true;
    notifyListeners();
    
    try {
      final hasToken = await _tokenStorage.hasValidToken();
      _isLoggedIn = hasToken;
      
      if (hasToken) {
        _crewId = await _tokenStorage.getCrewId();
        _fullName = await _tokenStorage.getFullName();
        _position = await _tokenStorage.getPosition();
      }
    } catch (e) {
      _error = e.toString();
    }
    
    _isLoading = false;
    notifyListeners();
  }
  
  // Login
  Future<bool> login({
    required String crewId,
    required String password,
  }) async {
    _isLoading = true;
    _error = null;
    notifyListeners();
    
    try {
      await _authRepository.login(crewId: crewId, password: password);
      
      // Load user info from token storage
      _isLoggedIn = true;
      _crewId = await _tokenStorage.getCrewId();
      _fullName = await _tokenStorage.getFullName();
      _position = await _tokenStorage.getPosition();
      
      _isLoading = false;
      notifyListeners();
      return true;
    } catch (e) {
      _error = e.toString();
      _isLoading = false;
      notifyListeners();
      return false;
    }
  }
  
  // Logout
  Future<void> logout() async {
    _isLoading = true;
    notifyListeners();
    
    try {
      await _authRepository.logout();
      await _cacheManager.clearAllCache();
      
      _isLoggedIn = false;
      _crewId = null;
      _fullName = null;
      _position = null;
    } catch (e) {
      _error = e.toString();
    }
    
    _isLoading = false;
    notifyListeners();
  }
}
```

---

## Testing Workflow

### 1. Test with Mock Backend
If backend is not ready yet, you can test with:
- Postman mock server
- JSON server
- Or keep using mock data in providers

### 2. Test Offline Mode
1. Login while online
2. Turn off WiFi
3. Complete a task
4. Check Settings → Sync queue (should show 1 item)
5. Turn on WiFi
6. Tap "Sync Now" or wait for auto-sync
7. Verify task is synced

### 3. Test All Screens
- [ ] Login
- [ ] Home dashboard
- [ ] Task list (all tabs)
- [ ] Task detail
- [ ] Complete task
- [ ] Profile
- [ ] Certificates
- [ ] Schedule
- [ ] Settings

---

## Common Issues & Solutions

### Issue: "packages not found"
```bash
flutter pub get
```

### Issue: "build_runner errors"
```bash
flutter clean
flutter pub get
flutter pub run build_runner build --delete-conflicting-outputs
```

### Issue: "No devices found"
```bash
# For Windows desktop
flutter run -d windows

# For Android emulator
flutter emulators
flutter emulators --launch <emulator_name>
```

### Issue: "API connection failed"
1. Check server URL in api_constants.dart
2. Verify backend is running
3. Check network connectivity
4. Try `ping <server_ip>` from command line

---

## Performance Tuning

### Enable Release Mode
```bash
flutter run --release
```

### Build Optimized APK
```bash
flutter build apk --release --target-platform android-arm64
```

### Profile Performance
```bash
flutter run --profile
```
Then use DevTools for profiling.

---

## Deployment Checklist

### Before Production Release

- [ ] Generate and test all Retrofit code
- [ ] Connect providers to repositories
- [ ] Test all features with real backend
- [ ] Test offline mode thoroughly
- [ ] Add error logging (Crashlytics/Sentry)
- [ ] Add analytics
- [ ] Set up proper server URL (HTTPS)
- [ ] Configure app signing (Android keystore)
- [ ] Update app icons and splash screen
- [ ] Test on multiple devices
- [ ] Get beta testers feedback
- [ ] Prepare store listings
- [ ] Create privacy policy
- [ ] Deploy to stores

---

## Quick Command Reference

```bash
# Get dependencies
flutter pub get

# Generate code
flutter pub run build_runner build --delete-conflicting-outputs

# Run app
flutter run

# Build APK
flutter build apk --release

# Clean build
flutter clean

# Check for issues
flutter doctor

# Analyze code
flutter analyze

# Run tests
flutter test

# Format code
flutter format lib/
```

---

## Need Help?

1. **Setup issues**: Check README.md
2. **Architecture questions**: Read DEVELOPMENT_SUMMARY.md
3. **API specs**: See DATABASE_SCHEMA_FOR_MOBILE_SYNC.md
4. **Feature requirements**: Check FLUTTER_MOBILE_DEVELOPMENT_GUIDE.md

---

**Status**: App is ready for backend integration
**Next**: Run build_runner and test with backend
