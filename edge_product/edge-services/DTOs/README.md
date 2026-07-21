# DTOs — Data Transfer Objects cho API request/response

## Mục đích

`DTOs/` định nghĩa hình dạng dữ liệu **đi qua API** (request body, response JSON) — tách biệt hoàn toàn khỏi entity EF Core trong `Models/`. Mục đích: không để lộ trực tiếp cấu trúc bảng DB ra ngoài, cho phép validate input (`[Required]`, `[MaxLength]`...) độc lập với ràng buộc DB, và cho phép 1 entity có nhiều "hình dạng" khác nhau tuỳ ngữ cảnh (tạo mới / cập nhật / hiển thị danh sách / hiển thị chi tiết).

## Cấu trúc & vai trò

### File gốc (namespace `MaritimeEdge.DTOs`)

| File | Domain | Ghi chú |
|---|---|---|
| `AuthDTOs.cs` | Đăng nhập/phiên/user/role | `LoginRequest/Response`, `TokenRefreshRequest/Response`, `ChangePasswordRequest`, `UserInfo`, `SessionInfo`... |
| `ReportingDTOs.cs` | Báo cáo IMO/SOLAS/MARPOL | File lớn nhất nhóm root: DTO cho Noon/Departure/Arrival/Bunker/Position report, workflow (Approve/Transmit/Amendment), báo cáo tuần/tháng. |
| `VoyageDtos.cs` | Voyage tổng quát | Port, PortCall, Crew Assignment, `VoyageDetailDto`, Phase 2 Planning (Cargo/Bunker/CrewChange/Cost/Revenue Plan), `FalForm5Dto` (FAL Form 5 — Crew List theo công ước FAL). |
| `VoyageCockpitDtos.cs` | "Buồng lái" giám sát voyage (Phase 3) | `VoyageCockpitDto`, `CockpitTimelineEvent` (timeline hợp nhất mọi nguồn sự kiện). |
| `VoyageFinancialDtos.cs` | Tài chính voyage (Phase 4) | Expense Request, Advance Payment, Disbursement, Actual Revenue, Settlement — đều có cặp Create/Update + DTO hiển thị. |
| `VoyageEfficiencyDtos.cs` | So sánh ước tính vs thực tế (Phase 5) | `VoyageEfficiencyReportDto` (điểm số 0-100), `EfficiencyDimension`, so sánh chi phí/doanh thu theo hạng mục. |
| `VoyageLogDtos.cs` | Nhật ký hành trình (SOLAS V/28) | `VoyageLogEventTypes` (hằng số + bản đồ icon/màu cho DEP/ARR/COSP/EOSP/NOON/PILOT_ON-OFF/ANCHOR...), timeline item. |
| `AbstractLogDtos.cs` | Nhật ký vắn tắt (tổng hợp hiệu suất voyage) | Header voyage, Leg (chặng), Daily Entry — đối chiếu tồn FO/DO/dầu nhờn/nước ngọt. |
| `DeferralDTOs.cs` | Workflow bảo trì | Deferral request (hoãn task), Task lifecycle (Start/Submit/Verify/BulkVerify), `MorningBriefingDto`, `ApprovalDashboardSummaryDto`. |
| `MaintenanceScheduleDto.cs`, `EquipmentAssetDto.cs`, `EquipmentGroupDto.cs` | PMS master data | Lịch bảo trì, thiết bị (hỗ trợ cây cha-con), nhóm thiết bị. |
| `MaterialDTOs.cs`, `MaterialReceiptDTOs.cs` | Kho vật tư | CRUD Category/Item, import phiếu nhập kho từ Excel (preview trước khi lưu thật). |
| `FuelAnalyticsDTOs.cs` | Phân tích nhiên liệu/CII | EEOI/SFOC/CII theo IMO DCS/EU MRV, dự báo tiêu thụ, cảnh báo hiệu suất. |
| `ShipDataDTOs.cs` | Hồ sơ tàu | DTO cho 9 "tab" dữ liệu + 10 DTO bảng con tương ứng `ShipData`. |
| `SensorNavigationDto.cs` | Cảm biến ESP32/MPU6050 | Pitch/Roll/Heading/Speed/Depth — dùng cho endpoint không cần auth (xem `SessionAuthMiddleware.SkipPaths`). |
| `AssignDetailsDto.cs`, `UpdateTaskDto.cs` | Task bảo trì | Gán chi tiết công việc, cập nhật trạng thái kiểu kéo-thả Kanban. |

### Thư mục con

