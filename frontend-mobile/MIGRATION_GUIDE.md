# 🔧 QUICK MIGRATION GUIDE - Convert Hardcoded Text to i18n

## 📋 Checklist: Các file cần update

### ✅ Priority 1 - Core Screens (QUAN TRỌNG)
- [ ] `lib/presentation/screens/auth/login_screen.dart`
- [ ] `lib/presentation/screens/home/home_screen.dart`
- [ ] `lib/presentation/screens/tasks/task_list_screen.dart`
- [ ] `lib/presentation/screens/tasks/task_detail_screen.dart`
- [ ] `lib/presentation/screens/settings/settings_screen.dart` ✅ (ĐÃ XONG)
- [ ] `lib/presentation/screens/settings/server_config_screen.dart`

### ⏳ Priority 2 - Secondary Screens
- [ ] `lib/presentation/screens/alarms/alarm_list_screen.dart`
- [ ] `lib/presentation/screens/schedule/schedule_screen.dart`
- [ ] `lib/presentation/screens/watchkeeping/watchkeeping_log_screen.dart`

---

## 🔄 MIGRATION PATTERN

### **TRƯỚC (❌ Hardcoded):**
```dart
Widget build(BuildContext context) {
  return Scaffold(
    appBar: AppBar(
      title: Text('Task Details'),  // ❌ Hardcoded
    ),
    body: Column(
      children: [
        Text('Status: Pending'),      // ❌ Hardcoded
        Text('Priority: High'),       // ❌ Hardcoded
        ElevatedButton(
          onPressed: () {},
          child: Text('Complete Task'), // ❌ Hardcoded
        ),
      ],
    ),
  );
}
```

### **SAU (✅ i18n):**
```dart
import 'package:flutter_gen/gen_l10n/app_localizations.dart';
// hoặc dùng extension:
import '../../../core/localization/localization_helper.dart';

Widget build(BuildContext context) {
  final l10n = AppLocalizations.of(context)!;
  // hoặc dùng extension: context.l10n
  
  return Scaffold(
    appBar: AppBar(
      title: Text(l10n.taskDetails),  // ✅ i18n
    ),
    body: Column(
      children: [
        Text('${l10n.taskStatus}: ${TaskStatusHelper.getStatusText(context, 'PENDING')}'),
        Text('${l10n.taskPriority}: ${TaskPriorityHelper.getPriorityText(context, 'HIGH')}'),
        ElevatedButton(
          onPressed: () {},
          child: Text(l10n.completeTask), // ✅ i18n
        ),
      ],
    ),
  );
}
```

---

## 📝 STEP-BY-STEP MIGRATION

### **Step 1: Tìm tất cả hardcoded text**
```bash
# Tìm tất cả Text widget có hardcoded string
grep -r "Text('" lib/presentation/screens/
grep -r 'Text("' lib/presentation/screens/
```

### **Step 2: List ra các text cần translate**
Ví dụ file `task_list_screen.dart`:
```
- "My Tasks"
- "No tasks available"
- "Loading tasks..."
- "Error loading tasks"
- "Pending"
- "In Progress"
- "Completed"
```

### **Step 3: Check xem key đã có trong .arb chưa**
Mở file `lib/l10n/app_en.arb` và tìm:
```json
{
  "myTasks": "My Tasks",           // ✅ Đã có
  "noTasksAvailable": "...",       // ✅ Đã có
  "loadingTasks": "...",           // ✅ Đã có
  "newKey": "New text"             // ⚠️ Cần thêm
}
```

### **Step 4: Thêm key mới (nếu chưa có)**

**Thêm vào `app_en.arb`:**
```json
{
  "engineRoom": "Engine Room",
  "bridgeWatch": "Bridge Watch",
  "deckCrew": "Deck Crew"
}
```

**Thêm vào `app_vi.arb`:**
```json
{
  "engineRoom": "Phòng máy",
  "bridgeWatch": "Trực buồng lái",
  "deckCrew": "Thủy thủ boong"
}
```

**Thêm vào `app_fil.arb`:**
```json
{
  "engineRoom": "Silid ng Makina",
  "bridgeWatch": "Bantay ng Tulay",
  "deckCrew": "Tauhan ng Deck"
}
```

