# 🏗️ TWO-BACKEND ARCHITECTURE - Maritime Management System

## 🎯 Tổng Quan Kiến Trúc Hai Backend

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         MARITIME MANAGEMENT SYSTEM                           │
│                                                                               │
│  ┌───────────────────────────────────┐  ┌────────────────────────────────┐ │
│  │     EDGE BACKEND (Trên Tàu)       │  │   SHORE BACKEND (Đất Liền)     │ │
│  │                                   │  │                                 │ │
│  │  🚢 Offline-First Architecture    │  │  🏢 Cloud-Based Architecture   │ │
│  │  📍 Location: Ship                │  │  📍 Location: Data Center      │ │
│  │  🔌 Network: Unstable/Intermittent│  │  🔌 Network: Stable/Fast       │ │
│  └───────────────────────────────────┘  └────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 📊 So Sánh Chi Tiết Hai Backend

| Tiêu Chí | **Edge Backend (Tàu)** | **Shore Backend (Bờ)** |
|----------|------------------------|------------------------|
| **Technology Stack** | ASP.NET Core 8.0 + EF Core | ASP.NET Core 8.0 + EF Core |
| **Database** | PostgreSQL 15 (Local) | PostgreSQL 15 (Central) + Redis |
| **Primary Users** | Thuyền viên (Mobile App)<br/>Thuyền trưởng (Web Local) | Admin<br/>Head Office<br/>Fleet Managers |
| **Network Dependency** | ❌ Must work OFFLINE | ✅ Always online |
| **Data Volume** | Single ship | Multiple ships (fleet) |
| **Data Retention** | 30-90 days (rolling) | Unlimited (archival) |
| **API Purpose** | Internal ship operations | Fleet management & reporting |
| **Authentication** | Local user accounts<br/>(No external auth) | JWT + OAuth2<br/>(SSO support) |
| **Deployment** | Ruggedized PC on ship<br/>(Windows/Linux) | Azure/AWS/On-Premise<br/>(Docker/Kubernetes) |
| **Backup Strategy** | Daily to USB/NAS | Hourly to cloud storage |
| **Performance** | Low latency (LAN) | Medium latency (Internet) |
| **Scalability** | Single instance | Horizontal scaling |
| **Security** | Physical access control | Multi-layer security |

---

## 🚢 EDGE BACKEND - Ship Operations

### **Mục Đích Hoạt Động:**
1. ✅ **Thu thập dữ liệu liên tục** từ sensors (GPS, engine, fuel, AIS)
2. ✅ **Lưu trữ local** trong PostgreSQL dù không có mạng
3. ✅ **Phục vụ Mobile App** cho thuyền viên nhập liệu (công việc, bảo trì, vật tư)
4. ✅ **Phục vụ Web App** cho thuyền trưởng quản lý và duyệt dữ liệu
5. ✅ **Tự động sync** data về shore khi có kết nối
6. ✅ **Alert real-time** cho tình huống nguy hiểm (engine overheat, fire, etc.)

### **Kiến Trúc Edge Backend:**

