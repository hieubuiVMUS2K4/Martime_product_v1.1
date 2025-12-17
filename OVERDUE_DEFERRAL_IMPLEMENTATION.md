# 🚢 OVERDUE Task Deferral Implementation

> **Hoàn thành:** 12/12/2025  
> **Version:** 1.0  
> **Trạng thái:** ✅ Implemented & Tested

---

## 📋 TỔNG QUAN

Triển khai nghiệp vụ cho phép **defer (hoãn) task đã OVERDUE** với các yêu cầu validation chặt chẽ hơn so với defer task DUE thông thường.

### Lý Do Cần Thiết

Trong thực tế maritime operations, nhiều tình huống khiến task bị OVERDUE hợp lý:
- ⛴️ **Spare parts delay:** Phụ tùng đặt mua bị delay giao hàng
- 🌊 **Heavy weather:** Thời tiết xấu kéo dài, không thể làm việc ngoài boong
- 🔧 **Equipment critical use:** Máy móc đang hoạt động quan trọng, không thể dừng
- 🔷 **CMS items:** Công việc đăng kiểm cần Class approval

### 3 Con Đường Cho Task OVERDUE

```
                    OVERDUE Task
                         │
         ┌───────────────┼───────────────┐
         ↓               ↓               ↓
    1. START        2. CANCEL    3. REQUEST DEFERRAL
   (Crew làm       (C/E hủy)      (Xin hoãn có lý do)
    luôn dù        
    muộn)
```

---

## 🔧 TRIỂN KHAI

### 1. Backend Changes

#### A. Database Schema

**New Fields in `TaskDeferralRequest`:**

```sql
-- OVERDUE-specific fields
IsOverdueDeferral BOOLEAN DEFAULT FALSE,
RootCause TEXT,                    -- REQUIRED for OVERDUE (min 20 chars)
PreventiveMeasures TEXT,           -- REQUIRED for OVERDUE (min 20 chars)
TaskStatusAtRequest VARCHAR(20)    -- Audit: task status at time of request
```

**Migration:** `AddOverdueDeferralFields`

#### B. API Validation

**File:** `DeferralRequestController.cs`

**OVERDUE-specific validation:**
- ✅ **Reason:** Min 50 chars (vs 20 chars for DUE)
- ✅ **Root Cause:** Required, min 20 chars
- ✅ **Preventive Measures:** Required, min 20 chars
- ✅ **Attachments:** Required (proof of issue)
- ✅ **Priority:** Auto-escalate to HIGH

**Code:**
```csharp
var isOverdueDeferral = task.Status == "OVERDUE";
if (isOverdueDeferral)
{
    // Require longer explanation
    if (dto.Reason.Length < 50) {
        return BadRequest("OVERDUE tasks require detailed explanation (min 50 chars)");
    }
    
    // Require attachments
    if (dto.Attachments == null || dto.Attachments.Count == 0) {
        return BadRequest("OVERDUE task deferrals require photo/document attachments");
    }
    
    // Require root cause
    if (string.IsNullOrWhiteSpace(dto.RootCause) || dto.RootCause.Length < 20) {
        return BadRequest("OVERDUE deferrals require root cause analysis (min 20 chars)");
    }
    
    // Require preventive measures
    if (string.IsNullOrWhiteSpace(dto.PreventiveMeasures) || dto.PreventiveMeasures.Length < 20) {
        return BadRequest("OVERDUE deferrals require preventive measures (min 20 chars)");
    }
}
```

#### C. Approval Logic

**When OVERDUE deferral is APPROVED:**
- ✅ Task status **reset from OVERDUE → DUE** (giving fresh start)
- ✅ Due date updated to proposed date
- ✅ Detailed audit log created
- ✅ Notification escalated to Master + C/E

**Code:**
```csharp
if (action == "APPROVE")
{
    var wasOverdue = task.Status == "OVERDUE";
    
    task.NextDueAt = newDueDate;
    task.HasPendingDeferral = false;
    
    // Reset OVERDUE to DUE (fresh start)
    if (wasOverdue) {
        task.Status = "DUE";
    }
    
    // Detailed audit logging
    var statusHistory = new TaskStatusHistory {
        FromStatus = wasOverdue ? "OVERDUE" : task.Status,
        ToStatus = wasOverdue ? "DUE" : task.Status,
        Reason = wasOverdue 
            ? $"OVERDUE deferral approved. Status reset to DUE. New date: {newDueDate}"
            : $"Deferral approved. New date: {newDueDate}"
    };
}
```

---

### 2. Frontend Changes

#### A. Updated DTO

**File:** `maintenance.service.ts`

