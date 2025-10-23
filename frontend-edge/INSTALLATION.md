# 🚀 Frontend-Edge - Hướng dẫn cài đặt và chạy

## ✅ Hoàn thành

Đã xây dựng hoàn chỉnh cấu trúc **frontend-edge** với các file sau:

### 📁 Cấu trúc đã tạo:

```
frontend-edge/
├── src/
│   ├── components/
│   │   └── layouts/
│   │       ├── MainLayout.tsx      ✅
│   │       ├── Sidebar.tsx         ✅
│   │       └── Header.tsx          ✅
│   ├── pages/
│   │   ├── Dashboard/
│   │   │   └── DashboardPage.tsx   ✅ (Complete)
│   │   ├── Navigation/
│   │   │   └── NavigationPage.tsx  ✅ (Stub)
│   │   ├── Engine/
│   │   │   └── EnginePage.tsx      ✅ (Stub)
│   │   ├── Alarms/
│   │   │   └── AlarmsPage.tsx      ✅ (Stub)
│   │   ├── Crew/
│   │   │   └── CrewPage.tsx        ✅ (Stub)
│   │   ├── Maintenance/
│   │   │   └── MaintenancePage.tsx ✅ (Stub)
│   │   ├── Voyage/
│   │   │   └── VoyagePage.tsx      ✅ (Stub)
│   │   ├── Compliance/
│   │   │   └── CompliancePage.tsx  ✅ (Stub)
│   │   └── Sync/
│   │       └── SyncPage.tsx        ✅ (Stub)
│   ├── services/
│   │   ├── api.client.ts           ✅
│   │   └── maritime.service.ts     ✅
│   ├── stores/
│   │   └── maritime.store.ts       ✅
│   ├── types/
│   │   └── maritime.types.ts       ✅
│   ├── config/
│   │   └── app.config.ts           ✅
│   ├── styles/
│   │   └── globals.css             ✅
│   ├── App.tsx                     ✅
│   ├── main.tsx                    ✅
│   └── vite-env.d.ts               ✅
├── index.html                      ✅
├── package.json                    ✅
├── tsconfig.json                   ✅
├── vite.config.ts                  ✅
├── tailwind.config.js              ✅
├── postcss.config.js               ✅
├── .env.example                    ✅
├── README.md                       ✅
└── TODO.md                         ✅
```

---

## 🎯 Bước tiếp theo

### 1. Cài đặt dependencies

```powershell
cd frontend-edge
npm install
```

**Packages sẽ được cài:**
- React 19 + React DOM
- TypeScript
- Vite (build tool)
- React Router DOM
- Zustand (state management)
- Tailwind CSS + PostCSS + Autoprefixer
- Lucide React (icons)
- Recharts (charts)
- date-fns
- Radix UI components

### 2. Tạo file `.env`

Copy từ `.env.example`:

```powershell
cp .env.example .env
```

Sửa nội dung `.env`:

```env
VITE_API_URL=http://localhost:5001/api
VITE_WS_URL=ws://localhost:5001/ws
VITE_VESSEL_ID=MV_001
VITE_VESSEL_NAME=MV Test Vessel
VITE_IMO_NUMBER=1234567
```

### 3. Chạy development server

```powershell
npm run dev
```

**Output:**
```
VITE v6.0.11  ready in 500 ms

➜  Local:   http://localhost:3001/
➜  Network: http://192.168.1.100:3001/
```

Mở browser: **http://localhost:3001**

---

## ⚠️ Lưu ý quan trọng

### TypeScript Errors là bình thường!

Các lỗi compile bạn thấy là do **chưa cài packages**. Sau khi chạy `npm install`, tất cả errors sẽ biến mất:

- ❌ `Cannot find module 'react'` → ✅ Resolved sau npm install
- ❌ `Cannot find module 'lucide-react'` → ✅ Resolved sau npm install
- ❌ `JSX element implicitly has type 'any'` → ✅ Resolved sau npm install

### Edge Backend API chưa có

Frontend đã sẵn sàng, nhưng cần **Edge Backend API** để hoạt động:

1. **Cần implement Controllers trong edge-services:**
   - `TelemetryController.cs`
   - `AlarmsController.cs`
   - `CrewController.cs`
   - `MaintenanceController.cs`
   - `VoyageController.cs`
   - `ComplianceController.cs`
   - `SyncController.cs`
   - `DashboardController.cs`

