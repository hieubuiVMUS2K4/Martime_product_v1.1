# Cấu trúc Pages đã Refactor

## Tổng quan

Đã chuyển đổi các file HTML tĩnh thành React Components với TypeScript, tuân thủ cấu trúc trong `frontend/src/README.md`.

## Các thay đổi chính

### 1. **Components Chung (Reusable)**
Đã tạo các components UI tái sử dụng trong `components/common/`:

- **Button** (`Button.tsx`) - Nút bấm với variants: primary, secondary, ghost
- **Card** (`Card.tsx`) - Thẻ card với CardHeader, CardBody, CardFooter
- **Modal** (`Modal.tsx`) - Modal dialog với size điều chỉnh
- **Input** (`Input.tsx`) - Input field và Select với label, error handling

Tất cả đều có:
- TypeScript interfaces đầy đủ
- CSS riêng biệt (`.css` files)
- Export qua `index.ts` barrel files

### 2. **Layout Components**
Trong `components/layout/`:

- **Sidebar** - Menu điều hướng với active state
- **MainLayout** - Layout chính với sidebar + main content area
- Sử dụng `<Outlet />` của React Router để render pages

### 3. **Styles Chung**
Trong `styles/`:

- **variables.css** - CSS variables cho colors, spacing, typography, shadows
- **common.css** - Utility classes (grid, card, headers)
- **global.css** - Global styles (đã có sẵn)

### 4. **Types Definitions**
Trong `types/`:

- **dashboard.types.ts** - Types cho Dashboard (StatCard, ModuleItem, DashboardData)
- **category.types.ts** - Types cho Category Management (Category, CategoryType, CategoryCard)
- **crew.types.ts** - Types cho Crew Management (CrewMember, CrewFormData, CrewFilter)

### 5. **Pages đã chuyển đổi**

#### Dashboard (`pages/Dashboard/`)
- **DashboardPage.tsx** - Trang dashboard với stats cards và modules grid
- **DashboardPage.css** - Styles riêng cho dashboard
- Sử dụng Card component từ common

#### Category Management (`pages/CategoryManagement/`)
- **CategoryManagementPage.tsx** - CRUD quản lý danh mục
- Features:
  - Grid hiển thị các category cards
  - Modal xem danh sách items
  - Modal form thêm/sửa
  - Search và filter
- **CategoryManagementPage.css** - Styles cho quản lý danh mục

#### Crew Management (`pages/CrewManagement/`)
- **CrewManagementPage.tsx** - CRUD quản lý thuyền viên
- Features:
  - Grid card hiển thị thuyền viên
  - Upload và preview avatar
  - Modal form với nhiều fields
  - Modal xem chi tiết
  - Filter theo tên, mã, chức vụ
  - Import/Export Excel (placeholder)
  - Tính tuổi tự động
- **CrewManagementPage.css** - Styles phức tạp với grid, avatar, forms

### 6. **Routes**
File `routes/AppRoutes.tsx` mẫu:
```tsx
<Route element={<MainLayout />}>
  <Route path="/dashboard" element={<DashboardPage />} />
  <Route path="/categories" element={<CategoryManagementPage />} />
  <Route path="/crew" element={<CrewManagementPage />} />
</Route>
```

## So sánh trước/sau

### Trước (HTML)
```
pages/
  Dashboard.html (2000+ lines CSS inline)
  qldm.html (mixed HTML/CSS/JS)
  qltv.html (large monolithic file)
```

### Sau (React + TypeScript)
```
components/
  common/
    Button/, Card/, Modal/, Input/
  layout/
    Sidebar/, MainLayout
pages/
  Dashboard/
    DashboardPage.tsx
    DashboardPage.css
    index.ts
  CategoryManagement/
    CategoryManagementPage.tsx
    CategoryManagementPage.css
    index.ts
  CrewManagement/
    CrewManagementPage.tsx
    CrewManagementPage.css
    index.ts
styles/
  variables.css (design tokens)
  common.css (utilities)
types/
  dashboard.types.ts
  category.types.ts
  crew.types.ts
```

## Lợi ích

✅ **Tái sử dụng code**: Components chung dùng cho nhiều pages  
✅ **Type Safety**: TypeScript interfaces đầy đủ  
✅ **Maintainability**: Tách biệt logic, UI, styles  
✅ **Responsive**: Mobile-friendly với media queries  
✅ **Consistent**: Design tokens thống nhất  
✅ **Scalable**: Dễ thêm pages/components mới  

## Cách sử dụng

### Import components:
```tsx
import { Button, Card, Modal, Input } from '@/components/common';
```

### Import pages:
```tsx
import { DashboardPage, CategoryManagementPage, CrewManagementPage } from '@/pages';
```

### Import types:
```tsx
import type { CrewMember, Category } from '@/types/crew.types';
```

## Dependencies cần cài đặt

Để chạy được code, cần cài:
```bash
npm install react-router-dom
npm install --save-dev @types/react-router-dom
```

Cho Excel import (tương lai):
```bash
npm install xlsx
npm install --save-dev @types/xlsx
```

## TODO tiếp theo

1. ✅ Chuyển đổi HTML pages sang React + TypeScript
2. ✅ Tạo components chung
3. ✅ Tạo layout components
4. ✅ Tạo types definitions
5. ⏳ Kết nối với backend API (services/)
6. ⏳ Thêm state management (store/)
7. ⏳ Implement Excel import/export thật
8. ⏳ Add form validation (React Hook Form + Zod)
9. ⏳ Add unit tests
10. ⏳ Chuyển các pages còn lại (Vessels, Voyages)

## Notes

- Lỗi `react-router-dom` là do chưa cài package, cần chạy npm install
- CSS Variables cho phép dễ dàng theme/customize
- Mỗi component có type definitions riêng
- State hiện tại là local, sau này sẽ chuyển sang store hoặc React Query
- Mock data sẵn trong pages để test UI

---

**Tác giả**: AI Assistant  
**Ngày**: 2025-10-08  
**Version**: 1.0
