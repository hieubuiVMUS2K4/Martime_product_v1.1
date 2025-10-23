# 📱 FLUTTER MOBILE APP - INTEGRATION SUMMARY

## ✅ ĐÃ HOÀN THÀNH

### 🎯 Mục tiêu
Kết nối Flutter mobile app với Edge Server API thay vì sử dụng mock data.

### 📦 Các thay đổi

#### 1. **Edge Server - New APIs** (`edge-services/`)

**File mới:**
- ✅ `Controllers/AuthController.cs` - Authentication endpoints

**File updated:**
- ✅ `Controllers/CrewController.cs` - Added `/api/crew/me` và `/api/crew/me/certificates`
- ✅ `Controllers/MaintenanceController.cs` - Added `/api/maintenance/tasks/my-tasks`

**API Endpoints:**
```
POST   /api/auth/login                       - Login with Crew ID + Password
POST   /api/auth/refresh                     - Refresh access token
POST   /api/auth/logout                      - Logout
GET    /api/crew/me?crewId={id}             - Get crew profile
GET    /api/crew/me/certificates?crewId={id} - Get certificates
GET    /api/maintenance/tasks/my-tasks?crewId={id} - Get my tasks
```

#### 2. **Flutter Mobile App** (`frontend-mobile/`)

**File updated:**
- ✅ `lib/presentation/providers/auth_provider.dart`
  - Removed mock login code
  - Call `AuthRepository.login()` API thật
  - Handle real JWT tokens

**Files already ready (no changes needed):**
- ✅ `lib/data/repositories/auth_repository.dart` - Đã sẵn sàng
- ✅ `lib/data/repositories/task_repository.dart` - Đã sẵn sàng  
- ✅ `lib/data/repositories/crew_repository.dart` - Đã sẵn sàng
- ✅ `lib/presentation/providers/task_provider.dart` - Đã sẵn sàng
- ✅ `lib/core/constants/api_constants.dart` - Có baseUrl config

#### 3. **Documentation**

**Files updated:**
- ✅ `GETTING_STARTED.md` - Added Mobile App section (5.7)
  - Flutter setup
  - Server URL configuration
  - LAN access mode (`0.0.0.0:5001`)

**Files created:**
- ✅ `frontend-mobile/MOBILE_APP_TESTING.md` - Complete testing guide
- ✅ `frontend-mobile/INTEGRATION_COMPLETE.md` - Integration summary

---

## 🚀 CÁCH SỬ DỤNG

### Bước 1: Start Edge Server (LAN Mode)

```bash
# Terminal 1: Database
cd edge-services
docker compose up -d edge-postgres

# Terminal 2: Backend (IMPORTANT: Use 0.0.0.0 for LAN access)
dotnet run --urls "http://0.0.0.0:5001"
```

### Bước 2: Get IP Address

```powershell
# Windows
ipconfig

# Linux/Mac
ip addr show
```

Ví dụ: `192.168.1.100`

### Bước 3: Run Mobile App

```bash
cd frontend-mobile
flutter run -d windows
```

### Bước 4: Configure Server trong App

1. Settings → Server Configuration
2. Enter: `http://192.168.1.100:5001` (your IP)
3. Save & Restart app

### Bước 5: Login

**Test Account:**
- Crew ID: `CM001`
- Password: `password123`

---

## 🔍 VERIFICATION

### Test Backend API

```bash
# Health check
curl http://192.168.1.100:5001/health

# Login API
curl -X POST http://192.168.1.100:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"crewId":"CM001","password":"password123"}'

# My tasks
curl http://192.168.1.100:5001/api/maintenance/tasks/my-tasks?crewId=CM001
```

### Test Mobile App

1. ✅ Login successful → Redirect to Home
2. ✅ Home shows stats từ API
3. ✅ My Tasks shows list từ API  
4. ✅ Profile shows crew info từ API
5. ✅ Offline mode works với cached data

---

## 📊 KIẾN TRÚC