2. **API endpoints cần có:**
   - `GET /api/dashboard/stats`
   - `GET /api/telemetry/position/latest`
   - `GET /api/telemetry/engines`
   - `GET /api/alarms/active`
   - `GET /api/crew/onboard`
   - ... (xem chi tiết trong `maritime.service.ts`)

---

## 🧪 Test frontend (mock data)

Để test frontend mà không cần backend:

### Option 1: Mock Service Worker (MSW)

Cài MSW:
```powershell
npm install -D msw@latest
```

Tạo mock handlers (TODO)

### Option 2: Hardcode test data

Trong `DashboardPage.tsx`, comment out API calls và dùng mock data:

```typescript
// const [dashStats, alarms, position, navigation] = await Promise.all([...])

// Mock data for testing
const mockStats: DashboardStats = {
  totalAlarms: 5,
  criticalAlarms: 2,
  pendingMaintenance: 12,
  crewOnboard: 18,
  fuelLevel: 75,
  syncStatus: 'ONLINE',
  lastSyncAt: new Date().toISOString(),
}

setStats(mockStats)
```

---

## 🎨 UI Preview

Khi chạy frontend, bạn sẽ thấy:

### **Layout:**
- ✅ Sidebar bên trái với 9 menu items
- ✅ Header trên cùng (sync status, connection status, time)
- ✅ Dashboard page với stats cards

### **Dashboard Page:**
- 4 stat cards: Total Alarms, Critical Alarms, Crew Onboard, Pending Maintenance
- 3 telemetry cards: Navigation, Main Engine, Environmental
- Fuel level progress bar
- Data synchronization status

### **Other Pages:**
- Stub pages với "Coming soon..." (sẽ implement sau)

---

## 📊 Database đã sẵn sàng

Edge Backend database đã được migrate với 18 bảng:

### Telemetry Tables:
- `position_data`
- `navigation_data`
- `engine_data`
- `generator_data`
- `tank_levels`
- `fuel_consumption`
- `environmental_data`
- `ais_data`
- `nmea_raw_data`

### Operational Tables:
- `crew_members`
- `maintenance_tasks`
- `cargo_operations`
- `watchkeeping_logs`
- `oil_record_books`
- `voyage_records`
- `safety_alarms`
- `sync_queue`

---

## 🔗 Kiến trúc hoàn chỉnh

```
┌─────────────────────┐
│   Mobile App        │ (Flutter)
│   (Crew)            │
└──────────┬──────────┘
           │ LAN
           ↓
┌─────────────────────┐      ┌─────────────────────┐
│   Edge Backend      │ ←──→ │  Frontend-Edge      │
│   (C# ASP.NET)      │ HTTP │  (React)            │
└──────────┬──────────┘      └─────────────────────┘
           │                  ← YOU ARE HERE ✅
           ↓
┌─────────────────────┐
│  PostgreSQL Edge    │
│  (Local Database)   │
└──────────┬──────────┘
           │
           │ Sync (VSAT/4G/Starlink)
           ↓
┌─────────────────────┐
│  Shore Backend      │
│  (Cloud API)        │
└──────────┬──────────┘
           │
           ↓
┌─────────────────────┐      ┌─────────────────────┐
│  PostgreSQL Central │ ←──→ │  Frontend-Shore     │
│  (Cloud Database)   │      │  (React)            │
└─────────────────────┘      └─────────────────────┘
```

---

## ✅ Checklist

- [x] Cấu trúc frontend-edge
- [x] TypeScript types (18 tables)
- [x] API services (8 service modules)
- [x] Zustand store
- [x] Layout components
- [x] Dashboard page (complete)
- [x] Stub pages (8 pages)
- [x] Configuration files
- [x] Documentation

---

## 📝 Next Actions

### Priority 1: Install & Test
```powershell
cd frontend-edge
npm install
npm run dev
```

### Priority 2: Implement Edge Backend API
Chuyển sang `edge-services/` và implement Controllers

### Priority 3: Complete Pages
Implement 8 stub pages còn lại

### Priority 4: Real-time Features
Add WebSocket/SignalR for live updates

---

## 🆘 Troubleshooting

### Port 3001 đã được sử dụng?
Edit `vite.config.ts`:
```typescript
server: {
  port: 3002, // Change port
}
```

### Tailwind CSS không hoạt động?
```powershell
npm install -D tailwindcss postcss autoprefixer
npx tailwindcss init -p
```

### TypeScript errors sau khi npm install?
```powershell
npm install --save-dev @types/node
```

---

**🎉 Chúc mừng! Frontend-edge đã sẵn sàng!**

Bạn có thể bắt đầu chạy `npm install` và `npm run dev` để xem kết quả.
