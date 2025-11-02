# 📱 Hướng dẫn Build APK cho Maritime Crew App

## 🔧 Yêu cầu trước khi build

### 1. Cài đặt Android Studio và Android SDK

1. **Download Android Studio:**
   - Truy cập: https://developer.android.com/studio
   - Tải phiên bản mới nhất cho Windows

2. **Cài đặt Android Studio:**
   - Chạy file cài đặt
   - Chọn "Standard Installation"
   - Android Studio sẽ tự động cài:
     - Android SDK
     - Android SDK Platform
     - Android Virtual Device (AVD)

3. **Cấu hình SDK:**
   - Mở Android Studio
   - File → Settings → Appearance & Behavior → System Settings → Android SDK
   - Tick chọn:
     - ✅ Android 13.0 (API 33) - Recommended
     - ✅ Android 14.0 (API 34)
   - Tab "SDK Tools", tick chọn:
     - ✅ Android SDK Build-Tools
     - ✅ Android SDK Command-line Tools
     - ✅ Android Emulator
     - ✅ Android SDK Platform-Tools
   - Click "Apply" để cài đặt

4. **Thiết lập biến môi trường:**
   ```powershell
   # Thêm vào System Environment Variables:
   ANDROID_HOME = C:\Users\[YourUsername]\AppData\Local\Android\Sdk
   
   # Thêm vào PATH:
   %ANDROID_HOME%\platform-tools
   %ANDROID_HOME%\tools
   %ANDROID_HOME%\tools\bin
   ```

5. **Cấu hình Flutter:**
   ```powershell
   flutter config --android-sdk "C:\Users\[YourUsername]\AppData\Local\Android\Sdk"
   flutter doctor --android-licenses  # Accept all licenses
   flutter doctor  # Verify installation
   ```

---

## 🔐 Bước 2: Cấu hình Signing Key (Bắt buộc cho Release)

### Tạo keystore file:

```powershell
# Chạy lệnh sau (thay đổi thông tin của bạn):
keytool -genkey -v -keystore d:\Martime_product_v1\frontend-mobile\android\app\maritime-release-key.jks -keyalg RSA -keysize 2048 -validity 10000 -alias maritime-key

# Nhập thông tin:
# - Password: [Tạo password mạnh, LƯU LẠI!]
# - First and last name: Maritime Crew Team
# - Organizational unit: Development
# - Organization: VMUS
# - City: [Your City]
# - State: [Your State]
# - Country code: VN
```

### Tạo file key.properties:

```powershell
# Tạo file: android/key.properties
```

Nội dung file `android/key.properties`:
```properties
storePassword=<password-bạn-vừa-tạo>
keyPassword=<password-bạn-vừa-tạo>
keyAlias=maritime-key
storeFile=maritime-release-key.jks
```

⚠️ **QUAN TRỌNG:** 
- File `key.properties` và `maritime-release-key.jks` KHÔNG được commit lên Git
- Lưu backup file keystore ở nơi an toàn
- Mất keystore = KHÔNG THỂ update app trên Google Play

---

## 📦 Bước 3: Cấu hình Android Build

### File: `android/app/build.gradle`

Thêm cấu hình signing (đã có sẵn trong code):

```gradle
def keystoreProperties = new Properties()
def keystorePropertiesFile = rootProject.file('key.properties')
if (keystorePropertiesFile.exists()) {
    keystoreProperties.load(new FileInputStream(keystorePropertiesFile))
}

android {
    ...
    
    signingConfigs {
        release {
            keyAlias keystoreProperties['keyAlias']
            keyPassword keystoreProperties['keyPassword']
            storeFile keystoreProperties['storeFile'] ? file(keystoreProperties['storeFile']) : null
            storePassword keystoreProperties['storePassword']
        }
    }
    
    buildTypes {
        release {
            signingConfig signingConfigs.release
            minifyEnabled true
            shrinkResources true
        }
    }
}
```

---

## 🚀 Bước 4: Build APK

### A. Build APK Debug (Cho Testing):

```powershell
cd d:\Martime_product_v1\frontend-mobile

# Build APK debug (không cần signing)
flutter build apk --debug

# File output:
# build/app/outputs/flutter-apk/app-debug.apk
```

### B. Build APK Release (Cho Production):

```powershell
cd d:\Martime_product_v1\frontend-mobile

# 1. Clean project
flutter clean
flutter pub get

# 2. Build release APK (signed)
flutter build apk --release

# File output:
# build/app/outputs/flutter-apk/app-release.apk
```

### C. Build APK Split (Tối ưu kích thước):

