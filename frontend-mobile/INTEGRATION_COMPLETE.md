# ✅ MOBILE APP - EDGE SERVER INTEGRATION COMPLETE

## 🎉 ĐÃ HOÀN THÀNH

### 1. Edge Server APIs (Backend)

✅ **AuthController.cs** - Xác thực người dùng
- `POST /api/auth/login` - Login với Crew ID + Password
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - Logout

✅ **CrewController.cs** - Thông tin thuyền viên  
- `GET /api/crew/me?crewId={id}` - Get profile
- `GET /api/crew/me/certificates?crewId={id}` - Get certificates với expiry status

✅ **MaintenanceController.cs** - Quản lý bảo trì
- `GET /api/maintenance/tasks/my-tasks?crewId={id}` - Get assigned tasks
- `GET /api/maintenance/tasks/{id}` - Get task detail
- `POST /api/maintenance/tasks/{id}/complete` - Complete task

### 2. Mobile App (Flutter)

✅ **AuthProvider** - Kết nối API thật thay vì mock data
- Call `AuthRepository.login()` → Edge Server API
- Save JWT tokens to Secure Storage
- Handle errors và network issues

✅ **TaskProvider** - Đã có sẵn, sử dụng TaskRepository

✅ **CrewRepository** - Đã có sẵn, call API thật

✅ **Server Configuration Screen** - Config base URL
- User có thể nhập IP của Edge Server
- Example: `http://192.168.1.100:5001`

### 3. Documentation

✅ **GETTING_STARTED.md** - Updated với Mobile App section

✅ **MOBILE_APP_TESTING.md** - Hướng dẫn test đầy đủ

---

## 🚀 CÁCH SỬ DỤNG

### Start Edge Server (LAN Mode)

```bash
# Terminal 1: Database
cd edge-services
docker compose up -d edge-postgres edge-pgadmin

# Terminal 2: Backend (LAN access)
cd edge-services
dotnet run --urls "http://0.0.0.0:5001"
```

### Get IP Address

```powershell
# Windows
ipconfig
# Tìm IPv4 của WiFi adapter
# Ví dụ: 192.168.1.100
```

### Run Mobile App

```bash
cd frontend-mobile
flutter run -d windows
```

### Configure Server URL

1. Open app
2. Settings → Server Configuration
3. Enter: `http://192.168.1.100:5001` (your IP)
4. Save Configuration
5. **Restart app**

### Login

**Test Accounts:**
- Crew ID: `CM001`, `CM002`, `CM003`
- Password: `password123` (default for all)

---

## 📋 CHÚ Ý QUAN TRỌNG

### 1. Backend MUST run with `0.0.0.0:5001`

❌ **SAI:** `dotnet run --urls "http://localhost:5001"`  
→ Chỉ local machine access được

✅ **ĐÚNG:** `dotnet run --urls "http://0.0.0.0:5001"`  
→ Tất cả devices trên LAN access được

### 2. Windows Firewall

Nếu mobile app không kết nối được, cho phép port 5001:

```powershell
# Run as Administrator
netsh advfirewall firewall add rule name="ASP.NET Core Edge Server" dir=in action=allow protocol=TCP localport=5001
```

### 3. Database phải có crew data

Verify crew members trong database:

```sql
SELECT * FROM crew_members WHERE is_onboard = true;
```

Nếu chưa có, insert sample crew:

```sql
INSERT INTO crew_members (crew_id, full_name, position, rank, is_onboard, created_at)
VALUES 
  ('CM001', 'John Smith', 'Chief Engineer', 'Chief Engineer', true, NOW()),
  ('CM002', 'David Wilson', '2nd Engineer', '2nd Engineer', true, NOW()),
  ('CM003', 'Mike Johnson', 'Electrician', 'Electrician', true, NOW());
```

### 4. Mobile App sẽ fallback to Mock Data

Nếu không kết nối được Edge Server:
- App tự động dùng mock data
- User vẫn test được UI/UX
- Khi server online → Auto switch sang real API

