# 🔧 TROUBLESHOOTING - Common Issues

## ❌ Vấn đề: Task List rỗng

### Triệu chứng:
- Login thành công
- Navigate đến Home Screen
- Task list hiển thị "No tasks found" hoặc rỗng
- Settings không hiển thị user info

### Nguyên nhân có thể:
1. ❌ `TaskProvider.fetchMyTasks()` không được gọi
2. ❌ Mock data không được generate
3. ❌ API error không được catch đúng
4. ❌ AuthProvider không lưu user info

### Giải pháp:

#### 1️⃣ Check Console Logs

Sau khi login, bạn phải thấy logs này:

```
🔐 AuthProvider: Login successful!
   - Crew ID: CM001
   - Full Name: John Smith
   - Position: Chief Engineer
```

Sau khi vào Home Screen, phải thấy:

```
🔍 TaskProvider: fetchMyTasks() called
📡 TaskProvider: Trying to fetch from API...
⚠️ API not available, using mock data: Exception: ...
✅ TaskProvider: Generated 5 mock tasks
✅ TaskProvider: fetchMyTasks() completed with 5 tasks
```

**Nếu không thấy logs → Provider không được call!**

#### 2️⃣ Verify HomeScreen initState

Check file: `lib/presentation/screens/home/home_screen.dart`

```dart
@override
void initState() {
  super.initState();
  // Fetch initial data
  WidgetsBinding.instance.addPostFrameCallback((_) {
    Provider.of<TaskProvider>(context, listen: false).fetchMyTasks();
  });
}
```

**Phải có đoạn code này!**

#### 3️⃣ Hot Restart (KHÔNG phải Hot Reload)

Khi app đang chạy, press:
- **`R`** (capital R) = Hot Restart
- **`r`** (lowercase r) = Hot Reload (có thể không đủ)

#### 4️⃣ Clean Rebuild

```bash
cd d:\flutter_project\frontend-mobile
flutter clean
flutter pub get
flutter pub run build_runner build --delete-conflicting-outputs
flutter run -d windows
```

---

## ❌ Vấn đề: Settings Screen không có gì

### Triệu chứng:
- Open Settings
- Account section hiển thị "User", "Position", "ID: N/A"
- Không có thông tin thật

### Nguyên nhân:
`AuthProvider` không lưu được `fullName`, `position`, `crewId` sau login

### Giải pháp:

#### Check AuthProvider state

Add debug code vào Settings Screen (temporary):

```dart
@override
Widget build(BuildContext context) {
  final authProvider = Provider.of<AuthProvider>(context);
  
  // DEBUG
  print('Settings - crewId: ${authProvider.crewId}');
  print('Settings - fullName: ${authProvider.fullName}');
  print('Settings - position: ${authProvider.position}');
  
  return Scaffold(...);
}
```

**Expected output:**
```
Settings - crewId: CM001
Settings - fullName: John Smith
Settings - position: Chief Engineer
```

**Nếu tất cả null → AuthProvider state bị mất!**

#### Fix: Check AuthProvider getter

File: `lib/presentation/providers/auth_provider.dart`

Must have:
```dart
bool get isLoggedIn => _isLoggedIn;
String? get crewId => _crewId;
String? get fullName => _fullName;
String? get position => _position;
```

#### Fix: Check login method saves state

```dart
_isLoggedIn = true;
_crewId = crewId.toUpperCase();
_fullName = mockFullName;
_position = mockPosition;

_isLoading = false;
notifyListeners();  // ← MUST call this!
```

---

## ❌ Vấn đề: App crash sau login

### Triệu chứng:
- Login thành công
- App tries to navigate
- White screen hoặc crash

### Nguyên nhân:
Route '/home' không được define

### Giải pháp:

Check `lib/app.dart`:

```dart
home: const LoginScreen(),
routes: {
  '/login': (context) => const LoginScreen(),
  '/home': (context) => const HomeScreen(),  // ← Must have this!
},
```

---

## ❌ Vấn đề: "Connection timeout" error

