# components/logbooks — Form MARPOL Annex V (Garbage Record)

## Mục đích

Khác với kỳ vọng ban đầu (tên thư mục trùng với `pages/logbooks`), thư mục này **chỉ chứa 2 file**: form
nhập liệu cho **Garbage Record Book Part I và Part II** theo MARPOL Annex V. Các sổ nhật ký còn lại
(Deck/Engine/Oil/Watchkeeping/Ballast/Voyage/Abstract) không có component riêng trong `components/` — chúng
dùng thẳng `components/common/LogbookGrid.tsx` + `MaritimeInput.tsx` ngay trong file `*Page.tsx` của
`pages/logbooks/`, không tách form riêng vì cấu trúc đơn giản hơn (chủ yếu là 1 dòng = 1 lần ghi).

Garbage Record được tách riêng vì đây là form **phức tạp nhất** trong nhóm logbook: phải chọn đúng nhóm rác
(category) trong 2 bảng phân loại khác nhau của MARPOL Annex V, mỗi nhóm có quy tắc "được/không được xả
xuống biển" riêng.

## Cấu trúc & vai trò

| File | Form | Nhóm chất thải (MARPOL Annex V) |
|---|---|---|
| `GarbagePartIForm.tsx` | Part I | A. Chất dẻo · B. Chất thải thực phẩm · C. Chất thải sinh hoạt · D. Dầu ăn · E. Tro lò đốt · F. Chất thải khai thác/bảo dưỡng · G. Dư lượng hàng hoá không nguy hại · H. Dư lượng hàng hoá nguy hại (HME) · I. Xác động vật |
| `GarbagePartIIForm.tsx` | Part II | J. Dư lượng hàng hoá không nguy hại (nước rửa hầm) · K. Dư lượng hàng hoá nguy hại (HME — **cấm tuyệt đối xả xuống biển**) |

Cả 2 form nhận props giống nhau: `form` (state hiện tại), `onChange(field, value)`, `onCategorySelect(code,
name)`, `categories` (danh sách nhóm kèm cờ `seaDischarge`), `onSubmit`, `onCancel` — hoàn toàn là
**controlled component**, không tự giữ state phức tạp bên trong, chỉ có state dịch tên nhóm sang tiếng Việt
(`translatedCategories`) dựa trên `useTranslationSafe().locale`.

## Luồng hoạt động chính

```
pages/logbooks/GarbageManagementPage.tsx
   │  giữ toàn bộ state: activeTab ('part-i' | 'part-ii'), form data, danh sách entries
   ▼
<GarbagePartIForm form={...} categories={PART_I_CATEGORIES} onChange={...} onCategorySelect={...} .../>
   │  người dùng chọn category (vd. "B — Food Wastes") → onCategorySelect('B', 'Food Wastes')
   │  selectedCategory.seaDischarge === false → ô "lượng xả xuống biển" bị disable (khoá nhập liệu)
   ▼  (bấm Lưu)
onSubmit()  (định nghĩa ở GarbageManagementPage)
   ▼
logbookService.createGarbagePartIEntry(dto) / createGarbagePartIIEntry(dto)   (services/logbook.service.ts)
   ▼
POST /api/logbooks/garbage/part1  hoặc  /part2
```

`PART_I_CATEGORIES`/`PART_II_CATEGORIES` (mảng cấu hình nhóm A-I, J-K) được định nghĩa **trực tiếp trong
`GarbageManagementPage.tsx`**, không phải trong `components/logbooks/` — 2 form ở đây chỉ nhận mảng này qua
prop `categories`, không tự biết danh sách nhóm chất thải là gì.

## Liên kết với phần khác

- **`pages/logbooks/GarbageManagementPage.tsx`**: nơi duy nhất sử dụng cả 2 form này.
- **`components/common/MaritimeInput.tsx`, `CoordinatePicker.tsx`**: input dùng bên trong 2 form.
- **`services/logbook.service.ts`**: nơi định nghĩa DTO `CreateGarbagePartIDto`/`CreateGarbagePartIIDto` và
  hàm gọi API tương ứng (xem `types/logbook.types.ts`).
- **`pages/logbooks/README.md`**: bức tranh tổng thể toàn bộ nhóm sổ nhật ký.

## Ghi chú khi đọc/dạy

- Đừng nhầm quy mô: thư mục `components/logbooks/` **nhỏ hơn nhiều** so với `pages/logbooks/` — chỉ phục vụ
  đúng 1 tính năng (Garbage Record), không phải "component dùng chung cho mọi logbook".
- Quy tắc nghiệp vụ quan trọng cần nhớ khi dạy: nhóm **H** (Part I) và **K** (Part II) là chất thải/dư lượng
  hàng hoá **nguy hại (HME — Harmful to the Marine Environment)** — theo MARPOL Annex V, các nhóm này
  **tuyệt đối không được xả xuống biển**, chỉ được lưu giữ trên tàu và giao nộp ở cảng. Logic
  `seaDischarge: false` trong bảng category chính là để hiện thực hoá quy tắc này trên UI (khoá ô nhập liệu
  liên quan đến xả biển).
- Cả 2 form dùng kiểu `any` cho prop `form`/`categories` (không có interface chặt chẽ) — khi mở rộng thêm
  field, TypeScript sẽ không cảnh báo nếu tên field gõ sai; nên đối chiếu trực tiếp với
  `CreateGarbagePartIDto`/`CreateGarbagePartIIDto` trong `types/logbook.types.ts` để chắc chắn field khớp.
