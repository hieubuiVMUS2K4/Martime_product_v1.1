# 🚀 QUICK REFERENCE - Maritime Edge System

## 📍 URLs

| Service | URL | Status |
|---------|-----|--------|
| **Frontend** | http://localhost:3002 | 🟢 RUNNING |
| **Backend API** | http://localhost:5001 | 🟢 RUNNING |
| **Swagger Docs** | http://localhost:5001/swagger | 🟢 AVAILABLE |
| **Database** | localhost:5433 (maritime_edge) | 🟢 CONNECTED |

---

## 🎯 Navigation

| Page | URL | Features |
|------|-----|----------|
| 🏠 **Dashboard** | `/` | Stats, Position, Telemetry |
| 👥 **Crew** | `/crew` | STCW Certificates, Crew Management |
| 🔧 **Maintenance** | `/maintenance` | PMS, Tasks, Calendar |
| 🧭 **Navigation** | `/navigation` | Position, Course, Speed |
| ⚙️ **Engine** | `/engine` | Engine Status, Generators |
| 🚨 **Alarms** | `/alarms` | Safety Alarms, Alerts |
| 🚢 **Voyage** | `/voyage` | Voyage Records, Cargo |
| 📋 **Compliance** | `/compliance` | Watchkeeping, ORB |
| 🔄 **Sync** | `/sync` | Data Sync Status |

---

## 🔧 Commands

### Start Backend API
```cmd
cd edge-services
dotnet run --urls "http://localhost:5001"
```

### Start Frontend
```cmd
cd frontend-edge
npm run dev
```

### Insert Sample Data
```cmd
# Using pgAdmin 4:
# Connect → maritime_edge → Query Tool → Open insert-sample-data.sql → Execute
```

### Database Connection
```
Host: localhost
Port: 5433
Database: maritime_edge
Username: edge_user
Password: ChangeMe_EdgePassword123!
```

---

## 📊 Sample Data

| Table | Count | Description |
|-------|-------|-------------|
| crew_members | 4 | Master, Engineers, Officers |
| maintenance_tasks | 4 | 1 overdue, 3 pending |
| safety_alarms | 3 | 1 critical, 2 warnings |
| position_data | 2 | GPS coordinates |
| engine_data | 2 | Main + Aux engines |
| generator_data | 3 | GEN1, GEN2, EMER |
| tank_levels | 6 | Fuel, Water, Lube Oil |

---

## 🎨 CSS Classes (Tailwind)

### Status Indicators
```css
.status-online      /* 🟢 Green pulsing */
.status-offline     /* 🔴 Red solid */
.status-syncing     /* 🟡 Yellow pulsing */
```

### Alarm Styles
```css
.alarm-critical     /* 🔴 Red background */
.alarm-warning      /* 🟡 Yellow background */
.alarm-info         /* 🔵 Blue background */
```

### Common Patterns
```tsx
<div className="bg-blue-500 text-white p-4 rounded-lg shadow-md hover:shadow-lg">
  Card with hover effect
</div>
```

---

## 🔑 API Endpoints

### Dashboard
```
GET /api/dashboard/stats
```

### Telemetry
```
GET /api/telemetry/position/latest
GET /api/telemetry/engines
GET /api/telemetry/generators
GET /api/telemetry/tanks
```

### Crew
```
GET /api/crew
GET /api/crew/onboard
GET /api/crew/expiring-certificates?days=90
POST /api/crew
PUT /api/crew/{id}
```

### Maintenance
```
GET /api/maintenance/tasks
GET /api/maintenance/tasks/pending
GET /api/maintenance/tasks/overdue
POST /api/maintenance/tasks/{id}/complete
```

### Alarms
```
GET /api/alarms/active
POST /api/alarms/{id}/acknowledge
POST /api/alarms/{id}/resolve
```

---

## 🐛 Troubleshooting

### Frontend won't start
```cmd
cd frontend-edge
npm install
npm run dev
```

### Backend won't start
```cmd
cd edge-services
dotnet restore
dotnet run --urls "http://localhost:5001"
```

### CSS errors in VS Code
```
Ctrl+Shift+P → "Developer: Reload Window"
```

### Port already in use
```
# Frontend will auto-switch to 3002
# Backend: Change port in dotnet run --urls
```

### CORS errors
```
Check edge-services/Program.cs
CORS policy "AllowFrontend" must include localhost:3002
```

---

## 📝 File Locations

### Frontend Structure
```
frontend-edge/
├── src/
│   ├── pages/
│   │   ├── Crew/CrewPage.tsx
│   │   ├── Maintenance/MaintenancePage.tsx
│   │   └── Dashboard/DashboardPage.tsx
│   ├── services/
│   │   ├── api.client.ts
│   │   └── maritime.service.ts
│   ├── types/
│   │   └── maritime.types.ts
│   └── styles/
│       └── globals.css
└── .vscode/
    ├── settings.json
    ├── css_custom_data.json
    └── extensions.json
```

### Backend Structure
```
edge-services/
├── Controllers/
│   ├── DashboardController.cs
│   ├── TelemetryController.cs
│   ├── AlarmsController.cs
│   ├── CrewController.cs
│   ├── MaintenanceController.cs
│   ├── VoyageController.cs
│   ├── ComplianceController.cs
│   └── SyncController.cs
├── Data/
│   └── EdgeDbContext.cs
├── Models/
│   └── EdgeModels.cs
└── Program.cs
```

---

## ✅ Quick Test Checklist

- [ ] Backend API running (http://localhost:5001)
- [ ] Frontend running (http://localhost:3002)
- [ ] Dashboard loads with stats
- [ ] Crew page shows 4 members
- [ ] Maintenance shows 4 tasks (1 overdue)
- [ ] Search & filters working
- [ ] No console errors (F12)
- [ ] CSS styling correct
- [ ] Tailwind classes applying

---

## 🎯 Maritime Standards

| Standard | Compliance | Feature |
|----------|-----------|---------|
| **STCW** | ✅ | Certificate tracking & expiry |
| **ISM Code** | ✅ | Planned Maintenance System |
| **MLC 2006** | ✅ | Crew welfare & records |
| **SOLAS** | ✅ | Safety equipment maintenance |
| **MARPOL** | ✅ | Oil Record Book compliance |

---

## 📞 Help

### Documentation Files
- `FINAL_STATUS_SUMMARY.md` - Complete system overview
- `CSS_ISSUES_FIXED.md` - CSS troubleshooting
- `CREW_MAINTENANCE_PAGES_COMPLETE.md` - Feature details
- `QUICK_TEST_GUIDE.md` - Testing instructions
- `API_SETUP_COMPLETE.md` - Backend setup
- `INSERT_DATA_GUIDE.md` - Sample data guide

### Check Logs
```
Backend: Check terminal running dotnet run
Frontend: F12 → Console tab
Database: pgAdmin Query Tool
```

---

**🎊 System Ready! Happy Maritime Operations! ⚓**
