# 🚢 MARITIME MOBILE APP - COMPLETE GUIDE

## ✅ HOÀN THÀNH

**Flutter Mobile App đã được kết nối hoàn toàn với Edge Server API!**

Không còn sử dụng mock data - tất cả features đều call API thật từ Edge Server.

---

## 🚀 QUICK START (2 Commands)

### Terminal 1: Start Edge Server

```cmd
START_EDGE_SERVER.bat
```

Hoặc manual:

```bash
cd edge-services
docker compose up -d edge-postgres
dotnet run --urls "http://0.0.0.0:5001"
```

### Terminal 2: Start Mobile App

```cmd
START_MOBILE_APP.bat
```

Hoặc manual:

```bash
cd frontend-mobile
flutter pub get
flutter run -d windows
```

---

## 📱 APP CONFIGURATION

### Bước 1: Get IP Address

```cmd
ipconfig
```

Tìm IPv4 của WiFi adapter (ví dụ: `192.168.1.100`)

### Bước 2: Configure trong App

1. Open app
2. Settings → Server Configuration  
3. Enter: `http://192.168.1.100:5001` (your IP)
4. Save Configuration
5. **Restart app**

### Bước 3: Login

**Test Accounts:**
- Crew ID: `CM001` / Password: `password123` - John Smith (Chief Engineer)
- Crew ID: `CM002` / Password: `password123` - David Wilson (2nd Engineer)
- Crew ID: `CM003` / Password: `password123` - Mike Johnson (Electrician)

---

## ✨ FEATURES

### 1. Authentication ✅
- Login với Crew ID + Password
- JWT token authentication
- Secure storage
- Auto logout on token expire

### 2. Dashboard ✅
- Total tasks count
- Completed tasks
- Pending tasks
- Statistics từ Edge Server API

### 3. My Tasks ✅
- List tasks assigned to crew
- Filter by status (All/Pending/Completed)
- View task details
- Start task
- Mark complete với notes

### 4. Profile ✅
- Crew information
- View certificates:
  - STCW Certificate
  - Medical Certificate
  - Passport
  - Visa
- Certificate expiry warnings:
  - 🟢 Green = Valid (>30 days)
  - 🟠 Orange = Expiring soon (<30 days)
  - 🔴 Red = Expired

### 5. Offline Mode ✅
- App caches data locally (Hive database)
- Works without internet
- Sync queue for offline actions
- Auto sync when reconnected

### 6. Settings ✅
- Server URL configuration
- Clear cache
- App version info
- About

---

## 🔧 SETUP DETAILS

### Requirements

- **Flutter SDK** >= 3.0.0
- **Dart SDK** >= 3.0.0  
- **.NET 8.0 SDK** (cho Edge Server)
- **Docker Desktop** (cho PostgreSQL)

### Edge Server APIs

**Base URL:** `http://YOUR_IP:5001`

**Endpoints:**
```
POST   /api/auth/login                       - Login
POST   /api/auth/logout                      - Logout
POST   /api/auth/refresh                     - Refresh token
GET    /api/crew/me?crewId={id}             - Get profile
GET    /api/crew/me/certificates?crewId={id} - Get certificates
GET    /api/maintenance/tasks/my-tasks?crewId={id} - Get tasks
GET    /api/maintenance/tasks/{id}          - Task detail
POST   /api/maintenance/tasks/{id}/complete - Complete task
GET    /api/dashboard/stats                 - Dashboard stats
```

---

## 📊 ARCHITECTURE

```
┌─────────────────────────────────────────┐
│         SHIP (LAN Network)               │
│                                          │
│  ┌──────────────────────────────────┐  │
│  │  Edge Server (ASP.NET Core)      │  │
│  │  IP: 192.168.1.100:5001          │  │
│  │  PostgreSQL Database             │  │
│  └────────────┬─────────────────────┘  │
│               │ REST API (WiFi)         │
│               │                          │
│  ┌────────────▼─────────────────────┐  │
│  │  Mobile App (Flutter)            │  │
│  │  - Authentication                │  │
│  │  - Task Management               │  │
│  │  - Profile & Certificates        │  │
│  │  - Offline Sync                  │  │
│  └──────────────────────────────────┘  │
└─────────────────────────────────────────┘
```

### Data Flow

1. **Login:**
   ```
   App → POST /api/auth/login → Server
   Server → JWT tokens → App (Secure Storage)
   ```

2. **Fetch Tasks:**
   ```
   App → GET /api/maintenance/tasks/my-tasks → Server
   Server → Task list → App (Cache in Hive)
   ```

3. **Offline Mode:**
   ```
   App → Read from Hive cache → Display
   User action → Save to sync queue → Hive
   Online → Sync queue → Server API
   ```

