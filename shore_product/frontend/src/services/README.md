# services/ — Lớp gọi API (Axios/Fetch) tới Shore Backend

## Mục đích

Đây là **tầng duy nhất được phép gọi mạng** trong frontend Shore. Mọi trang (`pages/`) hoặc hook (`hooks/`) muốn lấy/ghi dữ liệu đều đi qua một file `*.service.ts` ở đây — không gọi `fetch`/`axios` trực tiếp trong component.

Toàn bộ request đều nhắm tới đường dẫn tương đối `/api/...`. Khi chạy `npm run dev` (port 3000), Vite proxy (`vite.config.ts`) chuyển tiếp `/api` và `/uploads` sang Shore Backend thật ở `http://localhost:5000` (biến `VITE_BACKEND_URL` có thể override). Nhờ vậy code không cần quan tâm CORS hay hard-code host.

## Cấu trúc & vai trò

| File | Export chính | HTTP client | Domain |
|---|---|---|---|
| `api.ts` | *(không có)* | — | **File rỗng** — stub cũ không còn dùng, đừng import nhầm. |
| `api.client.ts` | `ApiClient`, `apiClient`, `getAuthToken()`, `getInternalApiKey()`, `buildAuthHeaders()` | fetch | Client dùng chung: tự gắn `Authorization: Bearer <token>` + header nội bộ `X-Internal-Api-Key`, parse lỗi validation của ASP.NET Core thành message dễ đọc. |
| `auth.service.ts` | `authService` (`login`, `me`) | qua `apiClient` | Đăng nhập, lấy user hiện tại. |
| `crew.service.ts` | `crewApi`, `certificateApi`, `referenceApi`, `logbookApi` | fetch (+`buildAuthHeaders`) | CRUD thuyền viên, tài liệu, service record, chứng chỉ (loại + của từng crew), ranks/countries, logbook, upload avatar/ảnh chứng chỉ. |
| `crewManagement.service.ts` | `onboardingApi`, `documentApi`, `crewProfileApi`, `auditLogApi` | fetch | Case onboarding (tuyển dụng mới), hàng đợi xác minh giấy tờ, lịch sử trạng thái crew, audit log. |
| `compliance.service.ts` | `complianceRuleSetApi`, `complianceRuleApi`, `complianceWaiverApi`, `complianceEvalApi`, `complianceSnapshotApi` | fetch | Bộ quy tắc tuân thủ (STCW/MLC), waiver (miễn trừ), engine đánh giá & simulate, snapshot theo crew/fleet. |
| `assignment.service.ts` | `manningStandardApi`, `manningPositionApi`, `assignmentApi`, `confirmationApi`, `commentApi`, `statusHistoryApi`, `planningApi` | fetch | Manning standard (định biên), phân công thuyền viên lên tàu, planning board, tìm ứng viên thay thế. |
| `onboard.service.ts` | `onboardEventApi`, `accessGrantApi`, `signOnApi`, `signOffApi` | fetch | Sự kiện lên/xuống tàu **vật lý** (arrival, cấp quyền ra vào, sổ sign-on/sign-off). |
| `travel.service.ts` | `travelRequestApi`, `travelSegmentApi`, `travelHistoryApi` | fetch | Yêu cầu di chuyển (vé máy bay...) phục vụ sign-on/sign-off. |
| `externalRequest.service.ts` | `externalRequestApi`, `candidateApi`, `messageApi` | fetch | Yêu cầu tuyển dụng qua đối tác/manning agency ngoài + ứng viên + trao đổi tin nhắn. |
| `voyage.service.ts` | `voyageApi` | fetch | Danh sách chuyến đi, fleet dashboard, timeline, performance, review chuyến đi. |
| `maritime.service.ts` | `maritimeService` (`maintenance`, `crew`, `voyage`), `voyageService` *(@deprecated)* | **axios** (đăng ký `axios.interceptors.request.use` gắn Bearer token — interceptor này là **toàn cục**, ảnh hưởng mọi file khác cũng import `axios`) | Đọc dữ liệu **đã đồng bộ từ Edge**: maintenance tasks, voyage records. Dùng nhiều trong PMS. |
| `pms.service.ts` | `pmsService` (class `PMSService`) | fetch (không gắn auth header) | Maintenance schedules, equipment assets, master schedule theo khoảng ngày/department. |
| `equipment-asset.service.ts` | `equipmentAssetService` | axios (đăng ký interceptor auth thứ 2, trùng lặp với `maritime.service.ts`) | CRUD equipment asset (cây thiết bị cha/con). |
| `equipment-group.service.ts` | `equipmentGroupService` | axios | Nhóm thiết bị. |
| `maintenance-schedule.service.ts` | `maintenanceScheduleService` | axios | CRUD lịch bảo trì định kỳ + preview lịch sinh task. |
| `maintenance.service.ts` | `getDeferralRequests`, `reviewDeferralRequest` | axios (`axios.create`, instance **riêng** — không có interceptor auth) | **Stub** — comment gốc: *"Shore does not have running maintenance operations. These stubs satisfy type imports used by copied PMS components."* |
| `materialService.ts` | `materialService` | qua `apiClient` | Danh mục & vật tư (categories/items), điều chỉnh tồn kho, gắn vật tư ↔ thiết bị, upload ảnh, lịch sử hoạt động item. |
| `materialRequest.service.ts` | `materialRequestService` | axios | Phiếu yêu cầu vật tư (tạo/sửa/submit/duyệt). |
| `stockReceipt.service.ts` | `stockReceiptService` | axios | Phiếu **nhập kho** đầy đủ (draft → complete) — dùng ở `pages/Materials/StockReceiptPage.tsx`. |
| `receiptService.ts` | `receiptService` | qua `apiClient` | Preview + tạo **Material Receipt** nhanh (Import Receipt) — dùng trong modal `ImportReceiptModal`/`ReceiptDetailModal` bên trong `MaterialPage`. ⚠️ Đây là luồng nhập kho **thứ hai**, song song với `stockReceipt.service.ts` (xem ghi chú bên dưới). |
| `inventory.service.ts` | `inventoryService` | axios | Tồn kho tổng hợp theo location, summary giá trị. |
| `store-location.service.ts` | `storeLocationService` | axios | Vị trí lưu kho (kệ/khu vực). |
| `port.service.ts` | `searchPorts`, `PortOption` | fetch | Autocomplete cảng biển — hậu thuẫn component `PortSelect`. |
| `sync.service.ts` | `syncApi` | fetch (không gắn auth header) | Trạng thái đồng bộ (outbox stats, log, node/tàu), health check, force-push, reconcile. Hậu thuẫn `pages/SyncManagement`. |
| `notification.service.ts` | `notificationApi` | fetch (+`buildAuthHeaders`) | Thông báo chuông trên `TopNavLayout` (sync batch, sign-on/off...). |
| `protectedMedia.ts` | `isProtectedMediaPath`, `resolveProtectedMediaRequestUrl`, `fetchProtectedMediaObjectUrl`, `openProtectedMediaInNewTab` | fetch (+`buildAuthHeaders`) | Tải file/ảnh nằm sau `/uploads/` (cần JWT) thành Blob URL — hậu thuẫn `ProtectedImage`, `ImageViewerModal`. |

