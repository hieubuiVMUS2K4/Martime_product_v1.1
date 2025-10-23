# 📱 APP STRUCTURE & NAVIGATION

## Current App Flow

```
┌─────────────────────┐
│   LoginScreen       │ 
│  (Initial screen)   │
│                     │
│ • Crew ID input     │
│ • Password input    │
│ • Login button      │
└──────────┬──────────┘
           │ login()
           ▼
┌─────────────────────┐
│   HomeScreen        │ ← MultiProvider wraps this
│  (Main container)   │
│                     │
│ ┌─────────────────┐ │
│ │  AppBar         │ │
│ │  - Title        │ │
│ │  - Sync badge   │ │
│ │  - Drawer icon  │ │
│ └─────────────────┘ │
│                     │
│ ┌─────────────────┐ │
│ │   Body          │ │ ← Changes based on selectedIndex
│ │  (IndexedStack) │ │
│ │                 │ │
│ │  Pages:         │ │
│ │  [0] Dashboard  │ │
│ │  [1] TaskList   │ │
│ │  [2] Schedule   │ │
│ │  [3] Profile    │ │
│ └─────────────────┘ │
│                     │
│ ┌─────────────────┐ │
│ │ BottomNavBar    │ │
│ │ • Home          │ │
│ │ • Tasks  ← HERE!│ │
│ │ • Schedule      │ │
│ │ • Profile       │ │
│ └─────────────────┘ │
└─────────────────────┘
```

## HomeScreen Internal Structure

`lib/presentation/screens/home/home_screen.dart` contains:

1. **State variable:**
```dart
int _selectedIndex = 0; // Default to Dashboard
```

2. **Pages list:**
```dart
final List<Widget> _pages = [
  _buildDashboard(context),      // index 0
  _buildTaskListTab(context),    // index 1 ← TASKS HERE
  _buildScheduleTab(context),    // index 2
  _buildProfileTab(context),     // index 3
];
```

3. **Bottom Nav Bar:**
```dart
BottomNavigationBar(
  currentIndex: _selectedIndex,
  onTap: _onItemTapped,
  items: [
    BottomNavigationBarItem(icon: Icon(Icons.home), label: 'Home'),
    BottomNavigationBarItem(icon: Icon(Icons.assignment), label: 'Tasks'),
    BottomNavigationBarItem(icon: Icon(Icons.calendar_today), label: 'Schedule'),
    BottomNavigationBarItem(icon: Icon(Icons.person), label: 'Profile'),
  ],
)
```

## Each Tab Contents

### Tab 0: Dashboard (_buildDashboard)
```
┌──────────────────────────┐
│   Welcome, John Smith    │
│                          │
│ ┌──────┐ ┌──────┐       │
│ │  3   │ │  1   │       │
│ │Pending│ │Overdue│      │
│ └──────┘ └──────┘       │
│                          │
│ ┌──────┐ ┌──────┐       │
│ │  1   │ │  1   │       │
│ │InProg │ │Compltd│      │
│ └──────┘ └──────┘       │
│                          │
│ Recent Tasks...          │
└──────────────────────────┘
```

### Tab 1: Tasks (_buildTaskListTab) 
**← YOU NEED TO CLICK HERE TO SEE TASKS!**

```
┌──────────────────────────┐
│ All │ Pending │ Overdue  │ ← Tabs
├──────────────────────────┤
│                          │
│ [TaskCard] Main Engine   │
│  TASK-001 • HIGH         │
│  Due in 5 days           │
│                          │
│ [TaskCard] Generator 1   │
│  TASK-002 • CRITICAL     │
│  OVERDUE (2 days ago)    │
│                          │
│ [TaskCard] Cooling Pump  │
│  TASK-003 • NORMAL       │
│  Due in 10 days          │
│                          │
│ ... (5 tasks total)      │
└──────────────────────────┘
```

### Tab 2: Schedule
```
┌──────────────────────────┐
│ Upcoming │ Week │ Month  │ ← Filters
├──────────────────────────┤
│ Stats: 5 Total, 1 Overdue│
│                          │
│ Oct 22, 2025 (2 tasks)   │
│  • Air Compressor        │
│  • Main Engine           │
│                          │
│ Oct 27, 2025 (1 task)    │
│  • Main Engine           │
└──────────────────────────┘
```

