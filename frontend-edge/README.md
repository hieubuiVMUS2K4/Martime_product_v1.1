# 🚢 Maritime Edge Dashboard - Frontend

## Tổng quan

**Maritime Edge Dashboard** là giao diện quản lý tàu (Vessel Operations) chạy **local trên tàu**, kết nối trực tiếp với **Edge Backend** qua mạng LAN. Dashboard này được thiết kế cho:

- **Thuyền trưởng (Master/Captain)**
- **Chief Engineer**  
- **Deck Officers**
- **Engine Room Personnel**

---

## 🎯 Chức năng chính

### 1. **Dashboard** - Tổng quan thời gian thực
- Tổng số alarms (critical, warning, info)
- Số thuyền viên onboard
- Nhiệm vụ bảo trì đang chờ
- Mức nhiên liệu hiện tại
- Trạng thái đồng bộ với shore

### 2. **Navigation** - Điều hướng và định vị
- Vị trí GPS real-time (Latitude, Longitude)
- Speed Over Ground (SOG), Course Over Ground (COG)
- Heading (True & Magnetic)
- Depth, Pitch, Roll
- Wind data (speed, direction)
- AIS targets (nearby vessels)

### 3. **Engine Room** - Giám sát máy móc
- Main Engine telemetry (RPM, Load, Temp, Pressure)
- Generators status (Running, Voltage, Frequency, Power)
- Tank levels (Fuel, Fresh Water, Ballast, Lube Oil)
- Fuel consumption tracking (IMO DCS compliance)

### 4. **Alarms & Safety**
- Active alarms (Critical, Warning, Info)
- Acknowledge/Resolve alarms
- Alarm history
- Fire, Bilge, Engine, Navigation alarms

### 5. **Crew Management** - Quản lý thuyền viên
- Danh sách thuyền viên onboard
- Chứng chỉ STCW (Certificate tracking)
- Medical certificates expiry
- Embark/Disembark dates

### 6. **Maintenance** - Bảo trì (ISM Code)
- Planned Maintenance System (PMS)
- Running hours-based tasks
- Calendar-based tasks
- Overdue tasks tracking
- Spare parts used

### 7. **Voyage & Cargo**
- Current voyage tracking
- Departure/Arrival ports & times
- Cargo operations (Loading/Discharging)
- Bill of Lading management

### 8. **Compliance** - Tuân thủ SOLAS/MARPOL
- **Watchkeeping Logs** (Bridge & Engine)
- **Oil Record Book** (MARPOL Annex I)
- **Cargo Records**

### 9. **Sync Status** - Trạng thái đồng bộ
- Pending records to sync
- Last sync timestamp
- Sync queue monitoring
- Manual sync trigger

---

## 📊 Database Tables (Đã migrate)

### Telemetry & Sensor Data
- `position_data` - GPS/GNSS position
- `navigation_data` - Heading, pitch, roll, depth
- `engine_data` - Engine telemetry
- `generator_data` - Generator status
- `tank_levels` - Tank levels monitoring
- `fuel_consumption` - Fuel tracking (IMO DCS)
- `environmental_data` - Weather, sea state
- `ais_data` - AIS targets
- `nmea_raw_data` - Raw NMEA sentences

### Operational Data
- `crew_members` - Crew management (STCW)
- `maintenance_tasks` - PMS (ISM Code)
- `cargo_operations` - Cargo loading/discharging
- `watchkeeping_logs` - Bridge/Engine watchkeeping
- `oil_record_books` - MARPOL compliance
- `voyage_records` - Voyage tracking
- `safety_alarms` - Alarms & alerts
- `sync_queue` - Data sync queue

---

## 🏗️ Cấu trúc thư mục

```
frontend-edge/
├── public/                 # Static assets
├── src/
│   ├── components/         # Reusable components
│   │   ├── layouts/        # Layout components
│   │   │   ├── MainLayout.tsx
│   │   │   ├── Sidebar.tsx
│   │   │   └── Header.tsx
│   │   ├── ui/             # UI primitives (buttons, cards, etc.)
│   │   └── widgets/        # Complex widgets
│   │
│   ├── pages/              # Page components
│   │   ├── Dashboard/
│   │   │   └── DashboardPage.tsx
│   │   ├── Navigation/
│   │   │   └── NavigationPage.tsx
│   │   ├── Engine/
│   │   │   └── EnginePage.tsx
│   │   ├── Alarms/
│   │   │   └── AlarmsPage.tsx
│   │   ├── Crew/
│   │   │   └── CrewPage.tsx
│   │   ├── Maintenance/
│   │   │   └── MaintenancePage.tsx
│   │   ├── Voyage/
│   │   │   └── VoyagePage.tsx
│   │   ├── Compliance/
│   │   │   └── CompliancePage.tsx
│   │   └── Sync/
│   │       └── SyncPage.tsx
│   │
│   ├── services/           # API services
│   │   ├── api.client.ts   # HTTP client
│   │   └── maritime.service.ts  # Maritime APIs
│   │
│   ├── stores/             # Zustand state management
│   │   └── maritime.store.ts
│   │
│   ├── types/              # TypeScript types
│   │   └── maritime.types.ts
│   │
│   ├── config/             # Configuration
│   │   └── app.config.ts
│   │
│   ├── styles/             # Global styles
│   │   └── globals.css
│   │
│   ├── App.tsx             # Main App component
│   └── main.tsx            # Entry point
│
├── index.html
├── package.json
├── tsconfig.json
├── vite.config.ts
├── tailwind.config.js
└── README.md
```

