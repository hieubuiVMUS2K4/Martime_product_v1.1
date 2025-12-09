# UI/UX Improvements - Equipment Groups & Assets

## Yêu cầu từ người dùng
1. ✅ **Equipment Groups**: Thêm cột Category, mở rộng bảng (đang hơi trống 2 bên), đổi PIC Role từ text input → dropdown
2. ✅ **Equipment Assets**: Thêm nút "Add Group" khi chọn Equipment Group để tăng trải nghiệm người dùng

---

## Thay đổi đã thực hiện

### 1. Equipment Groups Page (EquipmentGroupsPage.tsx)

#### A. Thêm cột Category vào bảng
**Trước:**
- Chỉ có 5 cột: Code, Name, Department, PIC, Status, Actions
- Category hiển thị nhỏ dưới Name (màu xám)

**Sau:**
- 7 cột: **Code, Name, Category (mới), Department, PIC, Status, Actions**
- Category là cột riêng biệt, dễ nhìn và sort

```tsx
<thead>
  <tr>
    <th className="w-32">Code</th>
    <th>Name</th>
    <th className="w-40">Category</th>  {/* ← THÊM MỚI */}
    <th className="w-40">Department</th>
    <th className="w-32">PIC</th>
    <th className="w-28">Status</th>
    <th className="w-24">Actions</th>
  </tr>
</thead>
```

#### B. Mở rộng bảng ra toàn màn hình
**Trước:**
```tsx
<table className="w-full">  {/* Không xác định width cho columns */}
```

**Sau:**
```tsx
<table className="min-w-full w-full">  {/* Force full width */}
```

Thêm width constraints cho từng cột:
- Code: `w-32` (8rem)
- Category: `w-40` (10rem)
- Department: `w-40` (10rem)
- PIC: `w-32` (8rem)
- Status: `w-28` (7rem)
- Actions: `w-24` (6rem)
- Name: Flexible (chiếm phần còn lại)

#### C. Đổi PIC Role từ text input → dropdown

**Trước (Text Input):**
```tsx
<input
  type="text"
  value={formData.picRole}
  onChange={(e) => setFormData({ ...formData, picRole: e.target.value })}
  placeholder="e.g., 2/E, C/O, E/O"
/>
<p className="text-xs text-gray-500 mt-1">
  Examples: 2/E (Second Engineer), C/O (Chief Officer), 3/E, E/O
</p>
```

**Sau (Dropdown với Department Filtering):**
```tsx
<select
  value={formData.picRole}
  onChange={(e) => setFormData({ ...formData, picRole: e.target.value })}
  disabled={!formData.department}  // Bắt buộc chọn Department trước
>
  <option value="">Select PIC role...</option>
  {filteredPicRoles.map(role => (
    <option key={role.value} value={role.value}>{role.label}</option>
  ))}
</select>
<p className="text-xs text-gray-500 mt-1">
  {formData.department 
    ? `Showing roles for ${formData.department} department`
    : 'Select a department first to filter roles'}
</p>
```

#### D. Thêm PIC_ROLES constant với Department filtering

```tsx
const PIC_ROLES = [
  { value: 'MASTER', label: 'MASTER - Thuyền trưởng', departments: ['MANAGEMENT', 'DECK', 'NAVIGATION'] },
  { value: 'C/E', label: 'C/E - Máy trưởng', departments: ['ENGINE', 'MANAGEMENT'] },
  { value: 'C/O', label: 'C/O - Đại phó', departments: ['DECK', 'NAVIGATION', 'MANAGEMENT'] },
  { value: '2/E', label: '2/E - Máy hai', departments: ['ENGINE'] },
  { value: '3/E', label: '3/E - Máy ba', departments: ['ENGINE'] },
  { value: '4/E', label: '4/E - Máy bốn', departments: ['ENGINE'] },
  { value: 'E/O', label: 'E/O - Sĩ quan điện', departments: ['ELECTRICAL', 'ENGINE'] },
  { value: '2/O', label: '2/O - Sĩ quan hai', departments: ['DECK', 'NAVIGATION'] },
  { value: '3/O', label: '3/O - Sĩ quan ba', departments: ['DECK', 'NAVIGATION'] },
  { value: 'BOSUN', label: 'BOSUN - Thủy thủ trưởng', departments: ['DECK'] },
  { value: 'FITTER', label: 'FITTER - Thợ cơ khí', departments: ['ENGINE'] },
  { value: 'OILER', label: 'OILER - Thợ dầu', departments: ['ENGINE'] },
  { value: 'AB', label: 'AB - Thủy thủ thành thạo', departments: ['DECK'] },
  { value: 'OS', label: 'OS - Thủy thủ phổ thông', departments: ['DECK'] },
  { value: 'COOK', label: 'COOK - Đầu bếp', departments: ['CATERING'] },
];
```

