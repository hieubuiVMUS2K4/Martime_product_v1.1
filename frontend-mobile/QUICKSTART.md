# 🚀 QUICK START - Flutter Mobile App

## Bước 1: Cài đặt Dependencies

Mở PowerShell trong thư mục `frontend-mobile`:

```powershell
cd d:\flutter_project\frontend-mobile
flutter pub get
```

## Bước 2: Generate Hive Adapters

```powershell
flutter pub run build_runner build --delete-conflicting-outputs
```

## Bước 3: Chạy App

### Option A: Android Emulator

```powershell
# Liệt kê emulators
flutter emulators

# Launch emulator
flutter emulators --launch <emulator_id>

# Run app
flutter run
```

### Option B: Thiết bị thực (qua USB)

```powershell
# Enable USB Debugging trên điện thoại
# Settings → About Phone → Tap "Build Number" 7 lần
# Settings → Developer Options → Enable USB Debugging

# Connect điện thoại và verify
flutter devices

# Run app
flutter run
```

### Option C: Chrome (Web - for testing only)

```powershell
flutter run -d chrome
```

## 📝 Login Test Credentials

Khi app chạy, có thể test với bất kỳ Crew ID nào:

- **Crew ID**: `CM001` (hoặc bất kỳ)
- **Password**: `password` (bất kỳ - vì chưa connect API)

## ⚠️ Lưu ý

1. **Compile errors**: Các lỗi compile hiện tại là bình thường vì chưa chạy `flutter pub get`. Sau khi chạy lệnh trên, các lỗi sẽ biến mất.

2. **API Connection**: App hiện chưa connect tới Edge Server thực tế. Cần implement API endpoints trên backend trước.

3. **Mock Data**: Login hiện đang dùng mock data. Sau khi backend ready, sẽ thay thế bằng API thực.

## 🔧 Thay đổi Server URL

Edit file: `lib/core/constants/api_constants.dart`

```dart
static String baseUrl = 'http://192.168.1.100:5001'; // Thay đổi IP này
```

## 📱 Build APK

```powershell
# Build release APK
flutter build apk --release

# File APK sẽ ở:
# build\app\outputs\flutter-apk\app-release.apk
```

## 🐛 Common Issues

### Issue: "Flutter not found"
```powershell
# Add Flutter to PATH
# Hoặc dùng full path:
C:\path\to\flutter\bin\flutter pub get
```

### Issue: "No devices found"
```powershell
# Start Android Emulator từ Android Studio
# Hoặc: flutter emulators --launch <emulator_id>
```

### Issue: "Build runner errors"
```powershell
flutter clean
flutter pub get
flutter pub run build_runner build --delete-conflicting-outputs
```

## ✅ Next Steps

Sau khi app chạy thành công:

1. ✅ Test Login screen
2. ✅ Test Home screen navigation
3. 🚧 Implement Task list screen
4. 🚧 Connect tới Edge Server API
5. 🚧 Test offline mode
6. 🚧 Implement các screens còn lại

---

**🚢 Happy Coding! ⚓**