---

## 🔍 TEST CHECKLIST

### ✅ API Endpoints

Test từ browser hoặc Postman:

1. **Health Check:**
   ```
   GET http://YOUR_IP:5001/health
   → "Healthy"
   ```

2. **Swagger UI:**
   ```
   http://YOUR_IP:5001/swagger
   → API documentation page
   ```

3. **Login API:**
   ```
   POST http://YOUR_IP:5001/api/auth/login
   Body: {"crewId": "CM001", "password": "password123"}
   → Returns access_token, refresh_token, user info
   ```

4. **My Tasks:**
   ```
   GET http://YOUR_IP:5001/api/maintenance/tasks/my-tasks?crewId=CM001
   → Returns array of tasks
   ```

### ✅ Mobile App Features

1. **Login Flow**
   - [ ] Enter Crew ID + Password
   - [ ] Click Login
   - [ ] Verify redirect to Home
   - [ ] Check token saved (Settings → About)

2. **Home Dashboard**
   - [ ] Shows total tasks count
   - [ ] Shows completed tasks count
   - [ ] Shows pending tasks count
   - [ ] Data từ API (not mock)

3. **My Tasks Screen**
   - [ ] List all assigned tasks
   - [ ] Filter by status
   - [ ] Click task → Detail screen

4. **Task Detail**
   - [ ] Shows equipment info
   - [ ] Shows due date
   - [ ] "Start Task" button works
   - [ ] "Mark Complete" button works

5. **Profile Screen**
   - [ ] Shows crew information
   - [ ] Shows certificates list
   - [ ] Certificate expiry colors:
     - 🟢 Green = Valid (>30 days)
     - 🟠 Orange = Expiring (<30 days)
     - 🔴 Red = Expired

6. **Offline Mode**
   - [ ] Disconnect WiFi
   - [ ] App shows cached data
   - [ ] Complete task offline
   - [ ] Reconnect WiFi
   - [ ] Auto sync pending changes

---

## 🐛 TROUBLESHOOTING

### App says "Failed to connect"

**Check:**
1. Edge Server running: `http://YOUR_IP:5001/swagger`
2. IP address correct in app settings
3. Firewall allows port 5001
4. Both devices on same WiFi network

### Login fails with "Invalid credentials"

**Check:**
1. Crew ID exists in database
2. Crew member `is_onboard = true`
3. Password is `password123` (default)

### No tasks showing

**Check:**
1. Database has maintenance_tasks
2. Tasks are assigned to crew member
3. Task status is PENDING or IN_PROGRESS

---

## 📱 BUILD FOR REAL DEVICE

### Android APK

```bash
cd frontend-mobile

# Debug APK (for testing)
flutter build apk --debug

# Release APK (for production)
flutter build apk --release
```

APK location: `build/app/outputs/flutter-apk/`

### Install on Android Phone

1. Enable "Developer Mode" + "USB Debugging"
2. Connect phone via USB
3. `flutter devices` → Check device detected
4. `flutter run -d YOUR_DEVICE_ID`

---

## 🎯 NEXT STEPS

### 1. Add JWT Middleware (Production)

Hiện tại authentication đơn giản (password = "password123").  
Production cần:
- BCrypt password hashing
- Proper JWT token generation
- Token validation middleware
- Role-based authorization

### 2. Add Photo Upload

Tasks có thể có photos attachment:
- Take photo with camera
- Upload to Edge Server
- Store in `wwwroot/uploads/` hoặc blob storage

### 3. Add Push Notifications

Alert crew members khi:
- New task assigned
- Task overdue
- Certificate expiring soon

### 4. Implement Real Sync Service

Background service để:
- Check network status
- Auto sync pending changes
- Retry failed uploads
- Show sync progress

---

**🚢 Integration Complete! Ready for Testing! ⚓**

**Các bước tiếp theo:**
1. Start Edge Server với LAN mode
2. Run Mobile App
3. Config server URL
4. Test login và các features
5. Build APK cho real device testing
