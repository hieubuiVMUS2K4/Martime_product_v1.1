# CSS Architecture - Tại sao cần CSS riêng cho từng page?

## 🤔 Câu hỏi: Có CSS chung rồi, tại sao còn cần CSS riêng?

## 💡 Trả lời

### 1. Phân cấp CSS theo mục đích

```
┌─────────────────────────────────────────────┐
│  Global CSS (Toàn dự án)                    │
│  - variables.css (design tokens)            │
│  - common.css (utilities)                   │
│  - global.css (reset, base)                 │
└─────────────────────────────────────────────┘
          ↓ Import & sử dụng
┌─────────────────────────────────────────────┐
│  Component CSS (Tái sử dụng)                │
│  - Button.css                               │
│  - Card.css                                 │
│  - Modal.css                                │
└─────────────────────────────────────────────┘
          ↓ Compose
┌─────────────────────────────────────────────┐
│  Page CSS (Đặc thù riêng)                   │
│  - DashboardPage.css                        │
│  - CategoryManagementPage.css               │
│  - CrewManagementPage.css                   │
└─────────────────────────────────────────────┘
```

---

## 📊 So sánh cụ thể

### ❌ Nếu chỉ dùng CSS chung:

```css
/* common.css - File này sẽ TRỞ NÊN KHỔNG LỒ */
.grid { display: grid; }
.card { background: white; }

/* Styles cho Dashboard */
.stats-grid { grid-template-columns: repeat(4, 1fr); }
.stat-number { font-size: 32px; }

/* Styles cho Category Management */
.category-count { padding: 4px 12px; }
.data-table th { position: sticky; }

/* Styles cho Crew Management */
.crew-avatar { width: 200px; height: 200px; }
.crew-detail-grid { grid-template-columns: 1fr 1fr; }

/* Styles cho Vessel Management */
.vessel-card { ... }
.vessel-form { ... }

/* 👎 File trở nên quá dài, khó maintain! */
```

**Vấn đề:**
- ❌ File CSS chung quá lớn (hàng nghìn dòng)
- ❌ Khó tìm styles cần sửa
- ❌ Conflict class names giữa các pages
- ❌ Load CSS không cần thiết cho mọi page
- ❌ Khó debug khi có lỗi

### ✅ Với CSS riêng cho từng page:

```css
/* common.css - NHẸ, chỉ utilities */
.grid { display: grid; }
.card { background: white; }
.btn { padding: 8px 16px; }

/* CategoryManagementPage.css - CHỈ CHO PAGE NÀY */
@import '../../styles/variables.css';

.category-count { 
  padding: 4px 12px; 
}

.data-table th { 
  position: sticky; 
}

/* 👍 Ngắn gọn, dễ maintain! */
```

**Lợi ích:**
- ✅ Mỗi file CSS nhỏ, dễ quản lý
- ✅ Tìm styles nhanh (biết ngay ở file nào)
- ✅ Không conflict giữa pages
- ✅ Code splitting (chỉ load CSS cần thiết)
- ✅ Dễ debug và sửa lỗi

---

## 🎨 Nguyên tắc: CSS nào đi đâu?

### 1. `variables.css` - Design Tokens
**Dùng cho**: Giá trị thiết kế cơ bản, dùng TOÀN DỰ ÁN

```css
:root {
  --color-primary: #333;
  --spacing-md: 15px;
  --font-size-lg: 16px;
}
```

**Khi nào dùng:**
- ✅ Colors, spacing, fonts chung
- ✅ Breakpoints
- ✅ Shadows, transitions
- ❌ KHÔNG dùng cho layout cụ thể

### 2. `common.css` - Utilities
**Dùng cho**: Classes helper dùng NHIỀU NƠI (≥3 pages)

```css
.grid { display: grid; }
.flex { display: flex; }
.text-center { text-align: center; }
.mt-2 { margin-top: var(--spacing-md); }
```

**Khi nào dùng:**
- ✅ Layout utilities (grid, flex)
- ✅ Text utilities (text-center, bold)
- ✅ Spacing utilities (margins, paddings)
- ❌ KHÔNG dùng cho styles đặc thù 1 page

### 3. `PageName.css` - Page-specific
**Dùng cho**: Styles CHỈ PAGE ĐÓ DÙNG (< 3 nơi)

```css
.category-count { /* Chỉ Category page dùng */ }
.crew-avatar { /* Chỉ Crew page dùng */ }
.dashboard-stat-card { /* Chỉ Dashboard dùng */ }
```

**Khi nào dùng:**
- ✅ Layout đặc biệt cho page
- ✅ Custom components trong page
- ✅ Responsive riêng cho page
- ❌ KHÔNG dùng cho styles tái sử dụng

---

## 📐 Quy tắc "3 lần"

```
Dùng 1 lần → Page CSS
Dùng 2 lần → Page CSS (chưa extract)
Dùng 3+ lần → Extract ra Common CSS
```

**Ví dụ:**

```css
/* dashboard-stat-card chỉ Dashboard dùng → DashboardPage.css */
.dashboard-stat-card { ... }

/* crew-avatar chỉ Crew page dùng → CrewManagementPage.css */
.crew-avatar { ... }

/* Nhưng "btn" dùng ở 5+ pages → common.css */
.btn { padding: 8px 16px; }
```

