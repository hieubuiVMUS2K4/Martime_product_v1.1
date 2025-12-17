# 🚢 PMS WORKFLOW DESIGN - MARITIME STANDARD

> **Tài liệu thiết kế tổng thể luồng PMS chuẩn Maritime**
> 
> **Ngày tạo:** 10/12/2025  
> **Version:** 2.0 (Updated with feedback)  
> **Trạng thái:** Approved for Implementation

---

## 📋 MỤC LỤC

1. [Tổng Quan](#1-tổng-quan)
2. [Task Status System](#2-task-status-system)
3. [Complete Workflow](#3-complete-workflow)
4. [Deferral System](#4-deferral-system)
5. [Verification & Rectify System](#5-verification--rectify-system)
6. [Database Schema](#6-database-schema)
7. [Business Rules](#7-business-rules)
8. [Notification Strategy](#8-notification-strategy)
9. [UI/UX Guidelines](#9-uiux-guidelines)
10. [Implementation Checklist](#10-implementation-checklist)

---

## 1. TỔNG QUAN

### 1.1. Mô Hình Hoạt Động

```
┌─────────────────────────────────────────────────────────────────┐
│                     PMS MARITIME WORKFLOW                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│   ┌──────────────┐         ┌──────────────┐                     │
│   │    SHORE     │◄───────►│     SHIP     │                     │
│   │  (Office)    │  Sync   │   (Vessel)   │                     │
│   └──────────────┘         └──────────────┘                     │
│         │                        │                               │
│         │                        │                               │
│         ▼                        ▼                               │
│   ┌──────────────┐         ┌──────────────┐                     │
│   │     WEB      │         │    WEB       │                     │
│   │  Dashboard   │         │  Dashboard   │                     │
│   │ (Monitoring) │         │  (Approval)  │                     │
│   └──────────────┘         └──────────────┘                     │
│                                  │                               │
│                                  ▼                               │
│                            ┌──────────────┐                     │
│                            │   MOBILE     │                     │
│                            │     APP      │                     │
│                            │ (Execution)  │                     │
│                            └──────────────┘                     │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 1.2. Phân Chia Vai Trò

| Platform | Vai Trò | Chức Năng Chính |
|----------|---------|-----------------|
| **Web (Shore)** | Superintendent, DPA | Giám sát, báo cáo, audit |
| **Web (Ship)** | Master, C/E, C/O | Approve/Reject, Review, Deferral Approval |
| **Mobile** | Officers, Crew | Thực thi công việc, Submit báo cáo, Request Deferral |

### 1.3. Thuật Ngữ

| Thuật Ngữ | Ý Nghĩa |
|-----------|---------|
| **DUE** | Task đã đến hạn, sẵn sàng thực hiện |
| **RECTIFY** | Task bị trả lại để khắc phục (thay cho "Rejected") |
| **Deferral** | Xin hoãn lịch thực hiện |
| **Verification** | Nghiệm thu báo cáo của Crew |
| **CMS** | Class Maintenance Survey (Đăng kiểm) |
| **RH** | Running Hours (Giờ chạy máy) |

---

## 2. TASK STATUS SYSTEM

### 2.1. Status Enum (Updated v2.0)

```typescript
enum TaskStatus {
  // === PLANNING PHASE ===
  SCHEDULED = 'SCHEDULED',        // Chưa đến hạn
  DUE = 'DUE',                    // Đã đến hạn, sẵn sàng thực hiện
  OVERDUE = 'OVERDUE',            // Quá hạn chưa làm
  
  // === EXECUTION PHASE ===
  IN_PROGRESS = 'IN_PROGRESS',    // Đang thực hiện
  
  // === VERIFICATION PHASE ===
  PENDING_APPROVAL = 'PENDING_APPROVAL',  // Chờ nghiệm thu
  RECTIFY = 'RECTIFY',            // ⚡ Cần khắc phục (thay REJECTED)
  
  // === FINAL STATES ===
  COMPLETED = 'COMPLETED',        // Hoàn thành
  CANCELLED = 'CANCELLED'         // Đã hủy
}
```

> **Note:** `RECTIFY` thay cho `REJECTED` vì:
> - Mang tính hành động tích cực hơn
> - Phù hợp với thuật ngữ PSC (Port State Control): "Rectify deficiencies"
> - Không gây hiểu nhầm là "bị từ chối/hủy bỏ"

### 2.2. Status Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│  PHASE 1: AUTO GENERATION                                        │
└─────────────────────────────────────────────────────────────────┘
                              ↓
                       [SCHEDULED]
                   (Chưa đến due date)
                              ↓
                  ⏰ Auto: Đến due date
                              ↓
                          [DUE]
                   (Sẵn sàng thực hiện)
                              │
              ┌───────────────┼───────────────┐
              ↓               ↓               ↓
      (Xin hoãn)        (Crew Start)    (Quá due date)
              │               │               │
              ▼               ▼               ▼
      [DEFERRAL_FLOW]  [IN_PROGRESS]    [OVERDUE]
                              │               │
                              └───────┬───────┘
                                      ↓
┌─────────────────────────────────────────────────────────────────┐
│  PHASE 2: EXECUTION                                              │
└─────────────────────────────────────────────────────────────────┘
                                      ↓
                              [IN_PROGRESS]
                         (Crew đang làm việc)
                                      │
                              📋 Điền checklist
                              📷 Chụp ảnh
                              📊 Nhập số liệu
                              📦 Chọn spare parts
                                      │
                                      ↓
                         📤 Crew bấm "Submit"
                                      ↓
┌─────────────────────────────────────────────────────────────────┐
│  PHASE 3: VERIFICATION                                           │
└─────────────────────────────────────────────────────────────────┘
                                      ↓
                          [PENDING_APPROVAL]
                      (Chờ C/E/Master kiểm tra)
                                      │
                 ┌────────────────────┴────────────────────┐
                 ↓                                         ↓
         👔 C/E Approve                            👔 C/E Reject
                 ↓                                         ↓
           [COMPLETED]                               [RECTIFY]
          ✅ Hoàn thành                          ⚠️ Cần khắc phục
                                                           │
                                                  🔔 Push notification
                                                   đến Mobile Crew
                                                           │
                                                           ▼
                                              👤 Crew bấm "Fix"
                                                           │
                                                           ▼
                                                   [IN_PROGRESS]
                                                           │
                                                           └─→ Submit lại
```

### 2.3. Kanban Column Order (Updated)

```typescript
// Thứ tự cột Kanban theo luồng công việc
const KANBAN_COLUMNS = [
  'DUE',              // Đến hạn
  'OVERDUE',          // Quá hạn
  'IN_PROGRESS',      // Đang làm
  'RECTIFY',          // ⚡ Cần khắc phục (ĐẶT NGAY SAU IN_PROGRESS)
  'PENDING_APPROVAL', // Chờ nghiệm thu
  'COMPLETED'         // Hoàn thành
];

// Note: RECTIFY đặt ngay sau IN_PROGRESS vì đây là trạng thái "nóng",
// cần được chú ý và xử lý ngay.
```

---

## 3. COMPLETE WORKFLOW

### 3.1. Happy Path

```
Schedule Config → Task Auto-Generated → SCHEDULED
                                            ↓
                         ⏰ Due date arrives
                                            ↓
                                          DUE
                                            ↓
                         📱 Crew: "Start Task"
                                            ↓
                                      IN_PROGRESS
                                            ↓
                         📱 Crew completes work:
                            - Fills checklist
                            - Takes photos (≥ required)
                            - Enters readings
                            - Selects spare parts
                                            ↓
                         📱 Crew: "Submit Report"
                                            ↓
                                   PENDING_APPROVAL
                                            ↓
                         💻 C/E reviews report
                                            ↓
                         💻 C/E: "Approve"
                                            ↓
                                       COMPLETED
                                            ↓
                         ⏰ Next task auto-scheduled
```

### 3.2. Rectify Path

```
                                   PENDING_APPROVAL
                                            ↓
                         💻 C/E reviews report
                         💻 C/E finds issues:
                            - Wrong reading
                            - Missing photo
                            - Incomplete checklist
                                            ↓
                         💻 C/E: "Reject" (+ reason)
                                            ↓
                                        RECTIFY
                                            ↓
                         🔔 Push notification to Crew:
                         "Task returned for correction"
                                            ↓
                         📱 Crew views rejection reason
                         📱 Crew: "Fix and Continue"
                                            ↓
                                      IN_PROGRESS
                                            ↓
                         📱 Crew fixes issues
                         📱 Crew: "Submit Report"
                                            ↓
                                   PENDING_APPROVAL
                                            ↓
                         💻 C/E: "Approve"
                                            ↓
                                       COMPLETED
```

### 3.3. Deferral Path

```
                                          DUE
                                            ↓
                         📱 Crew cannot do task:
                            - Heavy weather
                            - Spare parts not available
                            - Equipment in critical use
                                            ↓
                         📱 Crew: "Request Deferral"
                         📱 Fills form:
                            - Reason (required, min 20 chars)
                            - Proposed new date
                            - Priority
                            - Attachments (optional)
                                            ↓
                         DeferralRequest created
                         Task.hasPendingDeferral = true
                                            ↓
                         🔔 Web notification to Master/C/E
                                            ↓
                 ┌──────────────────────────┴──────────────────────────┐
                 ↓                                                      ↓
         💻 Master: "Approve"                               💻 Master: "Reject"
                 ↓                                                      ↓
         Task.dueDate = proposedDate                       Task.dueDate unchanged
         Task.deferralCount++                               hasPendingDeferral = false
         hasPendingDeferral = false                                     ↓
                 ↓                                          🔔 Push to Crew:
         🔔 Push to Crew:                                   "Deferral rejected,
         "Deferral approved"                                 please complete task"
                 ↓                                                      ↓
         Task remains [DUE]                                 Task remains [DUE]
         (with new date)                                    (original date)
```

### 3.4. OVERDUE Deferral Path (Critical Scenario)

```
                                       OVERDUE
                              (Task đã quá hạn chưa làm)
                                            ↓
                         📱 Crew nhận ra không thể làm:
                            - Thiếu spare parts đã đặt chậm
                            - Thời tiết xấu kéo dài
                            - CMS item cần Class approval
                            - Equipment đang critical operation
                                            ↓
                         📱 Crew: "Request Deferral"
                         📱 Fills form:
                            - Reason (REQUIRED, min 50 chars for OVERDUE)
                            - Proposed new date
                            - Root cause analysis
                            - Preventive measures
                            - Attachments (REQUIRED for OVERDUE)
                                            ↓
                         DeferralRequest created
                         Task.hasPendingDeferral = true
                         Task remains [OVERDUE] (not changed yet)
                                            ↓
                         🔔 HIGH PRIORITY notification to Master/C/E
                         ⚠️ "OVERDUE task deferral request requires attention"
                                            ↓
                 ┌──────────────────────────┴──────────────────────────┐
                 ↓                                                      ↓
         💻 Master/C/E: "Approve"                       💻 Master/C/E: "Reject"
         (After reviewing justification)                (Insufficient reason)
                 ↓                                                      ↓
         Task.dueDate = proposedDate                    Task.dueDate unchanged
         Task.status = DUE (reset from OVERDUE)         Task remains [OVERDUE]
         Task.deferralCount++                           hasPendingDeferral = false
         hasPendingDeferral = false                                     ↓
         Add audit log                                  🔔 Push to Crew:
                 ↓                                      "Deferral rejected.
         🔔 Push to Crew:                                Please complete immediately
         "OVERDUE deferral approved"                     or provide better justification"
         "New due date: {date}"                                        ↓
                 ↓                                      Crew must either:
         Task now [DUE]                                 - Start task immediately
         (Extended deadline)                            - Request new deferral with
                                                          stronger justification
```

**Key Differences for OVERDUE Deferrals:**

| Aspect | DUE Deferral | OVERDUE Deferral |
|--------|--------------|------------------|
| **Reason Length** | Min 20 chars | Min 50 chars (more detail required) |
| **Attachments** | Optional | **REQUIRED** (proof of issue) |
| **Root Cause** | Optional | **REQUIRED** |
| **Preventive Measures** | Optional | **REQUIRED** (how to prevent recurrence) |
| **Approval Priority** | Normal | **HIGH** (flagged for immediate attention) |
| **Notification Level** | C/E or C/O | **Master + C/E** (escalated) |
| **Status After Approval** | Remains DUE | **Reset to DUE** (fresh start) |
| **Audit Trail** | Standard log | **Detailed log** with root cause analysis |
| **KPI Impact** | Minor | **Major** (affects overdue metrics) |

---

## 4. DEFERRAL SYSTEM

### 4.1. Deferral Request Model

```typescript
interface TaskDeferralRequest {
  id: string;                     // UUID
  taskId: number;
  
  // Request info
  requestedBy: string;            // CrewId
  requestedAt: Date;
  reason: string;                 // Min 20 chars
  
  // Dates
  currentDueDate: Date;
  proposedDueDate: Date;
  deferralDays: number;           // Calculated
  
  // Approval
  status: 'PENDING' | 'APPROVED' | 'REJECTED';
  reviewedBy?: string;
  reviewedAt?: Date;
  reviewNotes?: string;
  
  // Metadata
  priority: 'LOW' | 'NORMAL' | 'HIGH';
  attachments?: string[];         // Photo URLs
  
  // CMS specific
  isCmsItem: boolean;
  classPermissionLetter?: string; // Required if CMS && deferralDays > 90
}
```

### 4.2. Deferral Rules

```typescript
const DEFERRAL_RULES = {
  // Basic limits
  minDeferralDays: 1,
  maxDeferralDays: 30,
  maxDeferralCount: 3,            // Per task
  
  // OVERDUE-specific rules
  overdue: {
    minReasonLength: 50,          // Longer explanation required
    requiresAttachments: true,    // Photo/document proof mandatory
    requiresRootCause: true,      // Why did it become overdue?
    requiresPreventiveMeasures: true,  // How to prevent next time?
    approvalLevel: 'MASTER',      // Always escalate to Master
    notificationPriority: 'HIGH', // Urgent notification
    resetStatusOnApproval: true,  // OVERDUE → DUE after approval
    auditLevel: 'DETAILED'        // Full audit trail
  },
  
  // CMS (Class) items - special rules
  cms: {
    maxDaysWithoutPermission: 90, // 3 months
    requiresClassLetter: true,    // If > 90 days
    approvalLevel: 'MASTER'       // Always Master for CMS
  },
  
  // Approval matrix
  approvers: {
    normal: ['CHIEF_ENGINEER', 'CHIEF_OFFICER'],
    critical: ['MASTER'],
    overdue: ['MASTER'],          // OVERDUE always needs Master
    cms: ['MASTER'],
    safety: ['MASTER']
  }
};
```

### 4.3. CMS Item Special Handling

```typescript
// Validation for CMS items
function validateCmsDeferral(deferral: DeferralRequest, task: Task) {
  if (!task.isCms) return true;
  
  if (deferral.deferralDays > 90) {
    // > 3 months requires Class Permission Letter
    if (!deferral.classPermissionLetter) {
      throw new Error(
        'Class Survey items deferred more than 3 months require ' +
        'a Class Permission Letter. Please upload the document.'
      );
    }
    
    // Must be approved by Master only
    // Auto-notify Office
    notifyOffice({
      type: 'CMS_LONG_DEFERRAL',
      task: task,
      deferral: deferral
    });
  }
  
  return true;
}
```

---

## 5. VERIFICATION & RECTIFY SYSTEM

### 5.1. Verification Flow

```typescript
interface VerificationAction {
  taskId: number;
  action: 'APPROVE' | 'REJECT';
  verifiedBy: string;             // C/E or Master crewId
  verifiedAt: Date;
  notes?: string;                 // Optional for approve
  rejectionReason?: string;       // Required for reject
}
```

### 5.2. Rejection (Rectify) Rules

```typescript
const RECTIFY_RULES = {
  // Reason validation
  minReasonLength: 10,
  
  // Warning thresholds
  warningThreshold: 3,            // Notify Master at 3rd rejection
  criticalThreshold: 5,           // Notify Office at 5th rejection
  
  // No hard limit - but track for KPI
  maxRejections: Infinity,
  
  // Notification recipients by count
  notifications: {
    1: ['CREW'],                  // Just the crew
    2: ['CREW'],
    3: ['CREW', 'MASTER'],        // + Master warning
    4: ['CREW', 'MASTER'],
    5: ['CREW', 'MASTER', 'OFFICE'] // + Office alert
  }
};
```

### 5.3. Rectify Process

```typescript
async function rejectTask(taskId: number, reason: string, rejectedBy: string) {
  // Validate reason
  if (reason.length < RECTIFY_RULES.minReasonLength) {
    throw new Error(`Rejection reason must be at least ${RECTIFY_RULES.minReasonLength} characters`);
  }
  
  // Update task
  const task = await db.task.update({
    where: { id: taskId },
    data: {
      status: 'RECTIFY',  // ⚡ Not "REJECTED"
      rejectionReason: reason,
      rejectionCount: { increment: 1 },
      lastRejectedAt: new Date(),
      lastRejectedBy: rejectedBy,
      verifiedBy: rejectedBy,
      verifiedAt: new Date(),
      verificationResult: 'REJECTED'
    }
  });
  
  // Add to rejection history
  await addRejectionHistory(task, reason, rejectedBy);
  
  // Send notifications based on count
  await sendRectifyNotifications(task);
  
  // Record KPI
  await recordKPI({
    crewId: task.assignedTo,
    metric: 'REJECTION_RATE',
    value: 1
  });
  
  return task;
}
```

---

## 6. DATABASE SCHEMA

### 6.1. MaintenanceTask (Updated)

```sql
CREATE TABLE MaintenanceTasks (
  -- Primary
  id SERIAL PRIMARY KEY,
  taskCode VARCHAR(50) UNIQUE NOT NULL,
  
  -- Schedule reference
  scheduleId INTEGER REFERENCES MaintenanceSchedules(id),
  
  -- Equipment
  equipmentGroupId INTEGER REFERENCES EquipmentGroups(id),
  individualEquipmentId INTEGER REFERENCES IndividualEquipments(id),
  
  -- Task info
  taskType VARCHAR(20) NOT NULL, -- PREVENTIVE, CORRECTIVE, INSPECTION, OVERHAUL
  description TEXT,
  priority VARCHAR(20) DEFAULT 'NORMAL',
  
  -- Status
  status VARCHAR(20) NOT NULL DEFAULT 'SCHEDULED',
  
  -- Dates
  dueDate TIMESTAMP NOT NULL,
  scheduledDate TIMESTAMP,
  
  -- Deferral tracking
  hasPendingDeferral BOOLEAN DEFAULT FALSE,
  deferralCount INTEGER DEFAULT 0,
  lastDeferredAt TIMESTAMP,
  lastDeferredBy VARCHAR(50),
  
  -- Execution tracking
  startedAt TIMESTAMP,
  startedBy VARCHAR(50),
  actualRunningHours DECIMAL(10,2),
  estimatedDuration INTEGER, -- minutes
  actualDuration INTEGER,    -- minutes
  
  -- Report data
  checklistCompleted BOOLEAN DEFAULT FALSE,
  photosUploaded INTEGER DEFAULT 0,
  requiredPhotos INTEGER DEFAULT 0,  -- ⚡ NEW: From template
  sparePartsUsed JSONB,
  notes TEXT,
  
  -- Submission
  submittedAt TIMESTAMP,
  submittedBy VARCHAR(50),
  
  -- Verification
  verifiedAt TIMESTAMP,
  verifiedBy VARCHAR(50),
  verificationResult VARCHAR(20), -- APPROVED, REJECTED
  verificationNotes TEXT,
  
  -- Rectify tracking
  rejectionReason TEXT,
  rejectionCount INTEGER DEFAULT 0,
  lastRejectedAt TIMESTAMP,
  lastRejectedBy VARCHAR(50),
  rejectionHistory JSONB, -- Array of {reason, by, at}
  
  -- Completion
  completedAt TIMESTAMP,
  completedBy VARCHAR(50),
  
  -- Cancellation
  cancelledAt TIMESTAMP,
  cancelledBy VARCHAR(50),
  cancellationReason TEXT,
  
  -- Assignment
  assignedTo VARCHAR(50),
  assignedDepartment VARCHAR(20),
  
  -- CMS flag
  isCms BOOLEAN DEFAULT FALSE,  -- ⚡ NEW: Class Survey item
  
  -- Audit
  createdAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  originNode VARCHAR(50),
  isSynced BOOLEAN DEFAULT FALSE,
  syncedAt TIMESTAMP
);

-- Indexes
CREATE INDEX idx_task_status ON MaintenanceTasks(status);
CREATE INDEX idx_task_duedate ON MaintenanceTasks(dueDate);
CREATE INDEX idx_task_assigned ON MaintenanceTasks(assignedTo);
CREATE INDEX idx_task_department ON MaintenanceTasks(assignedDepartment);
CREATE INDEX idx_task_equipment ON MaintenanceTasks(equipmentGroupId);
```

### 6.2. TaskDeferralRequests (New)

```sql
CREATE TABLE TaskDeferralRequests (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  taskId INTEGER NOT NULL REFERENCES MaintenanceTasks(id),
  
  -- Request
  requestedBy VARCHAR(50) NOT NULL,
  requestedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  reason TEXT NOT NULL,
  
  -- Dates
  currentDueDate TIMESTAMP NOT NULL,
  proposedDueDate TIMESTAMP NOT NULL,
  deferralDays INTEGER NOT NULL,
  
  -- Approval
  status VARCHAR(20) DEFAULT 'PENDING', -- PENDING, APPROVED, REJECTED
  reviewedBy VARCHAR(50),
  reviewedAt TIMESTAMP,
  reviewNotes TEXT,
  
  -- Metadata
  priority VARCHAR(20) DEFAULT 'NORMAL',
  attachments JSONB,
  
  -- CMS specific
  isCmsItem BOOLEAN DEFAULT FALSE,
  classPermissionLetter VARCHAR(255), -- File URL
  
  -- Audit
  createdAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_deferral_task ON TaskDeferralRequests(taskId);
CREATE INDEX idx_deferral_status ON TaskDeferralRequests(status);
CREATE INDEX idx_deferral_requested ON TaskDeferralRequests(requestedBy);
```

### 6.3. TaskStatusHistory (Audit Trail)

```sql
CREATE TABLE TaskStatusHistory (
  id SERIAL PRIMARY KEY,
  taskId INTEGER NOT NULL REFERENCES MaintenanceTasks(id),
  
  fromStatus VARCHAR(20),
  toStatus VARCHAR(20) NOT NULL,
  
  changedBy VARCHAR(50) NOT NULL,
  changedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  
  reason TEXT,
  notes TEXT,
  
  -- Client info
  deviceType VARCHAR(20), -- WEB, MOBILE
  ipAddress VARCHAR(45),
  userAgent TEXT
);

CREATE INDEX idx_history_task ON TaskStatusHistory(taskId);
CREATE INDEX idx_history_time ON TaskStatusHistory(changedAt);
```

---

## 7. BUSINESS RULES

### 7.1. Status Transition Matrix

```typescript
const STATUS_TRANSITIONS = {
  SCHEDULED: {
    allowed: ['DUE'],
    auto: true,  // System auto-transition when due date arrives
    permissions: ['SYSTEM']
  },
  
  DUE: {
    allowed: ['IN_PROGRESS', 'OVERDUE', 'CANCELLED'],
    permissions: {
      IN_PROGRESS: ['CREW', 'OFFICER'],
      OVERDUE: ['SYSTEM'],  // Auto
      CANCELLED: ['CHIEF_ENGINEER', 'MASTER']
    }
  },
  
  OVERDUE: {
    allowed: ['IN_PROGRESS', 'CANCELLED', 'DEFERRAL_REQUEST'],
    permissions: {
      IN_PROGRESS: ['CREW', 'OFFICER'],
      CANCELLED: ['CHIEF_ENGINEER', 'MASTER'],
      DEFERRAL_REQUEST: ['CREW', 'OFFICER']  // Allow deferral from OVERDUE
    },
    notes: [
      'OVERDUE tasks CAN request deferral with valid reason',
      'Deferral approval for OVERDUE requires stronger justification',
      'Task remains OVERDUE until deferral is approved'
    ]
  },
  
  IN_PROGRESS: {
    allowed: ['PENDING_APPROVAL', 'DUE'],
    permissions: {
      PENDING_APPROVAL: ['CREW', 'OFFICER'],  // Submit
      DUE: ['CREW', 'OFFICER']  // Abandon
    },
    validation: {
      PENDING_APPROVAL: (task) => {
        if (!task.checklistCompleted) {
          return { valid: false, error: 'Checklist must be completed' };
        }
        if (task.photosUploaded < task.requiredPhotos) {
          return { valid: false, error: `${task.requiredPhotos} photos required` };
        }
        return { valid: true };
      }
    }
  },
  
  PENDING_APPROVAL: {
    allowed: ['COMPLETED', 'RECTIFY'],
    permissions: {
      COMPLETED: ['CHIEF_ENGINEER', 'MASTER'],
      RECTIFY: ['CHIEF_ENGINEER', 'MASTER']
    },
    validation: {
      RECTIFY: (reason) => {
        if (!reason || reason.length < 10) {
          return { valid: false, error: 'Rejection reason required (min 10 chars)' };
        }
        return { valid: true };
      }
    }
  },
  
  RECTIFY: {
    allowed: ['IN_PROGRESS'],
    permissions: {
      IN_PROGRESS: ['CREW', 'OFFICER']  // Fix and continue
    }
  },
  
  COMPLETED: {
    allowed: [],  // Final state
    permissions: {}
  },
  
  CANCELLED: {
    allowed: [],  // Final state
    permissions: {}
  }
};
```

### 7.2. Photo Requirements

```typescript
// Defined in Schedule Template
interface ChecklistTemplate {
  items: ChecklistItem[];
  requiredPhotos: number;  // ⚡ Min photos required
  photoLabels?: string[];  // Optional: what photos to take
}

// Validation before submit
function validatePhotos(task: Task): ValidationResult {
  if (task.photosUploaded < task.requiredPhotos) {
    return {
      valid: false,
      error: `Please upload at least ${task.requiredPhotos} photos. ` +
             `Currently uploaded: ${task.photosUploaded}`,
      blocking: true  // Block submit button
    };
  }
  return { valid: true };
}
```

---

## 8. NOTIFICATION STRATEGY

### 8.1. Push Notifications (Mobile)

```typescript
const MOBILE_NOTIFICATIONS = {
  // Task Lifecycle
  TASK_ASSIGNED: {
    title: '📋 New Task Assigned',
    body: 'Task {taskCode}: {description}',
    priority: 'NORMAL',
    navigate: '/tasks/{taskId}'
  },
  
  TASK_DUE_SOON: {
    title: '⏰ Task Due Tomorrow',
    body: '{taskCode} is due tomorrow at {dueTime}',
    priority: 'NORMAL',
    triggerBefore: '24h'
  },
  
  TASK_OVERDUE: {
    title: '🚨 Task Overdue',
    body: '{taskCode} is now overdue. Please complete ASAP.',
    priority: 'HIGH',
    sound: 'alert'
  },
  
  // Verification
  TASK_APPROVED: {
    title: '✅ Report Approved',
    body: 'Your report for {taskCode} has been approved',
    priority: 'NORMAL'
  },
  
  TASK_RECTIFY: {  // ⚡ Renamed from REJECTED
    title: '⚠️ Report Needs Correction',
    body: '{verifierRank} returned {taskCode} for correction',
    priority: 'HIGH',
    sound: 'alert',
    navigate: '/tasks/{taskId}/rectify'
  },
  
  // Deferral
  DEFERRAL_APPROVED: {
    title: '✅ Deferral Approved',
    body: 'Your deferral for {taskCode} approved. New date: {newDate}',
    priority: 'NORMAL'
  },
  
  DEFERRAL_REJECTED: {
    title: '❌ Deferral Rejected',
    body: 'Please complete {taskCode} as scheduled',
    priority: 'HIGH'
  }
};
```

### 8.2. Morning Briefing (Daily Summary)

```typescript
// ⚡ NEW: Daily summary notification at 07:00
const MORNING_BRIEFING = {
  title: '☀️ Good Morning! Daily Task Summary',
  trigger: '07:00',  // Local time
  recipients: ['CHIEF_ENGINEER', 'CHIEF_OFFICER'],
  
  template: `
    Chào buổi sáng!
    
    📊 Tổng quan hôm nay ({department}):
    • Đến hạn: {dueCount} tasks
    • Quá hạn: {overdueCount} tasks
    • Chờ nghiệm thu: {pendingCount} tasks
    • Cần khắc phục: {rectifyCount} tasks
    
    👉 Xem chi tiết
  `,
  
  action: {
    label: 'Xem Tasks',
    navigate: '/pms/dashboard'
  }
};
```

### 8.3. Web Notifications

```typescript
const WEB_NOTIFICATIONS = {
  // For C/E / Master
  NEW_SUBMISSION: {
    title: '📋 New Report Submitted',
    body: '{crewName} submitted {taskCode}',
    badge: true,
    navigate: '/pms/approvals'
  },
  
  DEFERRAL_REQUEST: {
    title: '📅 Deferral Request',
    body: '{crewName} requests to defer {taskCode}',
    badge: true,
    navigate: '/pms/deferrals'
  },
  
  HIGH_REJECTION_ALERT: {
    title: '🚨 High Rejection Rate',
    body: '{taskCode} rejected {count} times by {crewName}',
    priority: 'HIGH',
    navigate: '/pms/tasks/{taskId}'
  }
};
```

---

## 9. UI/UX GUIDELINES

### 9.1. Kanban Board Layout

```
┌─────────────────────────────────────────────────────────────────────────┐
│  PMS Kanban Board                                          [Filter] [+] │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐│
│  │   DUE   │ │ OVERDUE │ │IN PROG. │ │ RECTIFY │ │APPROVAL │ │COMPLETE ││
│  │  (12)   │ │   (3)   │ │   (5)   │ │   (2)   │ │   (4)   │ │  (45)   ││
│  ├─────────┤ ├─────────┤ ├─────────┤ ├─────────┤ ├─────────┤ ├─────────┤│
│  │ 🔵      │ │ 🔴      │ │ 🟣      │ │ 🟠      │ │ 🟡      │ │ 🟢      ││
│  │ [Card]  │ │ [Card]  │ │ [Card]  │ │ [Card]  │ │ [Card]  │ │ [Card]  ││
│  │ [Card]  │ │ [Card]  │ │ [Card]  │ │ [Card]  │ │ [Card]  │ │ [Card]  ││
│  │ [Card]  │ │ [Card]  │ │         │ │         │ │ [Card]  │ │ [Card]  ││
│  │ ...     │ │         │ │         │ │         │ │         │ │ ...     ││
│  └─────────┘ └─────────┘ └─────────┘ └─────────┘ └─────────┘ └─────────┘│
│                                                                          │
│  Note: RECTIFY column placed after IN_PROGRESS for visibility            │
└─────────────────────────────────────────────────────────────────────────┘
```

### 9.2. Status Colors

```typescript
const STATUS_COLORS = {
  SCHEDULED:        { bg: '#F3F4F6', text: '#6B7280', border: '#D1D5DB' },  // Gray
  DUE:              { bg: '#DBEAFE', text: '#1D4ED8', border: '#93C5FD' },  // Blue
  OVERDUE:          { bg: '#FEE2E2', text: '#DC2626', border: '#FECACA' },  // Red
  IN_PROGRESS:      { bg: '#EDE9FE', text: '#7C3AED', border: '#C4B5FD' },  // Purple
  RECTIFY:          { bg: '#FFEDD5', text: '#EA580C', border: '#FDBA74' },  // Orange
  PENDING_APPROVAL: { bg: '#FEF3C7', text: '#D97706', border: '#FCD34D' },  // Amber
  COMPLETED:        { bg: '#D1FAE5', text: '#059669', border: '#6EE7B7' },  // Green
  CANCELLED:        { bg: '#F3F4F6', text: '#6B7280', border: '#D1D5DB' }   // Gray
};
```

### 9.3. Task Card Indicators

```typescript
const CARD_INDICATORS = {
  // Badges
  hasPendingDeferral: {
    icon: '⏳',
    tooltip: 'Pending deferral approval',
    color: 'yellow'
  },
  
  highRejectionCount: {
    icon: '⚠️',
    tooltip: 'Rejected {count} times',
    color: 'red',
    condition: (task) => task.rejectionCount >= 3
  },
  
  isCms: {
    icon: '🔷',
    tooltip: 'Class Survey Item',
    color: 'blue'
  },
  
  isCritical: {
    icon: '🔴',
    tooltip: 'Critical Priority',
    color: 'red',
    condition: (task) => task.priority === 'CRITICAL'
  },
  
  // Progress
  photosProgress: {
    show: (task) => task.status === 'IN_PROGRESS',
    format: '{uploaded}/{required} 📷'
  }
};
```

---

## 10. IMPLEMENTATION CHECKLIST

### 10.1. Backend (Week 1-2)

- [ ] **Database Migrations**
  - [ ] Add new columns to MaintenanceTasks
  - [ ] Create TaskDeferralRequests table
  - [ ] Create TaskStatusHistory table
  - [ ] Update indexes

- [ ] **API Endpoints**
  - [ ] `POST /tasks/:id/start` - Start task
  - [ ] `POST /tasks/:id/submit` - Submit report
  - [ ] `POST /tasks/:id/approve` - Approve (C/E)
  - [ ] `POST /tasks/:id/reject` - Reject → RECTIFY
  - [ ] `POST /tasks/:id/defer` - Create deferral request
  - [ ] `POST /deferrals/:id/approve` - Approve deferral
  - [ ] `POST /deferrals/:id/reject` - Reject deferral
  - [ ] `GET /deferrals` - List deferral requests
  - [ ] `GET /approvals` - List pending approvals

- [ ] **Business Logic**
  - [ ] Status transition validation
  - [ ] Photo requirement validation
  - [ ] Deferral rules validation
  - [ ] CMS item special handling
  - [ ] Rejection count tracking

- [ ] **Notifications**
  - [ ] Push notification service
  - [ ] Web notification service
  - [ ] Morning briefing scheduler

### 10.2. Frontend Web (Week 2-3)

- [ ] **Kanban Board**
  - [ ] Rename REJECTED → RECTIFY column
  - [ ] Reorder columns (RECTIFY after IN_PROGRESS)
  - [ ] Add card indicators
  - [ ] Update colors

- [ ] **Approval Screen**
  - [ ] List pending approvals
  - [ ] Approve button + modal
  - [ ] Reject button + reason modal
  - [ ] View full report

- [ ] **Deferral Management**
  - [ ] List pending deferrals
  - [ ] Approve/Reject deferrals
  - [ ] CMS item warning display
  - [ ] Class Permission Letter upload

- [ ] **Task Detail Modal**
  - [ ] Show rejection history
  - [ ] Show deferral history
  - [ ] Required photos indicator
  - [ ] CMS badge

### 10.3. Mobile App (Week 3-5)

See: [MOBILE_PMS_WORKFLOW_TODO.md](./MOBILE_PMS_WORKFLOW_TODO.md)

### 10.4. Testing (Week 5-6)

- [ ] **Unit Tests**
  - [ ] Status transition logic
  - [ ] Validation rules
  - [ ] Permission checks

- [ ] **Integration Tests**
  - [ ] Full workflow: DUE → COMPLETED
  - [ ] Rectify workflow
  - [ ] Deferral workflow
  - [ ] CMS deferral > 3 months

- [ ] **E2E Tests**
  - [ ] Web approval flow
  - [ ] Mobile execution flow
  - [ ] Notification delivery

---

## 📚 RELATED DOCUMENTS

- [Mobile TODO List](./MOBILE_PMS_WORKFLOW_TODO.md)
- [API Documentation](./backend/API_DOCS.md)
- [Notification Strategy](./NOTIFICATION_STRATEGY.md)

---

## 11. IMPLEMENTATION STATUS (Updated: 10/12/2025)

### 11.1. ✅ ĐÃ TRIỂN KHAI

#### Backend
| Item | File/Location | Status |
|------|---------------|--------|
| Database Migration - MaintenanceTask fields | `AddPmsWorkflowV2` | ✅ Done |
| Database Migration - TaskDeferralRequests table | `AddPmsWorkflowV2` | ✅ Done |
| Database Migration - TaskDeferralRequests OVERDUE fields | `AddOverdueDeferralFields` | ✅ Done |
| Database Migration - TaskStatusHistory table | `AddPmsWorkflowV2` | ✅ Done |
| `POST /tasks/:id/start` - Start task | `TaskWorkflowController.cs` | ✅ Done |
| `POST /tasks/:id/submit` - Submit task | `TaskWorkflowController.cs` | ✅ Done |
| `POST /tasks/:id/verify` - Approve/Reject | `TaskWorkflowController.cs` | ✅ Done |
| `POST /tasks/bulk-verify` - Bulk Approve/Reject | `TaskWorkflowController.cs` | ✅ Done |
| `GET /tasks/pending-approval` | `TaskWorkflowController.cs` | ✅ Done |
| `GET /tasks/rectify` | `TaskWorkflowController.cs` | ✅ Done |
| `GET /tasks/approval-summary` | `TaskWorkflowController.cs` | ✅ Done |
| `POST /deferral-requests` - Create deferral | `DeferralRequestController.cs` | ✅ Done |
| `POST /deferral-requests` - OVERDUE validation | `DeferralRequestController.cs` | ✅ Done |
| `POST /deferral-requests/:id/review` - Approve/Reject | `DeferralRequestController.cs` | ✅ Done |
| `POST /deferral-requests/:id/review` - OVERDUE status reset | `DeferralRequestController.cs` | ✅ Done |
| `GET /deferral-requests` - List deferrals | `DeferralRequestController.cs` | ✅ Done |
| DTOs for workflow | `DeferralDTOs.cs` | ✅ Done |
| DTOs for OVERDUE deferrals | `DeferralDTOs.cs` | ✅ Done |
| Auto-correct task statuses (Background Job) | `MaintenanceSchedulerService.cs` | ✅ Done |
| Task auto-generation from schedules | `MaintenanceSchedulerService.cs` | ✅ Done |
| 4-tier PIC assignment logic | `MaintenanceSchedulerService.cs` | ✅ Done |

#### Frontend Web
| Item | File/Location | Status |
|------|---------------|--------|
| Kanban - 8 columns (SCHEDULED→COMPLETED + DEFERRALS) | `KanbanBoard.tsx` | ✅ Done |
| Kanban - RECTIFY column | `KanbanBoard.tsx` | ✅ Done |
| Kanban - DEFERRALS column (virtual) | `KanbanBoard.tsx` | ✅ Done |
| Kanban - Drag validation rules (PMS v2.0) | `KanbanBoard.tsx` | ✅ Done |
| Kanban - Status colors | `KanbanBoard.tsx`, `KanbanCard.tsx` | ✅ Done |
| Task Card Indicators - hasPendingDeferral badge | `KanbanCard.tsx` | ✅ Done |
| Approval Dashboard Page | `ApprovalDashboardPage.tsx` | ✅ Done |
| Approval Dashboard - Clickable Pending Deferrals card | `ApprovalDashboardPage.tsx` | ✅ Done |
| ViewTaskModal - Full task details | `ViewTaskModal.tsx` | ✅ Done |
| ViewTaskModal - Approve button + notes | `ViewTaskModal.tsx` | ✅ Done |
| ViewTaskModal - Reject button + reason modal | `ViewTaskModal.tsx` | ✅ Done |
| ViewTaskModal - Deferral Information section | `ViewTaskModal.tsx` | ✅ Done |
| ViewTaskModal - Rectification Information section | `ViewTaskModal.tsx` | ✅ Done |
| ViewTaskModal - Link to Deferral Management | `ViewTaskModal.tsx` | ✅ Done |
| ColumnMenu - "Open Approval Queue" button | `ColumnMenu.tsx` | ✅ Done |
| ColumnMenu - "Manage Deferrals" button | `ColumnMenu.tsx` | ✅ Done |
| Deferral Management Page | `DeferralManagementPage.tsx` | ✅ Done |
| Create Deferral Modal | `CreateDeferralModal.tsx` | ✅ Done |
| Create Deferral Modal - OVERDUE validation | `CreateDeferralModal.tsx` | ✅ Done |
| Create Deferral Modal - Root Cause field | `CreateDeferralModal.tsx` | ✅ Done |
| Create Deferral Modal - Preventive Measures field | `CreateDeferralModal.tsx` | ✅ Done |
| Create Deferral Modal - Attachments field | `CreateDeferralModal.tsx` | ✅ Done |
| Types - MaintenanceTask updated (string id) | `maritime.types.ts` | ✅ Done |
| Types - New workflow types | `maritime.types.ts` | ✅ Done |
| Service - Workflow API methods | `maintenance.service.ts` | ✅ Done |
| Service - OVERDUE deferral DTOs | `maintenance.service.ts` | ✅ Done |
| Route - `/pms/deferrals` | `App.tsx` | ✅ Done |
| Route - `/pms/approval-dashboard` | `App.tsx` | ✅ Done |

---

### 11.2. ❌ CHƯA TRIỂN KHAI

#### 🌐 WEB (Frontend Edge)

| Priority | Item | Description |
|----------|------|-------------|
| 🟡 Medium | Task Card - isCms badge | Show 🔷 Class Survey Item badge |
| 🟡 Medium | Task Card - highRejectionCount badge | Show ⚠️ when rejectionCount >= 3 |
| 🟡 Medium | CMS Deferral Warning | Alert when CMS item deferred > 90 days |
| 🟢 Low | Class Permission Letter Upload | File upload for CMS deferrals > 90 days |
| 🟢 Low | Web Notifications | Browser notifications for new submissions |
| 🟢 Low | Sidebar - Add PMS menu items | Add Approval Dashboard, Deferrals to sidebar |

#### 📱 MOBILE (Flutter App)

| Priority | Item | Description |
|----------|------|-------------|
| 🔴 High | Task List Screen | Show DUE, OVERDUE, IN_PROGRESS, RECTIFY tasks |
| 🔴 High | Start Task Flow | Button "Start Task" → IN_PROGRESS |
| 🔴 High | Task Execution Screen | Checklist, photos, readings, spare parts |
| 🔴 High | Submit Report Flow | Validate checklist + photos → PENDING_APPROVAL |
| 🔴 High | Rectify Flow | View rejection reason → "Fix & Continue" → IN_PROGRESS |
| 🔴 High | Deferral Request | Form: reason, proposed date, attachments |
| 🟡 Medium | Photo Validation | Block submit if photos < required |
| 🟡 Medium | Offline Support | Queue submissions when offline |
| 🟡 Medium | Push Notifications | Task assigned, approved, rectify, deferral result |
| 🟢 Low | Morning Briefing | Daily summary at 07:00 |

#### 🔔 NOTIFICATIONS

| Priority | Item | Description |
|----------|------|-------------|
| 🔴 High | Push Service Setup | Firebase/OneSignal integration |
| 🔴 High | Mobile Push - Task Rectify | Notify crew when task rejected |
| 🔴 High | Mobile Push - Deferral Result | Notify crew when deferral approved/rejected |
| 🟡 Medium | Web Push - New Submission | Notify C/E when crew submits |
| 🟡 Medium | Web Push - Deferral Request | Notify C/E when crew requests deferral |
| 🟢 Low | Morning Briefing Scheduler | Daily summary at 07:00 |

#### 🧪 TESTING

| Priority | Item | Description |
|----------|------|-------------|
| 🟡 Medium | Unit Tests | Status transitions, validations |
| 🟡 Medium | Integration Tests | Full workflow E2E |
| 🟢 Low | E2E Tests | Web + Mobile flow |

---

### 11.3. IMPLEMENTATION PROGRESS

| Category | Completed | Total | Progress |
|----------|-----------|-------|----------|
| Backend API | 21 | 21 | **100%** ✅ |
| Frontend Web | 28 | 34 | **82%** |
| Mobile App | 0 | 10 | **0%** |
| Notifications | 0 | 6 | **0%** |
| Testing | 0 | 3 | **0%** |
| **OVERALL** | **49** | **74** | **~66%** |

---

### 11.4. PMS WORKFLOW v2.0 - STATUS FLOW

```
┌─────────────────────────────────────────────────────────────────┐
│                    PMS WORKFLOW v2.0                             │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│   SCHEDULED ──(auto: due date)──► DUE ──(auto: past due)──► OVERDUE
│       │                            │                          │
│       └────────────────────────────┴──────────────────────────┘
│                                    │
│                         POST /tasks/{id}/start (Mobile)
│                                    ▼
│                              IN_PROGRESS
│                                    │
│                         POST /tasks/{id}/submit (Mobile)
│                                    ▼
│                           PENDING_APPROVAL
│                                    │
│                    POST /tasks/{id}/verify (Web - C/E)
│                         ┌──────────┴──────────┐
│                         ▼                     ▼
│                    COMPLETED              RECTIFY
│                    (Approved)             (Rejected)
│                                               │
│                                  POST /tasks/{id}/start (Mobile)
│                                               ▼
│                                          IN_PROGRESS
│                                          (Loop back)
│                                                                  │
├─────────────────────────────────────────────────────────────────┤
│  DEFERRAL FLOW (Parallel):                                       │
│  Any status → POST /tasks/{id}/defer → hasPendingDeferral=true  │
│  C/E reviews → Approve: extend due date / Reject: keep date     │
└─────────────────────────────────────────────────────────────────┘
```

---

### 11.5. KANBAN BOARD COLUMNS (8 Columns)

| # | Column | Color | Description |
|---|--------|-------|-------------|
| 1 | SCHEDULED | Slate | Tasks not yet due |
| 2 | DUE | Blue | Tasks due today or ready |
| 3 | OVERDUE | Red | Tasks past due date |
| 4 | DEFERRALS | Yellow | Tasks with pending deferral (virtual) |
| 5 | IN_PROGRESS | Purple | Crew currently working |
| 6 | PENDING_APPROVAL | Amber | Waiting C/E verification |
| 7 | RECTIFY | Orange | Returned for correction |
| 8 | COMPLETED | Green | Approved and done |

---

### 11.6. KEY FILES REFERENCE

#### Backend (edge-services)
| File | Purpose |
|------|---------|
| `Controllers/TaskWorkflowController.cs` | Start, Submit, Verify, Bulk-verify APIs |
| `Controllers/DeferralRequestController.cs` | Deferral CRUD and Review APIs |
| `Controllers/MaintenanceController.cs` | Task CRUD, AutoCorrect statuses |
| `Services/MaintenanceSchedulerService.cs` | Background job: auto-generate tasks, auto-correct statuses |
| `DTOs/DeferralDTOs.cs` | All workflow-related DTOs |
| `Models/EdgeModels.cs` | MaintenanceTask, TaskDeferralRequest, TaskStatusHistory |

#### Frontend (frontend-edge)
| File | Purpose |
|------|---------|
| `components/maintenance/KanbanBoard.tsx` | 8-column Kanban with drag rules |
| `components/maintenance/KanbanCard.tsx` | Task card with status colors, badges |
| `components/maintenance/ViewTaskModal.tsx` | Task details + Approve/Reject |
| `components/maintenance/ColumnMenu.tsx` | Column actions (Approval Queue, Manage Deferrals) |
| `components/maintenance/CreateDeferralModal.tsx` | Create deferral request form |
| `pages/PMS/ApprovalDashboardPage.tsx` | C/E approval dashboard |
| `pages/PMS/DeferralManagementPage.tsx` | Deferral review page |
| `services/maintenance.service.ts` | API client for workflow endpoints |
| `types/maritime.types.ts` | TypeScript interfaces |

---

## 12. SCHEDULE AUTO-GENERATION LOGIC (Updated: 14/12/2025)

### 12.1. Lead Time Validation with Ceiling Rule

```csharp
private int ValidateAndCorrectLeadTime(int daysBeforeDue, string priority, 
    double? estimatedHours, int? intervalDays = null)
{
    var minimumLeadTime = GetMinimumLeadTime(priority);
    
    // SHORT INTERVALS (≤ 7 days): Proportional lead time
    if (intervalDays.HasValue && intervalDays.Value <= 7)
    {
        var proportionalLeadTime = Math.Max(1, intervalDays.Value / 2);
        
        if (daysBeforeDue != proportionalLeadTime)
        {
            _logger.LogWarning(
                "DaysBeforeDue {Configured} adjusted to proportional {Minimum} " +
                "for {Interval}-day interval. Short intervals require tight lead times.",
                daysBeforeDue, proportionalLeadTime, intervalDays.Value);
        }
        return proportionalLeadTime;
    }
    
    // LONG INTERVALS (> 7 days): ISM Code + Work-based + CEILING RULE
    var workDays = (int)Math.Ceiling((estimatedHours ?? 4) / 8.0);
    var workBasedMinimum = workDays * 3;
    var effectiveMinimum = Math.Max(minimumLeadTime, workBasedMinimum);
    
    // 🚨 CEILING RULE: Lead time MUST NOT exceed interval
    // Prevents task overlap (e.g., 14-day interval with 30-day lead time)
    if (intervalDays.HasValue)
    {
        var maxAllowedLeadTime = (int)Math.Floor(intervalDays.Value * 0.7);
        
        if (effectiveMinimum > maxAllowedLeadTime)
        {
            _logger.LogWarning(
                "ISM Code minimum {ISMMinimum} days for {Priority} priority " +
                "exceeds interval ceiling {Ceiling} days (70% of {Interval}-day interval). " +
                "Using ceiling to prevent task overlap.",
                effectiveMinimum, priority, maxAllowedLeadTime, intervalDays.Value);
            effectiveMinimum = Math.Max(1, maxAllowedLeadTime);
        }
    }
    
    if (daysBeforeDue < effectiveMinimum)
    {
        _logger.LogWarning(
            "DaysBeforeDue {Configured} is less than minimum {Minimum} " +
            "for {Priority} priority. Auto-correcting.",
            daysBeforeDue, effectiveMinimum, priority);
        return effectiveMinimum;
    }
    
    return daysBeforeDue;
}
```

### 12.2. ISM Code Priority Minimums

```csharp
private int GetMinimumLeadTime(string priority)
{
    return priority switch
    {
        "CRITICAL" => 30,  // 30 days
        "HIGH" => 14,      // 14 days
        "MEDIUM" => 10,    // 10 days
        "LOW" => 7,        // 7 days
        _ => 7
    };
}
```

### 12.3. Lead Time Calculation Examples

| Scenario | Interval | Priority | Est. Hours | Result | Logic Applied |
|----------|----------|----------|------------|--------|---------------|
| Daily inspection | 1 day | MEDIUM | 0.5h | **1 day** | Proportional (1 ÷ 2 = 1) |
| Weekly check | 7 days | LOW | 2h | **3 days** | Proportional (7 ÷ 2 = 3) |
| Bi-weekly maintenance | 14 days | CRITICAL | 4h | **9 days** | Ceiling (70% of 14) |
| Monthly service | 30 days | HIGH | 8h | **14 days** | ISM Code (HIGH min) |
| Quarterly overhaul | 90 days | CRITICAL | 16h | **30 days** | ISM Code (CRITICAL min) |

**Key Rules:**
- ✅ Short intervals (≤7 days): Lead time = 50% of interval
- ✅ Long intervals (>7 days): ISM Code minimum OR work-based, capped at 70% of interval
- ✅ Ceiling prevents task overlap (next task won't appear before previous completes)

---

## 13. MASTER SCHEDULE (GANTT CHART) - Updated: 14/12/2025

### 13.1. View Modes

| Mode | Days Shown | Start Date | Use Case |
|------|------------|------------|----------|
| **Day** | 7 days | Current date | Detailed daily planning |
| **Week** | 14 days | Current date | 2-week overview |
| **Month** | 60 days | Start of month | Monthly planning |
| **Quarter** | 90 days | Start of month | Quarterly overview |

### 13.2. Date Marker Positioning

```typescript
// Start Marker (Green) - Beginning of day
getDatePosition(startDate, days, 'start')  // Left edge of column

// Due Marker (Blue) - End of day (deadline)
getDatePosition(dueDate, days, 'end')      // Right edge of column

// Today Line (Blue vertical) - Center of day
getDatePosition(today, days, 'center')     // Middle of column
```

### 13.3. Work Bar Calculation

```typescript
// Bar spans from start of Start Day to end of Due Day
const width = (endIdx - startIdx + 1) * columnWidth;

// Visual padding to prevent overflow
return {
  start: startIdx * columnWidth,
  width: Math.max(columnWidth * 0.8, width)
};
```

**Example:** 
- Start: Dec 14, Due: Dec 15
- Bar starts at left edge of Dec 14 column
- Bar ends at right edge of Dec 15 column
- Full 2-column span representing 1-day lead time + 1-day work period

### 13.4. Date Range Display

```typescript
// Day view: "Dec 14 - Dec 20, 2025"
// Week view: "Dec 14 - Dec 27, 2025"  
// Month view: "Dec 1 - Jan 29, 2026"
// Quarter view: "Dec 1 - Feb 28, 2026"
```

---

**Document Version:** 2.2  
**Last Updated:** 14/12/2025 (Evening)  
**Approved By:** Technical Team Lead  
**Changes:** Added Ceiling Rule, Master Schedule enhancements, Day view mode  
**Next Review:** 21/12/2025