---

## 🚀 Cài đặt và chạy

### 1. Cài đặt dependencies

```powershell
cd frontend-edge
npm install
```

### 2. Cấu hình môi trường

Tạo file `.env` trong `frontend-edge/`:

```env
VITE_API_URL=http://localhost:5001/api
VITE_WS_URL=ws://localhost:5001/ws
VITE_VESSEL_ID=EDGE_LOCAL
VITE_VESSEL_NAME=MV Test Vessel
VITE_IMO_NUMBER=1234567
```

### 3. Chạy development server

```powershell
npm run dev
```

Frontend sẽ chạy tại: **http://localhost:3001**

### 4. Build production

```powershell
npm run build
```

---

## 🔌 Kết nối với Edge Backend

Frontend-edge kết nối với Edge Backend API (C# ASP.NET Core) qua:

- **HTTP API**: `http://localhost:5001/api`
- **WebSocket** (optional): `ws://localhost:5001/ws`

### API Endpoints cần implement trong Edge Backend:

```csharp
// Telemetry
GET /api/telemetry/position/latest
GET /api/telemetry/navigation/latest
GET /api/telemetry/engines
GET /api/telemetry/generators
GET /api/telemetry/tanks
GET /api/telemetry/fuel/consumption

// Alarms
GET /api/alarms/active
POST /api/alarms/{id}/acknowledge
POST /api/alarms/{id}/resolve

// Crew
GET /api/crew/onboard
POST /api/crew

// Maintenance
GET /api/maintenance/tasks/pending
POST /api/maintenance/tasks/{id}/complete

// Voyage
GET /api/voyages/current

// Sync
GET /api/sync/status
POST /api/sync/trigger

// Dashboard
GET /api/dashboard/stats
```

---

## 🎨 UI/UX Features

### Responsive Design
- Desktop-first (Bridge monitors, Engine room displays)
- Tablet support (Portable tablets on deck)
- Dark mode support

### Real-time Updates
- Auto-refresh every 5-15 seconds
- WebSocket for critical alarms
- Offline mode with local cache

### Maritime-specific
- Color coding by severity (Critical=Red, Warning=Yellow)
- Nautical units (knots, nautical miles, degrees)
- SOLAS/MARPOL compliance indicators

---

## 🔧 Technology Stack

- **React 19** - UI framework
- **TypeScript** - Type safety
- **Vite** - Build tool
- **React Router** - Routing
- **Zustand** - State management
- **Tailwind CSS** - Styling
- **Lucide React** - Icons
- **Recharts** - Charts & graphs
- **date-fns** - Date utilities

---

## 📝 Next Steps

### 1. Implement remaining pages:
- [ ] NavigationPage.tsx
- [ ] EnginePage.tsx
- [ ] AlarmsPage.tsx
- [ ] CrewPage.tsx
- [ ] MaintenancePage.tsx
- [ ] VoyagePage.tsx
- [ ] CompliancePage.tsx
- [ ] SyncPage.tsx

### 2. Add components:
- [ ] AlarmCard
- [ ] EngineGauge
- [ ] TankLevelIndicator
- [ ] CrewTable
- [ ] MaintenanceTaskCard
- [ ] VoyageTimeline

### 3. Implement Edge Backend API
- [ ] Controllers cho tất cả endpoints
- [ ] SignalR/WebSocket cho real-time updates
- [ ] Background services cho data collection

### 4. Testing
- [ ] Unit tests
- [ ] Integration tests với Edge Backend
- [ ] E2E tests

---

## 🛡️ Security

- Local network only (No internet exposure)
- JWT authentication (optional)
- Role-based access (Captain, Chief Engineer, Officer)
- Audit logging

---

## 📞 Support

Nếu có vấn đề, check:
1. Edge Backend đang chạy? (`http://localhost:5001`)
2. PostgreSQL Edge database connected?
3. Network connectivity (LAN)?
4. Browser console errors?

---

**Built with ⚓ for Maritime Operations**
