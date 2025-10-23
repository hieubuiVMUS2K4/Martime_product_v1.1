# 🚀 HƯỚNG DẪN TEST KÊT NỐI - ĐƠN GIẢN & CHI TIẾT

## 📝 Tổng quan
Hướng dẫn này sẽ chỉ bạn cách:
1. Khởi động Edge Server đúng cách
2. Tìm địa chỉ IP của máy tính
3. Cấu hình Mobile App kết nối đến Server
4. Test kết nối và đăng nhập

**Thời gian:** 5-10 phút  
**Yêu cầu:** PC và Mobile App trên cùng mạng WiFi

---

## 🔧 BƯỚC 1: CHUẨN BỊ DATABASE

### 1.1. Khởi động Database
```powershell
# Mở PowerShell tại thư mục gốc project
cd edge-services
docker compose up -d edge-postgres
```

**Đợi khoảng 10 giây** để database khởi động.

### 1.2. Kiểm tra Database đã chạy
```powershell
docker ps
```

**Kết quả mong đợi:**
```
CONTAINER ID   IMAGE            STATUS        PORTS
xxxxx          postgres:15      Up 30 seconds 0.0.0.0:5433->5432/tcp
```

✅ Thấy `postgres:15` với status `Up` → OK!  
❌ Không thấy → Chạy lại lệnh `docker compose up -d edge-postgres`

### 1.3. Thêm dữ liệu mẫu (nếu chưa có)
```powershell
# Vẫn ở thư mục edge-services
.\INSERT-SAMPLE-DATA.ps1
```

**Kết quả mong đợi:**
```
[OK] Docker is running
[OK] PostgreSQL container is running
[2/4] Executing SQL script...
[OK] SQL executed successfully

✅ SAMPLE DATA INSERTED SUCCESSFULLY!
- 6 Crew Members (CM001-CM006)
- 10 Maintenance Tasks
- 4 Safety Alarms
```

✅ Thấy "SAMPLE DATA INSERTED SUCCESSFULLY!" → OK!  
❌ Lỗi "already exists" → Không sao, data đã có sẵn, bỏ qua!

---

## 🌐 BƯỚC 2: TÌM ĐỊA CHỈ IP CỦA MÁY

### 2.1. Lấy địa chỉ IP
```powershell
ipconfig
```

### 2.2. Tìm dòng "IPv4 Address"
```
Wireless LAN adapter Wi-Fi:
   IPv4 Address. . . . . . . . . . . : 192.168.1.138
```

📝 **Ghi nhớ số IP này!** Ví dụ: `192.168.1.138`

### 2.3. Xác nhận đúng mạng WiFi
- ✅ Phải là dòng có "Wireless LAN" hoặc "Wi-Fi"
- ✅ Địa chỉ thường bắt đầu bằng `192.168.x.x` hoặc `10.0.x.x`
- ❌ Không phải `127.0.0.1` (đó là localhost)
- ❌ Không phải `169.254.x.x` (không có kết nối)

**IP của bạn:** `192.168.1.___` ← Điền vào đây

---

## 🖥️ BƯỚC 3: KHỞI ĐỘNG EDGE SERVER (QUAN TRỌNG!)

### 3.1. Mở PowerShell mới (hoặc tab mới)
```powershell
cd edge-services
```

### 3.2. Chạy Edge Server ở chế độ LAN
```powershell
dotnet run --urls "http://0.0.0.0:5001"
```

⚠️ **LƯU Ý:** Phải dùng `0.0.0.0` chứ KHÔNG phải `localhost`!

### 3.3. Đợi server khởi động
**Kết quả mong đợi:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://0.0.0.0:5001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

✅ Thấy "Now listening on: http://0.0.0.0:5001" → OK!  
❌ Thấy "http://localhost:5001" → SAI! Dừng lại (Ctrl+C) và chạy lại lệnh đúng

### 3.4. Xác nhận server lắng nghe đúng
Mở PowerShell mới (giữ server chạy):
```powershell
netstat -an | findstr ":5001"
```

**Kết quả đúng:**
```
TCP    0.0.0.0:5001           0.0.0.0:0              LISTENING
```

