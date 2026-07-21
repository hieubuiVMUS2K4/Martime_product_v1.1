# types/ — Định nghĩa TypeScript dùng chung

## Mục đích

Chứa `interface`/`type`/`enum` cho request/response của từng domain nghiệp vụ, để `services/` và `hooks/` cùng tham chiếu một nguồn sự thật (thay vì định nghĩa lại type trong từng component).

## Cấu trúc & vai trò

| File | Domain | Dùng bởi | Ghi chú |
|---|---|---|---|
| `crew.types.ts` | `CrewMember`, `CrewDetail`, `CrewCertificate`, `CrewDocument`, `ServiceRecord`, `CertificateType`, `Rank`, `Country`, `ComplianceReport`, `PaginatedResponse<T>`, `CrewLogbookEntry`... | `crew.service.ts`, `hooks/useCrew.ts` | Type nền tảng nhất — `PaginatedResponse<T>` cũng được tái sử dụng ở vài service khác. |
| `crewManagement.types.ts` | `OnboardingCase`, `OnboardingCaseStatus` (enum), `DocumentSubmission`, `VerificationTask`, `CrewStatusHistory`, `AuditLog`... | `crewManagement.service.ts`, `hooks/useCrewManagement.ts` | Case tuyển dụng + xác minh giấy tờ. |
| `onboard.types.ts` | `OnboardEventDto`, `CrewAccessGrantDto`, `SignOnRecordDto`, `SignOffRecordDto` + các `*_LABELS` (nhãn tiếng Việt cho enum) | `onboard.service.ts`, `hooks/useOnboard.ts` | Sự kiện lên/xuống tàu vật lý. |
| `externalTravel.types.ts` | `ExternalRequest`, `ExternalCandidate`, `TravelRequest`, `TravelSegment`, `TravelRequestStatus`, `ExternalRequestStatus` (enum) | `externalRequest.service.ts` **và** `travel.service.ts` (2 service dùng chung 1 file type) | Tên file gộp cả "External" (tuyển ngoài) lẫn "Travel" (di chuyển) — dễ đoán nhầm là 2 file riêng. |
| `assignment.types.ts` | `VesselManningStandard`, `ManningPosition`, `CrewAssignment`, `PositionFillStatus`, `AssignmentStatus`, `AssignmentConflict`, `Candidate`... | `assignment.service.ts`, `hooks/useAssignment.ts` | Định biên & phân công thuyền viên lên tàu. |
| `compliance.types.ts` | `ComplianceRuleSet`, `ComplianceRule`, `ComplianceWaiver`, `ComplianceEvaluation`, `EvaluationResult` (enum), `ComplianceSnapshot` | `compliance.service.ts`, `hooks/useCompliance.ts` | |
| `voyage.types.ts` | `VoyageListItem`, `VoyageDetail`, `FleetDashboard`, `VoyageTimeline`, `VoyagePerformance`, `VoyageReview` | `voyage.service.ts` | |
| `maritime.types.ts` | `MaintenanceTask`, `CrewMember` (bản riêng), `VoyageRecord`, `MaterialItem`, `MaterialCategory`, `PositionData`, `NavigationData`... + helper `parseTaskScheduleInfo()` | `maritime.service.ts`, `PMS/WorkPlanningPage`, `PMS/WorkReportPage` | Comment đầu file: *"Based on EdgeModels.cs"* — các type này phản chiếu model bên **Edge backend**, vì dữ liệu hiển thị (maintenance task, voyage record) là dữ liệu được đồng bộ **từ Edge sang Shore**. |
| `pms.types.ts` | `EquipmentAsset` (cây cha/con qua `parentId`/`children`), `EquipmentGroup`, `MaintenanceSchedule`, `MaterialRequest`, `StockReceipt`, `StoreLocation`, `InventoryStockItem`... | Toàn bộ service PMS/Materials (`equipment-*`, `maintenance-schedule`, `materialRequest`, `stockReceipt`, `store-location`, `inventory`) | File type "tổng hợp" cho cả cụm PMS lẫn Materials — không tách riêng theo từng service. |
| `category.types.ts` | `Category`, `CategoryType`, `CategoryCard` | **Không còn ai import** (chỉ được nhắc trong `pages/REFACTOR_NOTES.md`) | Tàn dư từ bản demo `CategoryManagementPage` gốc — `CategoryManagementPage.tsx` hiện tại (tab crew/certificate-types) không dùng type này nữa. |
| `dashboard.types.ts` | `StatCard`, `ModuleItem`, `DashboardData` | **Không còn ai import** (chỉ nhắc trong `REFACTOR_NOTES.md`) | Tàn dư từ `pages/Dashboard/DashboardPage.tsx` — page đó cũng không được route nào dùng (xem `pages/README.md`). |
| `work.types.ts` | `WorkStatus`, `WorkPlan`, `WorkAssignment` (type, **trùng tên** với thư mục page `WorkAssignment/`), `WorkFilter` | `pages/WorkAssignment/WorkAssignmentPage.tsx`, `components/common/StatusBadge` | Hậu thuẫn trang **mock data** — xem `pages/README.md`. |
| `index.d.ts` | *(không có)* | — | **File rỗng.** Không phải barrel, không declare global gì — an toàn để bỏ qua. |

## Luồng hoạt động chính

Không có luồng runtime (đây thuần là type, bị xoá hết lúc build nhờ `verbatimModuleSyntax`/`erasableSyntaxOnly` trong `tsconfig.app.json`). Luồng "biên dịch":

```
services/xxx.service.ts   import type { Foo } from '../types/xxx.types'
hooks/useXxx.ts           import type { Foo } from '../types/xxx.types'
pages/**/*.tsx            import type { Foo } from '../../types/xxx.types'
```

## Liên kết với phần khác

- **services/** là nơi dùng type nhiều nhất (định nghĩa tham số hàm + kiểu trả về của mọi API call).
- **components/common/StatusBadge**: đọc `WorkStatus` (từ `work.types.ts`) để tô màu badge.
- Backend tương ứng: các type ở đây được viết tay để khớp JSON trả về từ Shore Backend (`shore_product/backend/Controllers`, `Models`) — không có công cụ sinh type tự động (không OpenAPI codegen); khi Backend đổi field, phải tự sửa type tay ở đây.

## Ghi chú khi đọc/dạy

- **3 file type mồ côi thật sự**: `category.types.ts`, `dashboard.types.ts` không được import bởi bất kỳ file `.ts/.tsx` nào (chỉ còn nhắc tên trong tài liệu lịch sử `REFACTOR_NOTES.md`). `index.d.ts` rỗng từ đầu. Đừng tốn thời gian cập nhật các file này khi đổi API — chúng không ảnh hưởng runtime.
- Không có barrel `types/index.ts` tổng — mỗi nơi phải import trực tiếp đúng file `types/xxx.types.ts` cần dùng.
- `externalTravel.types.ts` là ví dụ điển hình của việc **một file phục vụ hai domain** (External Request + Travel) — nếu tìm type cho tính năng Travel mà gõ "travel.types" sẽ không ra file nào, phải nhớ tên ghép `externalTravel`.
