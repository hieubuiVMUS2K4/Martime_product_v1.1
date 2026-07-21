# config/ — Hằng số cấu hình phía client

## Mục đích

Tập trung các hằng số cấu hình runtime của frontend: base URL API, ngưỡng cảnh báo, chu kỳ refresh, định
dạng ngày giờ. Mục tiêu là để khi cần đổi một con số (vd. ngưỡng nhiệt độ máy chính cảnh báo), chỉ sửa một
chỗ thay vì rải rác trong nhiều component.

## Cấu trúc & vai trò

Chỉ có một file: `app.config.ts`, export các object hằng số:

| Object | Nội dung | Ghi chú |
|---|---|---|
| `API_CONFIG` | `BASE_URL` (mặc định `/api`, đọc từ `VITE_API_URL`), `TIMEOUT` (30000ms), `RETRY_ATTEMPTS` | Dùng bởi `services/api.client.ts` và `services/maritime.service.ts` |
| `WS_CONFIG` | `URL` (WebSocket, đọc từ `VITE_WS_URL`), `RECONNECT_INTERVAL` | Khai báo sẵn cho realtime qua WebSocket — hiện chưa thấy code nào trong `src/` thực sự mở kết nối WS (xem lưu ý) |
| `VESSEL_CONFIG` | `VESSEL_ID`, `VESSEL_NAME`, `IMO_NUMBER` (đọc từ biến môi trường `VITE_VESSEL_ID`...) | Thông tin tàu cục bộ của Edge instance đang chạy |
| `SYNC_CONFIG` | `AUTO_SYNC_ENABLED`, `SYNC_INTERVAL` (300000ms = 5 phút), `MAX_SYNC_BATCH` | Dùng ở `pages/Sync/SyncPage.tsx` để hiển thị cấu hình đồng bộ |
| `REFRESH_INTERVALS` | `REALTIME` (1s), `FAST` (5s), `NORMAL` (15s), `SLOW` (60s) | Gợi ý chu kỳ polling theo mức độ "nóng" của dữ liệu (navigation vs thống kê) |
| `ALERT_THRESHOLDS` | Ngưỡng nhiệt độ máy chính, áp suất dầu bôi trơn, mức nhiên liệu, điện áp ắc-quy | Dùng cho cảnh báo kỹ thuật (Engine/Alarms) |
| `DATE_FORMATS` | `FULL`, `DATE`, `TIME`, `SHORT` (chuỗi định dạng `date-fns`) | Định dạng ngày giờ hiển thị thống nhất |

## Luồng hoạt động chính

`config/` không tự thực thi logic — nó chỉ là nguồn hằng số được các nơi khác `import`:

```
services/api.client.ts     → import { API_CONFIG } → biết gọi tới đâu (BASE_URL) và timeout bao lâu
pages/Sync/SyncPage.tsx    → import { SYNC_CONFIG } → hiển thị "Tự động đồng bộ: Bật/Tắt", "5 phút"...
(các trang Engine/Alarms)  → import { ALERT_THRESHOLDS } → so sánh giá trị cảm biến để tô màu cảnh báo
```

## Liên kết với phần khác

- **`services/api.client.ts`**, **`services/maritime.service.ts`**: dùng `API_CONFIG.BASE_URL`.
- **`pages/Sync/SyncPage.tsx`**: dùng `SYNC_CONFIG` để hiển thị cấu hình đồng bộ hiện tại (chỉ đọc, không có
  UI chỉnh sửa các giá trị này trên frontend — muốn đổi phải sửa code hoặc biến môi trường rồi build lại).
- **`.env` / biến môi trường Vite** (`VITE_API_URL`, `VITE_WS_URL`, `VITE_VESSEL_ID`, `VITE_VESSEL_NAME`,
  `VITE_IMO_NUMBER`): nguồn override cho các giá trị mặc định trong `app.config.ts`.

## Ghi chú khi đọc/dạy

- Phần lớn service dùng **`axios`** trong `services/` (xem `services/README.md`) lại **không** import
  `API_CONFIG` — chúng tự khai báo `const API_BASE_URL = '/api'` cục bộ trong từng file. Vì vậy đổi
  `API_CONFIG.BASE_URL` **không** đổi hành vi của các service dùng `axios` — chỉ ảnh hưởng các service dùng
  `apiClient`. Đây là điểm dễ gây debug sai nếu không biết trước.
- `WS_CONFIG` được khai báo nhưng chưa thấy nơi nào trong `src/` mở `new WebSocket(WS_CONFIG.URL)` — có thể
  là chuẩn bị cho tính năng realtime tương lai (SignalR/WebSocket) chứ chưa hoạt động ở thời điểm hiện tại.
- Đây là hằng số **build-time/runtime của trình duyệt** (qua `import.meta.env`), khác với cấu hình phía
  backend (`edge-services/appsettings.json`, mục `Sync` — xem README gốc của dự án) dù tên gọi tương tự
  (`SyncInterval`). Hai nơi này **không tự đồng bộ giá trị với nhau** — đổi một bên không ảnh hưởng bên kia.
