# ⚡ Hướng Dẫn Nhanh: Kết Nối App Mobile với Edge Server

## 🎯 Tình Trạng Hiện Tại
- ✅ App đã chạy trên điện thoại OPPO CPH2251
- ✅ Edge Server đang chạy trên máy tính
- ❌ Firewall đang chặn kết nối từ điện thoại

## 🔥 Giải Pháp: Mở Windows Firewall

### Cách 1: Chạy PowerShell Script (Khuyến Nghị)

**Bước 1:** Mở PowerShell **với quyền Administrator**
- Nhấn `Windows + X`
- Chọn **"Windows PowerShell (Admin)"** hoặc **"Terminal (Admin)"**

**Bước 2:** Chạy script
```powershell
cd "f:\NCKH\Product\Martime_product_v1.1\edge-services"
.\open-firewall.ps1
```

**Bước 3:** Script sẽ tự động:
- ✅ Tạo firewall rule cho port 5001
- ✅ Cho phép Inbound và Outbound traffic

---

### Cách 2: Chạy Lệnh Thủ Công

Mở PowerShell **với quyền Administrator** và chạy:

```powershell
# Tạo Inbound rule
New-NetFirewallRule -DisplayName "Edge Server Port 5001" -Direction Inbound -Protocol TCP -LocalPort 5001 -Action Allow -Profile Any

# Tạo Outbound rule
New-NetFirewallRule -DisplayName "Edge Server Port 5001 (Outbound)" -Direction Outbound -Protocol TCP -LocalPort 5001 -Action Allow -Profile Any
```

---

### Cách 3: Qua Windows Defender Firewall GUI

1. Mở **Windows Defender Firewall with Advanced Security**
   - Nhấn `Windows + R` → Gõ `wf.msc` → Enter

2. Click **Inbound Rules** → **New Rule...**

3. Chọn **Port** → Next

4. **Protocol**: TCP, **Specific local ports**: `5001` → Next

5. **Allow the connection** → Next

6. Chọn tất cả profiles (Domain, Private, Public) → Next

7. **Name**: `Edge Server Port 5001` → Finish

8. **Lặp lại cho Outbound Rules**

---

## 🧪 Test Kết Nối

### Từ máy tính (test local):
```powershell
# Test API health
Invoke-WebRequest -Uri "http://localhost:5001/api/health" -UseBasicParsing
```

### Từ điện thoại:
1. Mở **Chrome** trên điện thoại
2. Truy cập: `http://10.20.67.86:5001/api/health`
3. Phải thấy response JSON từ server

---

## 📱 Login Trên App

Sau khi firewall đã mở:

1. **Mở app** Maritime Crew trên điện thoại
2. **Nhập thông tin:**
   - Crew ID: `CREW01`
   - Password: `password123`
3. **Tap Login**
4. ✅ Nếu thành công → Vào Dashboard

---

## 🐛 Troubleshooting

### 1. Vẫn timeout sau khi mở firewall?

**Kiểm tra Edge Server:**
```powershell
cd "f:\NCKH\Product\Martime_product_v1.1\edge-services"
dotnet run --urls "http://0.0.0.0:5001"
```

Phải thấy:
```
Now listening on: http://0.0.0.0:5001
```

---

### 2. Không thấy database data?

**Chèn sample data:**
```powershell
cd "f:\NCKH\Product\Martime_product_v1.1\edge-services"
psql -U postgres -d maritime_edge -f insert-sample-data-complete.sql
```

**Hoặc tạo user mới:**
```sql
INSERT INTO crew_members (crew_id, full_name, position, password_hash)
VALUES ('CREW01', 'Test User', 'Engineer', 'password123');
```

---

### 3. IP address thay đổi?

**Tìm IP mới:**
```powershell
ipconfig | Select-String "IPv4"
```

**Cập nhật trong app:**
- Sửa file: `lib/core/constants/api_constants.dart`
- Thay đổi `baseUrl = 'http://NEW_IP:5001'`
- Save file
- Trên terminal đang chạy `flutter run`, nhấn **`r`** để Hot Reload

---

### 4. Kiểm tra firewall rule đã tạo chưa:

```powershell
Get-NetFirewallRule -DisplayName "*Edge Server*" | Format-Table DisplayName, Direction, Action, Enabled
```

Phải thấy:
```
DisplayName                        Direction Action Enabled
-----------                        --------- ------ -------
Edge Server Port 5001              Inbound   Allow  True
Edge Server Port 5001 (Outbound)   Outbound  Allow  True
```

---

### 5. Xóa firewall rule nếu cần:

```powershell
Remove-NetFirewallRule -DisplayName "Edge Server Port 5001"
Remove-NetFirewallRule -DisplayName "Edge Server Port 5001 (Outbound)"
```

---

## ✅ Checklist Hoàn Tất

- [ ] Edge Server đang chạy (`dotnet run --urls "http://0.0.0.0:5001"`)
- [ ] Firewall rule đã được tạo (chạy `open-firewall.ps1`)
- [ ] Điện thoại và máy tính cùng mạng
- [ ] IP address đúng (`10.20.67.86`)
- [ ] Test từ browser trên điện thoại → Thành công
- [ ] Login trên app → Thành công ✨

---

## 🎉 Khi Đã Kết Nối Thành Công

App sẽ:
1. ✅ Login với JWT token
2. ✅ Fetch crew profile
3. ✅ Hiển thị dashboard với stats
4. ✅ Load danh sách tasks
5. ✅ Hoạt động offline với cache

**Hot Reload enabled:**
- Sửa code trong VS Code
- Nhấn **`r`** trong terminal
- App reload ngay lập tức trên điện thoại!

---

## 📞 Lệnh Hữu Ích

```powershell
# Start Edge Server
cd edge-services
dotnet run --urls "http://0.0.0.0:5001"

# Start Flutter app (nếu bị disconnect)
cd frontend-mobile
flutter run -d V8OF6H5PBAKF497P

# Hot Reload
# Nhấn 'r' trong terminal

# Hot Restart
# Nhấn 'R' trong terminal

# Quit
# Nhấn 'q' trong terminal

# View logs realtime
flutter logs
```

---

**✨ Good luck! App sắp chạy rồi! 🚀**