```
┌──────────────────────────────────────────────────────────────────┐
│                    TÀU - EDGE BACKEND                             │
├──────────────────────────────────────────────────────────────────┤
│                                                                   │
│  📱 CLIENTS (Trên tàu)                                           │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐             │
│  │ Mobile App  │  │  Web Local  │  │  Sensors    │             │
│  │ (Flutter)   │  │  (React)    │  │ NMEA/Modbus │             │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘             │
│         │                 │                 │                     │
│         │    WiFi LAN     │                 │                     │
│         └────────┬────────┘                 │                     │
│                  │                          │                     │
│  ┌───────────────▼──────────────────────────▼──────────────┐    │
│  │          ASP.NET Core Edge API                           │    │
│  │  Port: 5000 (HTTP) / 5001 (HTTPS)                       │    │
│  │                                                           │    │
│  │  📍 API Endpoints:                                       │    │
│  │  ┌──────────────────────────────────────────────────┐   │    │
│  │  │ LOCAL OPERATIONS (Không cần internet)            │   │    │
│  │  ├──────────────────────────────────────────────────┤   │    │
│  │  │ POST /api/crew/login                             │   │    │
│  │  │ GET  /api/crew/current-shift                     │   │    │
│  │  │ POST /api/maintenance/log                        │   │    │
│  │  │ POST /api/work-assignment/complete               │   │    │
│  │  │ GET  /api/inventory/items                        │   │    │
│  │  │ POST /api/inventory/consume                      │   │    │
│  │  │ GET  /api/dashboard/ship-status                  │   │    │
│  │  │ GET  /api/telemetry/latest (GPS, engine, fuel)  │   │    │
│  │  │ POST /api/alarms/acknowledge                     │   │    │
│  │  └──────────────────────────────────────────────────┘   │    │
│  │                                                           │    │
│  │  🔌 Background Services:                                 │    │
│  │  ┌──────────────────────────────────────────────────┐   │    │
│  │  │ • NmeaCollectorService (Serial COM1-4)           │   │    │
│  │  │ • ModbusCollectorService (Engine telemetry)      │   │    │
│  │  │ • AisDecoderService (Vessel tracking)            │   │    │
│  │  │ • AlertEngineService (Safety monitoring)         │   │    │
│  │  │ • SyncService (Ship→Shore upload)                │   │    │
│  │  └──────────────────────────────────────────────────┘   │    │
│  └───────────────────────┬──────────────────────────────────┘    │
│                          │                                        │
│  ┌───────────────────────▼──────────────────────────────────┐    │
│  │          PostgreSQL Edge Database (Local)                 │    │
│  │  Container: maritime-edge-postgres:5432                   │    │
│  │                                                            │    │
│  │  📊 Tables (12 tables):                                   │    │
│  │  - position_data (GPS tracks)                             │    │
│  │  - ais_data (Nearby vessels)                              │    │
│  │  - engine_data (Main engine + generators)                 │    │
│  │  - fuel_consumption (IMO DCS compliance)                  │    │
│  │  - tank_levels (Fuel, water, ballast)                     │    │
│  │  - navigation_data (Gyro, depth, wind)                    │    │
│  │  - environmental_data (Weather, sea state)                │    │
│  │  - safety_alarms (SOLAS alerts)                           │    │
│  │  - nmea_raw_data (Raw sentences for audit)                │    │
│  │  - voyage_records (Departure/arrival)                     │    │
│  │  - sync_queue (Store-and-forward buffer)                  │    │
│  │  - crew_members (Local user accounts)                     │    │
│  │  - maintenance_logs (Equipment maintenance)               │    │
│  │  - inventory_items (Spare parts, consumables)             │    │
│  │                                                            │    │
│  │  🔑 Key: is_synced BOOLEAN (track upload status)         │    │
│  └────────────────────────────────────────────────────────────┘   │
│                          │                                        │
│  ┌───────────────────────▼──────────────────────────────────┐    │
│  │            SyncService (Background Worker)                │    │
│  │                                                            │    │
│  │  While (true):                                            │    │
│  │    1. Check internet connectivity                         │    │
│  │    2. If ONLINE:                                          │    │
│  │       - Fetch records WHERE is_synced = false             │    │
│  │       - Compress (gzip) → Batch 500 records               │    │
│  │       - HTTPS POST to Shore API                           │    │
│  │       - If success: UPDATE is_synced = true               │    │
│  │       - If fail: Retry with backoff                       │    │
│  │    3. Sleep(5 minutes)                                    │    │
│  └────────────────────────────────────────────────────────────┘   │
│                          │                                        │
│                          │ VSAT/4G/Starlink                      │
│                          │ (Intermittent connection)              │
└──────────────────────────┼────────────────────────────────────────┘
                           │
                           │ HTTPS/TLS 1.3
                           │ JWT Token Auth
                           │ Compressed JSON
                           │
                           ▼
```

