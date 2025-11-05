# 📱 Hướng Dẫn Chạy App Trên Điện Thoại Thật

## 🎯 Tổng Quan
Để chạy ứng dụng Maritime Crew trên điện thoại thật, bạn có 3 cách:
1. **Debug Mode** (phát triển, testing) - Nhanh nhất
2. **APK Debug** - Build file APK để cài đặt
3. **APK Release** - Build bản chính thức để phát hành

---

## 🔧 Bước 1: Chuẩn Bị

### A. Cài đặt Flutter trên máy tính
```powershell
# Kiểm tra Flutter đã cài chưa
flutter --version

# Kiểm tra các yêu cầu
flutter doctor -v
```

**Nếu chưa cài Flutter:**
1. Download từ: https://docs.flutter.dev/get-started/install/windows
2. Giải nén vào `C:\src\flutter`
3. Thêm `C:\src\flutter\bin` vào PATH
4. Chạy: `flutter doctor`

### B. Cài đặt Android SDK
```powershell
# Kiểm tra Android toolchain
flutter doctor

# Nếu thiếu Android SDK:
# - Cài Android Studio từ: https://developer.android.com/studio
# - Mở Android Studio → Settings → Android SDK
# - Install: Android SDK Command-line Tools, Android SDK Platform-Tools
```

### C. Chuẩn bị dependencies cho project
```powershell
cd f:\NCKH\Product\Martime_product_v1.1\frontend-mobile

# Install dependencies
flutter pub get

# Generate code (Hive adapters, JSON serializers)
flutter pub run build_runner build --delete-conflicting-outputs
```

---

## 📱 Bước 2: Kết Nối Điện Thoại

### A. BẬT USB Debugging trên điện thoại Android

#### Samsung/Xiaomi/Oppo/Realme:
1. Vào **Settings** (Cài đặt)
2. Tìm **About phone** (Thông tin điện thoại)
3. Tìm **Build number** (Số bản dựng)
4. **Tap 7 lần** vào Build number → "You are now a developer!"
5. Quay lại Settings → **Developer options** (Tùy chọn nhà phát triển)
6. Bật **USB debugging** (Gỡ lỗi USB)
7. Bật **Install via USB** (Cài đặt qua USB) - nếu có

#### Một số máy khác (Vivo, Huawei):
- Tương tự nhưng có thể phải tap vào "Software version" thay vì Build number

### B. Kết nối điện thoại vào máy tính
1. Cắm cáp USB vào điện thoại và máy tính
2. Trên điện thoại, chọn **File Transfer** (MTP) hoặc **USB for charging** → **File Transfer**
3. Popup "Allow USB debugging?" → Chọn **Allow** (Cho phép)

### C. Kiểm tra kết nối
```powershell
# Kiểm tra thiết bị đã connect chưa
flutter devices

# Hoặc dùng ADB
adb devices
```

**Kết quả mong muốn:**
```
List of devices attached
ABC123456789    device
```

**Nếu thấy "unauthorized":**
- Kiểm tra lại popup "Allow USB debugging?" trên điện thoại
- Revoke USB debugging authorizations → Bật lại
- Hoặc chạy: `adb kill-server` rồi `adb start-server`

**Nếu không thấy thiết bị:**
- Kiểm tra cáp USB (thử cáp khác)
- Thử port USB khác trên máy tính
- Cài driver USB cho điện thoại (Google "your phone model USB driver")

---

## 🚀 Bước 3: Chạy App (Debug Mode)

### Cách nhanh nhất - Chạy trực tiếp:
```powershell
cd f:\NCKH\Product\Martime_product_v1.1\frontend-mobile

# Chạy trên thiết bị đã kết nối
flutter run

# Hoặc chỉ định thiết bị cụ thể (nếu có nhiều thiết bị)
flutter run -d <device_id>
```

**Lần đầu tiên sẽ mất 3-5 phút** để build. Những lần sau chỉ mất 10-30 giây.

### Hot Reload trong khi develop:
- Sau khi app chạy, bạn có thể sửa code
- Nhấn **r** trong terminal → Hot reload (cập nhật UI ngay lập tức)
- Nhấn **R** → Hot restart (restart app)
- Nhấn **q** → Quit (thoát)

---

## 📦 Bước 4: Build APK để Cài Đặt

### A. Build APK Debug (để test)
```powershell
cd f:\NCKH\Product\Martime_product_v1.1\frontend-mobile

# Build APK debug
flutter build apk --debug

# APK sẽ nằm ở:
# build/app/outputs/flutter-apk/app-debug.apk
```

**Kích thước:** ~40-50 MB