| Thư mục | Nội dung |
|---|---|
| `Common/PaginationParams.cs` | `PaginationParams` (record chuẩn hoá Page/PageSize/MaxPageSize) + `PaginatedResponse<T>` — **một trong ít nhất 3 kiểu pagination wrapper khác nhau** cùng tồn tại trong dự án (xem Ghi chú). |
| `Crew/CrewDtos.cs` | **Không định nghĩa DTO mới** — chỉ re-export (alias) từ `Maritime.Shared.DTOs.Crew`/`.Sync`. Xác nhận: DTO nghiệp vụ Crew "sống" trong thư viện dùng chung với Shore, không phải ở Edge. |
| `Drill/DrillDtos.cs` | DTO cho module Diễn tập: `DrillTypeDto`, `DrillScheduleDto` (Gantt), `DrillLogDto` (biên bản + participants), thống kê. |
| `Logbooks/*.cs` (9 file) | DTO cho từng loại sổ nhật ký — **cùng 1 khuôn mẫu 4 DTO/loại**: `CreateXxxDto`, `UpdateXxxDto`, `XxxResponseDto`, `SignXxxDto`. Có riêng `LogbookCommonDtos.cs` cho `LogbookPaginationDto`/`PaginatedLogbookResponseDto<T>` (pagination wrapper riêng, khác `Common/PaginationParams.cs`). |

## Luồng hoạt động chính

Khuôn mẫu lặp lại xuyên suốt gần như mọi domain (rõ nhất ở `Logbooks/`):

```
CreateXxxDto      → input khi tạo mới (validate qua DataAnnotations, [Required]/[MaxLength]...)
UpdateXxxDto       → input khi sửa — thường property đều nullable để hỗ trợ "chỉ gửi field muốn đổi"
XxxResponseDto      → hình dạng trả về cho client (có thể gộp thêm field tính toán, vd ReportNumber
                       lấy từ MaritimeReport cha thay vì nằm trên chính entity con)
SignXxxDto          → input khi ký (MasterSignature/ChiefEngineerSignature)
```

Entity ↔ DTO được map theo 2 cách tuỳ module: **AutoMapper** (`Mappings/MappingProfiles.cs` — dùng cho Voyage/Crew/Port/Reporting) hoặc **thủ công trong Service** (phổ biến ở Logbooks, ShipData — gán field-by-field trực tiếp trong code).

## Liên kết với phần khác

- **`Models/README.md`** — mỗi DTO thường tương ứng 1 entity, nhưng KHÔNG bắt buộc 1-1 (vd `NoonReportDto` gộp field từ cả `NoonReport` và `MaritimeReport` cha).
- **`Mappings/MappingProfiles.cs`** (xem README gốc `edge-services/README.md`, mục Ghi chú) — nơi khai báo ánh xạ AutoMapper cho 1 phần DTO (Voyage, Crew, Port, Reporting); `MaintenanceProfile` hiện rỗng ("No maintenance mappings at this time") dù có nhiều DTO Maintenance — nghĩa là mapping PMS được làm thủ công trong service, không qua AutoMapper.
- **`Maritime.Shared`** — nguồn của mọi DTO Crew (`CrewMemberDto`, `CertificateDto`...) và DTO Sync (`SyncQueueItemDto`, `SyncPullResponse`...).
- **`Controllers/`** — mọi controller nhận `[FromBody]`/`[FromQuery]` bằng các DTO định nghĩa ở đây.

## Ghi chú khi đọc/dạy

- **Có ít nhất 3 kiểu "pagination wrapper" khác nhau cùng tồn tại**: `PaginationParams`/`PaginatedResponse<T>` (`DTOs/Common/`), `LogbookPaginationDto`/`PaginatedLogbookResponseDto<T>` (`DTOs/Logbooks/`), và `ReportPaginationDto`/`PaginatedReportResponseDto<T>` (`ReportingDTOs.cs`) — chưa được hợp nhất thành 1 chuẩn chung. Khi dạy phân trang, nên chỉ rõ đây là 3 cài đặt độc lập, không phải 1 cái dùng chung.
- **`DTOs/Crew/CrewDtos.cs` gần như trống về mặt định nghĩa mới** — chỉ có `using` alias. Nếu tìm "DTO của CrewMember", đường dẫn đúng là `Maritime.Shared.DTOs.Crew.CrewMemberDto`, không phải tìm trong thư mục `DTOs/Crew/` của edge-services.
- **`GarbageRecordDtos.cs` vs `GarbagePartIDtos.cs`/`GarbagePartIIDtos.cs`**: đây là 2 hệ thống sổ rác song song (bản gộp cũ hơn vs. bản chia theo MARPOL Annex V Part I/Part II) — dễ nhầm là trùng lặp nếu không đọc kỹ Controllers/Services tương ứng.