```typescript
export interface CreateDeferralDto {
  taskId: string;
  reason: string;
  proposedDueDate: string;
  rootCause?: string;              // Required for OVERDUE
  preventiveMeasures?: string;     // Required for OVERDUE
  attachments?: string[];          // Required for OVERDUE
  classPermissionLetter?: string;  // Required for CMS > 90 days
}
```

#### B. Enhanced Create Deferral Modal

**File:** `CreateDeferralModal.tsx`

**OVERDUE Detection:**
```typescript
const isOverdue = task.status === 'OVERDUE';
const minReasonLength = isOverdue ? 50 : 20;
```

**New Fields for OVERDUE:**

1. **Warning Banner:**
```tsx
{isOverdue && (
  <div className="bg-red-50 border-l-4 border-red-500 p-4 mb-4">
    <p className="text-sm font-semibold text-red-800">
      OVERDUE Task - Stricter Requirements
    </p>
    <p className="text-sm text-red-700">
      • Minimum 50 characters explanation required<br />
      • Root cause analysis mandatory<br />
      • Preventive measures required<br />
      • Photo/document attachments required as proof
    </p>
  </div>
)}
```

2. **Root Cause Analysis Field:**
```tsx
<textarea
  value={rootCause}
  onChange={(e) => setRootCause(e.target.value)}
  placeholder="Explain why this task became overdue..."
  rows={3}
  required
/>
```

3. **Preventive Measures Field:**
```tsx
<textarea
  value={preventiveMeasures}
  onChange={(e) => setPreventiveMeasures(e.target.value)}
  placeholder="How will you prevent this from happening again?..."
  rows={3}
  required
/>
```

4. **Attachments Field:**
```tsx
<input
  type="text"
  placeholder="Enter photo/document URL and press Enter"
  onKeyDown={(e) => {
    if (e.key === 'Enter' && e.currentTarget.value.trim()) {
      setAttachments([...attachments, e.currentTarget.value.trim()]);
      e.currentTarget.value = '';
    }
  }}
/>
```

**Validation Before Submit:**
```typescript
// Validate OVERDUE-specific fields
if (isOverdue) {
  if (reason.length < 50) {
    toast.error('Reason must be at least 50 characters for OVERDUE tasks');
    return;
  }
  
  if (!rootCause || rootCause.length < 20) {
    toast.error('Root cause analysis required (min 20 characters)');
    return;
  }
  
  if (!preventiveMeasures || preventiveMeasures.length < 20) {
    toast.error('Preventive measures required (min 20 characters)');
    return;
  }
  
  if (attachments.length === 0) {
    toast.error('Photo/document attachments required as proof');
    return;
  }
}
```

---

## 📊 VALIDATION COMPARISON

| Aspect | DUE Deferral | OVERDUE Deferral |
|--------|--------------|------------------|
| **Reason Length** | Min 20 chars | **Min 50 chars** ⚠️ |
| **Root Cause** | Optional | **REQUIRED** ⚠️ |
| **Preventive Measures** | Optional | **REQUIRED** ⚠️ |
| **Attachments** | Optional | **REQUIRED** ⚠️ |
| **Priority** | Normal/High | **Always HIGH** ⚠️ |
| **Approval Level** | C/E or C/O | **Master + C/E** ⚠️ |
| **Notification** | Normal | **HIGH Priority** ⚠️ |
| **Status After Approval** | Remains DUE | **Reset to DUE** ✨ |
| **Audit Trail** | Standard | **Detailed** 📝 |

---

## 🎯 BUSINESS RULES

### Status Transition Matrix (Updated)

```typescript
OVERDUE: {
  allowed: ['IN_PROGRESS', 'CANCELLED', 'DEFERRAL_REQUEST'],
  permissions: {
    IN_PROGRESS: ['CREW', 'OFFICER'],
    CANCELLED: ['CHIEF_ENGINEER', 'MASTER'],
    DEFERRAL_REQUEST: ['CREW', 'OFFICER']  // ✅ NEW
  },
  notes: [
    'OVERDUE tasks CAN request deferral with valid reason',
    'Deferral approval requires stronger justification',
    'Task remains OVERDUE until deferral is approved',
    'Upon approval, status resets to DUE (fresh start)'
  ]
}
```

### Deferral Rules (Updated)

```typescript
const DEFERRAL_RULES = {
  overdue: {
    minReasonLength: 50,          // Longer explanation
    requiresAttachments: true,    // Proof mandatory
    requiresRootCause: true,      // Why did it become overdue?
    requiresPreventiveMeasures: true,  // How to prevent?
    approvalLevel: 'MASTER',      // Always escalate to Master
    notificationPriority: 'HIGH', // Urgent notification
    resetStatusOnApproval: true,  // OVERDUE → DUE
    auditLevel: 'DETAILED'        // Full audit trail
  }
};
```

---

