# 🧪 TESTING GUIDE - Maritime Crew App

## 📱 App đang chạy với MOCK DATA

Hiện tại app sử dụng mock data để test UI và flow. Backend chưa kết nối.

---

## 🎯 Test Scenarios

### 1️⃣ **Login Screen**

**Mock Users:**
- **Crew ID**: `CM001` → John Smith (Chief Engineer)
- **Crew ID**: `CM002` → David Wilson (2nd Engineer)  
- **Crew ID**: `CM003` → Mike Johnson (Electrician)
- **Password**: Bất kỳ (không validate trong mock mode)

**Test Steps:**
1. Launch app
2. Enter Crew ID: `CM001`
3. Enter Password: `123456`
4. Click **Login**
5. ✅ Should navigate to Home Screen in 1 second

**Expected:**
- Loading indicator hiển thị
- Success → Navigate to Home
- User info saved (crewId, fullName, position)

---

### 2️⃣ **Home Screen / Dashboard**

**Mock Data Stats:**
- **Pending Tasks**: 3 tasks
- **Overdue Tasks**: 1 task (Generator 1)
- **In Progress**: 1 task (Cooling Water Pump)
- **Completed Today**: 1 task (Main Engine Overhaul)

**Test Steps:**
1. After login, observe Home Screen
2. Check stats cards at top
3. Navigate using Bottom Nav Bar
4. Try pull-to-refresh

**Expected:**
- Stats cards show correct counts
- Bottom nav: Home, Tasks, Schedule, Profile
- Drawer with navigation menu
- Sync indicator (offline/online status)

---

### 3️⃣ **Task List Screen**

**Mock Tasks (5 total):**

| Task ID | Equipment | Priority | Status | Due Date |
|---------|-----------|----------|--------|----------|
| TASK-001 | Main Engine | HIGH | PENDING | +5 days |
| TASK-002 | Generator 1 | CRITICAL | PENDING | -2 days (OVERDUE) |
| TASK-003 | Cooling Water Pump | NORMAL | IN_PROGRESS | +10 days |
| TASK-004 | Air Compressor | HIGH | PENDING | Today |
| TASK-005 | Main Engine | CRITICAL | COMPLETED | -1 day |

**Test Steps:**
1. Tap **Tasks** in bottom nav
2. Switch between tabs:
   - **All** (5 tasks)
   - **Pending** (3 tasks)
   - **Overdue** (1 task: Generator 1)
   - **Completed** (1 task)
3. Tap on **TASK-002 (Generator 1)**
4. Observe detail screen

**Expected:**
- Tasks filtered correctly by status
- Overdue tasks shown in red
- Priority badges colored correctly:
  - 🔴 CRITICAL = Red
  - 🟠 HIGH = Orange
  - 🔵 NORMAL = Blue
  - ⚪ LOW = Gray
- Pull-to-refresh works

---

### 4️⃣ **Task Detail Screen**

**Test with TASK-002 (Generator 1 - OVERDUE):**

**Expected to see:**
- Equipment Name: **Generator 1**
- Task ID: **TASK-002**
- Priority: **CRITICAL** (Red badge)
- Status: **PENDING** (Gray badge)
- Description: "Oil change and filter replacement"
- Interval: **250 hours**
- Last Done: 3 days ago
- Next Due: 2 days ago (OVERDUE - Red text)
- Running Hours at Last Done: **800.0**
- Assigned To: **CM001**

**Action Buttons:**
- ✅ **START TASK** button visible (because status = PENDING)

**Test Steps:**
1. Tap **START TASK** button
2. Observe loading indicator
3. Check if status changes to IN_PROGRESS

**Expected:**
- Button disabled during loading
- Success message shown
- Status updated to IN_PROGRESS
- **COMPLETE TASK** button now visible

---

### 5️⃣ **Complete Task Screen**

**Test with TASK-003 (Cooling Water Pump - IN_PROGRESS):**