**Cài đặt APK lên điện thoại:**
```powershell
# Cách 1: Qua ADB
adb install build/app/outputs/flutter-apk/app-debug.apk

# Cách 2: Copy file APK vào điện thoại và cài thủ công
# - Copy app-debug.apk vào thư mục Downloads trên điện thoại
# - Mở File Manager → Tìm file APK → Tap để cài
# - Cho phép "Install from unknown sources" nếu có yêu cầu
```

### B. Build APK Release (bản chính thức)
```powershell
# Build APK release (đã optimize, nhẹ hơn)
flutter build apk --release

# APK sẽ nằm ở:
# build/app/outputs/flutter-apk/app-release.apk
```

**Kích thước:** ~20-25 MB (nhỏ hơn gấp đôi)

**Khác biệt Debug vs Release:**
- **Debug**: Có debugging tools, chậm hơn, dung lượng lớn
- **Release**: Đã optimize, nhanh hơn, nhỏ hơn, KHÔNG có debugging

### C. Build App Bundle (cho Google Play Store)
```powershell
# Build App Bundle (định dạng .aab)
flutter build appbundle --release

# File sẽ nằm ở:
# build/app/outputs/bundle/release/app-release.aab
```

**Chú ý:** File .aab chỉ dùng để upload lên Google Play Store, KHÔNG thể cài trực tiếp.

---

## 🌐 Bước 5: Cấu Hình Kết Nối Backend

### A. Tìm IP của Edge Server
Trên máy chạy Edge Server:
```powershell
# Xem IP address
ipconfig

# Tìm Wireless LAN adapter WiFi hoặc Ethernet adapter
# VD: IPv4 Address: 192.168.1.100
```

### B. Cập nhật API URL trong app

**Cách 1: Sửa code (trước khi build)**
```powershell
# Mở file API constants
notepad lib/core/constants/api_constants.dart
```

Sửa dòng:
```dart
static String baseUrl = 'http://192.168.1.100:5001'; // Thay IP thật
```

**Cách 2: Settings trong app (nếu đã implement)**
- Mở app → Settings → Server URL → Nhập IP mới

### C. Test kết nối
1. Đảm bảo điện thoại và máy chủ **cùng WiFi**
2. Test API từ browser trên điện thoại:
   - Mở Chrome/Firefox trên điện thoại
   - Truy cập: `http://192.168.1.100:5001/api/health`
   - Phải thấy response từ server

**Nếu không kết nối được:**
- Kiểm tra firewall trên máy chủ Windows
- Tắt Public network firewall hoặc allow port 5001
```powershell
# Mở firewall cho port 5001
netsh advfirewall firewall add rule name="Edge Server" dir=in action=allow protocol=TCP localport=5001
```

---

## 🔑 Bước 6: Login và Test App

### Thông tin login mẫu (nếu đã có data):
```
Crew ID: CM001 (hoặc ID từ database)
Password: password123
```

### Test các tính năng:
1. ✅ **Login** → Verify JWT token được lưu
2. ✅ **Dashboard** → Hiển thị stats (tasks, alerts)
3. ✅ **My Tasks** → Danh sách nhiệm vụ
4. ✅ **Task Detail** → Chi tiết task
5. ✅ **Complete Task** → Đánh dấu hoàn thành
6. ✅ **Offline Mode** → Tắt WiFi, app vẫn hoạt động

---

## 🎨 Build Variants (Tùy chọn nâng cao)

### Build cho các kiến trúc CPU cụ thể (giảm dung lượng):

```powershell
# Chỉ build cho ARM64 (hầu hết điện thoại hiện đại)
flutter build apk --release --target-platform android-arm64

# Build cho ARM 32-bit (điện thoại cũ)
flutter build apk --release --target-platform android-arm

# Build cho x86 (emulator/tablet)
flutter build apk --release --target-platform android-x64

# Build tất cả (mặc định)
flutter build apk --release --split-per-abi
# Sẽ tạo 3 APK: arm64-v8a, armeabi-v7a, x86_64
```

### Build với tên package custom:
Edit `android/app/build.gradle`:
```gradle
android {
    defaultConfig {
        applicationId "com.maritime.crew.app" // Thay đổi này
        // ...
    }
}
```

---

## 🐛 Xử Lý Lỗi Thường Gặp

### 1. "Gradle build failed"
```powershell
# Clean build
cd android
./gradlew clean
cd ..
flutter clean
flutter pub get
flutter build apk
```

### 2. "SDK licenses not accepted"
```powershell
flutter doctor --android-licenses
# Nhấn 'y' để accept tất cả licenses
```