### Triệu chứng:
```
DioError: Connection timeout
The semaphore timeout period has expired
```

### Nguyên nhân:
App đang cố kết nối backend (http://192.168.1.100:5001) nhưng backend chưa chạy

### Giải pháp:

**Đây KHÔNG phải là lỗi!** - Đây là expected behavior!

App sẽ:
1. Try connect to API
2. Timeout after 30s
3. **Fallback to mock data** ← This is correct!

You should see:
```
⚠️ API not available, using mock data: Exception: ...
✅ TaskProvider: Generated 5 mock tasks
```

**App vẫn hoạt động bình thường với mock data!**

---

## ❌ Vấn đề: TaskCard không hiển thị

### Triệu chứng:
- Tasks có data (check console: "5 tasks")
- Nhưng screen vẫn trống

### Nguyên nhân:
ListView không build items đúng

### Giải pháp:

Check TaskListScreen:

```dart
ListView.builder(
  itemCount: tasks.length,  // ← Must have this
  itemBuilder: (context, index) {
    final task = tasks[index];
    return TaskCard(task: task);  // ← Must pass task
  },
)
```

---

## ❌ Vấn đề: No errors, but screen blank

### Checklist:

1. **Check console logs** - Are providers called?
2. **Check data exists** - Print `tasks.length` in build method
3. **Check widget tree** - Use Flutter DevTools Inspector
4. **Check loading state** - Maybe stuck in loading?

#### Quick debug code:

In any screen:
```dart
@override
Widget build(BuildContext context) {
  final taskProvider = Provider.of<TaskProvider>(context);
  
  print('🎯 Build - isLoading: ${taskProvider.isLoading}');
  print('🎯 Build - tasks.length: ${taskProvider.tasks.length}');
  print('🎯 Build - error: ${taskProvider.error}');
  
  // Your actual UI...
}
```

---

## 🎯 Test Workflow

### Complete test from scratch:

```bash
# 1. Clean build
flutter clean
flutter pub get
flutter pub run build_runner build --delete-conflicting-outputs

# 2. Run app
flutter run -d windows

# 3. In app:
#    - Login with CM001 / any password
#    - Watch console logs
#    - Navigate to Home
#    - Check task list
#    - Go to Settings
#    - Verify user info

# 4. If issues, Hot Restart:
#    Press 'R' in terminal
```

### Expected console output:

```
Launching lib\main.dart on Windows...
Building Windows application...
√ Built maritime_crew_app.exe

🔐 AuthProvider: Login successful!
   - Crew ID: CM001
   - Full Name: John Smith
   - Position: Chief Engineer

🔍 TaskProvider: fetchMyTasks() called
📡 TaskProvider: Trying to fetch from API...
⚠️ API not available, using mock data: Exception: ...
✅ TaskProvider: Generated 5 mock tasks
✅ TaskProvider: fetchMyTasks() completed with 5 tasks
```

---

## 🆘 Still not working?

### Last resort:

1. **Delete generated files:**
```bash
rm -rf .dart_tool
rm -rf build
rm pubspec.lock
```

2. **Reinstall dependencies:**
```bash
flutter pub get
flutter pub run build_runner build --delete-conflicting-outputs
```

3. **Check Flutter Doctor:**
```bash
flutter doctor -v
```

4. **Try different device:**
```bash
# Instead of Windows
flutter emulators --launch Pixel_5_API_33
flutter run
```

5. **Check file structure:**
```bash
# Make sure these files exist:
ls lib/presentation/providers/task_provider.dart
ls lib/presentation/providers/auth_provider.dart
ls lib/presentation/screens/home/home_screen.dart
ls lib/data/models/maintenance_task.dart
```

---

## 📞 Report Bug

If all else fails, provide:

1. **Console logs** (full output)
2. **Screenshots** of blank screens
3. **Flutter doctor -v** output
4. **Steps to reproduce**

Include logs showing:
- Did AuthProvider print login success?
- Did TaskProvider print task count?
- Any error messages?

---

**Good luck! 🚢⚓**