✅ Có `0.0.0.0:5001` → OK! Server có thể nhận kết nối từ mọi thiết bị  
❌ Chỉ có `127.0.0.1:5001` → SAI! Server chỉ nhận localhost

### 3.5. Test server bằng trình duyệt
Mở trình duyệt, vào:
```
http://localhost:5001/swagger
```

✅ Thấy trang Swagger UI → Server hoạt động!  
❌ Không load được → Kiểm tra lại server có đang chạy không

---

## 📱 BƯỚC 4: CHẠY MOBILE APP

### 4.1. Mở PowerShell mới
```powershell
cd frontend-mobile
```

### 4.2. Chạy app
```powershell
flutter run -d windows
```

### 4.3. Đợi app build và mở
**Kết quả mong đợi:**
```
Building Windows application...                  57.2s
√ Built build\windows\x64\runner\Debug\maritime_crew_app.exe
```

✅ App mở ra hiển thị màn hình Login → OK!

---

## 🔗 BƯỚC 5: CÂ̂U HÌNH SERVER TRONG APP

### 5.1. Xem URL mặc định
Ở màn hình Login, **scroll xuống** phía dưới form đăng nhập.

**Bạn sẽ thấy:**
```
┌─────────────────────────┐
│  [Crew ID field]        │
│  [Password field]       │
│  [    Login    ]        │
│                         │
│  🔵 http://192.168.1.100:5001  ← URL hiện tại
│                         │
│  ⚙️ Server Settings     │
└─────────────────────────┘
```

📝 URL mặc định: `http://192.168.1.100:5001`

### 5.2. Mở Server Settings
1. Click nút **"⚙️ Server Settings"**
2. Dialog sẽ mở ra

### 5.3. Xem hướng dẫn nhanh
Trong dialog, click nút **"?"** (dấu hỏi)

**Sẽ thấy example URLs:**
```
http://localhost:5001          ← Khi test trên cùng máy
http://192.168.1.100:5001      ← Example với IP khác
http://192.168.0.50:5001       ← Example với subnet khác
http://10.0.0.100:5001         ← Example với mạng khác
```

💡 Đây chỉ là ví dụ, bạn cần dùng IP thực tế của mình!

---

## ✅ BƯỚC 6: NHẬP VÀ TEST URL

### 6.1. Xóa URL cũ và nhập URL mới

**Cách 1: Nhập thủ công**
1. Xóa URL cũ trong ô input
2. Nhập: `http://` + `IP của bạn` + `:5001`
3. Ví dụ: `http://192.168.1.138:5001`

**Cách 2: Paste từ clipboard**
1. Copy URL này vào clipboard: `http://192.168.1.138:5001`
2. Click nút **"📋"** (paste) trong dialog
3. URL tự động điền vào

### 6.2. Kiểm tra URL đúng format

✅ **Đúng:**
```
http://192.168.1.138:5001
http://localhost:5001
https://192.168.1.138:5001
```

❌ **Sai:**
```
192.168.1.138:5001        ← Thiếu http://
http://                    ← Thiếu IP
htp://192.168.1.138:5001  ← Sai chính tả http
```

### 6.3. Test Connection
1. URL đã nhập: `http://192.168.1.138:5001`
2. Click nút **"Test"**
3. **Đợi 2-5 giây**

**Kết quả có thể có:**

#### ✅ **Kết nối thành công:**
```
┌─────────────────────────────┐
│ ✓ Connection successful!    │  ← Màu xanh lá
└─────────────────────────────┘
```

→ Tuyệt! Server hoạt động đúng, bỏ qua phần Troubleshooting bên dưới.

#### ❌ **Kết nối thất bại:**
```
┌─────────────────────────────────────────┐
│ ✗ Connection failed: DioException...    │  ← Màu đỏ/cam
└─────────────────────────────────────────┘
```

→ Có vấn đề! Xem phần **TROUBLESHOOTING** ở dưới.

---

## 💾 BƯỚC 7: LƯU CẤU HÌNH

### 7.1. Sau khi test thành công
1. Thấy "✓ Connection successful!" màu xanh
2. Click nút **"Save"**

