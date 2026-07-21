# services/ — Tầng gọi Edge API (port 5001)

## Mục đích

Đây là tầng **duy nhất** chịu trách nhiệm giao tiếp HTTP giữa Edge Frontend (port 3002) và Edge Backend
`edge-services` (ASP.NET Core, port 5001). Mỗi file `*.service.ts` bọc một nhóm endpoint REST thành các
hàm TypeScript có kiểu dữ liệu rõ ràng (dựa theo `types/`), để page/component không phải tự ráp URL hay
tự parse JSON.

Toàn bộ request trong dev đi qua **Vite proxy** khai báo tại `vite.config.ts`:

```
/api      → http://localhost:5001   (Edge backend REST API)
/uploads  → http://localhost:5001   (file tĩnh: avatar, chứng chỉ scan...)
/vietmap, /maps → https://maps.vietmap.vn (bản đồ)
```

Nghĩa là code luôn gọi đường dẫn tương đối (`/api/crew`, `/api/maintenance/tasks`...); Vite dev server sẽ
forward sang backend thật. Production build cần cấu hình reverse proxy tương đương (hoặc set
`VITE_BACKEND_URL` lúc build).

## Cấu trúc & vai trò

| File | Miền nghiệp vụ | Cách gọi | Ghi chú |
|---|---|---|---|
| `api.client.ts` | Hạ tầng dùng chung | `fetch` thô | **Nền tảng**: class `ApiClient` (`get/post/put/patch/delete`) + hàm `registerAuthProvider/getAuthToken/getCurrentAccountName`. Xem chi tiết bên dưới |
| `maritime.service.ts` | Telemetry, crew, maintenance, voyage, cargo, compliance, material, certificates, countries, ranks, sync, dashboard, audit log | `apiClient` + `fetch` riêng | File **lớn nhất**, export `class MaritimeService` (singleton `maritimeService`) + hàng loạt object export rời (`telemetryService`, `alarmService`, `syncService`, `dashboardService`, `auditLogService`, `materialService` — alias). Có nhiều export `@deprecated` (xem mục Ghi chú) |
| `auth.service.ts` | Đăng nhập/đăng xuất/refresh token/đổi mật khẩu/quản lý user & role | `apiClient` | Dùng bởi `stores/auth.store.ts` |
| `crew.service.ts` | Lấy nhanh danh sách thuyền viên onboard | `apiClient` | Bản rút gọn, chủ yếu phục vụ dropdown chọn thuyền viên (vd. Drill) |
| `voyage.service.ts` | Port, PortCall, Voyage Crew Assignment, Voyage Detail, Cargo Operation, FAL Form 5 | `fetch` riêng (private `request()`) | Không dùng `apiClient` lẫn `axios` — tự cài lại logic inject token, là pattern thứ 3 (xem dưới) |
| `equipment-asset.service.ts` | Danh mục thiết bị (cây parent/child), running hours | `axios` | Tự đăng ký `axios.interceptors.request.use` để gắn Bearer token |
| `equipment-group.service.ts` | Nhóm thiết bị | `axios` | |
| `maintenance-schedule.service.ts` | Lịch bảo trì định kỳ (PMS schedule) + preview cho Gantt chart | `axios` | |
| `maintenance.service.ts` | Workflow duyệt bảo trì v2: pending approval, approval summary, `verifyTask`, deferral request review | `axios` | **Khác** với `maritimeService.maintenance` (CRUD task/Kanban) — xem cảnh báo trùng tên |
| `materialService.ts` | Danh mục vật tư: category + item (CRUD đầy đủ) | `apiClient` | Là bản **đang được dùng thật** bởi `pages/Material`; xem cảnh báo trùng tên với `maritimeService.material` |
| `materialRequest.service.ts` | Phiếu yêu cầu vật tư (Material Request / đề xuất cấp phát) | `axios` | |
| `stockReceipt.service.ts` | Phiếu nhập kho (Stock Receipt) | `axios` | |
| `receiptService.ts` | Import nhanh danh sách vật tư từ phiếu nhập (bulk import item) | `apiClient` | Khác mục đích với `stockReceipt.service.ts` dù tên gần giống |
| `store-location.service.ts` | Vị trí kho / kệ hàng | `axios` | |
| `inventory.service.ts` | Tồn kho (số lượng, giá trị, export CSV) | `axios` | |
| `ship-data.service.ts` | Dữ liệu tàu: basic/dimensions/machinery/shipowner/charterer/class-flag/insurance/radio/tanks | `axios` | Phục vụ `pages/ShipData` (9 tab) |
| `fuelAnalyticsService.ts` | Dashboard phân tích nhiên liệu (CII/hiệu suất) cho Recharts | `axios` | |
| `drill.service.ts` | Lịch diễn tập an toàn (Drill), tính vị trí thanh Gantt | `axios` | |
| `logbook.service.ts` | **Tất cả** sổ nhật ký: Deck, Engine, Oil Record, Watchkeeping, Garbage (Part I & II), Ballast Water, Voyage Log + endpoint ký (sign) từng loại | `apiClient` | Service trung tâm của `pages/logbooks` |
| `abstractlog.service.ts` | Abstract Log (nhật ký tổng hợp theo chuyến đi) + export Excel/PDF | `apiClient` + `fetch` thô (tải blob) | |
| `reporting.service.ts` | 5 báo cáo IMO (Noon/Departure/Arrival/Bunker/Position) + workflow duyệt + báo cáo tổng hợp Weekly/Monthly | `apiClient` | Dùng chung bởi `pages/Reporting` **và** `components/WeeklyReport`, `components/MonthlyReport` |
| `sms.service.ts` | Tài liệu SMS/ISM: cây chapter/procedure, form template, bản ghi đã điền, ký/duyệt | `apiClient` | Hậu thuẫn cho `pages/HSQE` (SmsDocumentPage) |
| `document.service.ts` | Document Library kiểu ISO 9001: tài liệu, đính kèm, read log, lịch sử | `apiClient` | Hậu thuẫn `pages/HSQE/components/DocumentLibrary` (module viết sẵn nhưng **chưa được route** — xem README của HSQE) |