```powershell
# Build cho từng kiến trúc CPU riêng biệt
flutter build apk --release --split-per-abi

# File outputs (nhỏ hơn 30-40%):
# build/app/outputs/flutter-apk/app-armeabi-v7a-release.apk  (ARM 32-bit)
# build/app/outputs/flutter-apk/app-arm64-v8a-release.apk    (ARM 64-bit)
# build/app/outputs/flutter-apk/app-x86_64-release.apk       (x86 64-bit)
```

### D. Build App Bundle (Cho Google Play):

```powershell
# Build AAB (Android App Bundle) - Recommended for Play Store
flutter build appbundle --release

# File output:
# build/app/outputs/bundle/release/app-release.aab
```

---

## 📊 Kích thước ước tính:

- **APK Universal:** ~40-60 MB
- **APK Split (arm64-v8a):** ~25-35 MB
- **AAB:** ~45-55 MB (Google Play tự optimize)

---

## 🧪 Bước 5: Test APK

### Cài đặt trên thiết bị thật:

```powershell
# 1. Bật USB Debugging trên điện thoại Android
# 2. Kết nối USB
# 3. Kiểm tra kết nối:
flutter devices

# 4. Cài đặt APK:
flutter install

# Hoặc dùng ADB:
adb install build/app/outputs/flutter-apk/app-release.apk
```

### Test trên Emulator:

```powershell
# 1. Tạo emulator trong Android Studio
# 2. Start emulator
# 3. Install:
flutter install
```

---

## 🎯 Build Commands Quick Reference

```powershell
# Debug APK (testing only)
flutter build apk --debug

# Release APK (single universal)
flutter build apk --release

# Release APK (optimized, split)
flutter build apk --release --split-per-abi

# App Bundle for Google Play
flutter build appbundle --release

# Build with verbose output
flutter build apk --release -v

# Build with specific flavor
flutter build apk --release --flavor production

# Analyze APK size
flutter build apk --release --analyze-size
```

---

## ⚙️ Tối ưu hóa APK Size

### 1. Enable ProGuard/R8 (Code shrinking):

File `android/app/build.gradle`:
```gradle
buildTypes {
    release {
        minifyEnabled true
        shrinkResources true
        proguardFiles getDefaultProguardFile('proguard-android-optimize.txt'), 'proguard-rules.pro'
    }
}
```

### 2. Remove unused resources:

```yaml
# pubspec.yaml
flutter:
  assets:
    # Only include assets you actually use
```

### 3. Optimize images:
- Sử dụng WebP thay vì PNG/JPG
- Compress images trước khi thêm vào assets

### 4. Split APK by ABI:
```powershell
flutter build apk --release --split-per-abi
```

---

## 🔒 Bảo mật

### Files KHÔNG được commit:

```gitignore
# android/key.properties
# android/app/*.jks
# android/app/*.keystore
```

### Backup quan trọng:

✅ Lưu file keystore (.jks) ở 2-3 nơi khác nhau
✅ Lưu password keystore
✅ Lưu alias name
✅ Backup vào cloud storage riêng tư

---

## 🐛 Troubleshooting

### Lỗi: "Android SDK not found"
```powershell
flutter config --android-sdk "C:\Users\[YourUsername]\AppData\Local\Android\Sdk"
```

### Lỗi: "Gradle sync failed"
```powershell
cd android
./gradlew clean
cd ..
flutter clean
flutter pub get
```

### Lỗi: "License not accepted"
```powershell
flutter doctor --android-licenses
# Accept all licenses
```

### Lỗi build: "Out of memory"
```gradle
// android/gradle.properties
org.gradle.jvmargs=-Xmx4096m -XX:MaxPermSize=512m
```

---

## 📝 Checklist trước khi build Release:

- [ ] Đã cài Android Studio và Android SDK
- [ ] Đã tạo keystore file
- [ ] Đã tạo key.properties
- [ ] Đã cấu hình signing trong build.gradle
- [ ] Đã test ở chế độ debug
- [ ] Đã update version trong pubspec.yaml
- [ ] Đã update versionCode và versionName trong build.gradle
- [ ] Đã test tất cả features
- [ ] Đã backup keystore file
- [ ] Đã xóa debug logs/console.log

---

## 🎉 Sau khi build xong:

### Chia sẻ APK:
- Copy file từ `build/app/outputs/flutter-apk/`
- Share qua email, drive, hoặc internal distribution
- Hoặc upload lên Google Play Console

### Upload lên Google Play:
1. Tạo tài khoản Google Play Developer ($25 một lần)
2. Tạo app listing
3. Upload AAB file (app-release.aab)
4. Điền thông tin app
5. Submit for review

---

## 📞 Hỗ trợ:

- Flutter Docs: https://docs.flutter.dev/deployment/android
- Android Docs: https://developer.android.com/studio/build/building-cmdline

---

**Happy Building! 🚀**