### 7.2. Xác nhận lưu thành công
**Sẽ thấy thông báo:**
```
┌─────────────────────────────────────────┐
│ ✓ Server URL updated to:                │
│   http://192.168.1.138:5001             │  ← Màu xanh
└─────────────────────────────────────────┘
```

### 7.3. Kiểm tra URL đã cập nhật
- Dialog đóng lại
- Quay về màn hình Login
- **Scroll xuống** xem URL mới

**Sẽ thấy:**
```
🔵 http://192.168.1.138:5001  ← URL mới của bạn!
```

✅ URL đã thay đổi → Thành công!

---

## 🔑 BƯỚC 8: ĐĂNG NHẬP

### 8.1. Nhập thông tin đăng nhập
```
Crew ID:  CM001
Password: password123
```

### 8.2. Click nút "Login"

### 8.3. Kết quả mong đợi

✅ **Đăng nhập thành công:**
- App chuyển sang màn hình Dashboard
- Hiển thị tên: **John Smith**
- Chức vụ: **Chief Engineer**
- Thấy thống kê: Tasks, Alarms, Crew

✅ **Màn hình Dashboard:**
```
┌─────────────────────────────────┐
│  👤 John Smith                  │
│     Chief Engineer              │
│                                 │
│  📊 Dashboard                   │
│                                 │
│  🔧 My Tasks:        2          │
│  🚨 Active Alarms:   3          │
│  👥 Crew Onboard:    6          │
│                                 │
│  [Task List]                    │
│  └─ Main Engine oil change      │
│  └─ Fuel Filter Replacement     │
└─────────────────────────────────┘
```

🎉 **HOÀN THÀNH!** Kết nối thành công!

---

## 🔧 TROUBLESHOOTING (Khắc phục sự cố)

### ❌ Vấn đề 1: "Connection failed"

#### Kiểm tra 1: Server có đang chạy?
```powershell
netstat -an | findstr ":5001"
```

**Phải thấy:**
```
TCP    0.0.0.0:5001    0.0.0.0:0    LISTENING
```

❌ Không thấy → Server chưa chạy, quay lại **BƯỚC 3**  
❌ Chỉ thấy `127.0.0.1:5001` → Server chạy sai, phải restart với `0.0.0.0`

#### Kiểm tra 2: Firewall có block không?
```powershell
# Tắt tạm thời Windows Firewall để test
# Hoặc thêm rule cho port 5001
```

**Cách thêm rule:**
1. Mở "Windows Defender Firewall"
2. Click "Advanced settings"
3. Inbound Rules → New Rule
4. Port → TCP → 5001 → Allow

#### Kiểm tra 3: Cả 2 thiết bị cùng WiFi?
- Kiểm tra tên mạng WiFi trên PC
- Kiểm tra tên mạng WiFi trên Mobile/Tablet
- Phải **giống nhau**!

#### Kiểm tra 4: IP đúng không?
```powershell
ipconfig | findstr "IPv4"
```

So sánh IP từ lệnh này với IP bạn nhập trong app.

---

### ❌ Vấn đề 2: "Invalid credentials" khi login

#### Nguyên nhân: Database chưa có crew members

**Giải pháp:**
```powershell
cd edge-services
.\INSERT-SAMPLE-DATA.ps1
```

**Kiểm tra:**
```powershell
docker exec maritime-edge-postgres psql -U edge_user -d maritime_edge -c "SELECT crew_id, full_name FROM crew_members;"
```

**Phải thấy:**
```
 crew_id | full_name
---------+---------------
 CM001   | John Smith
 CM002   | David Wilson
 CM003   | Mike Johnson
 ...
```

---

### ❌ Vấn đề 3: URL validation error

**Lỗi:** "Invalid URL format (must start with http:// or https://)"

**Nguyên nhân:** URL không đúng format

**Kiểm tra:**
- ✅ Phải bắt đầu bằng `http://` hoặc `https://`
- ✅ Phải có IP hoặc domain sau `://`
- ✅ Không có khoảng trắng
- ✅ Port đúng format `:5001`

**Ví dụ đúng:**
```
http://192.168.1.138:5001
```

---