### Tab 3: Profile
```
┌──────────────────────────┐
│     [Avatar]             │
│   John Smith             │
│  Chief Engineer          │
│     CM001                │
├──────────────────────────┤
│ Personal Info            │
│  • Nationality: British  │
│  • DOB: 1985-06-15       │
│  ...                     │
│                          │
│ Certificates →           │
│ Documents                │
│ Employment Status        │
└──────────────────────────┘
```

## Navigation Flow to See Tasks

```
1. Launch App
   ↓
2. Login Screen
   - Enter CM001 / any password
   - Click Login
   ↓
3. Home Screen (shows Dashboard by default)
   ↓
4. Click "Tasks" icon in Bottom Nav Bar
   (2nd icon from left)
   ↓
5. NOW you see Task List with 5 tasks!
```

## How to Access Settings

### Method 1: Drawer Menu
```
1. At Home Screen
2. Click hamburger icon (☰) top-left
3. Drawer opens with menu items
4. Click "Settings"
```

### Method 2: Profile Tab
```
1. Click Profile tab in bottom nav
2. Scroll down
3. Find "Settings" button
```

## Expected Settings Screen Content

```
┌────────────────────────────┐
│         Settings           │
├────────────────────────────┤
│ Account                    │
│  [JS] John Smith           │
│       Chief Engineer       │
│       ID: CM001            │
├────────────────────────────┤
│ Synchronization            │
│  🔴 Offline                │
│  0 items waiting to sync   │
│  Sync Now →                │
├────────────────────────────┤
│ Server Configuration →     │
├────────────────────────────┤
│ Clear Cache                │
├────────────────────────────┤
│ Logout                     │
└────────────────────────────┘
```

## Debug: Where Are You Now?

When app is running, check:

1. **What do you see at the top?**
   - "Maritime Crew App" = Home Screen ✅
   - "Login" = Still at login ❌

2. **Do you see bottom nav bar with 4 icons?**
   - Yes = Home Screen ✅
   - No = Something wrong ❌

3. **Which tab is highlighted?**
   - Home (house icon) = Tab 0 (Dashboard)
   - Tasks (clipboard icon) = Tab 1 (Task List) ← Should be here!
   - Schedule (calendar icon) = Tab 2
   - Profile (person icon) = Tab 3

4. **What's in the body?**
   - Stats cards (Pending/Overdue/etc) = Dashboard (Tab 0)
   - List of tasks with cards = Task List (Tab 1) ← Expected!
   - Calendar view = Schedule (Tab 2)
   - Profile info = Profile (Tab 3)

## Verification Checklist

Run through this checklist:

- [ ] App launches to Login Screen
- [ ] Login with CM001 / 123
- [ ] Console shows: "🔐 AuthProvider: Login successful!"
- [ ] Console shows: "✅ TaskProvider: Generated 5 mock tasks"
- [ ] Navigate to Home Screen
- [ ] See AppBar title: "Maritime Crew App"
- [ ] See bottom nav bar with 4 icons
- [ ] Default view = Dashboard with stats cards
- [ ] Click 2nd icon (Tasks)
- [ ] NOW see list of 5 tasks
- [ ] Click on any task → Task Detail Screen
- [ ] Go back
- [ ] Click Profile tab
- [ ] See profile info with "John Smith"
- [ ] Open drawer (hamburger menu)
- [ ] Click Settings
- [ ] See account info: "John Smith", "Chief Engineer", "ID: CM001"

## If Still Not Working

Please tell me:

1. **After login, what do you see?**
   - Blank white screen?
   - Dashboard with stats?
   - Something else?

2. **Do you see bottom navigation bar?**
   - Yes/No?

3. **What happens when you click 2nd icon (Tasks)?**
   - Nothing changes?
   - Screen goes blank?
   - Shows "No tasks"?

4. **In Settings, what exactly do you see?**
   - Complete blank?
   - "User" / "Position" / "ID: N/A"?
   - Something else?

5. **Console logs - do you see ALL of these?**
   ```
   🔐 AuthProvider: Login successful!
   ✅ TaskProvider: Generated 5 mock tasks
   ```

With this info, I can pinpoint the exact issue!
