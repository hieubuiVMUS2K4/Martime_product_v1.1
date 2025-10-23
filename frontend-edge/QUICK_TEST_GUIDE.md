# 🎯 QUICK START GUIDE - Crew & Maintenance Pages

## 🚀 Chạy hệ thống ngay

### Bước 1: Start Edge API
```cmd
cd edge-services
dotnet run --urls "http://localhost:5001"
```
✅ API running at: http://localhost:5001
✅ Swagger: http://localhost:5001/swagger

### Bước 2: Insert Sample Data
Mở **pgAdmin 4**:
1. Connect: localhost:5433, database: `maritime_edge`
2. Query Tool → Open `insert-sample-data.sql`
3. Execute (F5)

**Sample data includes:**
- 4 crew members (all onboard)
- 1 certificate expiring in 90 days
- 4 maintenance tasks (1 overdue)

### Bước 3: Start Frontend
```cmd
cd frontend-edge
npm run dev
```
✅ Frontend: http://localhost:3001

---

## 👥 CREW MANAGEMENT PAGE

**URL:** http://localhost:3001/crew

### Tab 1: Onboard Crew
![Onboard Crew](Screenshot would show crew table)

**Features:**
- Full crew roster với photos (initials)
- STCW Certificate status:
  - 🟢 Valid (>90 days)
  - 🟡 Warning (30-90 days)  
  - 🔴 Critical (<30 days)
- Medical certificate tracking
- Search by name/position/crew ID
- Filter by rank (Officer/Rating)

**Sample Data Visible:**
```
✅ John Smith - Master (Officer)
   STCW: Expires in 180 days ✅
   Medical: Expires in 120 days ✅

⚠️ Wei Chen - Chief Officer (Officer)  
   STCW: Expires in 90 days ⚠️
   Medical: Expires in 60 days ⚠️

✅ Maria Garcia - Chief Engineer (Officer)
✅ Ahmed Hassan - 2nd Engineer (Officer)
```

### Tab 2: Certificate Monitor
![Certificate Monitor](Screenshot would show expiring certificates)

**Features:**
- Priority list sorted by expiry date
- Cards for each crew member với expiring certs
- Action buttons:
  - 📧 Send Reminder
  - 📝 Update Certificate
- Visual countdown với days remaining
- Separate display cho STCW & Medical

**Alert Banner:**
```
⚠️ Certificate Expiry Alert
1 crew member(s) have certificates expiring within 90 days.
Ensure timely renewals to maintain STCW compliance.
```

### Tab 3: Reports
![Reports](Screenshot would show statistics)

**Statistics:**
- Total Crew: 4
- Onboard: 4
- Officers: 4
- Ratings: 0

**Certificate Status:**
- ✅ Valid: 3
- ⚠️ Expiring Soon: 1
- ❌ Expired: 0

**Export Options:**
- 📄 Export Crew List (PDF)
- 📊 Export Certificate Report (Excel)

---

## 🔧 MAINTENANCE PAGE

**URL:** http://localhost:3001/maintenance

### Tab 1: Pending Tasks
![Pending Tasks](Screenshot would show task list)

**Sample Tasks Visible:**

**1. Main Engine - Lube Oil Filter Change**
```
🔧 MAIN_ENGINE
Priority: 🔴 HIGH
Status: 🟡 PENDING

Next Due: [Date] (50 days left)
Last Done: 450 hours ago
Interval: 500 running hours
Assigned: Chief Engineer

[Mark Complete] [Start Task] [Edit]
```

**2. Generator 1 - Fuel Injector Inspection**
```
🔧 GEN_1  
Priority: 🔵 NORMAL
Status: 🟡 PENDING

Next Due: [Date] (200 days left)
Last Done: 800 hours ago
Interval: 1000 running hours
Assigned: Not assigned

[Mark Complete] [Start Task] [Edit]
```

### Tab 2: Overdue Tasks
![Overdue Tasks](Screenshot would show red alerts)

**Sample Overdue:**

**Lifeboat Port - Monthly Test**
```
🔧 LIFEBOAT_1
Priority: 🔴 CRITICAL  
Status: 🔴 OVERDUE

Next Due: 2 days ago (OVERDUE!)
Last Done: 28 days ago
Interval: 30 calendar days
Assigned: Deck Officer

⚠️ This is a SOLAS critical task!

[Mark Complete] [Start Task] [Edit]
```

### Tab 3: Calendar View
![Calendar](Screenshot would show 30-day schedule)

