# 📱 MOBILE PMS WORKFLOW - TODO LIST

> **Tài liệu này mô tả chi tiết các tính năng cần triển khai cho Mobile App theo luồng PMS chuẩn Maritime**
> 
> **Ngày tạo:** 10/12/2025  
> **Version:** 1.0  
> **Trạng thái:** Pending Implementation

---

## 📋 MỤC LỤC

1. [Tổng Quan Luồng](#1-tổng-quan-luồng)
2. [Task Status System](#2-task-status-system)
3. [Screen Implementations](#3-screen-implementations)
4. [API Integrations](#4-api-integrations)
5. [Notification Handling](#5-notification-handling)
6. [Offline Support](#6-offline-support)
7. [Testing Checklist](#7-testing-checklist)

---

## 1. TỔNG QUAN LUỒNG

### 1.1. Workflow Flow (Mobile Perspective)

```
┌─────────────────────────────────────────────────────────────────┐
│  CREW WORKFLOW (Mobile App)                                      │
└─────────────────────────────────────────────────────────────────┘

[DUE/OVERDUE] ──┬──> "Start" ───> [IN_PROGRESS]
                │                      │
                └──> "Request          │
                      Deferral"        │
                      (Modal)          │
                                       │
                        ┌──────────────┴──────────────┐
                        │                             │
                        │  📝 Làm việc:               │
                        │  - Điền checklist           │
                        │  - Chụp ảnh (bắt buộc)      │
                        │  - Nhập số liệu             │
                        │  - Chọn spare parts         │
                        │                             │
                        └──────────────┬──────────────┘
                                       │
                                       ▼
                              "Submit" ───> [PENDING_APPROVAL]
                                                   │
                        ┌──────────────────────────┴───┐
                        │                              │
                        ▼                              ▼
                  [COMPLETED]                    [RECTIFY]
                  (C/E Approved)              (C/E Rejected)
                                                   │
                                                   ▼
                                        📱 Push Notification
                                        "Task bị trả lại"
                                                   │
                                                   ▼
                                        "Fix & Resubmit"
                                                   │
                                                   ▼
                                           [IN_PROGRESS]
                                                   │
                                                   └──> Submit lại
```

### 1.2. Các Vai Trò Trên Mobile

| Vai Trò | Quyền Hạn Mobile |
|---------|------------------|
| **Crew (AB, Oiler, Fitter...)** | Xem task, Start, Submit, Request Deferral |
| **Junior Officer (3/E, 3/O)** | Như Crew + Xem task của cấp dưới |
| **Senior Officer (C/E, C/O)** | Tất cả + Approve/Reject task (Web preferred, Mobile optional) |
| **Master** | Tất cả + Approve Deferral |

---

## 2. TASK STATUS SYSTEM

### 2.1. Enum TaskStatus

```typescript
enum TaskStatus {
  // === PLANNING PHASE ===
  SCHEDULED = 'SCHEDULED',        // Chưa đến hạn (ẩn trên mobile?)
  DUE = 'DUE',                    // ✅ Sẵn sàng làm
  OVERDUE = 'OVERDUE',            // ⚠️ Quá hạn
  
  // === EXECUTION PHASE ===
  IN_PROGRESS = 'IN_PROGRESS',    // 🔧 Đang làm
  
  // === VERIFICATION PHASE ===
  PENDING_APPROVAL = 'PENDING_APPROVAL',  // ⏳ Chờ nghiệm thu
  RECTIFY = 'RECTIFY',            // ❌ Bị trả lại (thay cho REJECTED)
  
  // === FINAL STATES ===
  COMPLETED = 'COMPLETED',        // ✅ Hoàn thành
  CANCELLED = 'CANCELLED'         // 🚫 Đã hủy
}
```

### 2.2. Status Colors & Icons

```typescript
const STATUS_CONFIG = {
  SCHEDULED: {
    color: '#9CA3AF',  // Gray
    icon: 'calendar-clock',
    label: 'Scheduled',
    labelVi: 'Đã lên lịch'
  },
  DUE: {
    color: '#3B82F6',  // Blue
    icon: 'clock',
    label: 'Due',
    labelVi: 'Đến hạn'
  },
  OVERDUE: {
    color: '#EF4444',  // Red
    icon: 'alert-circle',
    label: 'Overdue',
    labelVi: 'Quá hạn'
  },
  IN_PROGRESS: {
    color: '#8B5CF6',  // Purple
    icon: 'wrench',
    label: 'In Progress',
    labelVi: 'Đang làm'
  },
  PENDING_APPROVAL: {
    color: '#F59E0B',  // Amber
    icon: 'clipboard-check',
    label: 'Waiting Verification',
    labelVi: 'Chờ nghiệm thu'
  },
  RECTIFY: {
    color: '#F97316',  // Orange
    icon: 'refresh-cw',
    label: 'Rectify',
    labelVi: 'Cần khắc phục'
  },
  COMPLETED: {
    color: '#10B981',  // Green
    icon: 'check-circle',
    label: 'Completed',
    labelVi: 'Hoàn thành'
  },
  CANCELLED: {
    color: '#6B7280',  // Gray
    icon: 'x-circle',
    label: 'Cancelled',
    labelVi: 'Đã hủy'
  }
};
```

### 2.3. Status Transition Rules (Mobile)

```typescript
// Crew chỉ được thực hiện các transitions sau:
const MOBILE_TRANSITIONS = {
  DUE: ['IN_PROGRESS'],           // Start task
  OVERDUE: ['IN_PROGRESS'],       // Start (muộn)
  IN_PROGRESS: ['PENDING_APPROVAL', 'DUE'],  // Submit hoặc Abandon
  RECTIFY: ['IN_PROGRESS'],       // Fix and continue
};

// Validation trước khi Submit
const SUBMIT_VALIDATION = {
  checklistCompleted: true,       // Tất cả items phải checked
  photosUploaded: '>= requiredPhotos',  // Đủ số ảnh yêu cầu
  readingsValid: true,            // Số liệu trong khoảng cho phép
};
```

---

## 3. SCREEN IMPLEMENTATIONS

### 3.1. Task List Screen

**File:** `screens/tasks/TaskListScreen.tsx`

#### TODO:
- [ ] **Tab Navigation**
  - [ ] Tab "Due Today" - Filter: status IN ['DUE'] AND dueDate = today
  - [ ] Tab "My Tasks" - Filter: assignedTo = currentUser
  - [ ] Tab "Overdue" - Filter: status = 'OVERDUE' (badge đỏ)
  - [ ] Tab "All" - Tất cả tasks

- [ ] **Task List Item**
  - [ ] Hiển thị: taskCode, description, equipmentName
  - [ ] Hiển thị: dueDate với màu (xanh = còn hạn, đỏ = quá hạn)
  - [ ] Hiển thị: assignedTo (avatar + name)
  - [ ] Badge: status với màu tương ứng
  - [ ] Badge: hasPendingDeferral (icon clock vàng)
  - [ ] Badge: rejectionCount >= 3 (warning icon)
  - [ ] Swipe actions: Quick Start, View Details

- [ ] **Pull to Refresh**
  - [ ] Refresh task list từ API
  - [ ] Sync pending changes (offline)

- [ ] **Search & Filter**
  - [ ] Search by taskCode, description, equipment
  - [ ] Filter by department, priority, status
  - [ ] Sort by dueDate, priority, status

- [ ] **Empty States**
  - [ ] "No tasks due today" - icon calendar
  - [ ] "All caught up!" - icon check-circle

#### UI Mockup:
```
┌─────────────────────────────────────┐
│  📋 My Tasks                    🔍  │
├─────────────────────────────────────┤
│ [Due Today] [My Tasks] [Overdue•3]  │
├─────────────────────────────────────┤
│ ┌─────────────────────────────────┐ │
│ │ 🔧 ENG-PM-001                   │ │
│ │ Check Main Engine Oil Level    │ │
│ │ 📍 Main Engine #1              │ │
│ │ 📅 Today, 14:00   👤 3/E Minh  │ │
│ │ [DUE]                     [→]  │ │
│ └─────────────────────────────────┘ │
│                                     │
│ ┌─────────────────────────────────┐ │
│ │ ⚠️ DECK-PM-015                  │ │
│ │ Lifeboat Davit Inspection      │ │
│ │ 📍 Lifeboat #1                 │ │
│ │ 📅 Yesterday   👤 Bosun        │ │
│ │ [OVERDUE]  [⚡ 2x reject]  [→] │ │
│ └─────────────────────────────────┘ │
│                                     │
│ ┌─────────────────────────────────┐ │
│ │ ⏳ ENG-PM-003                   │ │
│ │ Replace Fuel Filter            │ │
│ │ 📍 Generator #2                │ │
│ │ 📅 Submitted 2h ago            │ │
│ │ [PENDING_APPROVAL]        [→]  │ │
│ └─────────────────────────────────┘ │
└─────────────────────────────────────┘
```

---

### 3.2. Task Detail Screen

**File:** `screens/tasks/TaskDetailScreen.tsx`

#### TODO:
- [ ] **Header Section**
  - [ ] Task code + Status badge
  - [ ] Priority indicator (CRITICAL = red border)
  - [ ] Equipment name + location

- [ ] **Info Section**
  - [ ] Description
  - [ ] Due date (with countdown: "Due in 2 hours")
  - [ ] Assigned to
  - [ ] Schedule info (interval, last done)
  - [ ] Department

- [ ] **Required Spare Parts Section**
  - [ ] List spare parts từ schedule config
  - [ ] Hiển thị: name, code, quantity required, mandatory flag
  - [ ] Indicator: available in stock / not available

- [ ] **Checklist Preview** (readonly khi chưa Start)
  - [ ] List checkpoint items
  - [ ] Progress indicator (0/5 completed)

- [ ] **Deferral History** (nếu có)
  - [ ] Timeline các lần xin hoãn
  - [ ] Status của từng request

- [ ] **Rejection History** (nếu status = RECTIFY)
  - [ ] Alert box màu cam
  - [ ] Rejection reason
  - [ ] Rejected by + time
  - [ ] Rejection count

- [ ] **Action Buttons** (theo status)

```typescript
// Status-based actions
const ACTIONS_BY_STATUS = {
  DUE: [
    { label: 'Start Task', action: 'start', variant: 'primary' },
    { label: 'Request Deferral', action: 'defer', variant: 'outline' }
  ],
  OVERDUE: [
    { label: 'Start Task', action: 'start', variant: 'primary' },
    { label: 'Request Deferral', action: 'defer', variant: 'outline' }
  ],
  IN_PROGRESS: [
    { label: 'Continue Working', action: 'continue', variant: 'primary' },
    { label: 'Submit Report', action: 'submit', variant: 'success', disabled: '!canSubmit' },
    { label: 'Abandon', action: 'abandon', variant: 'ghost' }
  ],
  PENDING_APPROVAL: [
    // No actions - waiting
  ],
  RECTIFY: [
    { label: 'Fix and Resubmit', action: 'fix', variant: 'primary' }
  ],
  COMPLETED: [
    { label: 'View Report', action: 'viewReport', variant: 'outline' }
  ]
};
```

#### UI Mockup:
```
┌─────────────────────────────────────┐
│ ← Task Details                      │
├─────────────────────────────────────┤
│                                     │
│  ENG-PM-001                [DUE]    │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  │
│                                     │
│  Check Main Engine Oil Level        │
│                                     │
│  📍 Main Engine #1                  │
│  📅 Due: Today, 14:00 (in 2 hours)  │
│  👤 Assigned: 3/E Nguyễn Minh       │
│  🔄 Interval: Every 500 RH          │
│  📊 Last Done: 05/12/2025           │
│                                     │
│  ─────────────────────────────────  │
│  📦 Required Spare Parts            │
│  ─────────────────────────────────  │
│  │ Oil Filter 15W40    x2   [✓]  │  │
│  │ Gasket Ring         x1   [✓]  │  │
│  │ Drain Plug          x1   [!]  │  │
│                                     │
│  ─────────────────────────────────  │
│  📋 Checklist (0/5 completed)       │
│  ─────────────────────────────────  │
│  ○ Check oil level                  │
│  ○ Record temperature               │
│  ○ Inspect for leaks                │
│  ○ Take photos                      │
│  ○ Clean area                       │
│                                     │
├─────────────────────────────────────┤
│  ┌─────────────────────────────┐    │
│  │      🚀 Start Task          │    │
│  └─────────────────────────────┘    │
│  ┌─────────────────────────────┐    │
│  │      📅 Request Deferral    │    │
│  └─────────────────────────────┘    │
└─────────────────────────────────────┘
```

---

### 3.3. Task Execution Screen

**File:** `screens/tasks/TaskExecutionScreen.tsx`

#### TODO:
- [ ] **Header**
  - [ ] Task code + "In Progress" badge
  - [ ] Timer: đang làm bao lâu
  - [ ] Progress bar: checklist completion

- [ ] **Checklist Section**
  - [ ] List all checkpoint items
  - [ ] Checkbox để mark complete
  - [ ] Input fields cho readings (số liệu)
    - [ ] Validation: min/max range
    - [ ] Unit display
  - [ ] Required indicator (*)
  - [ ] Notes field cho mỗi item

- [ ] **Photo Section**
  - [ ] Required photos indicator: "2/3 photos required"
  - [ ] Camera button để chụp ảnh
  - [ ] Gallery để xem ảnh đã chụp
  - [ ] Delete ảnh
  - [ ] Photo labels (tự động hoặc manual)

- [ ] **Spare Parts Used Section**
  - [ ] Dropdown/Search để chọn spare parts
  - [ ] Pre-populated từ Required Spare Parts
  - [ ] Quantity used input
  - [ ] Add custom spare part

- [ ] **Notes Section**
  - [ ] Free text input
  - [ ] Placeholder: "Add any observations or issues..."

- [ ] **Submit Section**
  - [ ] Validation summary
    - [ ] ✅ Checklist: 5/5 completed
    - [ ] ✅ Photos: 3/3 uploaded
    - [ ] ⚠️ Reading out of range (warning only)
  - [ ] Submit button (enabled only when valid)
  - [ ] Save Draft button

- [ ] **Offline Support**
  - [ ] Auto-save draft locally
  - [ ] Indicator: "Saved locally, will sync when online"
  - [ ] Queue photos for upload

#### UI Mockup:
```
┌─────────────────────────────────────┐
│ ← ENG-PM-001            ⏱️ 00:15:32 │
│   [IN_PROGRESS]         ████░░ 60%  │
├─────────────────────────────────────┤
│                                     │
│  📋 CHECKLIST                       │
│  ─────────────────────────────────  │
│                                     │
│  [✓] 1. Check oil level             │
│      └─ Notes: Level OK             │
│                                     │
│  [✓] 2. Record temperature *        │
│      └─ Reading: [85] °C            │
│         (Normal: 80-95°C)           │
│                                     │
│  [✓] 3. Inspect for leaks           │
│      └─ Notes: No leaks found       │
│                                     │
│  [ ] 4. Take photos *               │
│      └─ 📷 1/2 photos required      │
│         [IMG1] [+Add]               │
│                                     │
│  [ ] 5. Clean area                  │
│                                     │
│  ─────────────────────────────────  │
│  📷 PHOTOS (1/2 required)           │
│  ─────────────────────────────────  │
│  [🖼️ IMG1] [+ Add Photo]            │
│                                     │
│  ─────────────────────────────────  │
│  📦 SPARE PARTS USED                │
│  ─────────────────────────────────  │
│  │ Oil Filter 15W40    Qty: [2]  │  │
│  │ Gasket Ring         Qty: [1]  │  │
│  [+ Add Spare Part]                 │
│                                     │
│  ─────────────────────────────────  │
│  📝 NOTES                           │
│  ─────────────────────────────────  │
│  ┌─────────────────────────────┐    │
│  │ Engine running smoothly...  │    │
│  └─────────────────────────────┘    │
│                                     │
├─────────────────────────────────────┤
│  ⚠️ 1 photo still required          │
│                                     │
│  ┌─────────────────────────────┐    │
│  │   💾 Save Draft             │    │
│  └─────────────────────────────┘    │
│  ┌─────────────────────────────┐    │
│  │   📤 Submit Report          │    │
│  └─────────────────────────────┘    │
└─────────────────────────────────────┘
```

---

### 3.4. Deferral Request Modal

**File:** `components/modals/DeferralRequestModal.tsx`

#### TODO:
- [ ] **Form Fields**
  - [ ] Reason (required, min 20 chars)
    - [ ] Dropdown suggestions: "Heavy weather", "Spare parts not available", "Equipment in use", "Other"
  - [ ] Current due date (readonly)
  - [ ] Proposed new date (date picker)
    - [ ] Validation: +1 to +30 days
    - [ ] CMS items: max 3 months, warning if > 3 months
  - [ ] Deferral days (calculated, readonly)
  - [ ] Priority dropdown: Low, Normal, High
  - [ ] Attachments (optional photos)

- [ ] **CMS Item Warning**
  - [ ] If task.isCms === true AND deferralDays > 90:
    - [ ] Show warning: "Class Survey item - requires Class Permission Letter"
    - [ ] Require file upload for Class Permission Letter
    - [ ] Block submit nếu không có attachment

- [ ] **Validation**
  - [ ] Reason >= 20 characters
  - [ ] Date within allowed range
  - [ ] Attachment required for CMS > 3 months

- [ ] **Submit**
  - [ ] Create DeferralRequest
  - [ ] Set task.hasPendingDeferral = true
  - [ ] Show success message
  - [ ] Notify Master/C/E

#### UI Mockup:
```
┌─────────────────────────────────────┐
│        Request Deferral        [X]  │
├─────────────────────────────────────┤
│                                     │
│  📋 Task: ENG-PM-001                │
│  Check Main Engine Oil Level        │
│                                     │
│  ─────────────────────────────────  │
│                                     │
│  Reason *                           │
│  ┌─────────────────────────────┐    │
│  │ Select reason...         ▼ │    │
│  └─────────────────────────────┘    │
│  ○ Heavy weather                    │
│  ○ Spare parts not available        │
│  ○ Equipment in use                 │
│  ○ Crew shortage                    │
│  ○ Other (please specify)           │
│                                     │
│  Details *                          │
│  ┌─────────────────────────────┐    │
│  │ Due to heavy weather       │    │
│  │ conditions, cannot safely  │    │
│  │ access the engine room...  │    │
│  └─────────────────────────────┘    │
│  (min 20 characters)                │
│                                     │
│  Current Due Date                   │
│  📅 10/12/2025                      │
│                                     │
│  New Proposed Date *                │
│  ┌─────────────────────────────┐    │
│  │ 📅 13/12/2025            ▼ │    │
│  └─────────────────────────────┘    │
│                                     │
│  ⏱️ Deferral: 3 days                │
│                                     │
│  Priority                           │
│  ┌─────────────────────────────┐    │
│  │ Normal                   ▼ │    │
│  └─────────────────────────────┘    │
│                                     │
│  Attachments (optional)             │
│  [+ Add Photo]                      │
│                                     │
├─────────────────────────────────────┤
│  ┌─────────────────────────────┐    │
│  │     Submit Request          │    │
│  └─────────────────────────────┘    │
└─────────────────────────────────────┘
```

---

### 3.5. Rectify Alert Screen

**File:** `screens/tasks/RectifyAlertScreen.tsx`

#### TODO:
- [ ] **Alert Banner**
  - [ ] Icon: alert-triangle (orange)
  - [ ] Title: "Task Returned for Correction"
  - [ ] Subtitle: "Please review and fix the issues below"

- [ ] **Rejection Details**
  - [ ] Rejected by (name + rank)
  - [ ] Rejected at (datetime)
  - [ ] Rejection count (nếu > 1: "This is the 2nd rejection")
  - [ ] Reason (highlighted box)

- [ ] **Rejection History** (nếu rejectionCount > 1)
  - [ ] Timeline các lần reject
  - [ ] Mỗi item: reason, by, at

- [ ] **Warning Banner** (nếu rejectionCount >= 3)
  - [ ] Red alert box
  - [ ] Message: "Multiple rejections detected. Please contact your supervisor."
  - [ ] Button: "Contact Supervisor" (opens phone/chat)

- [ ] **Action Buttons**
  - [ ] "Fix and Resubmit" → Navigate to TaskExecutionScreen
  - [ ] "Contact Supervisor" → Open contact options

#### UI Mockup:
```
┌─────────────────────────────────────┐
│ ← Task Details                      │
├─────────────────────────────────────┤
│                                     │
│  ┌─────────────────────────────┐    │
│  │  ⚠️ TASK RETURNED           │    │
│  │     FOR CORRECTION          │    │
│  └─────────────────────────────┘    │
│                                     │
│  ENG-PM-001              [RECTIFY]  │
│  Check Main Engine Oil Level        │
│                                     │
│  ─────────────────────────────────  │
│  ❌ REJECTION DETAILS               │
│  ─────────────────────────────────  │
│                                     │
│  ┌─────────────────────────────┐    │
│  │ "Wrong temperature reading. │    │
│  │  85°C is outside normal     │    │
│  │  range for this equipment.  │    │
│  │  Please verify and re-enter │    │
│  │  the correct value."        │    │
│  └─────────────────────────────┘    │
│                                     │
│  👤 Rejected by: C/E Nguyễn Văn A   │
│  📅 Time: 10/12/2025, 09:30         │
│  🔄 Rejection count: 2              │
│                                     │
│  ─────────────────────────────────  │
│  📜 REJECTION HISTORY               │
│  ─────────────────────────────────  │
│                                     │
│  ● 10/12, 09:30 - C/E               │
│    "Wrong temperature reading..."   │
│                                     │
│  ● 09/12, 16:45 - C/E               │
│    "Missing photo of oil level..."  │
│                                     │
│  ┌─────────────────────────────┐    │
│  │  🚨 Multiple rejections!    │    │
│  │  Please contact supervisor  │    │
│  │  if you need assistance.    │    │
│  └─────────────────────────────┘    │
│                                     │
├─────────────────────────────────────┤
│  ┌─────────────────────────────┐    │
│  │    🔧 Fix and Resubmit      │    │
│  └─────────────────────────────┘    │
│  ┌─────────────────────────────┐    │
│  │    📞 Contact Supervisor    │    │
│  └─────────────────────────────┘    │
└─────────────────────────────────────┘
```

---

### 3.6. Pending Approval Screen (Read-only)

**File:** `screens/tasks/PendingApprovalScreen.tsx`

#### TODO:
- [ ] **Info Banner**
  - [ ] Icon: clock (amber)
  - [ ] Title: "Waiting for Verification"
  - [ ] Subtitle: "Your report is being reviewed by C/E"

- [ ] **Submitted Report Summary**
  - [ ] Checklist completion status
  - [ ] Photos uploaded (gallery preview)
  - [ ] Spare parts used
  - [ ] Notes
  - [ ] Submitted at

- [ ] **Timeline**
  - [ ] Started at
  - [ ] Duration
  - [ ] Submitted at
  - [ ] Waiting for: X hours

- [ ] **No actions** (readonly)
  - [ ] Message: "You will be notified when the report is reviewed"

---

## 4. API INTEGRATIONS

### 4.1. Task APIs

```typescript
// GET /api/maintenance/tasks
// Query params: status, assignedTo, department, dueDate, search
interface GetTasksParams {
  status?: TaskStatus[];
  assignedTo?: string;
  department?: string;
  dueDateFrom?: string;
  dueDateTo?: string;
  search?: string;
  page?: number;
  limit?: number;
}

// GET /api/maintenance/tasks/:id
// Returns: Full task details with checklist, spare parts, history

// POST /api/maintenance/tasks/:id/start
// Body: none
// Returns: Updated task with status = IN_PROGRESS
interface StartTaskResponse {
  success: boolean;
  task: MaintenanceTask;
  message: string;
}

// POST /api/maintenance/tasks/:id/submit
// Body: ExecutionReport
interface SubmitReportBody {
  checklistItems: ChecklistItemResult[];
  photos: Photo[];
  sparePartsUsed: SparePartUsed[];
  notes?: string;
  actualDuration: number;  // minutes
  actualRunningHours?: number;
}

// POST /api/maintenance/tasks/:id/abandon
// Body: { reason: string }
// Returns: Task status back to DUE

// GET /api/maintenance/tasks/:id/report
// Returns: Submitted report for viewing
```

### 4.2. Deferral APIs

```typescript
// POST /api/maintenance/tasks/:id/defer
interface CreateDeferralBody {
  reason: string;
  proposedDueDate: string;
  priority: 'LOW' | 'NORMAL' | 'HIGH';
  attachments?: string[];  // base64 or URLs
}

// GET /api/maintenance/deferrals
// Query: status, taskId
// Returns: List of deferral requests

// GET /api/maintenance/deferrals/:id
// Returns: Deferral details
```

### 4.3. Photo Upload APIs

```typescript
// POST /api/maintenance/tasks/:id/photos
// Content-Type: multipart/form-data
interface UploadPhotoBody {
  file: File;
  label?: string;
  checklistItemId?: number;
}

// DELETE /api/maintenance/tasks/:id/photos/:photoId
```

---

## 5. NOTIFICATION HANDLING

### 5.1. Push Notification Types

```typescript
enum NotificationType {
  // Task lifecycle
  TASK_ASSIGNED = 'TASK_ASSIGNED',
  TASK_DUE_SOON = 'TASK_DUE_SOON',      // 24h before
  TASK_OVERDUE = 'TASK_OVERDUE',
  
  // Verification
  TASK_APPROVED = 'TASK_APPROVED',
  TASK_REJECTED = 'TASK_REJECTED',       // → RECTIFY
  
  // Deferral
  DEFERRAL_APPROVED = 'DEFERRAL_APPROVED',
  DEFERRAL_REJECTED = 'DEFERRAL_REJECTED',
  
  // Alerts
  HIGH_REJECTION_ALERT = 'HIGH_REJECTION_ALERT',  // rejectionCount >= 3
  
  // Daily summary
  MORNING_BRIEFING = 'MORNING_BRIEFING'  // 07:00
}
```

### 5.2. Notification Handlers

```typescript
// TODO: Handle each notification type
const notificationHandlers = {
  TASK_ASSIGNED: (data) => {
    // Navigate to task detail
    navigate(`/tasks/${data.taskId}`);
  },
  
  TASK_REJECTED: (data) => {
    // Show alert, navigate to rectify screen
    showAlert({
      title: 'Task Rejected',
      message: data.reason,
      type: 'warning'
    });
    navigate(`/tasks/${data.taskId}/rectify`);
  },
  
  MORNING_BRIEFING: (data) => {
    // Show summary modal
    showMorningBriefing(data);
  },
  
  // ... other handlers
};
```

### 5.3. Morning Briefing Modal

**TODO:**
- [ ] Trigger at 07:00 local time
- [ ] Summary content:
  - [ ] Tasks due today: X
  - [ ] Overdue tasks: X
  - [ ] Pending approval: X
  - [ ] Tasks rejected: X
- [ ] Quick actions:
  - [ ] "View Due Tasks"
  - [ ] "View Overdue"
- [ ] Dismiss button

---

## 6. OFFLINE SUPPORT

### 6.1. Offline Capabilities

```typescript
const OFFLINE_FEATURES = {
  // READ operations
  viewTasks: true,           // Cached task list
  viewTaskDetails: true,     // Cached task details
  viewChecklist: true,       // Cached checklist items
  
  // WRITE operations (queued)
  startTask: true,           // Queue, sync when online
  updateChecklist: true,     // Save locally, sync later
  capturePhotos: true,       // Save locally, upload later
  submitReport: true,        // Queue, sync when online
  requestDeferral: true,     // Queue, sync when online
  
  // NOT supported offline
  viewPhotos: false,         // Requires download
  receiveNotifications: false
};
```

### 6.2. Sync Queue

```typescript
interface SyncQueueItem {
  id: string;
  action: 'START' | 'UPDATE' | 'SUBMIT' | 'DEFER' | 'UPLOAD_PHOTO';
  taskId: number;
  payload: any;
  createdAt: Date;
  retryCount: number;
  status: 'PENDING' | 'SYNCING' | 'FAILED' | 'SUCCESS';
}

// TODO:
// - [ ] Implement sync queue storage (SQLite/AsyncStorage)
// - [ ] Background sync service
// - [ ] Retry logic with exponential backoff
// - [ ] Conflict resolution
// - [ ] UI indicator: "X changes pending sync"
```

---

## 7. TESTING CHECKLIST

### 7.1. Functional Tests

- [ ] **Task List**
  - [ ] Load tasks correctly
  - [ ] Filter by status works
  - [ ] Search works
  - [ ] Pull to refresh works
  - [ ] Pagination works

- [ ] **Task Lifecycle**
  - [ ] Start task: DUE → IN_PROGRESS
  - [ ] Submit report: IN_PROGRESS → PENDING_APPROVAL
  - [ ] Abandon task: IN_PROGRESS → DUE
  - [ ] Fix rectify: RECTIFY → IN_PROGRESS
  - [ ] View completed task

- [ ] **Checklist**
  - [ ] Check/uncheck items
  - [ ] Enter readings with validation
  - [ ] Add notes
  - [ ] Required items validation

- [ ] **Photos**
  - [ ] Capture photo
  - [ ] View photo
  - [ ] Delete photo
  - [ ] Required photos validation
  - [ ] Photo upload (online)
  - [ ] Photo queue (offline)

- [ ] **Deferral**
  - [ ] Create deferral request
  - [ ] Validation (reason, date)
  - [ ] CMS item warning
  - [ ] Submit successfully

- [ ] **Notifications**
  - [ ] Receive push notifications
  - [ ] Navigate from notification
  - [ ] Morning briefing display

### 7.2. Edge Cases

- [ ] **Offline Mode**
  - [ ] View cached tasks
  - [ ] Start task offline
  - [ ] Complete checklist offline
  - [ ] Capture photos offline
  - [ ] Submit queued when online
  - [ ] Conflict resolution

- [ ] **Error Handling**
  - [ ] Network timeout
  - [ ] API errors
  - [ ] Photo upload failure
  - [ ] Sync failure

- [ ] **Validation**
  - [ ] Empty required fields
  - [ ] Reading out of range
  - [ ] Reason too short
  - [ ] Date out of range

### 7.3. Performance Tests

- [ ] Load 100+ tasks
- [ ] Large images (>5MB)
- [ ] Multiple photos upload
- [ ] Long running background sync

---

## 📝 IMPLEMENTATION PRIORITY

### Phase 1: Core (Week 1-2)
1. [ ] Task List Screen
2. [ ] Task Detail Screen
3. [ ] Start Task flow
4. [ ] Basic Checklist

### Phase 2: Execution (Week 3-4)
5. [ ] Full Checklist with readings
6. [ ] Photo capture & upload
7. [ ] Submit Report flow
8. [ ] Spare Parts selection

### Phase 3: Advanced (Week 5-6)
9. [ ] Deferral Request
10. [ ] Rectify flow
11. [ ] Push Notifications
12. [ ] Morning Briefing

### Phase 4: Polish (Week 7-8)
13. [ ] Offline support
14. [ ] Sync queue
15. [ ] Error handling
16. [ ] Performance optimization
17. [ ] Testing & bug fixes

---

## 📚 REFERENCES

- [Main PMS Workflow Design](./PMS_WORKFLOW_DESIGN.md)
- [API Documentation](./backend/API_DOCS.md)
- [Database Schema](./backend/DB_SCHEMA.md)
- [Notification Strategy](./NOTIFICATION_STRATEGY.md)

---

**Document Version:** 1.0  
**Last Updated:** 10/12/2025  
**Author:** PMS Development Team