---

## 🏗️ Ví dụ thực tế

### Dashboard Page

```tsx
// DashboardPage.tsx
import './DashboardPage.css';  // Import CSS riêng

export const DashboardPage = () => (
  <div className="dashboard-page">
    {/* Dùng class từ DashboardPage.css */}
    <div className="stats-grid">
      <div className="stat-card">
        <h3>TỔNG SỐ TÀU</h3>
        <div className="stat-number">24</div>
      </div>
    </div>
  </div>
);
```

```css
/* DashboardPage.css - Chỉ Dashboard dùng */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 20px;
}

.stat-number {
  font-size: 32px;
  font-weight: bold;
}
```

### Category Management Page

```tsx
// CategoryManagementPage.tsx
import './CategoryManagementPage.css';  // CSS riêng KHÁC

export const CategoryManagementPage = () => (
  <div className="category-management-page">
    {/* Dùng class KHÁC, không conflict với Dashboard */}
    <div className="categories-grid">
      <span className="category-count">5</span>
    </div>
  </div>
);
```

```css
/* CategoryManagementPage.css - Chỉ Category page dùng */
.categories-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  /* Layout KHÁC so với dashboard-stats-grid */
}

.category-count {
  background: #f0f0f0;
  padding: 4px 12px;
  border-radius: 12px;
  /* Style KHÁC, không ảnh hưởng Dashboard */
}
```

---

## 🚀 Performance Benefits

### Code Splitting với CSS riêng:

```
User vào Dashboard
  ↓
  Load: variables.css + common.css + DashboardPage.css
  ✅ KHÔNG load CategoryManagementPage.css
  ✅ KHÔNG load CrewManagementPage.css
  
User vào Category Management
  ↓
  Load: variables.css + common.css + CategoryManagementPage.css
  ✅ KHÔNG load DashboardPage.css
  ✅ KHÔNG load CrewManagementPage.css
```

**Kết quả:**
- ✅ Bundle nhỏ hơn (chỉ load CSS cần thiết)
- ✅ Tốc độ load nhanh hơn
- ✅ Better caching (CSS pages thay đổi độc lập)

---

## 🎯 Khi nào nên Extract ra Common CSS?

### ❌ KHÔNG extract (giữ trong page CSS):

```css
/* Chỉ 1 page dùng */
.category-count { ... }
.crew-avatar { ... }
.dashboard-stat-number { ... }
```

### ✅ NÊN extract (chuyển ra common CSS):

```css
/* 3+ pages dùng */
.btn { ... }
.card { ... }
.modal { ... }
.input { ... }
```

### 🤔 Cân nhắc extract:

```css
/* 2 pages dùng - có thể chờ thêm 1 page nữa */
.toolbar { ... }
.action-buttons { ... }
```

---

## 📝 Checklist: CSS nên đi đâu?

Khi viết CSS mới, tự hỏi:

- [ ] Style này dùng ở **bao nhiêu pages**?
  - 1 page → Page CSS ✅
  - 2 pages → Page CSS (chưa extract)
  - 3+ pages → Common CSS ✅

- [ ] Style này là **design token** không?
  - Color/spacing/font → `variables.css` ✅
  - Specific style → Page CSS ✅

- [ ] Style này là **utility** không?
  - Grid/flex/text helpers → `common.css` ✅
  - Custom layout → Page CSS ✅

- [ ] Style này **tái sử dụng** được không?
  - Có thể tái sử dụng → Component CSS ✅
  - Đặc thù riêng → Page CSS ✅

---

## 🏆 Best Practices

### ✅ DO (Nên làm)

```css
/* Page CSS - Specific & scoped */
.category-management-page { }
.category-count { }
.data-table { }

/* Common CSS - Generic & reusable */
.btn { }
.card { }
.grid { }
```

### ❌ DON'T (Không nên)

```css
/* ❌ Đừng bỏ ALL styles vào common.css */
.dashboard-stat-number { } /* Nên ở DashboardPage.css */
.crew-avatar { }           /* Nên ở CrewManagementPage.css */
.category-count { }        /* Nên ở CategoryManagementPage.css */
```

---

## 🎓 Tóm tắt

| CSS File | Mục đích | Ví dụ | Khi nào dùng |
|----------|----------|-------|--------------|
| **variables.css** | Design tokens | Colors, spacing, fonts | Giá trị dùng toàn dự án |
| **common.css** | Utilities | `.grid`, `.btn`, `.card` | Classes dùng 3+ pages |
| **Component.css** | Component styles | `Button.css`, `Modal.css` | Styles cho component |
| **PageName.css** | Page-specific | `DashboardPage.css` | Styles chỉ page đó dùng |

### 🎯 Nguyên tắc vàng:

> **"Start specific, extract when repeated"**
> 
> (Bắt đầu với page CSS, extract ra common CSS khi dùng ≥3 lần)

---

## 📚 Tài liệu tham khảo

- **SMACSS** (Scalable and Modular Architecture for CSS)
- **ITCSS** (Inverted Triangle CSS)
- **BEM** (Block Element Modifier)
- **Atomic Design** principles

---

**Kết luận**: CSS riêng cho từng page giúp code **modular**, **maintainable**, và **performant**! 🚀
