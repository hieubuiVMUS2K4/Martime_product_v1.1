# pages/ — Các trang (routes) của Edge Dashboard

## Mục đích

Mỗi thư mục con của `pages/` tương ứng với một route (hoặc một nhóm route) khai báo trong `src/App.tsx`.
Đây là "điểm vào" của mỗi tính năng: một `*Page.tsx` load dữ liệu (qua `services/`), quản lý state cục bộ
(`useState`, đôi khi `stores/`), và ráp các `components/` lại thành giao diện hoàn chỉnh. Toàn bộ route
(trừ `/login`) được bọc bởi `AuthGuard` + `MainLayout` (`components/layouts/`) — xem `App.tsx` để biết cây
route chính xác.

Các trang **phức tạp** (nhiều tầng con hoặc nhiều luồng nghiệp vụ) có README riêng ngay trong thư mục của
chúng: **`HSQE/`**, **`Sync/`**, **`Maintenance/`** + **`PMS/`**, **`Reporting/`**, **`logbooks/`**. Đọc file
này trước để có bản đồ tổng quan, sau đó đi sâu vào README con tương ứng.

## Cấu trúc & vai trò

| Thư mục | Route chính (`App.tsx`) | Vai trò | Trạng thái |
|---|---|---|---|
| `Dashboard/` | `/dashboard` | Trang chủ: tổng quan KPI, vị trí, cảnh báo | Hoạt động |
| `Navigation/` | `/navigation` | Hải đồ (leaflet/VesselMap), vị trí GPS real-time | Hoạt động |
| `Engine/` | *(route bị comment "Temporarily hidden")* | Giám sát máy chính | Chỉ là placeholder "Coming soon" |
| `Alarms/` | *(route bị comment)* | Danh sách báo động an toàn | Chỉ là placeholder "Coming soon" |
| `Compliance/` | *(route bị comment)* | Tuân thủ chung | Chỉ là placeholder "Coming soon" |
| `FuelAnalytics/` | *(route bị comment)* | Dashboard phân tích nhiên liệu (Recharts, CII) | **Đã code đầy đủ** (`fuelAnalyticsService`) nhưng tạm ẩn khỏi route |
| `Auth/` | `/login` (public, không qua `AuthGuard`) | Đăng nhập, giới thiệu sản phẩm (tab about/services/contact) | Hoạt động |
| `Admin/` | `/admin/accounts` | Quản lý tài khoản & vai trò (chỉ role ADMIN) | Hoạt động |
| `AuditLog/` | `/audit-log` | Nhật ký audit hệ thống (ISM Code Ch.12 / IMO MSC.428) | Hoạt động |
| **`Crew/`** | `/crew/members`, `/crew/certificates(/:id)`, `/crew/:id` | Quản lý thuyền viên, chứng chỉ, hồ sơ chi tiết, duyệt thuyền viên mới từ Shore | Hoạt động — **README riêng**, đọc cùng `components/crew/README.md` |
| `Voyage/` | `/voyage`, `/ports` | Quản lý chuyến đi (tabs: overview/port-calls/crew/cargo/planning/cockpit/financial/efficiency/FAL Form 5), quản lý cảng | Hoạt động |
| `ShipData/` | `/ship-data` | Hồ sơ kỹ thuật + thương mại tàu (9 tab) | Hoạt động — xem `components/ship-data/*Tab.tsx` |
| **`Maintenance/`** | `/pms/maintenance(/:id)` | Kanban công việc bảo trì + trang chi tiết | Hoạt động — **README riêng**, đọc cùng `PMS/` |
| **`PMS/`** | `/pms/*` (catalog, logistics, master-schedule, work-planning, approval-dashboard...) | 10 trang con: tài sản, lịch bảo trì, duyệt, kế hoạch công việc, lịch sử | Hoạt động — **README riêng** |
| **`Material/`** | `/pms/catalog/materials` | Danh mục vật tư (category/item) | Hoạt động — **README riêng** |
| **`MaterialRequest/`** | `/pms/logistics/material-requests` | Phiếu yêu cầu vật tư (Draft→Submitted→Approved) | Hoạt động — **README riêng** |
| `StockReceipt/` | `/pms/logistics/stock-receipts` | Phiếu nhập kho | Hoạt động |
| `StoreLocation/` | `/pms/catalog/store-locations` | Vị trí kho/kệ | Hoạt động |
| `Inventory/` | `/pms/logistics/inventory` | Tồn kho | Hoạt động |
| **`Reporting/`** | `/reporting`, `/reporting/reports(/:id)`, `/reporting/{noon,departure,arrival,bunker,position}/...` | 5 báo cáo IMO + báo cáo Weekly/Monthly tổng hợp | Hoạt động — **README riêng** |
| **`logbooks/`** | `/logbooks/{deck,engine,oil,garbage,ballast,watchkeeping,voyage,abstract}` | Toàn bộ sổ nhật ký SOLAS/MARPOL | Hoạt động — **README riêng** |
| `Safety/` | `/safety/drills` | Timeline diễn tập an toàn (Gantt-style) | Hoạt động |
| **`HSQE/`** | `/safety/hsqe`, `/safety/hsqe/form/:templateId` | Quản lý tài liệu SMS/ISM, sự cố & CAPA, đánh giá rủi ro & giấy phép làm việc | Hoạt động — **README riêng**, cấu trúc nhiều tầng |
| **`Sync/`** | `/sync` | Giám sát & kích hoạt đồng bộ dữ liệu Edge ↔ Shore | Hoạt động — **README riêng** |