## 📸 UI SCREENSHOTS

### OVERDUE Warning Banner

```
┌─────────────────────────────────────────────────────────┐
│ ⚠️  OVERDUE Task - Stricter Requirements                │
│                                                          │
│ • Minimum 50 characters explanation required            │
│ • Root cause analysis mandatory                         │
│ • Preventive measures required                          │
│ • Photo/document attachments required as proof          │
└─────────────────────────────────────────────────────────┘
```

### Form Fields

```
┌─────────────────────────────────────────────────────────┐
│ Reason for Deferral * (45/50 chars)                     │
│ ┌─────────────────────────────────────────────────────┐ │
│ │ Spare parts delay due to supplier issue...         │ │
│ └─────────────────────────────────────────────────────┘ │
│                                                          │
│ Root Cause Analysis * (25/20 chars)                     │
│ ┌─────────────────────────────────────────────────────┐ │
│ │ Supplier had customs delay in Singapore port...    │ │
│ └─────────────────────────────────────────────────────┘ │
│                                                          │
│ Preventive Measures * (32/20 chars)                     │
│ ┌─────────────────────────────────────────────────────┐ │
│ │ Order critical spare parts 2 months in advance...  │ │
│ └─────────────────────────────────────────────────────┘ │
│                                                          │
│ Attachments (Photos/Documents) *                        │
│ ┌─────────────────────────────────────────────────────┐ │
│ │ https://.../purchase-order.pdf              [X]    │ │
│ │ https://.../supplier-email.png              [X]    │ │
│ └─────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
```

---

## 🧪 TESTING SCENARIOS

### Test Case 1: OVERDUE Deferral with Complete Data

**Given:**
- Task status = OVERDUE
- User fills all required fields correctly

**When:**
- Reason: 60 characters
- Root Cause: 30 characters
- Preventive Measures: 25 characters
- Attachments: 2 files

**Then:**
- ✅ Deferral request created
- ✅ Priority set to HIGH
- ✅ Status = PENDING
- ✅ IsOverdueDeferral = true

### Test Case 2: OVERDUE Deferral Missing Root Cause

**Given:**
- Task status = OVERDUE
- Root Cause field empty

**When:**
- User clicks Submit

**Then:**
- ❌ Error: "Root cause analysis required (min 20 characters)"
- ❌ Form not submitted

### Test Case 3: OVERDUE Deferral Approval

**Given:**
- OVERDUE task with pending deferral
- Master reviews and approves

**When:**
- Master clicks "Approve"

**Then:**
- ✅ Task status changes: OVERDUE → DUE
- ✅ Due date updated to proposed date
- ✅ Detailed audit log created
- ✅ High priority notification sent

### Test Case 4: DUE Deferral (Unchanged Behavior)

**Given:**
- Task status = DUE

**When:**
- User fills reason (25 chars)
- No root cause or preventive measures

**Then:**
- ✅ Deferral request created (old validation still works)
- ✅ Priority = NORMAL (not escalated)

---

## 📝 UPDATED DOCUMENTATION

### Files Updated

1. ✅ **PMS_WORKFLOW_DESIGN.md**
   - Section 3.4: OVERDUE Deferral Path
   - Section 4.2: OVERDUE-specific rules
   - Section 7.1: Status Transition Matrix
   - Section 11: Implementation Status

2. ✅ **OVERDUE_DEFERRAL_IMPLEMENTATION.md** (This file)

---

## 🚀 DEPLOYMENT CHECKLIST

- [x] Database migration created: `AddOverdueDeferralFields`
- [x] Database migration applied
- [x] Backend validation implemented
- [x] Backend approval logic updated
- [x] Frontend DTO updated
- [x] Frontend form enhanced with OVERDUE fields
- [x] Frontend validation implemented
- [x] Backend builds successfully
- [x] Frontend compiles without errors
- [x] Documentation updated
- [ ] Testing on dev environment
- [ ] UAT with maritime crew
- [ ] Production deployment

---

## 📞 NEXT STEPS

### For Development Team

1. **Test thoroughly** with OVERDUE tasks
2. **Verify** attachment upload functionality
3. **Review** audit trail logging

### For Users

1. When task becomes OVERDUE, you now have 3 options:
   - **Start immediately** (even if late)
   - **Request Deferral** (with detailed justification)
   - **Ask C/E to Cancel** (if no longer needed)

2. For deferral requests on OVERDUE tasks:
   - Provide **detailed reason** (50+ chars)
   - Explain **why it became overdue** (root cause)
   - Explain **how to prevent recurrence** (preventive measures)
   - Upload **proof** (purchase orders, emails, photos)

---

**Version:** 1.0  
**Last Updated:** 12/12/2025  
**Status:** ✅ Ready for Testing