### **Step 5: Run code generation**
```bash
cd frontend-mobile
flutter pub get
# Auto generate code vào .dart_tool/flutter_gen/gen_l10n/
```

### **Step 6: Replace trong code**
```dart
// TRƯỚC
Text('Engine Room')

// SAU
Text(l10n.engineRoom)
```

---

## 🎯 EXAMPLE: Migrate LoginScreen

### **File: login_screen.dart**

**TRƯỚC:**
```dart
class LoginScreen extends StatelessWidget {
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('Login')),
      body: Column(
        children: [
          TextField(
            decoration: InputDecoration(labelText: 'Username'),
          ),
          TextField(
            decoration: InputDecoration(labelText: 'Password'),
          ),
          ElevatedButton(
            onPressed: () {},
            child: Text('Login'),
          ),
        ],
      ),
    );
  }
}
```

**SAU:**
```dart
import 'package:flutter_gen/gen_l10n/app_localizations.dart';

class LoginScreen extends StatelessWidget {
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    
    return Scaffold(
      appBar: AppBar(title: Text(l10n.login)),
      body: Column(
        children: [
          TextField(
            decoration: InputDecoration(labelText: l10n.username),
          ),
          TextField(
            decoration: InputDecoration(labelText: l10n.password),
          ),
          ElevatedButton(
            onPressed: () {},
            child: Text(l10n.login),
          ),
        ],
      ),
    );
  }
}
```

---

## ⚡ HELPER EXTENSION (RECOMMENDED)

Thay vì viết `AppLocalizations.of(context)!` mỗi lần, dùng extension:

```dart
import '../../../core/localization/localization_helper.dart';

// Giờ có thể dùng:
Text(context.l10n.taskDetails)
Text(context.l10n.statusPending)
```

---

## 🧪 TESTING

### Test với ngôn ngữ khác nhau:
```dart
testWidgets('displays Vietnamese text', (tester) async {
  await tester.pumpWidget(
    MaterialApp(
      locale: Locale('vi'),
      localizationsDelegates: AppLocalizations.localizationsDelegates,
      supportedLocales: AppLocalizations.supportedLocales,
      home: LoginScreen(),
    ),
  );
  
  expect(find.text('Đăng nhập'), findsOneWidget);
});
```

---

## 📊 PROGRESS TRACKING

### Core Screens Migration Status:

| Screen | Status | Hardcoded Count | Notes |
|--------|--------|-----------------|-------|
| LoginScreen | ❌ TODO | ~10 | Username, Password, Login button |
| HomeScreen | ❌ TODO | ~20 | Tab labels, titles |
| TaskListScreen | ❌ TODO | ~15 | Status, priority labels |
| TaskDetailScreen | ❌ TODO | ~25 | Form labels, buttons |
| SettingsScreen | ✅ DONE | 0 | All i18n keys used |
| ServerConfigScreen | ❌ TODO | ~12 | URL, connection messages |
| AlarmListScreen | ❌ TODO | ~18 | Alarm types, severity |

**Total Progress: 1/7 (14%)**

---

## 🚨 COMMON MISTAKES

### ❌ SAI:
```dart
// Concatenate strings
Text('Status: ' + l10n.statusPending)  // KHÔNG DI ĐỘNG

// Hardcode format
Text('Task #${task.id}')  // KHÔNG LINH HOẠT
```

### ✅ ĐÚNG:
```dart
// Use placeholders
Text(l10n.statusWithLabel(l10n.statusPending))  // LINH HOẠT

// Define format in .arb
{
  "taskIdFormat": "Task #{id}",
  "@taskIdFormat": {
    "placeholders": {
      "id": { "type": "String" }
    }
  }
}

Text(l10n.taskIdFormat(task.id))
```

---

## 🎯 NEXT STEPS

1. **Run command để generate code:**
   ```bash
   cd frontend-mobile
   flutter pub get
   ```

2. **Migrate LoginScreen** (bắt đầu từ màn hình đơn giản nhất)

3. **Migrate TaskListScreen** (màn hình phức tạp hơn)

4. **Test với 2-3 ngôn ngữ**

5. **Tiếp tục migrate các màn hình còn lại**

---

**⚠️ LƯU Ý:** Sau khi thêm key mới vào file .arb, PHẢI chạy `flutter pub get` để generate code!