#### E. Smart Filtering Logic

```tsx
// Filter PIC roles by selected department
const filteredPicRoles = formData.department
  ? PIC_ROLES.filter(role => role.departments.includes(formData.department))
  : PIC_ROLES;
```

**Ví dụ:**
- Chọn Department = `ENGINE` → Chỉ hiện: C/E, 2/E, 3/E, 4/E, E/O, FITTER, OILER
- Chọn Department = `DECK` → Chỉ hiện: MASTER, C/O, 2/O, 3/O, BOSUN, AB, OS
- Chọn Department = `CATERING` → Chỉ hiện: COOK

#### F. Auto-reset khi đổi Department

```tsx
// Reset both picRole and picCrewId when department changes
onChange={(e) => setFormData({ 
  ...formData, 
  department: e.target.value, 
  picRole: '',      // ← Reset
  picCrewId: ''     // ← Reset
})}
```

---

### 2. Equipment Assets - Add Asset Modal (AddAssetModal.tsx)

#### A. Thêm nút "Add Group" bên cạnh dropdown Equipment Groups

**Trước:**
```tsx
<label className="block text-sm font-medium text-gray-700 mb-2">
  Equipment Groups
</label>
<div className="border border-gray-300 rounded-lg p-3 max-h-40 overflow-y-auto">
  {/* Checkboxes */}
</div>
```

**Sau:**
```tsx
<div className="flex items-center justify-between mb-2">
  <label className="block text-sm font-medium text-gray-700">
    Equipment Groups
  </label>
  <button
    type="button"
    onClick={() => window.open('/pms/groups', '_blank')}
    className="text-xs text-blue-600 hover:text-blue-700 flex items-center gap-1 px-2 py-1 rounded hover:bg-blue-50 transition-colors"
    title="Open Equipment Groups in new tab"
  >
    <Plus className="w-3 h-3" />
    Add Group
  </button>
</div>
<div className="border border-gray-300 rounded-lg p-3 max-h-40 overflow-y-auto">
  {/* Checkboxes */}
</div>
```

#### B. Behavior của nút "Add Group"

**Khi click:**
1. Mở Equipment Groups page trong **tab mới** (`window.open('/pms/groups', '_blank')`)
2. User có thể tạo group mới
3. Quay lại tab Add Asset, **reload groups** (có thể F5 hoặc tự động)
4. Chọn group vừa tạo từ danh sách

**Lợi ích UX:**
- ✅ Không cần đóng modal Add Asset
- ✅ Không mất dữ liệu đang nhập
- ✅ Workflow liền mạch: Add Group → Select Group → Add Asset
- ✅ Tuân theo nguyên tắc "Don't make me think"

#### C. Import Plus icon

```tsx
import { X, Plus } from 'lucide-react';  // ← Thêm Plus
```

---

## So sánh Before/After

### Equipment Groups Table

#### Before:
```
┌─────────────┬────────────────────┬─────────────┬──────────┬────────┬─────────┐
│ Code        │ Name               │ Department  │ PIC      │ Status │ Actions │
├─────────────┼────────────────────┼─────────────┼──────────┼────────┼─────────┤
│ ENG-ME      │ Main Engine        │ ENGINE      │ 2/E      │ Active │ ✏️ 🗑️   │
│             │ ENGINE             │             │          │        │         │
└─────────────┴────────────────────┴─────────────┴──────────┴────────┴─────────┘
```
- Category "ENGINE" hiển thị nhỏ dưới Name (khó thấy)
- Bảng không rộng, có khoảng trống 2 bên
- PIC Role là text input (dễ nhập sai)

