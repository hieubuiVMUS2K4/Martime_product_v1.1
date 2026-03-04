# 🚀 Quick Start Guide - Sau khi Refactor

## Bước 1: Cài đặt dependencies

```powershell
cd f:\NCKH\Product\frontend

# Cài react-router-dom (bắt buộc)
npm install react-router-dom
npm install --save-dev @types/react-router-dom

# Cài packages khác (tùy chọn, cài sau)
# npm install react-hook-form zod @hookform/resolvers
# npm install xlsx
# npm install zustand
```

## Bước 2: Cập nhật App.tsx

Mở file `src/App.tsx` và thay thế nội dung:

```tsx
import { BrowserRouter } from 'react-router-dom';
import { AppRoutes } from './routes/AppRoutes';
import './styles/variables.css';
import './styles/common.css';
import './styles/global.css';
import './App.css';

function App() {
  return (
    <BrowserRouter>
      <AppRoutes />
    </BrowserRouter>
  );
}

export default App;
```

## Bước 3: Cập nhật main.tsx

Đảm bảo import đúng thứ tự CSS:

```tsx
import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App.tsx';
import './styles/variables.css';
import './styles/common.css';
import './styles/global.css';
import './index.css';

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
);
```

## Bước 4: Chạy development server

```powershell
npm run dev
```

## Bước 5: Kiểm tra trong browser

Mở trình duyệt và truy cập:
- http://localhost:5173/ → redirect to Dashboard
- http://localhost:5173/dashboard → Dashboard Page
- http://localhost:5173/categories → Category Management
- http://localhost:5173/crew → Crew Management

## ✅ Checklist Testing

- [ ] Dashboard hiển thị đúng với stats cards và modules
- [ ] Sidebar navigation hoạt động (click chuyển page)
- [ ] Category Management: mở modal, thêm/sửa/xóa
- [ ] Crew Management: xem cards, mở form, upload avatar
- [ ] Responsive: test trên mobile (F12 → device toolbar)
- [ ] Không có lỗi TypeScript trong console

## 🐛 Troubleshooting

### Lỗi: "Cannot find module 'react-router-dom'"
```powershell
npm install react-router-dom
```

### Lỗi TypeScript về types
```powershell
npm install --save-dev @types/react-router-dom
```

### Page trắng / Không hiển thị gì
1. Check console (F12) xem có errors không
2. Đảm bảo đã update App.tsx theo Bước 2
3. Clear cache và refresh (Ctrl+Shift+R)

### Styles không hiển thị đúng
1. Check thứ tự import CSS trong main.tsx
2. Đảm bảo variables.css được import trước common.css

### Routing không hoạt động
1. Đảm bảo đã wrap App trong `<BrowserRouter>`
2. Check routes trong AppRoutes.tsx

## 📝 Next Steps

Sau khi chạy được basic app:

1. **Kết nối Backend**: Tạo services trong `src/services/`
2. **Form Validation**: Thêm React Hook Form + Zod
3. **State Management**: Cài Zustand hoặc React Query
4. **Excel Import**: Implement xlsx parsing
5. **Toast Notifications**: Thêm notification system

Xem chi tiết trong `CHECKLIST.md`!

## 🎯 Các Pages đang hoạt động

### ✅ Dashboard
- Stats cards (4 số liệu)
- Modules grid (6 modules)
- Mock data hiển thị

### ✅ Category Management
- 7 category cards
- Click "Quản lý" → mở modal với table
- "Thêm mới" → form modal
- Mock CRUD operations

### ✅ Crew Management
- Grid hiển thị thuyền viên cards
- Filter theo tên/chức vụ
- "Thêm mới" → form với avatar upload
- Click "Xem" → detail modal
- Click "Sửa" → edit form
- Mock data với 2 thuyền viên mẫu

## 💡 Tips

- **Hot Reload**: Vite tự động reload khi save file
- **TypeScript Errors**: Check terminal hoặc VS Code Problems panel
- **Component Reuse**: Import từ `@/components/common` (sau khi setup path aliases)
- **Styling**: Ưu tiên dùng CSS variables từ `variables.css`

## 📚 Useful Commands

```powershell
# Development
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Type check
npm run type-check  # (nếu có script này)

# Lint
npm run lint
```

## 🎊 Success!

Nếu bạn thấy Dashboard hiển thị đúng và có thể navigate giữa các pages → **Refactor thành công!** 🎉

---

**Need help?** Check:
- `REFACTOR_SUMMARY.md` - Tổng quan refactor
- `CHECKLIST.md` - Danh sách việc cần làm
- `src/pages/REFACTOR_NOTES.md` - Chi tiết kỹ thuật
- `src/README.md` - Cấu trúc project