**Test Steps:**
1. From Task List → Tap TASK-003
2. Tap **COMPLETE TASK** button
3. Fill form:
   - **Running Hours**: `850` (must be >= last done: 800)
   - **Spare Parts**: `Bearings x2, Grease 500g`
   - **Notes**: `Alignment checked. All normal.`
4. Tap **COMPLETE TASK** button

**Expected:**
- ⚠️ Offline warning banner shown (no backend)
- Form validation:
  - Running Hours required
  - Must be number
- Loading indicator during save
- Success message: "Task saved for offline sync"
- Navigate back to Task List
- Task status updated to COMPLETED

**With Backend:**
- If ONLINE → Sync immediately
- If OFFLINE → Add to sync queue
- Sync queue counter in Settings increases

---

### 6️⃣ **Profile Screen**

**Mock Profile Data:**

```
Full Name: John Smith (or based on crewId)
Position: Chief Engineer
Crew ID: CM001
Department: Engine
Nationality: British
Date of Birth: 1985-06-15
Phone: +44 7700 900123
Email: john.smith@maritime.com

Emergency Contact:
Name: Jane Smith
Phone: +44 7700 900456
Relationship: Spouse

Employment:
Status: ONBOARD (Green badge)
Join Date: 2024-01-15
Contract End: 2024-07-15
```

**Test Steps:**
1. Tap **Profile** in bottom nav
2. Scroll through sections
3. Tap **Certificates** to see certificate details
4. Pull to refresh

**Expected:**
- All personal info displayed correctly
- Document info with expiry dates
- Employment status badge
- Navigation to Certificates screen works

---

### 7️⃣ **Certificates Screen**

**Mock Certificates:**

| Certificate | Number | Issue | Expiry | Status |
|-------------|--------|-------|--------|--------|
| STCW | STCW-2024-001 | 2023-01-01 | 2028-01-01 | ✅ VALID (4+ years) |
| Medical | MED-2024-001 | 2024-01-01 | 2025-01-01 | ⚠️ EXPIRING (3 months) |
| Passport | GB123456789 | 2019-05-01 | 2029-05-01 | ✅ VALID |
| Visa | VISA-2024-001 | 2024-01-01 | 2024-11-01 | ⚠️ EXPIRING (< 90 days) |
| Seaman Book | SB-2020-001 | 2020-01-01 | N/A | ✅ VALID |

**Test Steps:**
1. From Profile → Tap **View Certificates**
2. Observe warning banner (if any expired/expiring)
3. Check color coding of certificate cards
4. Check "Days Remaining" countdown

**Expected:**
- 🔴 EXPIRED certificates: Red card + "EXPIRED" badge
- 🟠 EXPIRING certificates: Orange card + "EXPIRING SOON" badge + Days remaining
- 🟢 VALID certificates: Green border + "VALID" badge
- Warning banner at top shows count: "2 certificates expiring soon"

---

### 8️⃣ **Schedule Screen**

**Mock Schedule Data:**
- Tasks grouped by due date
- Filter chips: Upcoming (7 days), This Week, This Month, All
- Stats: Total, Overdue, Due Soon

**Test Steps:**
1. Tap **Schedule** in bottom nav
2. Observe tasks grouped by date
3. Tap filter chips to filter by time range
4. Tap on a task to view details

**Expected:**
- Date headers with task count badges
- Tasks sorted by due date
- Overdue tasks highlighted in red
- Stats update when filter changes
- Empty state if no tasks in selected range

---

### 9️⃣ **Settings Screen**

**Features:**
- Account info display
- Sync status (Online/Offline)
- Sync queue count (pending items)
- Server configuration
- Clear cache
- Logout

**Test Steps:**
1. Open drawer → Tap **Settings**
2. Check account section (name, position, crewId)
3. Check sync section:
   - Connection status (should be OFFLINE with mock)
   - Pending items: 0 (or more if completed tasks offline)
   - Tap **Sync Now** button
4. Tap **Server Configuration**
5. Try changing server URL
6. Tap **Clear Cache** → Confirm
7. Tap **Logout** → Confirm

**Expected:**
- Account info matches logged-in user
- Offline indicator shown (red icon)
- Pending items count accurate
- Server config allows URL change
- Clear cache → All cached data removed
- Logout → Navigate to Login screen

