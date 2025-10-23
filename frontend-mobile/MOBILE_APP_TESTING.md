# 🚀 QUICK START - Test Mobile App với Edge Server

## 1️⃣ Start Edge Server

```bash
# Terminal 1: Start Database
cd edge-services
docker compose up -d edge-postgres

# Terminal 2: Start Backend (LAN mode)
cd edge-services
dotnet run --urls "http://0.0.0.0:5001"
```

## 2️⃣ Get IP Address của máy tính

### Windows:
```powershell
ipconfig
# Tìm "IPv4 Address" của WiFi adapter
# Ví dụ: 192.168.1.100
```

### Linux/Mac:
```bash
ifconfig | grep inet
# Hoặc
ip addr show
```

## 3️⃣ Run Mobile App

```bash
cd frontend-mobile
flutter run -d windows
```

## 4️⃣ Configure Server URL trong App

1. Mở app
2. Click "Settings" (icon gear) ở bottom navigation
3. Click "Server Configuration"
4. Nhập Server URL: `http://192.168.1.100:5001` (thay IP của bạn)
5. Click "Save Configuration"
6. **Restart app** để áp dụng thay đổi

## 5️⃣ Test Login

**Tài khoản test mặc định:**

| Crew ID | Password | Full Name | Position |
|---------|----------|-----------|----------|
| CM001 | password123 | John Smith | Chief Engineer |
| CM002 | password123 | David Wilson | 2nd Engineer |
| CM003 | password123 | Mike Johnson | Electrician |

**Note:** Password mặc định là `password123` cho tất cả crew trong development mode.

## 6️⃣ Test Features

### ✅ Login
- Nhập Crew ID và Password
- Click "Login"
- Verify token saved và redirect to Home

### ✅ Home Dashboard
- Xem total tasks, completed tasks, pending tasks
- Verify data từ Edge Server API

### ✅ My Tasks
- Xem list maintenance tasks được assign
- Filter by status (All, Pending, Completed)
- Click vào task để xem detail

### ✅ Task Actions
- **Start Task**: Click "Start Task" button
- **Complete Task**: Click "Mark Complete", nhập notes
- **View History**: Xem completion history

### ✅ Profile
- Xem crew information
- Xem certificates với expiry status:
  - 🟢 Green = Valid
  - 🟠 Orange = Expiring soon (< 30 days)
  - 🔴 Red = Expired

### ✅ Offline Mode
- Disconnect WiFi
- App vẫn hiển thị cached data
- Complete task → Lưu vào sync queue
- Reconnect WiFi → Auto sync pending changes

## 🔧 Troubleshooting

### ❌ "Failed to connect to server"

**Nguyên nhân:**
- Backend chưa chạy
- IP address sai
- Firewall block port 5001

**Giải pháp:**
1. Verify backend running: http://YOUR_IP:5001/swagger
2. Test từ browser trên mobile device
3. Check Windows Firewall:
   ```powershell
   # Allow port 5001
   netsh advfirewall firewall add rule name="ASP.NET Core" dir=in action=allow protocol=TCP localport=5001
   ```

### ❌ "Login failed: Invalid credentials"

**Nguyên nhân:**
- Crew ID không tồn tại trong database
- Crew member không onboard

**Giải pháp:**
1. Check database có crew data:
   ```sql
   SELECT * FROM crew_members WHERE is_onboard = true;
   ```
2. Insert test crew nếu chưa có (xem TESTING_GUIDE.md)

### ❌ "No tasks found"

**Nguyên nhân:**
- Chưa có maintenance tasks trong database
- Tasks không được assign cho crew

**Giải pháp:**
1. Insert sample tasks:
   ```sql
   INSERT INTO maintenance_tasks (task_id, equipment_name, task_description, assigned_to, status, next_due_at)
   VALUES ('MT001', 'Main Engine', 'Oil Change', 'John Smith', 'PENDING', NOW() + INTERVAL '7 days');
   ```
2. Verify trong Swagger: GET /api/maintenance/tasks/my-tasks?assignedTo=John Smith

## 📊 API Endpoints được sử dụng

| Endpoint | Method | Purpose |
|----------|--------|---------|
| /api/auth/login | POST | Login authentication |
| /api/auth/logout | POST | Logout |
| /api/crew/me | GET | Get user profile |
| /api/crew/me/certificates | GET | Get certificates |
| /api/maintenance/tasks/my-tasks | GET | Get assigned tasks |
| /api/maintenance/tasks/{id} | GET | Get task detail |
| /api/maintenance/tasks/{id}/complete | POST | Complete task |
| /api/dashboard/stats | GET | Get dashboard stats |

## 🎯 Next Steps

1. **Test Offline Mode**:
   - Disconnect WiFi
   - Complete tasks offline
   - Reconnect và verify sync

2. **Test Certificate Warnings**:
   - Update crew certificate expiry dates
   - Verify warning colors in Profile

3. **Test Error Handling**:
   - Stop backend → App shows error gracefully
   - Invalid credentials → Clear error message

4. **Build APK for Real Device**:
   ```bash
   flutter build apk --debug
   # APK: build/app/outputs/flutter-apk/app-debug.apk
   ```

---

**🚢 Happy Testing! ⚓**