### **Edge Backend - Tech Stack:**
```csharp
// Startup.cs hoặc Program.cs
var builder = WebApplication.CreateBuilder(args);

// 1. Database
builder.Services.AddDbContext<EdgeDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("EdgeDatabase")));

// 2. Background Services
builder.Services.AddHostedService<NmeaCollectorService>();
builder.Services.AddHostedService<ModbusCollectorService>();
builder.Services.AddHostedService<SyncService>(); // Ship→Shore sync
builder.Services.AddHostedService<AlertEngineService>();

// 3. Local Authentication (không cần external provider)
builder.Services.AddAuthentication("LocalAuth")
    .AddCookie("LocalAuth", options => {
        options.LoginPath = "/api/auth/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(12); // 12-hour shift
    });

// 4. CORS cho Mobile App (same LAN)
builder.Services.AddCors(options => {
    options.AddPolicy("LocalNetwork", builder => {
        builder.WithOrigins("http://192.168.1.0/24") // Ship LAN
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// 5. Serial Port Communication
builder.Services.AddSingleton<ISerialPortManager, SerialPortManager>();
builder.Services.AddSingleton<INmeaParser, NmeaParser>();

// 6. Modbus Communication
builder.Services.AddSingleton<IModbusFactory, ModbusFactory>();

var app = builder.Build();

// Run migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EdgeDbContext>();
    db.Database.Migrate(); // Auto-apply EF Core migrations
}

app.UseHttpsRedirection();
app.UseCors("LocalNetwork");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run("http://0.0.0.0:5000"); // Listen on all interfaces
```

---

## 🏢 SHORE BACKEND - Fleet Management

### **Mục Đích Hoạt Động:**
1. ✅ **Nhận dữ liệu** từ nhiều tàu qua sync API
2. ✅ **Tổng hợp và phân tích** data toàn fleet
3. ✅ **Cung cấp dashboard** cho Head Office, Admin
4. ✅ **Tạo báo cáo** (IMO DCS, EU MRV, fuel efficiency)
5. ✅ **Quản lý tập trung** (users, vessels, permissions)
6. ✅ **Alerts tập trung** từ tất cả tàu
7. ✅ **API cho external systems** (ERP, third-party monitoring)

### **Kiến Trúc Shore Backend:**