### 3. "Device not found"
```powershell
# Kill và restart ADB server
adb kill-server
adb start-server
adb devices
```

### 4. "App crashes on startup"
- Kiểm tra logs:
```powershell
# Realtime logs từ điện thoại
flutter logs

# Hoặc
adb logcat | Select-String "flutter"
```

### 5. "Permission denied" khi cài APK
- Bật "Install from unknown sources" trong Settings
- Security → Unknown sources → Allow

### 6. "Cannot connect to server"
- Kiểm tra cùng WiFi
- Test từ browser: `http://SERVER_IP:5001`
- Check firewall trên server
- Verify API URL trong code

---

## 📊 Performance Optimization

### Trước khi build release:

**1. Minify code (giảm dung lượng)**
Edit `android/app/build.gradle`:
```gradle
buildTypes {
    release {
        minifyEnabled true
        shrinkResources true
        proguardFiles getDefaultProguardFile('proguard-android.txt'), 'proguard-rules.pro'
    }
}
```

**2. Optimize images**
- Dùng WebP thay vì PNG
- Compress images trước khi add vào `assets/`

**3. Remove unused packages**
```powershell
# Analyze dependencies
flutter pub deps

# Remove unused imports trong code
```

---

## 📱 Test Trên Nhiều Thiết Bị

### Kết nối nhiều điện thoại cùng lúc:
```powershell
# Liệt kê tất cả devices
flutter devices

# Chạy trên device cụ thể
flutter run -d 192.168.1.101 # Wireless debugging
flutter run -d ABC123456789  # USB device ID
```

### Wireless debugging (Android 11+):
1. Settings → Developer options → Wireless debugging → ON
2. Pair device
3. Connect:
```powershell
adb pair IP:PORT
adb connect IP:PORT
flutter devices
```

---

## 🚀 Deploy Lên Google Play Store (Tương lai)

### Bước chuẩn bị:

**1. Tạo keystore (signing key):**
```powershell
keytool -genkey -v -keystore ~/upload-keystore.jks -keyalg RSA -keysize 2048 -validity 10000 -alias upload
```

**2. Cấu hình signing:**
Tạo file `android/key.properties`:
```properties
storePassword=<password>
keyPassword=<password>
keyAlias=upload
storeFile=<path to keystore>
```

**3. Build signed APK/AAB:**
```powershell
flutter build appbundle --release
```

**4. Upload lên Google Play Console**

---

## ✅ Checklist Trước Khi Deploy

- [ ] Test app trên ít nhất 2 điện thoại khác nhau
- [ ] Test offline mode
- [ ] Test trên WiFi yếu (slow network)
- [ ] Test login/logout
- [ ] Test tất cả màn hình chính
- [ ] Verify không có crash
- [ ] Verify UI responsive (màn hình khác nhau)
- [ ] Test rotation (xoay ngang/dọc)
- [ ] Check memory leaks (chạy lâu không bị crash)
- [ ] Verify app icons và splash screen
- [ ] Check app name và version

---

## 📞 Hỗ Trợ

### Lệnh hữu ích:

```powershell
# Xem logs realtime
flutter logs

# Xem performance
flutter run --profile

# Analyze app size
flutter build apk --analyze-size

# Screenshot từ điện thoại
adb shell screencap -p /sdcard/screen.png
adb pull /sdcard/screen.png

# Record video
adb shell screenrecord /sdcard/demo.mp4
adb pull /sdcard/demo.mp4
```

### Debugging:
```powershell
# Chạy với verbose
flutter run -v

# Debug inspector
flutter run --observatory-port=8888

# Memory profiling
flutter run --profile
```

---

## 🎯 Quick Reference

### Workflow chuẩn khi develop:

```powershell
# 1. Kết nối điện thoại
flutter devices

# 2. Chạy app
flutter run

# 3. Code thay đổi → Nhấn 'r' để hot reload

# 4. Test xong → Build APK
flutter build apk --release

# 5. Cài APK lên điện thoại test khác
adb install build/app/outputs/flutter-apk/app-release.apk
```

### One-liner build và install:
```powershell
flutter build apk --release ; adb install -r build/app/outputs/flutter-apk/app-release.apk
```

---

**✅ Hoàn tất! Bây giờ app của bạn đã chạy trên điện thoại thật. 🎉**

**Các bước tiếp theo:**
1. ✅ Login vào app
2. ✅ Test tất cả tính năng
3. ✅ Report bugs nếu có
4. ✅ Thu thập feedback từ users
5. ✅ Optimize và cải thiện

**Good luck! 🚢⚓**
