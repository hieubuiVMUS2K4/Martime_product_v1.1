# Controllers/Crew — Dữ liệu HR cơ bản của thuyền viên

## Mục đích

Nhóm controller này trả lời câu hỏi **"thuyền viên này là ai, thông tin gì về họ"**: hồ sơ cá nhân, chứng chỉ chuyên môn (STCW), dữ liệu tham chiếu (quốc gia, chức danh), và sổ nhật ký cá nhân. Đây là lớp dữ liệu HR "phẳng" — khác hẳn với `Controllers/CrewManagement/` là cỗ máy quy trình/workflow (xem phần phân biệt ở cuối file). Toàn bộ 5 controller đều gắn `[Authorize(Policy = "InternalAccess")]` ở cấp class (nhưng đọc lưu ý trong `Controllers/README.md` — policy này hiện không chặn ai theo cấu hình mặc định).

## Cấu trúc & vai trò

| File | Route | Vai trò |
|---|---|---|
| `CrewController.cs` | `api/crew` | Trung tâm CRUD thuyền viên: danh sách phân trang, gán/bỏ gán tàu, 4 loại tài liệu tùy thân (travel/seafarer/employment/health), lịch sử phục vụ (`service-records`), upload avatar, và cơ chế xác nhận thay đổi từ Edge (`EdgeChanges`) |
| `CertificatesController.cs` | `api/certificates` | Vừa quản lý danh mục **loại** chứng chỉ (catalog), vừa quản lý chứng chỉ **cụ thể** mà từng thuyền viên đang giữ (`crew-certificates`), cộng báo cáo hết hạn/compliance toàn hạm đội |
| `CountriesController.cs` | `api/countries` | CRUD dữ liệu tham chiếu quốc gia (quốc tịch/flag state) |
| `RanksController.cs` | `api/ranks` | CRUD dữ liệu tham chiếu chức danh (Master, Chief Engineer...), có `Department`/`SortOrder` |
| `LogbookController.cs` | `api/crew/{crewMemberId}/logbook` | CRUD nhật ký cá nhân của thuyền viên, tạo từ phía Shore (`EntryOrigin = "SHORE"`) |

## Luồng hoạt động chính

Phần lớn action đi thẳng: `Controller → ICrewService`/`ICertificateService` (`Services/Crew/`) → `AppDbContext`. Nhưng `CountriesController`, `RanksController`, và một phần của `CertificatesController`/`CrewController` (mapping quốc gia/chức danh, upload file) **bỏ qua service, gọi thẳng `AppDbContext`** — một pattern lặp lại nhiều lần trong codebase này.

Luồng đáng chú ý nhất là **upload tài liệu/chứng chỉ + đẩy sync**:
```
PUT .../crew-certificates/{id}/file  (hoặc .../documents/{category}/{id}/file, hoặc .../avatar)
   │
   ▼
1. Xóa file cũ qua ISyncFileStorageService.DeleteIfExistsAsync
2. Ghi file mới vào uploads/crew/...
3. Cập nhật cột đường dẫn (DocumentFilePath/FileUrl/PhotoUrl) trên entity
4. ISyncOutboxService.BroadcastAsync("crew_certificate" / "crew_member" / ..., ...) → đẩy xuống MỌI tàu
```
Đây chính là idiom "sửa xong rồi phát tán" (mutate-then-broadcast) dùng lặp lại ở cả `CertificatesController`, `CrewController`, `LogbookController`.

Luồng đặc biệt thứ hai là **xác nhận thay đổi từ Edge** (`CrewController.MarkChangesViewed`): khi thuyền viên chỉnh sửa hồ sơ ngay trên tàu, Edge đẩy diff vào cột `CrewMember.EdgeChanges` (JSON) và đặt `EdgeChangesViewed = false`; Shore hiển thị banner "có thay đổi mới", khi nhân viên HR bấm xác nhận thì endpoint này xóa `EdgeChanges` cục bộ **và** phát một sự kiện sync `SyncActionType.CLEAR_EDGE_CHANGES` xuống lại tàu để banner phía Edge cũng biến mất. Đây là cơ chế "hiển thị xung đột", không phải cơ chế "merge" — mọi merge thật sự nằm ở `Services/Sync/ConflictResolverService.cs`.

## Liên kết với phần khác

- Gọi xuống `Services/Crew/ICrewService`, `Services/Crew/ICertificateService` (xem `Services/Crew/README.md`).
- Gọi `ISyncOutboxService` (từ `Services/Sync/`) để đẩy dữ liệu xuống Edge sau mỗi thay đổi — xem `Services/Sync/README.md`.
- `CrewController.CreateAsync` (qua `CrewService`) tự động gọi `IOnboardingService.CreateCaseAsync` để khởi tạo hồ sơ onboarding — đây là **cầu nối duy nhất** từ `Crew/` sang `CrewManagement/`.
- Dữ liệu ở đây (đặc biệt `crew_member`, `crew_certificate`) là các entity đồng bộ hai chiều với Edge — có trường `IsSynced`/`OriginNode`, được xử lý bởi `Services/Sync/SyncInboxService.cs` khi Edge push lên.

## Ghi chú khi đọc/dạy

- **`Crew/` vs `CrewManagement/`** — quy tắc ghi nhớ: nếu câu hỏi là "tên/chức danh/hạn chứng chỉ của thuyền viên X là gì" → tìm ở `Crew/`. Nếu câu hỏi là "thuyền viên X đang ở bước nào của quy trình phân công/onboarding/duyệt hồ sơ, ai cần duyệt tiếp theo" → tìm ở `CrewManagement/`. Cả hai đều thao tác trên cùng entity `CrewMember` (qua `crewMemberId`), nhưng `CrewManagement` thêm các bảng theo dõi quy trình (assignment, case, waiver, submission...) chứ không thay thế hồ sơ gốc.
- **Có 2 khái niệm "compliance" khác nhau** trong hệ thống: `CertificatesController.GetFleetCompliance()`/`GetCrewComplianceAsync()` (ở đây) là kiểm tra đơn giản, hardcode kiểu STCW (thiếu bằng nào, hết hạn bằng nào); `Controllers/CrewManagement/ComplianceController` là một rule-engine cấu hình được, phức tạp hơn nhiều. Đừng nhầm hai cái này khi tìm bug liên quan "compliance".
- Trong `CrewController.cs`, endpoint upload tài liệu theo `category` (`travel`/`seafarer`/`employment`/`health`) rẽ nhánh vào **4 bảng EF khác nhau** (`TravelDocuments`, `SeafarerDocuments`, `EmploymentDocuments`, `HealthDocuments`) — logic xóa-cũ/ghi-mới/broadcast bị copy-paste 4 lần thay vì factor ra hàm dùng chung; nếu sửa một loại tài liệu, rất dễ quên sửa 3 loại còn lại.
- `CertificatesController`: hai endpoint mapping quốc gia/chức danh (`.../countries`, `.../ranks`) là ngoại lệ duy nhất trong file này — khi lỗi, chúng nuốt exception và trả về mảng rỗng thay vì lỗi 500, không giống các action khác trong cùng file.
- Route `crew-certificates/{id}` và route certificate-type `{id}` đều dùng `int id` nhưng thuộc **hai không gian khóa khác nhau** (junction table `crew_certificates` vs bảng catalog `certificates`) — rất dễ nhầm khi đọc nhanh route.
- Các DTO request nhỏ (`AssignVesselRequest` trong `CrewController.cs`, `CountryRequest` trong `CountriesController.cs`) được khai báo ngay trong file controller, không nằm trong `DTOs/`.