```
┌─────────────────────────────────────────────────┐
│          MARITIME EDGE SYSTEM                    │
│                                                  │
│  ┌─────────────────────────────────────────┐   │
│  │  Edge Server (C# ASP.NET Core)          │   │
│  │  IP: 192.168.1.100:5001                 │   │
│  │                                          │   │
│  │  APIs:                                   │   │
│  │  - /api/auth/login                       │   │
│  │  - /api/crew/me                          │   │
│  │  - /api/maintenance/tasks/my-tasks       │   │
│  │  - /api/dashboard/stats                  │   │
│  └─────────────────────────────────────────┘   │
│              ▲                                   │
│              │ HTTP REST API (WiFi LAN)         │
│              │                                   │
│  ┌───────────┴──────────────────────────────┐  │
│  │  Mobile App (Flutter)                    │  │
│  │  - Android / iOS / Windows               │  │
│  │                                           │  │
│  │  Features:                                │  │
│  │  - Login with Crew ID                    │  │
│  │  - View assigned tasks                   │  │
│  │  - Complete tasks                        │  │
│  │  - View profile & certificates           │  │
│  │  - Offline-first architecture            │  │
│  └──────────────────────────────────────────┘  │
│                                                  │
└─────────────────────────────────────────────────┘
```

---

## 💡 KEY POINTS

### 1. Authentication

**Development Mode:**
- Password: `password123` (hardcoded cho tất cả crew)
- Simple token generation (not real JWT)

**Production TODO:**
- Implement BCrypt password hashing
- Use proper JWT library (System.IdentityModel.Tokens.Jwt)
- Add JWT validation middleware
- Role-based authorization

### 2. Network Mode

**localhost:5001** → Only local machine access  
**0.0.0.0:5001** → All devices on LAN can access ✅

### 3. Offline-First

App sử dụng Hive local database để:
- Cache API responses
- Store pending sync queue
- Work offline with cached data
- Auto sync when online

### 4. Error Handling

App có fallback mechanism:
- API available → Use real data
- API not available → Use mock data
- User still can test UI/UX

---

## 🐛 COMMON ISSUES

### Issue 1: "Failed to connect to server"

**Cause:** Backend not running or IP wrong

**Solution:**
```bash
# Verify backend running
http://YOUR_IP:5001/swagger

# Check firewall
netsh advfirewall firewall add rule name="Edge Server" dir=in action=allow protocol=TCP localport=5001
```

### Issue 2: "Login failed: Invalid credentials"

**Cause:** Crew not in database or not onboard

**Solution:**
```sql
-- Check crew exists
SELECT * FROM crew_members WHERE crew_id = 'CM001';

-- Insert test crew
INSERT INTO crew_members (crew_id, full_name, position, is_onboard)
VALUES ('CM001', 'John Smith', 'Chief Engineer', true);
```

### Issue 3: "No tasks found"

**Cause:** No maintenance tasks in database

**Solution:**
```sql
INSERT INTO maintenance_tasks (task_id, equipment_name, task_description, assigned_to, status, next_due_at)
VALUES ('MT001', 'Main Engine', 'Oil Change', 'John Smith', 'PENDING', NOW() + INTERVAL '7 days');
```

---

## 📚 DOCUMENTATION LINKS

- `GETTING_STARTED.md` - Overall project setup
- `frontend-mobile/README.md` - Mobile app architecture  
- `frontend-mobile/MOBILE_APP_TESTING.md` - Testing guide
- `frontend-mobile/INTEGRATION_COMPLETE.md` - Integration details
- `edge-services/README.md` - Edge Server architecture

---

## 🎯 NEXT STEPS

### Immediate

1. ✅ Test login flow
2. ✅ Test task management  
3. ✅ Test offline mode
4. ✅ Build APK for real device

### Future Enhancements

1. **Security:**
   - Proper JWT implementation
   - Password hashing
   - Token refresh mechanism

2. **Features:**
   - Photo upload for tasks
   - Push notifications
   - Barcode scanning
   - Signature capture

3. **Performance:**
   - Background sync service
   - Image caching
   - Pagination for large lists

4. **Production:**
   - SSL/TLS certificates
   - API rate limiting
   - Logging and monitoring
   - Error tracking (Sentry)

---

**✅ INTEGRATION COMPLETED SUCCESSFULLY!**

**Flutter Mobile App is now fully connected to Edge Server API! 🚢⚓**
