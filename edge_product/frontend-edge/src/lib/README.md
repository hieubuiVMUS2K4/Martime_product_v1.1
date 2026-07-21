# lib/ — Tiện ích & hạ tầng cấp thấp

## Mục đích

Nhóm các hàm/module không thuộc về "gọi API" (`services/`) cũng không phải "state toàn cục theo domain
nghiệp vụ" (`stores/`): CSS class helper, IndexedDB offline, và tiện ích in ấn. Một file (`store.ts`) là
tàn dư lịch sử, không còn được dùng — xem lưu ý bên dưới.

## Cấu trúc & vai trò

| File | Export chính | Công dụng |
|---|---|---|
| `utils.ts` | `cn(...inputs)` | Helper kinh điển của shadcn/ui: gộp `clsx` + `tailwind-merge` để ghép class Tailwind an toàn (loại trùng, ưu tiên class sau) |
| `draftsDb.ts` | `draftsDb` (instance `Dexie`) | Định nghĩa database IndexedDB `FormDraftsDatabase`, bảng `drafts` (key, data, updatedAt) — nơi `hooks/useOfflineDraft.ts` đọc/ghi nháp form |
| `printUtils.ts` | `printSmsDocument(options)` | Mở cửa sổ `window.open` riêng, render HTML A4 chuẩn hoá (tiêu đề, watermark, chữ ký) rồi gọi in — dùng cho tài liệu SMS/ISM và một số biểu mẫu HSQE |
| `store.ts` | `useStore`, `useCrew`, `useMaintenance`, `useDashboard` | **Không còn được sử dụng** (xem lưu ý) |

## Luồng hoạt động chính

`draftsDb.ts` là ví dụ tiêu biểu nhất cho tinh thần "Edge offline-first" ở tầng frontend:

```
Form dài (vd. Noon Report) đang được nhập dở
   ▼
hooks/useOfflineDraft(key, formState) chạy setInterval 5s
   ▼
draftsDb.drafts.put({ key, data: formState, updatedAt })   (lib/draftsDb.ts, IndexedDB — KHÔNG qua mạng)
   ▼
Nếu tab bị đóng / mất điện / rớt kết nối VSAT giữa chừng...
   ▼
Lần sau mở lại form: draftsDb.drafts.get(key) tìm thấy bản nháp
   → useOfflineDraft trả về hasDraft = true → form hỏi người dùng có muốn khôi phục
```

Khác với `stores/*` (Zustand, thường dùng `localStorage`), IndexedDB qua Dexie phù hợp hơn cho dữ liệu form
lớn/nhiều field vì không bị giới hạn ~5MB như `localStorage`.

## Liên kết với phần khác

- **`hooks/useOfflineDraft.ts`**: người tiêu thụ duy nhất của `draftsDb.ts`.
- **`pages/HSQE`** (`SmsDocumentPage.tsx`, `IncidentManagement.tsx`): dùng `printSmsDocument` từ
  `printUtils.ts` để in tài liệu/biểu mẫu.
- **`components/ui/*`**: mọi component UI (button, card, badge...) đều dùng `cn()` từ `utils.ts` để ghép
  class có điều kiện.

## Ghi chú khi đọc/dạy

- **`lib/store.ts` là code chết**: đây là một bản Zustand store khác (`useStore`, kèm 3 hook tiện ích
  `useCrew/useMaintenance/useDashboard`, cache vào `sessionStorage` với TTL 5 phút) — rà bằng `grep` toàn bộ
  `src/` cho thấy **không còn file nào import nó**. Trạng thái thật của app hiện đi qua
  `stores/maritime.store.ts` + gọi trực tiếp `services/maritime.service.ts` trong từng page. Khi dạy người
  mới, nên nói rõ để họ không tốn thời gian tìm hiểu nhầm một luồng dữ liệu không còn hoạt động.
- `utils.ts` chỉ có một hàm nhưng được import ở **rất nhiều nơi** (`components/ui/*` và hầu hết component có
  class động) — đáng để người mới nắm ngay từ đầu vì sẽ gặp lại liên tục.
- `printUtils.ts` thao tác DOM/`window.open` trực tiếp (không qua React) — cách ly khỏi luồng render bình
  thường, cần lưu ý khi debug vì nó không xuất hiện trong React DevTools.
