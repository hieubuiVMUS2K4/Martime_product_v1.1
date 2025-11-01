# 📱 Cài Đặt APK Lên Điện Thoại - Hướng Dẫn Nhanh

## ✅ Sau khi build xong, file APK nằm ở:
```
f:\NCKH\Product\Martime_product_v1.1\frontend-mobile\build\app\outputs\flutter-apk\app-release.apk
```

## 📲 Cách 1: Copy file APK vào điện thoại (Dễ nhất)

### Bước 1: Copy file APK
- Kết nối điện thoại vào máy tính qua USB
- Chọn **File Transfer** (MTP) trên điện thoại
- Copy file `app-release.apk` vào thư mục **Downloads** trên điện thoại

### Bước 2: Cài đặt APK trên điện thoại
1. Mở **File Manager** (Quản lý file) trên điện thoại
2. Vào thư mục **Downloads**
3. Tap vào file `app-release.apk`
4. Nếu có popup **"Install from unknown sources"**:
   - Chọn **Settings** (Cài đặt)
   - Bật **Allow from this source** (Cho phép từ nguồn này)
   - Quay lại và tap APK lại
5. Chọn **Install** (Cài đặt)
6. Đợi 5-10 giây
7. Chọn **Open** (Mở) để chạy app

---

## 📲 Cách 2: Cài qua ADB (Qua USB)

### Điều kiện:
- USB Debugging đã bật
- Điện thoại đã kết nối qua USB

### Lệnh cài đặt:
```powershell
adb install "f:\NCKH\Product\Martime_product_v1.1\frontend-mobile\build\app\outputs\flutter-apk\app-release.apk"
```

Hoặc nếu muốn cài đè lên phiên bản cũ:
```powershell
adb install -r "f:\NCKH\Product\Martime_product_v1.1\frontend-mobile\build\app\outputs\flutter-apk\app-release.apk"
```

---

## 📲 Cách 3: Chia sẻ qua WiFi/Bluetooth

### Google Drive/Cloud:
1. Upload file APK lên Google Drive
2. Mở Google Drive trên điện thoại
3. Download và cài đặt

### Bluetooth:
1. Send file qua Bluetooth từ máy tính
2. Receive trên điện thoại
3. Tap file để cài

### Email:
1. Gửi file APK qua email
2. Mở email trên điện thoại
3. Download attachment và cài

---

## 🔧 Bật "Install from Unknown Sources"

### Android 8+ (Oreo trở lên):
- Settings → Apps & notifications → Special app access → Install unknown apps
- Chọn app (File Manager, Chrome, etc.)
- Bật **Allow from this source**

### Android cũ hơn:
- Settings → Security → **Unknown sources** → Bật ON

---

## ✅ Sau khi cài xong

### 1. Mở app
- Tìm icon "maritime_crew_app" trên màn hình chính
- Hoặc trong App drawer (danh sách ứng dụng)

### 2. Cấu hình kết nối Backend
App cần kết nối tới Edge Server để hoạt động.

**Mặc định:** `http://192.168.1.100:5001`

**Để thay đổi:** (nếu server ở IP khác)
- Settings trong app (nếu có tính năng này)
- Hoặc rebuild app với IP mới trong `lib/core/constants/api_constants.dart`

### 3. Đảm bảo cùng WiFi
- Điện thoại và máy chủ Edge Server phải **cùng mạng WiFi**
- Test bằng cách mở browser trên điện thoại:
  ```
  http://YOUR_SERVER_IP:5001/api/health
  ```

### 4. Login
- Crew ID: `CM001` (hoặc ID từ database)
- Password: `password123`

---

## 🐛 Xử Lý Lỗi

### Lỗi: "App not installed"
- Gỡ phiên bản cũ nếu có
- Xóa data trong Settings → Apps → Maritime Crew App → Clear data
- Thử cài lại

### Lỗi: "Parse error"
- File APK bị lỗi, build lại:
  ```powershell
  flutter clean
  flutter build apk --release
  ```

### Lỗi: Không kết nối được server
- Kiểm tra IP address của server
- Ping server từ điện thoại (dùng app Network Utilities)
- Tắt firewall trên server hoặc allow port 5001
- Đảm bảo cùng WiFi

### Lỗi: App crashes khi mở
- Check logs:
  ```powershell
  adb logcat | Select-String "flutter"
  ```
- Có thể cần permissions (camera, storage, etc.)

---

## 📊 Thông Tin APK

- **Tên package:** `com.example.maritime_crew_app`
- **Version:** 1.0.0+1
- **Min SDK:** Android 5.0 (API 21)
- **Kích thước:** ~20-25 MB (release)
- **Architecture:** arm64-v8a, armeabi-v7a, x86_64

---

## 🔄 Cập Nhật App

Khi có phiên bản mới:
1. Build APK mới
2. Cài đè lên phiên bản cũ (data được giữ lại)
3. Hoặc gỡ phiên bản cũ → Cài phiên bản mới (mất data)

---

## 📝 Tips

### Share APK cho người khác:
- Upload lên Google Drive
- Chia sẻ link download
- Người nhận download về và cài

### Test trên nhiều điện thoại:
- Build 1 lần
- Copy APK vào nhiều điện thoại
- Tất cả đều chạy được

### Giảm dung lượng APK:
```powershell
# Build cho ARM64 only (điện thoại hiện đại)
flutter build apk --release --target-platform android-arm64

# Kích thước: ~10-12 MB thay vì ~25 MB
```

---

**✅ Xong! App đã chạy trên điện thoại thật của bạn! 🎉**