## Luồng hoạt động chính

```
Component (pages/ hoặc components/)
   │  gọi hook (hooks/useXxx.ts) hoặc gọi service trực tiếp
   ▼
services/xxx.service.ts
   │  fetch(`${ENV.API_BASE_URL}/...`)  hoặc  axios.get('/api/...')  hoặc  apiClient.get('/...')
   ▼
Vite dev server proxy (vite.config.ts: '/api' → VITE_BACKEND_URL, mặc định http://localhost:5000)
   ▼
Shore Backend ASP.NET Core (port 5000) → PostgreSQL (port 5434)
```

`ENV.API_BASE_URL` (`config/env.ts`) và `API_CONFIG.BASE_URL` (`config/app.config.ts`) đều mặc định là chuỗi tương đối `'/api'` — **không** trỏ thẳng tới `localhost:5000`, vì việc forward là trách nhiệm của Vite proxy lúc dev (và của Nginx lúc build production, xem `nginx/`).

## Liên kết với phần khác

- **hooks/**: phần lớn hook (`useCrew`, `useAssignment`, `useCompliance`, `useOnboard`, `useTravel`, `useExternalRequest`, `useCrewManagement`) chỉ là wrapper `useState`+`useEffect` quanh đúng một service tương ứng — xem `hooks/README.md`.
- **pages/**: nhiều trang (SyncManagement, PMS, Materials, VesselManagement) gọi service trực tiếp mà không qua hook riêng.
- **contexts/AuthContext.tsx**: lưu token vào `localStorage['authToken']` sau khi `authService.login()` thành công; `api.client.ts` đọc lại token đó.
- **components/common/Toast, ConfirmDialog**: hầu hết page bắt lỗi service bằng `try/catch` rồi hiển thị qua `useToast()`.

## Ghi chú khi đọc/dạy

1. **Ba kiểu gọi HTTP cùng tồn tại** — đừng ngạc nhiên khi thấy 3 phong cách khác nhau trong cùng thư mục:
   - `fetch` + hàm `request<T>()` copy-paste riêng trong từng file (crew, crewManagement, compliance, assignment, onboard, travel, externalRequest, voyage, sync, pms, port).
   - `axios` trực tiếp, có interceptor gắn Bearer token đăng ký **toàn cục** trong `maritime.service.ts` và lặp lại lần nữa trong `equipment-asset.service.ts` (PMS/Materials — vì axios là singleton nên interceptor này áp dụng luôn cho `equipment-group`, `inventory`, `maintenance-schedule`, `materialRequest`, `stockReceipt`, `store-location` dù các file đó không tự đăng ký).
   - `apiClient` dùng chung (`auth.service.ts`, `materialService.ts`, `receiptService.ts`) — cách "sạch" nhất, luôn tự gắn header qua `buildAuthHeaders()`.
2. **Không phải service nào cũng gắn JWT.** Các file fetch thủ công liệt kê ở trên (sync, voyage, compliance, assignment, onboard, travel, externalRequest, crewManagement, port, pms.service) **không gọi `buildAuthHeaders()`** — request đi ra ngoài không có `Authorization`. Nếu sau này Backend siết bắt buộc JWT cho các route này, phải rà lại từng file.
3. **`maintenance.service.ts` là code giả (stub)** — tự nhận trong comment đầu file. Đừng debug tưởng nó gọi API thật; nó tồn tại chỉ để các component PMS copy từ Edge sang biên dịch được.
4. **Hai luồng "nhập kho" song song**: `receiptService.ts` (Material Receipt, gọi từ modal trong catalogue vật tư) và `stockReceipt.service.ts` (Stock Receipt, trang riêng `/materials/receipts`). Tên gần giống nhau nhưng là hai API backend khác nhau (`/material/receipts...` vs `/api/stock-receipts`) — khi debug nhớ xác định đang ở luồng nào.
5. **Quy ước đặt tên file không đồng nhất**: `crew.service.ts` (dot-case) vs `materialService.ts`/`receiptService.ts` (camelCase, không dấu chấm). Tìm file bằng tên domain, không đoán theo pattern.
6. Token được đọc từ nhiều key khả dĩ (`authToken`, `token`, `accessToken`, `jwt` — xem `getAuthToken()`), nhưng `AuthContext` chỉ *ghi* vào `authToken`. Các key khác tồn tại để tương thích ngược/phòng hờ, không phải đang được dùng chủ động.
