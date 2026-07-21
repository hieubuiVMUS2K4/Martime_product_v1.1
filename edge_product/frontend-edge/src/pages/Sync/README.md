# pages/Sync — Giám sát & điều khiển đồng bộ Edge ↔ Shore

## Mục đích

`SyncPage` (`/sync`) là "bảng điều khiển" đồng bộ dữ liệu cho thuyền trưởng/sĩ quan trên tàu: xem còn bao
nhiêu bản ghi đang chờ gửi lên Shore, kích hoạt đồng bộ thủ công, xử lý lỗi đồng bộ, và tạo "snapshot" (đẩy
một lô dữ liệu chọn lọc lên Shore ngay cả khi chưa có thay đổi mới). Đây là nơi trực quan hoá khái niệm
**Store-and-Forward** mô tả trong README gốc của dự án (mục 5 — Cơ chế Đồng bộ Dữ liệu).

File duy nhất: `SyncPage.tsx` (một file ~930 dòng, gồm cả trang chính lẫn 2 modal con
`SyncConfirmModal`, `SnapshotModal` định nghĩa ngay trong cùng file).

## Cấu trúc & vai trò

| Phần | Vai trò |
|---|---|
| `SyncPage()` | Component chính: KPI cards (kết nối, số bản ghi chờ, lần sync gần nhất, số lỗi), bảng chi tiết hàng đợi (`SyncQueue`), auto-refresh 30s |
| `SyncConfirmModal` | Modal xác nhận trước khi bấm "Đồng bộ ngay" — hiển thị 2 cột song song: **Ship/Local** (dữ liệu đang chờ, nhóm theo bảng) vs **Shore** (trạng thái kết nối, số bản ghi Shore đang chờ xử lý) |
| `SnapshotModal` | Modal chọn nhóm dữ liệu để đẩy "chụp nhanh" lên Shore: `ship_data`, `crew`, `pms`, `voyage` (có lọc theo ngày), `report` (có lọc theo ngày) |
| `TABLE_TO_KEY` | Bảng ánh xạ tên bảng CSDL (`crew_member`, `noon_report`...) sang khoá i18n để hiển thị tên thân thiện thay vì tên bảng kỹ thuật |

## Luồng hoạt động chính

```
SyncPage mount
   ▼
fetchData()  →  Promise.all([ syncService.getSyncStatus(), syncService.getSyncQueue() ])
   │                (services/maritime.service.ts, dùng apiClient)
   ▼
GET /api/sync/status   → { pendingRecords, lastSyncAt, isOnline }
GET /api/sync/queue    → SyncQueue[]  (mỗi item: tableName, recordId, priority, retryCount, lastError...)
   ▼
setState(status, queue) → render KPI cards + bảng hàng đợi
   ▼ (mỗi 30 giây, nếu bật "Tự động làm mới")
   setInterval(fetchData, 30000)
```

Khi người dùng bấm **"Đồng bộ ngay"**:

```
setShowSyncModal(true) → SyncConfirmModal hiển thị bản tóm tắt (nhóm theo bảng, cảnh báo nếu offline)
   ▼ (bấm xác nhận)
handleTriggerSync() → syncService.triggerSync() → POST /api/sync/trigger
   ▼
Edge Backend: SyncBackgroundWorker chạy ngay lập tức thay vì chờ chu kỳ 30s
   (đọc SyncQueue → lọc theo NetworkType hiện tại → gửi batch tới Shore /api/sync)
   ▼
Response { totalSynced, pendingRecords } → cập nhật banner thành công → fetchData() làm mới bảng
```

**Snapshot** (nút "Chụp nhanh dữ liệu") khác với đồng bộ thường: thay vì chỉ gửi *thay đổi* (delta) đang có
trong `SyncQueue`, nó chủ động enqueue **toàn bộ** dữ liệu thuộc nhóm được chọn (kể cả chưa đổi) —
`syncService.snapshotGroups(groups, fromDate?, toDate?)` → `POST /api/sync/snapshot`. Dùng khi cần "gửi lại
từ đầu" cho một tàu mới kết nối lại sau thời gian dài mất mạng, hoặc khi nghi ngờ Shore thiếu dữ liệu.