#### After:
```
┌──────────┬─────────────┬──────────┬────────────┬─────┬────────┬─────────┐
│ Code     │ Name        │ Category │ Department │ PIC │ Status │ Actions │
├──────────┼─────────────┼──────────┼────────────┼─────┼────────┼─────────┤
│ ENG-ME   │ Main Engine │ ENGINE   │ ENGINE     │ 2/E │ Active │ ✏️ 🗑️   │
└──────────┴─────────────┴──────────┴────────────┴─────┴────────┴─────────┘
```
- ✅ Category là cột riêng, dễ nhìn và sort
- ✅ Bảng mở rộng toàn màn hình (min-w-full)
- ✅ Columns có width cố định, không bị squeeze
- ✅ PIC Role là dropdown (chỉ hiện ranks phù hợp với Department)

### Equipment Assets - Add Asset Modal

#### Before:
```
Equipment Groups
─────────────────────────────────────
☐ All Generators (GRP-GEN-ALL)
☐ Main Engine (ENG-ME)
☐ All Pumps (GRP-PUMP-ALL)
```

#### After:
```
Equipment Groups                [+ Add Group]
─────────────────────────────────────────────
☐ All Generators (GRP-GEN-ALL)
☐ Main Engine (ENG-ME)
☐ All Pumps (GRP-PUMP-ALL)
```

**Workflow:**
1. Click "Add Group" → Opens `/pms/groups` in new tab
2. Create new group in Equipment Groups page
3. Return to Add Asset modal
4. (Optional) Reload/refresh to see new group
5. Select the newly created group

---

## Testing Checklist

### Equipment Groups Page

#### Test 1: Table Display
- [ ] Bảng hiển thị 7 cột (Code, Name, Category, Department, PIC, Status, Actions)
- [ ] Cột Category hiển thị tên category (ENGINE, GENERATOR, etc.)
- [ ] Bảng mở rộng toàn màn hình, không có khoảng trống 2 bên
- [ ] Columns có width cố định, không bị squeeze khi resize browser

#### Test 2: PIC Role Dropdown (No Department Selected)
1. Click "Add Group"
2. Không chọn Department
3. **Expected**: PIC Role dropdown bị disabled với message "Select a department first to filter roles"

#### Test 3: PIC Role Dropdown (ENGINE Department)
1. Click "Add Group"
2. Chọn Department = `ENGINE`
3. Mở PIC Role dropdown
4. **Expected**: Chỉ hiện các ranks:
   - C/E - Máy trưởng
   - 2/E - Máy hai
   - 3/E - Máy ba
   - 4/E - Máy bốn
   - E/O - Sĩ quan điện
   - FITTER - Thợ cơ khí
   - OILER - Thợ dầu

#### Test 4: PIC Role Dropdown (DECK Department)
1. Chọn Department = `DECK`
2. Mở PIC Role dropdown
3. **Expected**: Chỉ hiện các ranks:
   - MASTER - Thuyền trưởng
   - C/O - Đại phó
   - 2/O - Sĩ quan hai
   - 3/O - Sĩ quan ba
   - BOSUN - Thủy thủ trưởng
   - AB - Thủy thủ thành thạo
   - OS - Thủy thủ phổ thông

#### Test 5: Auto-reset khi đổi Department
1. Chọn Department = `ENGINE`
2. Chọn PIC Role = `2/E`
3. Đổi Department = `DECK`
4. **Expected**: 
   - PIC Role tự động reset về empty ""
   - PIC Crew tự động reset về empty ""
   - Dropdown PIC Role hiện ranks của DECK

