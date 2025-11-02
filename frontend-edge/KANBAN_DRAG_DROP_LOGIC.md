# Kanban Drag & Drop Logic - Hoàn chỉnh

## Tóm tắt
Logic kéo thả task giữa các column đã được hoàn thiện với validation đầy đủ và UI toast notifications (sonner).

## Status Flow Matrix

| From → To | PENDING | IN_PROGRESS | COMPLETED | OVERDUE |
|-----------|---------|-------------|-----------|---------|
| **PENDING** | ✅ Same column (no API) | ⚠️ **Confirm required** | ❌ Blocked | ❌ Blocked |
| **IN_PROGRESS** | ⚠️ **Confirm required** | ✅ Same column (no API) | ❌ Blocked (crew only) | ❌ Blocked |
| **COMPLETED** | ❌ Blocked | ❌ Blocked | ✅ Same column (no API) | ❌ Blocked |
| **OVERDUE** | ❌ Blocked | ⚠️ **Confirm required** | ❌ Blocked | ✅ Same column (no API) |

## Chi tiết Rules

### ✅ Hợp lệ - Cần Confirm (3 cases)

#### 1. PENDING → IN_PROGRESS (Rule 6)
```
Toast: "Giao task cho thuyền viên bắt đầu?"
Description: "Bạn muốn chuyển task này sang IN PROGRESS?"
Action: Click "Xác nhận" → Gọi API update status
Success: Toast "✅ Task đã được chuyển sang IN PROGRESS"
```

#### 2. IN_PROGRESS → PENDING (Rule 9)
```
Toast: "Chuyển task đang thực hiện về PENDING?"
Description: "Task đã được thuyền viên bắt đầu. Bạn có chắc muốn hủy giao? StartedAt sẽ được giữ."
Action: Click "Xác nhận" → Gọi API update status
Success: Toast "✅ Task đã được chuyển về PENDING"
```

#### 3. OVERDUE → IN_PROGRESS (Rule 4)
```
Toast: "Giao task quá hạn cho thuyền viên?"
Description: "Task đã quá hạn. Bạn muốn giao cho thuyền viên bắt đầu ngay?"
Action: Click "Xác nhận" → Gọi API update status
Success: Toast "✅ Task đã được chuyển sang IN PROGRESS"
```

### ❌ Bị chặn (7 cases)

1. **COMPLETED → Anywhere** (Rule 1): Task đã hoàn thành không thể di chuyển
2. **Anywhere → OVERDUE** (Rule 2): Hệ thống tự động set, không thể manual
3. **OVERDUE → PENDING** (Rule 3): Chỉ backend tự động khi update due date
4. **OVERDUE → COMPLETED** (Rule 5): Phải qua IN_PROGRESS trước
5. **PENDING → COMPLETED** (Rule 7): Phải qua IN_PROGRESS trước
6. **IN_PROGRESS → COMPLETED** (Rule 8): Chỉ crew mobile app mới được complete
7. **IN_PROGRESS → OVERDUE** (Rule 10): Hệ thống tự động xử lý

## Technical Implementation

### Files Changed
1. `KanbanBoard.tsx`: Validation logic + sonner toast integration
2. `MaintenancePage.tsx`: Import toast, replace alert() với toast.error()
3. `App.tsx`: Added `<Toaster position="top-right" />` provider

### Toast Actions
```typescript
toast('Message', {
  description: 'Details...',
  action: {
    label: 'Xác nhận',
    onClick: async () => {
      await onTaskUpdate(taskId, newStatus)
      toast.success('✅ Success message')
    }
  }
})
```

### Error Handling
- Client-side validation: Toast error với message rõ ràng
- Server-side error: Catch trong MaintenancePage, show toast error
- Auto-reload sau error để sync UI với DB

## Testing Checklist

- [ ] Kéo PENDING → IN_PROGRESS: Hiện toast confirm ✅
- [ ] Click "Xác nhận": Task chuyển sang IN_PROGRESS ✅
- [ ] Kéo IN_PROGRESS → PENDING: Hiện toast confirm ✅
- [ ] Click "Xác nhận": Task chuyển về PENDING ✅
- [ ] Kéo OVERDUE → IN_PROGRESS: Hiện toast confirm ✅
- [ ] Kéo PENDING → COMPLETED: Hiện toast error (blocked) ❌
- [ ] Kéo IN_PROGRESS → COMPLETED: Hiện toast error (crew only) ❌
- [ ] Kéo COMPLETED → Anywhere: Hiện toast error (blocked) ❌
- [ ] Kéo task trong cùng column: Không gọi API (reorder only) ✅

## Notes
- Sonner toast auto-dismiss sau 3-5s
- Toast confirm có button action, không block UI như alert()
- Background refresh (10s) đảm bảo sync với mobile changes
- StartedAt timestamp được giữ lại khi IN_PROGRESS → PENDING