### ❌ Vấn đề 4: URL không lưu sau khi restart

#### Nguyên nhân: Hive database bị lỗi

**Giải pháp:**
```bash
cd frontend-mobile
flutter clean
flutter pub get
flutter run -d windows
```

Sau đó cấu hình lại server URL.

---

### ❌ Vấn đề 5: App bị crash khi mở Server Settings

**Kiểm tra console log:**
```powershell
# Trong terminal đang chạy flutter run
# Xem có lỗi gì không
```

**Thử:**
```bash
flutter clean
flutter pub get
flutter run -d windows
```

---

## 📊 CHECKLIST HOÀN CHỈNH

In ra checklist này và đánh dấu từng bước:

### Chuẩn bị:
- [ ] Database đang chạy (`docker ps`)
- [ ] Data mẫu đã insert (`INSERT-SAMPLE-DATA.ps1`)
- [ ] Đã tìm được IP (`ipconfig`)

### Edge Server:
- [ ] Server chạy với `dotnet run --urls "http://0.0.0.0:5001"`
- [ ] Thấy "Now listening on: http://0.0.0.0:5001"
- [ ] `netstat` cho thấy `0.0.0.0:5001 LISTENING`
- [ ] Swagger UI mở được (`http://localhost:5001/swagger`)

### Mobile App:
- [ ] App build và chạy thành công
- [ ] Thấy màn hình Login
- [ ] Thấy URL hiện tại ở dưới form login

### Cấu hình:
- [ ] Mở Server Settings
- [ ] Nhập URL: `http://YOUR_IP:5001`
- [ ] Test connection → "✓ Connection successful!" (xanh)
- [ ] Click Save → Thấy thông báo thành công
- [ ] URL mới hiển thị trên màn hình Login

### Đăng nhập:
- [ ] Nhập CM001 / password123
- [ ] Click Login
- [ ] Chuyển sang Dashboard
- [ ] Thấy thông tin John Smith
- [ ] Thấy tasks và alarms

### Kiểm tra persistence:
- [ ] Đóng app
- [ ] Mở lại app
- [ ] URL vẫn là `http://YOUR_IP:5001`

---

## 🎯 TÓM TẮT NHANH

```
1. Start Database:    docker compose up -d edge-postgres
2. Insert Data:       .\INSERT-SAMPLE-DATA.ps1
3. Get IP:            ipconfig
4. Start Server:      dotnet run --urls "http://0.0.0.0:5001"
5. Run App:           flutter run -d windows
6. Configure:         Server Settings → http://YOUR_IP:5001
7. Test:              Click "Test" → See green success
8. Save:              Click "Save"
9. Login:             CM001 / password123
10. Done!             🎉
```

---

## 📞 HỖ TRỢ

**Nếu vẫn gặp vấn đề:**

1. **Đọc kỹ phần Troubleshooting** ở trên
2. **Kiểm tra logs:**
   - Edge Server console
   - Flutter console
   - Browser console (F12)

3. **Commands hữu ích:**
```powershell
# Kiểm tra Docker
docker ps

# Kiểm tra port 5001
netstat -an | findstr :5001

# Kiểm tra IP
ipconfig | findstr "IPv4"

# Test API với curl
curl http://localhost:5001/swagger/index.html

# Kiểm tra database
docker exec maritime-edge-postgres psql -U edge_user -d maritime_edge -c "SELECT COUNT(*) FROM crew_members;"
```

---

## ✅ KẾT LUẬN

**Bạn vừa hoàn thành:**
- ✅ Khởi động Edge Server đúng cách (LAN mode)
- ✅ Tìm và sử dụng IP address
- ✅ Cấu hình Mobile App kết nối server
- ✅ Test kết nối thành công
- ✅ Đăng nhập vào hệ thống

**Giờ bạn có thể:**
- 🔄 Thay đổi server URL bất cứ lúc nào
- 📱 Dùng app từ bất kỳ thiết bị nào trên cùng mạng
- 🚀 Deploy app lên mạng thực tế
- 🧪 Test với nhiều server khác nhau

---

**Chúc mừng! 🎉**  
**Status:** ✅ Connected & Ready to Use!