```
┌──────────────────────────────────────────────────────────────────┐
│                  ĐẤT LIỀN - SHORE BACKEND                        │
├──────────────────────────────────────────────────────────────────┤
│                                                                   │
│  🌐 CLIENTS (Internet)                                           │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐             │
│  │ Admin Web   │  │ Head Office │  │ Ship Edge   │             │
│  │ Dashboard   │  │   Portal    │  │  Sync API   │             │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘             │
│         │                 │                 │                     │
│         │    HTTPS/TLS    │                 │                     │
│         └────────┬────────┴─────────────────┘                     │
│                  │                                                │
│  ┌───────────────▼──────────────────────────────────────────┐    │
│  │          ASP.NET Core Shore API                           │    │
│  │  Port: 443 (HTTPS) - Public Internet                     │    │
│  │                                                            │    │
│  │  📍 API Endpoints:                                        │    │
│  │  ┌──────────────────────────────────────────────────┐    │    │
│  │  │ FLEET MANAGEMENT (Cần authentication)            │    │    │
│  │  ├──────────────────────────────────────────────────┤    │    │
│  │  │ GET  /api/fleet/vessels (Danh sách tàu)          │    │    │
│  │  │ GET  /api/fleet/vessel/{id}/status               │    │    │
│  │  │ GET  /api/fleet/map (Realtime map view)          │    │    │
│  │  │ GET  /api/reports/fuel-consumption               │    │    │
│  │  │ GET  /api/reports/imo-dcs                        │    │    │
│  │  │ GET  /api/alerts/active (Tất cả tàu)             │    │    │
│  │  │ GET  /api/analytics/kpi-dashboard                │    │    │
│  │  └──────────────────────────────────────────────────┘    │    │
│  │                                                            │    │
│  │  ┌──────────────────────────────────────────────────┐    │    │
│  │  │ SYNC API (Ship→Shore data ingestion)             │    │    │
│  │  ├──────────────────────────────────────────────────┤    │    │
│  │  │ POST /api/sync/position (Batch GPS data)         │    │    │
│  │  │ POST /api/sync/engine-data                       │    │    │
│  │  │ POST /api/sync/fuel-consumption                  │    │    │
│  │  │ POST /api/sync/alarms                            │    │    │
│  │  │ POST /api/sync/voyage-records                    │    │    │
│  │  │ POST /api/sync/maintenance-logs                  │    │    │
│  │  │ POST /api/sync/inventory-transactions            │    │    │
│  │  │                                                   │    │    │
│  │  │ 🔒 Auth: JWT Bearer Token (from ship)           │    │    │
│  │  │ 📦 Body: gzip-compressed JSON batch              │    │    │
│  │  │ ✅ Response: { received: 500, timestamp: ... }  │    │    │
│  │  └──────────────────────────────────────────────────┘    │    │
│  │                                                            │    │
│  │  ┌──────────────────────────────────────────────────┐    │    │
│  │  │ ADMIN API (User management)                      │    │    │
│  │  ├──────────────────────────────────────────────────┤    │    │
│  │  │ POST /api/admin/users/create                     │    │    │
│  │  │ PUT  /api/admin/users/{id}/permissions           │    │    │
│  │  │ POST /api/admin/vessels/register                 │    │    │
│  │  │ GET  /api/admin/audit-logs                       │    │    │
│  │  └──────────────────────────────────────────────────┘    │    │
│  └────────────────────────┬──────────────────────────────────┘   │
│                           │                                       │
│  ┌────────────────────────▼─────────────────────────────────┐    │
│  │      PostgreSQL Shore Database (Central)                 │    │
│  │  Host: RDS/Cloud SQL/Azure Database                      │    │
│  │                                                            │    │
│  │  📊 Tables (Extended schema):                             │    │
│  │  ┌─────────────────────────────────────────────────┐     │    │
│  │  │ VESSELS TABLE (Master data)                     │     │    │
│  │  ├─────────────────────────────────────────────────┤     │    │
│  │  │ - id (PK)                                       │     │    │
│  │  │ - imo_number (UNIQUE)                           │     │    │
│  │  │ - mmsi (UNIQUE)                                 │     │    │
│  │  │ - vessel_name                                   │     │    │
│  │  │ - vessel_type (cargo, tanker, passenger...)     │     │    │
│  │  │ - flag_state                                    │     │    │
│  │  │ - last_position_update                          │     │    │
│  │  │ - connection_status (ONLINE/OFFLINE)            │     │    │
│  │  │ - api_key_hash (for ship authentication)        │     │    │
│  │  └─────────────────────────────────────────────────┘     │    │
│  │                                                            │    │
│  │  ┌─────────────────────────────────────────────────┐     │    │
│  │  │ TELEMETRY TABLES (với vessel_id FK)             │     │    │
│  │  ├─────────────────────────────────────────────────┤     │    │
│  │  │ - position_data (vessel_id, timestamp, lat...)  │     │    │
│  │  │ - engine_data (vessel_id, engine_id, rpm...)    │     │    │
│  │  │ - fuel_consumption (vessel_id, fuel_type...)    │     │    │
│  │  │ - safety_alarms (vessel_id, alarm_type...)      │     │    │
│  │  │ - voyage_records (vessel_id, voyage_number...)  │     │    │
│  │  │                                                  │     │    │
│  │  │ 🔑 Additional columns:                          │     │    │
│  │  │   + vessel_id (FK to vessels table)             │     │    │
│  │  │   + received_at (server timestamp)              │     │    │
│  │  │   + data_source (EDGE/MANUAL/API)               │     │    │
│  │  └─────────────────────────────────────────────────┘     │    │
│  │                                                            │    │
│  │  ┌─────────────────────────────────────────────────┐     │    │
│  │  │ USERS & PERMISSIONS                              │     │    │
│  │  ├─────────────────────────────────────────────────┤     │    │
│  │  │ - users (id, email, role, vessel_id)            │     │    │
│  │  │ - roles (ADMIN, FLEET_MANAGER, SHIP_MANAGER)    │     │    │
│  │  │ - permissions (user_id, resource, action)       │     │    │
│  │  │ - audit_logs (user_id, action, timestamp)       │     │    │
│  │  └─────────────────────────────────────────────────┘     │    │
│  └────────────────────────────────────────────────────────────┘   │
│                           │                                       │
│  ┌────────────────────────▼─────────────────────────────────┐    │
│  │              Redis Cache                                  │    │
│  │  - Latest vessel positions (TTL: 10 minutes)              │    │
│  │  - Active alarms count                                    │    │
│  │  - Session tokens                                         │    │
│  │  - Rate limiting counters                                 │    │
│  └────────────────────────────────────────────────────────────┘   │
│                                                                   │
│  ┌─────────────────────────────────────────────────────────┐     │
│  │       Background Jobs (Hangfire/Quartz.NET)              │     │
│  │                                                           │     │
│  │  • DailyReportGenerator (IMO DCS, EU MRV)                │     │
│  │  • VesselConnectionMonitor (detect offline ships)        │     │
│  │  • DataAggregationJob (hourly summaries)                 │     │
│  │  • AlertNotificationJob (email/SMS for critical alarms)  │     │
│  │  • DatabaseCleanupJob (archive old data)                 │     │
│  └─────────────────────────────────────────────────────────┘     │
└──────────────────────────────────────────────────────────────────┘
```