---

### 🔟 **Server Configuration Screen**

**Test Steps:**
1. From Settings → Tap **Server Configuration**
2. Observe current URL: `http://192.168.1.100:5001`
3. Try changing to:
   - Invalid URL: `invalid` → Shows error
   - Valid URL: `http://192.168.1.200:5001` → Saves
4. Tap **Reset to Default**
5. Observe endpoints list

**Expected:**
- Current URL displayed
- Validation requires http:// or https://
- Save button disabled until valid URL entered
- Reset button restores default
- Warning: "App will restart after saving"

---

## 🔄 Offline Mode Testing

### Test Scenario:

1. **Online → Offline transition:**
   - Start app (online)
   - Disconnect WiFi
   - Observe sync indicator turns red
   - Try fetching tasks → Shows cached data
   - Complete a task → Saved to sync queue

2. **Offline → Online transition:**
   - Reconnect WiFi
   - Observe sync indicator turns green
   - Sync queue processes automatically
   - Check Settings → Pending items = 0

**Expected:**
- App continues working offline
- User actions saved to queue
- Auto-sync when online
- No data loss

---

## 🐛 Known Limitations (Mock Mode)

### ⚠️ Features not working without backend:

1. **Login validation**: Any password works
2. **Real task data**: Using 5 hardcoded tasks
3. **Task updates**: Updates not persisted to server
4. **Profile fetch**: Cannot load real crew data
5. **Certificate data**: Using mock expiry dates
6. **Photo upload**: Not implemented
7. **Push notifications**: Not implemented

### ✅ Features fully working (UI/UX):

- ✅ Navigation between screens
- ✅ Tab filtering (All, Pending, Overdue, Completed)
- ✅ Task status badges and colors
- ✅ Priority badges and colors
- ✅ Certificate expiry warnings
- ✅ Loading states
- ✅ Error states
- ✅ Empty states
- ✅ Pull-to-refresh animations
- ✅ Form validation
- ✅ Logout flow
- ✅ Settings persistence (local only)

---

## 📊 Test Completion Checklist

Use this checklist to verify all features:

- [ ] Login with CM001, CM002, CM003
- [ ] Home screen displays correct stats
- [ ] Task list shows 5 mock tasks
- [ ] Filter tasks by status (All, Pending, Overdue, Completed)
- [ ] View task details
- [ ] Start a pending task
- [ ] Complete an in-progress task with form
- [ ] View profile information
- [ ] View certificates with expiry warnings
- [ ] Check schedule screen with filters
- [ ] Navigate to settings
- [ ] Change server configuration
- [ ] Clear cache
- [ ] Logout and login again
- [ ] Test pull-to-refresh on all list screens
- [ ] Test bottom navigation
- [ ] Test drawer navigation
- [ ] Verify loading indicators
- [ ] Verify error messages on invalid input
- [ ] Check empty states

---

## 🎯 Performance Benchmarks

**Expected performance:**
- Login: < 2 seconds (mock delay)
- Task list load: < 1 second
- Task detail navigation: Instant
- Complete task form: < 1 second to save
- Navigation between screens: Instant (< 100ms)

**Memory usage:**
- Idle: ~150 MB
- After loading all screens: ~200 MB
- After caching data: ~220 MB

---

## 🚀 Next Steps: Connecting Backend

When backend is ready, follow **README.md** section:
**"🔄 Chuyển từ Mock Data sang Backend thật"**

### Quick checklist:
1. Update `ApiConstants.baseUrl` with real Edge Server IP
2. Update `AuthProvider.login()` to call real API
3. Remove `_generateMockTasks()` from TaskProvider
4. Test with real crew credentials
5. Verify JWT token storage
6. Test offline sync queue

### Estimated time: 30 minutes

---

## 📞 Report Issues

If you find bugs during testing:

1. Note the steps to reproduce
2. Take screenshot if possible
3. Check console for error logs
4. Report to development team

---

**Happy Testing! 🚢⚓**