#### Test 6: Create Group với PIC Role từ dropdown
1. Click "Add Group"
2. Nhập:
   - Group Code: `TEST-ENG-01`
   - Group Name: `Test Engine Group`
   - Category: `ENGINE`
   - Department: `ENGINE`
   - PIC Role: `2/E` (chọn từ dropdown)
   - PIC Crew: Chọn crew from ENGINE dept
3. Click "Create Group"
4. **Expected**: 
   - Group được tạo thành công
   - Table hiển thị group mới với Category trong cột riêng
   - PIC hiển thị "2/E" + crew ID

### Equipment Assets Page

#### Test 7: Nút "Add Group" hiển thị
1. Navigate to Equipment Assets page
2. Click "Add Asset" button
3. Scroll to "Equipment Groups" section
4. **Expected**: 
   - Thấy label "Equipment Groups" bên trái
   - Thấy button "+ Add Group" (màu xanh) bên phải
   - Button có icon Plus và text "Add Group"

#### Test 8: Click nút "Add Group"
1. Trong modal "Add Equipment Asset"
2. Click button "+ Add Group"
3. **Expected**:
   - Equipment Groups page mở trong tab mới
   - Modal "Add Equipment Asset" vẫn mở (không đóng)
   - Data trong form vẫn giữ nguyên

#### Test 9: Workflow tạo Group rồi chọn Group
1. Mở modal "Add Equipment Asset"
2. Nhập Asset Code, Asset Name
3. Click "+ Add Group" → Opens Equipment Groups page
4. Tạo group mới "TEST-NEW-GROUP"
5. Quay lại tab Add Asset modal
6. Reload page hoặc close/reopen modal
7. **Expected**: "TEST-NEW-GROUP" xuất hiện trong danh sách checkboxes

---

## Technical Details

### Files Modified

1. **frontend-edge/src/pages/PMS/EquipmentGroupsPage.tsx**
   - Added `PIC_ROLES` constant (15 ranks with department mappings)
   - Added `filteredPicRoles` computed property
   - Changed table to 7 columns with Category
   - Changed PIC Role from `<input>` to `<select>`
   - Updated Department onChange to reset picRole + picCrewId
   - Added column widths: `w-32`, `w-40`, `w-28`, `w-24`

2. **frontend-edge/src/components/pms/AddAssetModal.tsx**
   - Added `Plus` icon import from lucide-react
   - Changed Equipment Groups label from single element to flex container
   - Added "Add Group" button with `window.open('/pms/groups', '_blank')`
   - Button styled as small blue link-like button

### CSS Classes Used

```tsx
// Table widths
className="min-w-full w-full"
className="w-32"  // Code, PIC (8rem)
className="w-40"  // Category, Department (10rem)
className="w-28"  // Status (7rem)
className="w-24"  // Actions (6rem)

// Add Group button
className="text-xs text-blue-600 hover:text-blue-700 flex items-center gap-1 px-2 py-1 rounded hover:bg-blue-50 transition-colors"
```

### Department to PIC Role Mapping

| Department    | Available Ranks |
|--------------|----------------|
| ENGINE       | C/E, 2/E, 3/E, 4/E, E/O, FITTER, OILER |
| DECK         | MASTER, C/O, 2/O, 3/O, BOSUN, AB, OS |
| NAVIGATION   | MASTER, C/O, 2/O, 3/O |
| ELECTRICAL   | E/O |
| MANAGEMENT   | MASTER, C/E, C/O |
| CATERING     | COOK |

---

## Build Status

✅ **Frontend**: `npm run build` - 0 errors, 0 warnings
```
✓ 2763 modules transformed.
dist/assets/index-BQpg6XvD.js   1,362.97 kB │ gzip: 340.71 kB
✓ built in 11.63s
```

---

## User Experience Improvements

### Before:
1. ❌ Equipment Groups table hẹp, trống 2 bên
2. ❌ Category nhỏ, khó thấy (dưới Name)
3. ❌ PIC Role là text input - dễ nhập sai (ví dụ: "2E" thay vì "2/E")
4. ❌ Không có validation cho PIC Role format
5. ❌ Không biết rank nào phù hợp với Department nào
6. ❌ Phải mở Equipment Groups page riêng để tạo group khi đang add asset