## Luồng hoạt động chính

Mẫu chung của gần như mọi `*Page.tsx`:

```
Route trong App.tsx (vd. <Route path="pms/maintenance" element={<MaintenancePage />} />)
   ▼
*Page.tsx
   │  useEffect(() => { fetchData() }, [])
   │  gọi 1+ hàm trong services/*.service.ts
   ▼
services/*.service.ts → apiClient/axios/fetch → Vite proxy /api → Edge API (port 5001)
   ▼
setState (useState cục bộ trong page)  — ĐA SỐ page tự quản lý state riêng, ít khi đẩy lên Zustand
   hoặc: cập nhật stores/*.store.ts (chủ yếu maritime.store cho dashboard/telemetry, auth.store cho user)
   ▼
Render UI bằng components/ (modal thêm/sửa, bảng, kanban...) + components/ui, components/common
```

Toàn bộ route (trừ `/login`) nằm trong `<Route element={<AuthGuard><MainLayout /></AuthGuard>}>` —
`AuthGuard` (`components/auth/`) chặn nếu chưa đăng nhập, `MainLayout` (`components/layouts/`) vẽ
Sidebar + Header dùng chung.

## Liên kết với phần khác

- **`services/`**: mọi page gọi service tương ứng để lấy/ghi dữ liệu — xem `services/README.md` để biết 3
  cách gọi API khác nhau đang tồn tại song song.
- **`components/`**: page ráp UI từ các thư mục con tương ứng cùng tên miền (vd. `pages/Maintenance` dùng
  `components/maintenance/KanbanBoard`).
- **`stores/`**: `auth.store` (toàn bộ page cần biết user hiện tại), `settings.store` qua
  `useTranslationSafe()`, `maritime.store` (chủ yếu Dashboard).
- **`App.tsx`**: nguồn duy nhất định nghĩa route thật — khi một trang "không thấy được" trên UI, luôn kiểm
  tra file này trước (có thể route đang bị comment, như `Engine`, `Alarms`, `Compliance`, `FuelAnalytics`).

## Ghi chú khi đọc/dạy

- **Không phải mọi thư mục trong `pages/` đều có route đang hoạt động.** `Engine`, `Alarms`, `Compliance` là
  các trang "Coming soon" thật sự (không có logic), còn `FuelAnalytics` đã code đầy đủ nhưng route bị comment
  tạm thời trong `App.tsx` — nghĩa là code chạy được nếu bật lại route, chỉ là hiện không tiếp cận được từ
  menu.
- Nhiều trang có **bản `.old.tsx` / `_Old.tsx`** nằm cạnh bản đang dùng, ví dụ:
  `Material/oldMaterialPage.tsx`, `MaterialRequest/MaterialRequestPage.old.tsx`,
  `PMS/WorkPlanningPage.old.tsx`, `PMS/EquipmentGroupsPage_Old.tsx`, `StockReceipt/StockReceiptPage.old.tsx`.
  Đã kiểm tra: **không file nào trong số này được import ở bất kỳ đâu** — đây là bản lưu trữ lịch sử, không
  phải code đang chạy. Khi tìm trang đang hoạt động, luôn ưu tiên file **không** có hậu tố `old`.
  `PMS/ScheduleConfigPage.tsx` cũng không còn được route riêng — theo comment trong `App.tsx`, tính năng này
  đã được gộp vào tab "Cấu hình" trong `WorkPlanningPage`.
- `Voyage/`, `ShipData/`, `Safety/` không có README riêng (không thuộc nhóm "phức tạp nổi bật" theo
  phạm vi tài liệu này) nhưng vẫn là các trang lớn, nhiều state — khi cần đào sâu, đọc trực tiếp file
  `*Page.tsx` tương ứng và service cùng tên miền trong `services/README.md`.
- `Auth/LoginPage.tsx` là trang **duy nhất** không nằm trong `AuthGuard`/`MainLayout` — nó tự vẽ toàn bộ
  layout riêng (kể cả tab "giới thiệu sản phẩm/dịch vụ/liên hệ" — không chỉ là form đăng nhập).