**Reset lỗi**: nếu có item trong hàng đợi có `retryCount > 0`, nút "Xoá lỗi" gọi
`syncService.resetErrors()` → `POST /api/sync/reset-errors` để đưa các item lỗi về trạng thái chờ gửi lại
(thay vì bị kẹt mãi ở trạng thái lỗi).

## Liên kết với phần khác

- **`services/maritime.service.ts`** (`export const syncService = {...}`): toàn bộ lời gọi API của trang này
  đi qua object `syncService` định nghĩa cuối file đó — KHÔNG có file `sync.service.ts` riêng.
- **`types/maritime.types.ts`**: định nghĩa `SyncQueue` (item hàng đợi).
- **`config/app.config.ts`**: `SYNC_CONFIG` (chỉ hiển thị thông tin — `AUTO_SYNC_ENABLED`, `SYNC_INTERVAL`,
  `MAX_SYNC_BATCH` — không có UI chỉnh sửa các giá trị này, phải sửa code/env).
- **`components/layouts/SyncNotificationBell.tsx`**: một luồng đồng bộ *khác chiều* — bell trên Header
  poll `/api/sync/notifications` mỗi 30s để báo "Shore vừa cập nhật/tạo thuyền viên mới", độc lập với
  `SyncPage` (không dùng chung state, chỉ dùng chung ý tưởng polling).
- **README gốc dự án** (`Martime_product_v1.1/README.md`, mục 5 & 6): giải thích chi tiết giao thức Push
  (Edge → Shore, `SyncBackgroundWorker` mỗi 30s), Pull (Shore → Edge, cursor-based pagination) và quy tắc xử
  lý xung đột theo domain — trang này chỉ là **giao diện quan sát/kích hoạt**, toàn bộ logic đồng bộ thật
  nằm ở backend (`edge-services`).
- **`pages/Crew/CrewPage.tsx`**: gọi trực tiếp `fetch('/api/sync/notifications/crew-summary')` (không qua
  `syncService`) để hiển thị số trường đã bị Shore thay đổi cho từng thuyền viên — một ví dụ khác của
  "gọi API không qua tầng service" (xem `services/README.md`, mục 3 cách gọi API).

## Ghi chú khi đọc/dạy

- `SyncPage.tsx` không dùng `stores/maritime.store.ts` (dù store này có sẵn field `isSyncing`,
  `lastSyncTime`, `isOnline`) — toàn bộ state (`status`, `queue`, `syncing`...) được quản lý cục bộ bằng
  `useState` trong chính page. Đây là điểm đáng lưu ý: state "đồng bộ" trong Zustand store và state hiển thị
  trên trang Sync **không phải lúc nào cũng là một** — nếu cần trạng thái `isSyncing` toàn cục (vd. hiện icon
  xoay ở Sidebar khi đang đồng bộ), phải tự nối 2 nơi này lại, hiện tại chưa thấy code nào làm việc đó.
  Bear kiểm tra `components/layouts/Sidebar.tsx`/`Header.tsx` nếu cần xác nhận thêm.
- Khái niệm "Priority" (Critical/Operational/Low) hiển thị bằng màu (`getPriorityColor`) dựa trên
  `item.priority` là số nguyên (`<= 1` = đỏ/cao, `<= 3` = vàng/trung bình, còn lại = xanh/thấp) — ánh xạ số
  này khớp với enum `SyncPriority` phía backend (`Maritime.Shared`) nhưng **không import chung enum**, chỉ so
  sánh bằng số cứng trong code frontend.
- Do cả trang nằm trong 1 file lớn, khi cần sửa chỉ một phần (vd. chỉ sửa `SnapshotModal`), nên dùng tìm
  kiếm theo tên hàm (`function SnapshotModal`, `function SyncConfirmModal`) thay vì đọc tuần tự từ đầu.