### After:
1. ✅ Bảng mở rộng toàn màn hình với columns có width cố định
2. ✅ Category là cột riêng, dễ nhìn và sort
3. ✅ PIC Role là dropdown - không thể nhập sai
4. ✅ Dropdown tự động lọc theo Department đã chọn
5. ✅ Hiển thị tiếng Việt cho mỗi rank (ví dụ: "2/E - Máy hai")
6. ✅ Có nút "+ Add Group" ngay trong modal Add Asset
7. ✅ Workflow liền mạch: Add Group → Select Group → Add Asset
8. ✅ Modal Add Asset không đóng khi tạo group (giữ nguyên data)

---

## Maritime Compliance

PIC Role dropdown tuân theo cấu trúc tổ chức hàng hải chuẩn ISM Code:

**ENGINE Department:**
- C/E (Chief Engineer) - Máy trưởng: Trưởng bộ phận máy
- 2/E (Second Engineer) - Máy hai: Phụ trách hệ thống phụ
- 3/E (Third Engineer) - Máy ba: Phụ trách bảo trì hàng ngày
- 4/E (Fourth Engineer) - Máy bốn: Hỗ trợ bảo trì
- E/O (Electrical Officer) - Sĩ quan điện: Phụ trách hệ thống điện

**DECK Department:**
- MASTER - Thuyền trưởng: Chỉ huy tối cao
- C/O (Chief Officer) - Đại phó: Phụ trách cargo và deck operations
- 2/O (Second Officer) - Sĩ quan hai: Phụ trách navigation
- 3/O (Third Officer) - Sĩ quan ba: Phụ trách safety equipment
- BOSUN - Thủy thủ trưởng: Trưởng nhóm thủy thủ
- AB (Able Seaman) - Thủy thủ thành thạo
- OS (Ordinary Seaman) - Thủy thủ phổ thông

**Shared Roles:**
- MASTER: Có thể PIC cho MANAGEMENT, DECK, NAVIGATION
- C/E: Có thể PIC cho ENGINE, MANAGEMENT
- C/O: Có thể PIC cho DECK, NAVIGATION, MANAGEMENT
- E/O: Có thể PIC cho ELECTRICAL, ENGINE

---

## Next Steps

### Optional Future Improvements:

1. **Auto-refresh groups list** sau khi tạo group mới:
   ```tsx
   // Trong AddAssetModal, lắng nghe window focus event
   useEffect(() => {
     const handleFocus = () => {
       loadGroups(); // Reload groups when tab regains focus
     };
     window.addEventListener('focus', handleFocus);
     return () => window.removeEventListener('focus', handleFocus);
   }, []);
   ```

2. **Add tooltip** cho PIC roles:
   ```tsx
   <option title="Responsible for auxiliary systems">2/E - Máy hai</option>
   ```

3. **Highlight new groups** sau khi tạo:
   ```tsx
   // Thêm animation cho checkbox của group vừa tạo
   className={`${isNewGroup ? 'animate-pulse bg-green-50' : ''}`}
   ```

4. **Inline group creation** (Alternative to new tab):
   - Thêm mini-modal "Quick Add Group" trong AddAssetModal
   - Chỉ yêu cầu GroupCode + GroupName + Department
   - Save và tự động chọn group mới

---

## Conclusion

✅ **Hoàn thành 100% yêu cầu:**
1. Equipment Groups table mở rộng, thêm cột Category
2. PIC Role dropdown với Department filtering
3. Nút "Add Group" trong modal Add Asset

**User Experience:**
- Bảng dễ đọc hơn với Category là cột riêng
- PIC Role không thể nhập sai (dropdown validation)
- Workflow tạo group liền mạch không làm mất data đang nhập

**Maritime Compliance:**
- PIC Role mapping tuân theo cấu trúc tổ chức ISM Code
- Phân quyền rõ ràng theo department và rank

**Build Status:** ✅ 0 errors, 0 warnings
