# utils/ — (Hiện tại: trống / không dùng)

## Mục đích

Thư mục dự kiến chứa các hàm tiện ích thuần (format ngày, format tiền tệ, validate...) dùng chung toàn app.

## Cấu trúc & vai trò

| File | Nội dung |
|---|---|
| `index.ts` | **File rỗng (0 byte).** Không export gì. |

## Luồng hoạt động chính

Không có. Trong thực tế, mỗi page/component hiện đang **tự định nghĩa lại** các hàm helper nhỏ ngay trong file của nó thay vì import từ đây, ví dụ:
- `fmtDate`/`fmt`/`formatDateTime` (định dạng ngày `toLocaleDateString('vi-VN'|'en-GB', ...)`) — lặp lại gần như y hệt ở `CrewListPage.tsx`, `PlanningBoardPage.tsx`, `ComplianceDashboardPage.tsx`, `VoyageListPage.tsx`, `OnboardingDashboardPage.tsx`...
- `formatCurrency` — định nghĩa lại trong `VoyageListPage.tsx`.
- `getStatusBadge`/`STATUS_COLORS`/`STATUS_LABELS` dạng `Record<string, string>` — lặp lại trong `TravelListPage.tsx`, `ExternalRequestListPage.tsx`, `SyncDashboardPage.tsx`...
- `cn()` (gộp className) thì **có** chỗ dùng chung thật sự, nhưng nằm ở `lib/utils.ts`, không phải ở đây (xem `lib/README.md`).

## Liên kết với phần khác

- Không có import nào từ `utils/` ở nơi khác trong `src/` (đã kiểm tra bằng grep).

## Ghi chú khi đọc/dạy

- Đừng tìm hàm format ngày/tiền tệ dùng chung ở đây — chúng **không tồn tại tập trung**; mỗi page viết lại phiên bản riêng của mình (đôi khi khác locale: có nơi `'vi-VN'`, có nơi `'en-GB'`). Nếu được giao việc "chuẩn hoá format ngày/tiền tệ toàn app", đây chính là thư mục hợp lý để bắt đầu gom code trùng lặp về.
- Không nhầm với `lib/` (có nội dung thật — hàm `cn()` cho Tailwind) hay `config/` (hằng số cấu hình) — ba thư mục nghe tên "linh tinh/tiện ích" dễ gây nhầm lẫn nhưng vai trò khác nhau.
