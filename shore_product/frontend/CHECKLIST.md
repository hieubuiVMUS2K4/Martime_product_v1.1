# Checklist: Hoàn thiện Frontend sau Refactor

## ✅ Đã hoàn thành

- [x] Tạo components chung (Button, Card, Modal, Input, Select)
- [x] Tạo Layout components (Sidebar, MainLayout)
- [x] Tạo CSS variables và utility styles
- [x] Định nghĩa types cho Dashboard, Category, Crew
- [x] Chuyển Dashboard.html → DashboardPage.tsx
- [x] Chuyển qldm.html → CategoryManagementPage.tsx
- [x] Chuyển qltv.html → CrewManagementPage.tsx
- [x] Xóa các file HTML cũ
- [x] Tạo file routes mẫu

## 🔄 Cần làm ngay

### 1. Cài đặt dependencies
```bash
npm install react-router-dom
npm install --save-dev @types/react-router-dom
```

### 2. Cập nhật App.tsx
```tsx
import { BrowserRouter } from 'react-router-dom';
import { AppRoutes } from './routes/AppRoutes';
import './styles/variables.css';
import './styles/common.css';
import './styles/global.css';

function App() {
  return (
    <BrowserRouter>
      <AppRoutes />
    </BrowserRouter>
  );
}

export default App;
```

### 3. Import styles vào main.tsx
```tsx
import './styles/variables.css';
import './styles/common.css';
import './styles/global.css';
import './index.css';
```

## 📋 Các bước tiếp theo

### Phase 1: Hoàn thiện cơ bản
- [ ] Cấu hình path aliases trong tsconfig.json
  ```json
  {
    "compilerOptions": {
      "baseUrl": ".",
      "paths": {
        "@/*": ["src/*"],
        "@/components/*": ["src/components/*"],
        "@/pages/*": ["src/pages/*"],
        "@/types/*": ["src/types/*"]
      }
    }
  }
  ```
- [ ] Test routing hoạt động
- [ ] Test responsive trên mobile
- [ ] Fix các lỗi TypeScript còn lại

### Phase 2: Kết nối Backend
- [ ] Tạo services cho từng entity:
  - [ ] `services/dashboard.service.ts`
  - [ ] `services/category.service.ts`
  - [ ] `services/crew.service.ts`
- [ ] Cấu hình axios instance với interceptors
- [ ] Thay mock data bằng API calls
- [ ] Handle loading states
- [ ] Handle error states

### Phase 3: State Management
- [ ] Quyết định sử dụng:
  - [ ] Zustand (đơn giản, nhẹ) - recommended
  - [ ] React Query (server state) - recommended
  - [ ] Redux Toolkit (nếu phức tạp)
- [ ] Tạo stores:
  - [ ] `store/useAuthStore.ts`
  - [ ] `store/useCategoryStore.ts`
  - [ ] `store/useCrewStore.ts`

### Phase 4: Form Validation
- [ ] Cài đặt React Hook Form + Zod
  ```bash
  npm install react-hook-form zod @hookform/resolvers
  ```
- [ ] Tạo validation schemas trong `utils/validation/`
- [ ] Refactor forms sử dụng React Hook Form
- [ ] Thêm error messages tiếng Việt

### Phase 5: Excel Import/Export
- [ ] Cài đặt xlsx
  ```bash
  npm install xlsx
  npm install --save-dev @types/xlsx
  ```
- [ ] Implement import Excel cho Crew Management
- [ ] Implement export Excel
- [ ] Add validation cho imported data
- [ ] Add progress indicator cho large files

### Phase 6: UX Improvements
- [ ] Thêm Loading component
- [ ] Thêm Toast/Notification component
- [ ] Thêm Confirmation Dialog component
- [ ] Thêm Empty State component
- [ ] Thêm Pagination component
- [ ] Thêm Search debounce
- [ ] Thêm keyboard shortcuts (Ctrl+K search, etc.)

### Phase 7: Testing
- [ ] Setup testing library
  ```bash
  npm install --save-dev @testing-library/react @testing-library/jest-dom vitest
  ```
- [ ] Test components chung (Button, Card, Modal)
- [ ] Test pages
- [ ] E2E tests với Playwright (optional)

### Phase 8: Optimization
- [ ] Lazy load routes với React.lazy()
- [ ] Memoize expensive computations
- [ ] Optimize re-renders
- [ ] Add React.memo where needed
- [ ] Image optimization
- [ ] Bundle size analysis

### Phase 9: Accessibility
- [ ] Add ARIA labels
- [ ] Keyboard navigation
- [ ] Focus management trong Modals
- [ ] Screen reader support
- [ ] Color contrast check

### Phase 10: Documentation
- [ ] Component documentation (Storybook optional)
- [ ] API documentation
- [ ] User guide
- [ ] Developer guide

## 🐛 Known Issues

1. **react-router-dom not installed** - Cần cài package
2. **Mock data trong components** - Cần thay bằng API calls
3. **No form validation** - Cần thêm validation
4. **Excel import placeholder** - Cần implement thật
5. **No error boundaries** - Cần thêm error handling
6. **No loading states** - Cần thêm loading indicators

## 📝 Best Practices cần tuân thủ

- ✅ Mọi component đều có TypeScript types
- ✅ Mọi component có file CSS riêng
- ✅ Sử dụng barrel exports (index.ts)
- ✅ Không có logic trong JSX phức tạp
- ⏳ Props validation
- ⏳ Error boundaries
- ⏳ Loading states
- ⏳ Empty states

## 🎯 Priority Order

### High Priority (Làm ngay)
1. Cài react-router-dom
2. Cập nhật App.tsx để routing hoạt động
3. Test các pages hiển thị đúng
4. Fix TypeScript errors

### Medium Priority (Tuần tới)
5. Kết nối backend APIs
6. Thêm form validation
7. Thêm loading/error states
8. Implement Excel import/export

### Low Priority (Sau này)
9. Unit tests
10. Storybook
11. E2E tests
12. Performance optimization

---

**Note**: Cập nhật checklist này khi hoàn thành mỗi item!