## 3 cách gọi API cùng tồn tại — điều đầu tiên cần biết

Codebase **không đồng nhất** cách gọi HTTP. Khi đọc một service mới, hãy xác định ngay nó thuộc nhóm nào:

| Nhóm | Cách nhận diện | Nơi tiêm token |
|---|---|---|
| **1. `apiClient` (khuyến nghị, phổ biến nhất)** | `import { apiClient } from './api.client'` | Tự động trong `ApiClient.request()`: đọc token qua closure `_getToken` |
| **2. `axios` trực tiếp** | `import axios from 'axios'` + gọi `axios.get/post(...)` | Mỗi file tự `axios.interceptors.request.use(...)` — nghĩa là **mỗi file import sẽ đăng ký thêm 1 interceptor** vào instance `axios` mặc định (dùng chung toàn app, không gây lỗi nhưng dư thừa) |
| **3. `fetch` tự viết lại** | `voyage.service.ts` có method `private request()` riêng, `abstractlog.service.ts` dùng `fetch` trực tiếp khi tải file blob | Tự copy logic gắn header `Authorization`/`X-User-Name` bằng cách import `getAuthToken`/`getCurrentAccountName` từ `api.client.ts` |

`api.client.ts` là nơi duy nhất định nghĩa `registerAuthProvider()` — được gọi **một lần** khi `AuthGuard`
(`components/auth/AuthGuard.tsx`) load module, để tránh phụ thuộc vòng (`auth.store` cần gọi API, còn
`api.client` cần đọc token từ `auth.store`).

## Luồng hoạt động chính

```
Page/Component
   │  gọi hàm service (vd. logbookService.createDeckEntry(dto))
   ▼
services/*.service.ts
   │  apiClient.post(...) HOẶC axios.post(...) HOẶC fetch(...) riêng
   │  tự động gắn header: Authorization: Bearer <accessToken>, X-User-Name: <username>
   ▼
Vite dev proxy  (/api/* → http://localhost:5001/api/*)
   ▼
Edge Backend (edge-services, ASP.NET Core, Controllers/*)  →  PostgreSQL (port 5433)
   ▼
JSON response
   ▼
service trả về type đã định nghĩa trong types/*.ts
   ▼
Component setState (useState) HOẶC store action (Zustand: stores/*.store.ts) HOẶC React Query cache
   (hooks/useCachedMetadata.ts) → re-render
```

Khi request trả `401` (trừ các endpoint `/auth/*` để tránh vòng lặp), `apiClient` gọi `_onUnauthorized()` —
được `registerAuthProvider` trỏ tới `useAuthStore.getState().clearAuth()` → toàn app quay lại `/login`.

## Liên kết với phần khác

- **`stores/auth.store.ts`**: cung cấp token cho `api.client.ts` qua `registerAuthProvider`; gọi
  `auth.service.ts` cho login/refresh.
- **`types/*.ts`**: mỗi service import DTO/interface tương ứng (vd. `logbook.service.ts` ↔
  `types/logbook.types.ts`).
- **`config/app.config.ts`**: `API_CONFIG.BASE_URL` (mặc định `/api`) dùng bởi `api.client.ts` và
  `maritime.service.ts`; các file dùng `axios` lại tự khai `const API_BASE_URL = '/api'` **riêng lẻ**
  (không import từ `config/`).
- **`pages/Sync`**: dùng `syncService` (trong `maritime.service.ts`) để hiển thị hàng đợi đồng bộ.
- **`hooks/useCachedMetadata.ts`**: bọc thêm React Query lên trên `maritimeService.countries/ranks/voyage`.

## Ghi chú khi đọc/dạy

- **Đừng ngạc nhiên khi thấy nhiều tên trùng nhau**:
  - `materialService` tồn tại ở **2 nơi**: alias `maritimeService.material` (trong `maritime.service.ts`,
    ít trường hơn) và file riêng `services/materialService.ts` (đầy đủ DTO hơn — đây mới là bản
    `pages/Material/MaterialPage.tsx` thực sự import).
  - `maintenance.service.ts` (axios, workflow duyệt/deferral) **khác** `maritimeService.maintenance`
    (apiClient, CRUD task cho Kanban) — hai tầng phục vụ hai trang khác nhau (`ApprovalDashboardPage` vs
    `MaintenancePage`).
- **`maritime.service.ts` chứa nhiều export gắn `@deprecated`** (`crewService`, `maintenanceService`,
  `voyageService`, `cargoService`, `complianceService` — các object export rời ở cuối file). Đây là bản cũ
  không phân trang ở DB, giữ lại để không phá code cũ; code mới nên dùng `maritimeService.crew`,
  `maritimeService.maintenance`... (namespace bên trong class).
- **Quy ước đặt tên file không đồng nhất**: có file `kebab-case.service.ts` (`equipment-group.service.ts`),
  có file `camelCase.ts` không có hậu tố `.service` rõ ràng (`fuelAnalyticsService.ts`, `receiptService.ts`,
  `materialService.ts`). Khi tìm service cho một domain, hãy `grep` theo tên domain thay vì đoán quy ước.
- Không có tầng "interceptor tập trung" — logic xử lý lỗi (401, validation message của ASP.NET
  ModelState) chỉ được viết đầy đủ trong `api.client.ts`; các service dùng `axios` trực tiếp **không** có
  behaviour tương tự (lỗi sẽ ném nguyên dạng `AxiosError`).