---

## 🐛 TROUBLESHOOTING

### ❌ "Failed to connect to server"

**Causes:**
- Edge Server not running
- Wrong IP address
- Firewall blocking port 5001
- Not on same WiFi network

**Solutions:**
```bash
# 1. Verify Edge Server running
curl http://YOUR_IP:5001/health

# 2. Open browser, visit:
http://YOUR_IP:5001/swagger

# 3. Check firewall (Windows):
netsh advfirewall firewall add rule name="Edge Server" dir=in action=allow protocol=TCP localport=5001

# 4. Verify app config:
Settings → Server Configuration → Check URL
```

### ❌ "Login failed: Invalid credentials"

**Causes:**
- Crew ID không tồn tại
- Crew member không onboard
- Password sai

**Solutions:**
```sql
-- Check crew exists
SELECT * FROM crew_members WHERE crew_id = 'CM001';

-- If not exists, insert:
INSERT INTO crew_members (crew_id, full_name, position, is_onboard, created_at)
VALUES ('CM001', 'John Smith', 'Chief Engineer', true, NOW());
```

### ❌ "No tasks found"

**Solution:**
```sql
INSERT INTO maintenance_tasks 
  (task_id, equipment_name, task_description, assigned_to, status, next_due_at, created_at)
VALUES 
  ('MT001', 'Main Engine', 'Oil Change', 'John Smith', 'PENDING', NOW() + INTERVAL '7 days', NOW()),
  ('MT002', 'Generator 1', 'Filter Replacement', 'John Smith', 'PENDING', NOW() + INTERVAL '14 days', NOW());
```

---

## 📱 BUILD FOR PRODUCTION

### Android APK

```bash
cd frontend-mobile

# Debug (for testing)
flutter build apk --debug

# Release (for production)
flutter build apk --release
```

**Output:** `build/app/outputs/flutter-apk/app-release.apk`

### Install on Android Device

```bash
# Connect phone via USB (with USB debugging enabled)
flutter devices

# Run on device
flutter run -d DEVICE_ID

# Or install APK
adb install build/app/outputs/flutter-apk/app-release.apk
```

---

## 📚 DOCUMENTATION

| File | Description |
|------|-------------|
| `GETTING_STARTED.md` | Overall project setup |
| `MOBILE_INTEGRATION_SUMMARY.md` | Integration overview |
| `frontend-mobile/README.md` | Mobile app architecture |
| `frontend-mobile/MOBILE_APP_TESTING.md` | Testing guide |
| `frontend-mobile/INTEGRATION_COMPLETE.md` | Detailed integration info |
| `edge-services/README.md` | Edge Server architecture |

---

## ✅ TEST CHECKLIST

### Backend APIs

- [ ] Health check: `http://YOUR_IP:5001/health`
- [ ] Swagger UI: `http://YOUR_IP:5001/swagger`
- [ ] Login API returns tokens
- [ ] My tasks API returns data
- [ ] Crew profile API works

### Mobile App

- [ ] Login flow successful
- [ ] Dashboard shows stats from API
- [ ] My Tasks loads from API
- [ ] Task detail screen works
- [ ] Complete task updates database
- [ ] Profile shows crew info
- [ ] Certificates show with expiry colors
- [ ] Offline mode works
- [ ] Server configuration works

---

## 🎯 PRODUCTION READY CHECKLIST

### Security

- [ ] Implement proper JWT token generation
- [ ] Add password hashing (BCrypt)
- [ ] Enable HTTPS/SSL
- [ ] Add rate limiting
- [ ] Implement refresh token rotation

### Features

- [ ] Photo upload for tasks
- [ ] Push notifications
- [ ] Barcode/QR scanner
- [ ] Digital signature
- [ ] Export reports (PDF)

### Performance

- [ ] Background sync service
- [ ] Image caching & compression
- [ ] Pagination for large lists
- [ ] Database indexing
- [ ] API response caching

### Monitoring

- [ ] Error tracking (Sentry)
- [ ] Analytics (Firebase)
- [ ] Logging service
- [ ] Performance monitoring
- [ ] Crash reporting

---

## 📞 SUPPORT

Nếu gặp vấn đề:

1. Check logs trong terminal
2. Verify Edge Server status
3. Test APIs với Postman/curl
4. Check network connectivity
5. Review documentation above

---

**✅ INTEGRATION COMPLETE! READY FOR TESTING! 🚢⚓**

**Các bước tiếp theo:**
1. Run `START_EDGE_SERVER.bat`
2. Run `START_MOBILE_APP.bat`
3. Configure server URL in app
4. Login and test features
5. Build APK for real device testing
