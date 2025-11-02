# 🚀 Build APK - Quick Start

## ⚡ Cách nhanh nhất (Debug APK - Không cần signing):

```powershell
# Double click file này:
build-debug.bat

# Hoặc chạy lệnh:
flutter build apk --debug
```

**File output:** `build/app/outputs/flutter-apk/app-debug.apk` (~40-60 MB)

---

## 🔐 Build Release APK (Cần signing key):

### Lần đầu tiên:

#### 1. Tạo Keystore:
```powershell
cd android\app
keytool -genkey -v -keystore maritime-release-key.jks -keyalg RSA -keysize 2048 -validity 10000 -alias maritime-key

# Nhập thông tin:
# Password: [Tạo password mạnh, LƯU LẠI!]
# Name: Maritime Team
# Org: VMUS
# City/Country: Hanoi/VN
```

#### 2. Tạo key.properties:
```powershell
# Copy file mẫu:
copy android\key.properties.example android\key.properties

# Sửa file android\key.properties:
storePassword=<password-bạn-vừa-tạo>
keyPassword=<password-bạn-vừa-tạo>
keyAlias=maritime-key
storeFile=maritime-release-key.jks
```

### Build Release:

```powershell
# Double click:
build-release.bat

# Hoặc chạy lệnh:
flutter build apk --release --split-per-abi
```

**Files output:**
- `app-arm64-v8a-release.apk` (~25 MB) - Cho hầu hết điện thoại hiện đại
- `app-armeabi-v7a-release.apk` (~25 MB) - Cho điện thoại cũ
- `app-x86_64-release.apk` (~30 MB) - Cho emulator

---

## 📱 Cài đặt APK:

### Trên điện thoại:
1. Copy file APK vào điện thoại
2. Mở File Manager → tìm file APK
3. Tap để cài đặt
4. Cho phép "Install from Unknown Sources" nếu được hỏi

### Qua USB (ADB):
```powershell
# Kết nối điện thoại qua USB, bật USB Debugging
flutter install

# Hoặc:
adb install build/app/outputs/flutter-apk/app-release.apk
```

---

## ❓ Troubleshooting:

### Lỗi: "Android SDK not found"
```powershell
# Cài Android Studio: https://developer.android.com/studio
# Sau đó chạy:
flutter doctor --android-licenses
```

### Lỗi: "Signing key not found"
```powershell
# Kiểm tra file có tồn tại:
dir android\key.properties
dir android\app\maritime-release-key.jks

# Nếu không có, tạo lại theo bước 1-2 ở trên
```

### Lỗi: "Out of memory"
```powershell
# Tăng memory cho Gradle trong android/gradle.properties:
org.gradle.jvmargs=-Xmx4G
```

---

## 📊 So sánh các loại build:

| Type | Size | Use Case | Signing Required |
|------|------|----------|------------------|
| Debug | ~60 MB | Testing, Development | ❌ No |
| Release Universal | ~50 MB | Single APK for all devices | ✅ Yes |
| Release Split | ~25 MB each | Smaller size, upload to Play Store | ✅ Yes |
| App Bundle (.aab) | ~45 MB | Google Play Store only | ✅ Yes |

---

## 🎯 Recommended:

- **Testing**: Use `build-debug.bat`
- **Distribution**: Use `build-release.bat` (split APKs)
- **Play Store**: Use `flutter build appbundle --release`

---

## 📞 Support:

Xem hướng dẫn đầy đủ tại: [BUILD_APK_GUIDE.md](BUILD_APK_GUIDE.md)