### **Shore Backend - Tech Stack:**
```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// 1. Central Database
builder.Services.AddDbContext<ShoreDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ShoreDatabase"))
           .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)); // Performance

// 2. Redis Cache
builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "MaritimeShore:";
});

// 3. JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
            )
        };
    });

// 4. Authorization Policies
builder.Services.AddAuthorization(options => {
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("ADMIN"));
    options.AddPolicy("FleetManager", policy => policy.RequireRole("ADMIN", "FLEET_MANAGER"));
    options.AddPolicy("ShipSync", policy => policy.RequireClaim("ship_mmsi")); // For edge sync
});

// 5. Background Jobs
builder.Services.AddHangfire(config => {
    config.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("ShoreDatabase"));
});
builder.Services.AddHangfireServer();

// 6. SignalR for real-time updates
builder.Services.AddSignalR();

// 7. HTTP Client for external APIs
builder.Services.AddHttpClient<ImoReportingClient>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<FleetMonitoringHub>("/hubs/fleet"); // Real-time dashboard

// Schedule background jobs
RecurringJob.AddOrUpdate<DailyReportGenerator>(
    "daily-imo-report", 
    job => job.GenerateReport(), 
    Cron.Daily(2) // 2 AM UTC
);

app.Run();
```

---

## 🔄 Tương Tác Giữa Hai Backend

### **Scenario 1: Thuyền viên nhập dữ liệu bảo trì**

```
┌──────────────┐         ┌──────────────┐         ┌──────────────┐
│ Mobile App   │  WiFi   │ Edge Backend │  HTTPS  │Shore Backend │
│  (Tàu)       │────────>│   (Tàu)      │────────>│  (Đất liền)  │
└──────────────┘         └──────────────┘         └──────────────┘

1. [09:00] Thuyền viên mở app, login local
   POST /api/auth/login → Edge Backend
   Response: { token, crew_name, shift }

2. [09:15] Nhập maintenance log
   POST /api/maintenance/log
   Body: { equipment_id, task, hours_spent }
   → Lưu vào Edge PostgreSQL
   → is_synced = FALSE

3. [09:30] Tàu có VSAT connection
   → SyncService (background) tự động chạy
   → Fetch maintenance logs WHERE is_synced = FALSE
   → Compress + POST to Shore Backend
   POST /api/sync/maintenance-logs
   Headers: { Authorization: Bearer <ship_jwt> }
   Body: gzip([{ equipment_id, task, ... }])

4. [09:31] Shore Backend nhận data
   → Decompress
   → Validate JWT (identify ship: vessel_id)
   → Insert to shore_maintenance_logs table
   → Return { received: 1, timestamp: ... }

5. [09:32] Edge Backend nhận response success
   → UPDATE maintenance_logs SET is_synced = TRUE
   → Log metrics

6. [10:00] Head Office xem dashboard
   GET /api/reports/maintenance-summary
   → Shore Backend query tất cả tàu
   → Return aggregated data
```

### **Scenario 2: Engine alarm triggered**