**This Week:**
- Fire Pump Test (1 day) - HIGH
- Lifeboat Test (OVERDUE!) - CRITICAL

**Next Week:**
- Main Engine Oil Filter (7 days) - HIGH

**Week 3:**
- [Other scheduled tasks]

---

## 🎨 UI Features Demonstrated

### Color Coding System
- 🔴 **Red**: Expired, overdue, critical priority
- 🟡 **Yellow**: Warning, expiring soon, in progress
- 🟢 **Green**: Valid, completed, normal
- 🔵 **Blue**: Active, pending, info
- ⚫ **Gray**: Inactive, unassigned

### Interactive Elements
- ✅ **Hover effects** on cards (shadow-md)
- 🔍 **Real-time search** (no page reload)
- 🎯 **Filter dropdowns** (rank, priority, equipment)
- 📊 **Statistics cards** with icons
- 🔘 **Tab navigation** with active states
- 🔔 **Alert banners** for warnings

### Responsive Design
- ✅ Mobile-friendly (grid breakpoints)
- ✅ Touch targets >44px
- ✅ Scrollable tables
- ✅ Collapsible sections

---

## 📊 Dashboard Stats (Homepage)

**URL:** http://localhost:3001

**Statistics Updated:**
- 👥 **Crew Onboard**: 4
- ⚠️ **Active Alarms**: 2 (1 critical)
- 🔧 **Pending Maintenance**: 3
- ⛽ **Fuel Level**: 75%
- 🔄 **Sync Status**: OFFLINE

---

## ✅ Test Checklist

### Crew Management Tests:
- [ ] View all onboard crew (should show 4 members)
- [ ] Search for "Wei Chen" (should filter to 1 result)
- [ ] Filter by "Officer" rank (should show all 4)
- [ ] Check Certificate Monitor (should show 1 expiring)
- [ ] View Reports statistics (4 total, 3 valid certs)

### Maintenance Tests:
- [ ] View Pending Tasks (should show 3 tasks)
- [ ] View Overdue Tasks (should show 1 overdue lifeboat)
- [ ] Filter by "CRITICAL" priority (should show lifeboat)
- [ ] Search "Main Engine" (should filter to engine tasks)
- [ ] Check Calendar View (should show next 30 days)
- [ ] Click "Mark Complete" (should call API - will fail if not implemented)

---

## 🐛 Known Limitations (To Implement)

### Not Yet Implemented:
- ❌ Add/Edit crew member (buttons present but not functional)
- ❌ Mark Complete task (API call works, but UI needs refresh)
- ❌ Export PDF/Excel reports
- ❌ Send certificate reminders
- ❌ File upload for certificates
- ❌ Photo upload for crew
- ❌ Digital signatures

### Working Features:
- ✅ View crew list
- ✅ Search & filter crew
- ✅ Certificate status monitoring
- ✅ View maintenance tasks
- ✅ Search & filter tasks
- ✅ Calendar schedule view
- ✅ Real-time statistics

---

## 🎯 What You Should See

### On Crew Page:
1. **Header**: "Crew Management" with STCW subtitle
2. **4 Stats Cards**: Crew Onboard (4/4), Officers (4), Expiring Soon (1), Expired (0)
3. **Tabs**: Onboard Crew | All Crew | Certificate Monitor | Reports
4. **Search Bar** + Rank Filter dropdown
5. **Table** với 4 crew members:
   - John Smith (Master) - All certs valid
   - Maria Garcia (Chief Engineer) - All certs valid
   - Wei Chen (Chief Officer) - Certificate expiring warning
   - Ahmed Hassan (2nd Engineer) - All certs valid

### On Maintenance Page:
1. **Header**: "Planned Maintenance System" with ISM Code subtitle
2. **5 Stats Cards**: Total (4), Pending (3), Overdue (1), In Progress (0), Critical (1)
3. **Tabs**: Pending Tasks | Overdue | All Tasks | Schedule View
4. **Search Bar** + Priority Filter + Equipment Filter
5. **Task Cards** với detailed information:
   - Main Engine oil filter (pending, 50 days left)
   - Generator 1 injectors (pending, 200 days left)
   - Lifeboat test (OVERDUE, 2 days late) ⚠️
   - Fire pump test (pending, 1 day left)

---

## 📞 Support

If something doesn't work:
1. Check Edge API is running (http://localhost:5001)
2. Check sample data inserted (run SQL script)
3. Check browser console for errors (F12)
4. Verify CORS in Edge API Program.cs

---

**🎊 Enjoy exploring the professional maritime crew & maintenance management system!**