```
1. [14:23:45] Engine sensor detect overheat
   → ModbusCollectorService (Edge) đọc register
   → Temperature = 105°C (threshold: 95°C)

2. [14:23:46] AlertEngineService (Edge) process
   → Insert to safety_alarms table
   → severity = CRITICAL
   → is_synced = FALSE
   → Notify local bridge display (WebSocket)

3. [14:23:47] SyncService detect critical alarm
   → Priority 1 (IMMEDIATE sync)
   → Bypass normal 5-minute interval
   → POST /api/sync/alarms to Shore Backend

4. [14:23:50] Shore Backend receive alarm
   → Insert to central safety_alarms table
   → vessel_id = identify from JWT
   → Trigger AlertNotificationJob
   → Send email to Fleet Manager
   → Send SMS to Ship Manager
   → Push to dashboard via SignalR

5. [14:24:00] Head Office dashboard updates
   → Red alert banner appears
   → "MV SAMPLE SHIP - Engine Overheat - 105°C"
   → Action buttons: Call Ship / View Details
```

### **Scenario 3: Fleet Manager xem fuel efficiency**

```
1. [16:00] Fleet Manager login to shore dashboard
   POST /api/auth/login
   → Shore Backend validate credentials
   → Return JWT token

2. [16:01] Access fuel analytics page
   GET /api/analytics/fuel-efficiency?vessels=all&period=30days
   → Shore Backend query position_data + fuel_consumption
   → Join with vessels table
   → Calculate:
     - Distance traveled per vessel
     - Fuel consumed per vessel
     - Efficiency (MT/NM)
     - Trend analysis
   → Return aggregated JSON

3. [16:02] Dashboard renders chart
   → Bar chart: Fuel efficiency per vessel
   → Line chart: Trend over 30 days
   → Red highlight: Vessels below target efficiency

4. [16:05] Drill down to specific vessel
   GET /api/fleet/vessel/12345/fuel-details
   → Shore Backend query specific vessel
   → Include last position, engine RPM, speed
   → Return detailed breakdown

5. [16:10] Export IMO DCS report
   GET /api/reports/imo-dcs?vessel=12345&year=2025
   → Shore Backend aggregate annual data
   → Generate PDF report
   → Email to Fleet Manager
```

---

## 🔑 Key Differences Summary

| Aspect | Edge Backend | Shore Backend |
|--------|-------------|---------------|
| **Data Flow** | **INPUT**: Sensors + Users<br/>**OUTPUT**: Shore sync | **INPUT**: Ships sync<br/>**OUTPUT**: Reports & analytics |
| **Critical Feature** | ✅ **Offline operation** | ✅ **Multi-vessel aggregation** |
| **Failure Impact** | ❌ Single ship affected | ❌ Entire fleet affected |
| **Update Frequency** | Real-time (sensors: 1/sec) | Batch (sync: 5-15 min) |
| **API Latency** | <50ms (LAN) | 200-500ms (Internet) |
| **User Concurrency** | 5-20 crew members | 100+ users (fleet) |
| **Database Size** | ~10-50 GB (30-90 days) | ~1-10 TB (unlimited) |
| **Backup Frequency** | Daily to USB/NAS | Hourly to S3/Azure Blob |
| **High Availability** | ⚠️ Single instance (ship PC) | ✅ Load balanced (3+ instances) |

---

## 📋 Development Checklist

### **Edge Backend:**
- [ ] Implement local authentication (no external deps)
- [ ] EF Core migrations for edge schema
- [ ] Serial port communication (NMEA parser)
- [ ] Modbus RTU/TCP client
- [ ] SyncService with retry logic
- [ ] Offline-first API design
- [ ] Local data retention policy (auto-cleanup after 90 days)
- [ ] Docker Compose for local PostgreSQL

### **Shore Backend:**
- [ ] JWT authentication with role-based access
- [ ] Sync API endpoints (position, engine, fuel, alarms, etc.)
- [ ] gzip decompression middleware
- [ ] Vessel identification from JWT claims
- [ ] Multi-tenancy (vessel_id FK on all tables)
- [ ] Redis caching for real-time queries
- [ ] Background jobs (reports, notifications, cleanup)
- [ ] SignalR hub for dashboard updates
- [ ] API documentation (Swagger/OpenAPI)
- [ ] Rate limiting (prevent abuse)
- [ ] Audit logging (who did what, when)

---

*Two-Backend Architecture - Distributed, Resilient, Scalable* 🚢🏢📡
