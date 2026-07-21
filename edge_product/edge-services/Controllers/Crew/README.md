# Controllers/Crew — Thuyền viên, chứng chỉ, danh mục quốc gia/chức danh

## Mục đích

Quản lý hồ sơ thuyền viên trên tàu và toàn bộ dữ liệu master liên quan (quốc gia, chức danh, loại chứng chỉ, chứng chỉ theo rank/quốc gia bắt buộc). Đây cũng là nơi triển khai **quy trình duyệt thuyền viên mới từ Shore gửi xuống** (PendingReview → Approved/OnHold/Rejected) — điểm giao thoa trực tiếp với cơ chế `SyncConflictHandler` (`Services/Core/`).

## Cấu trúc & vai trò

| File | Route | Vai trò |
|---|---|---|
| `CrewController.cs` (**1930 dòng — controller lớn nhất toàn bộ dự án**) | `api/crew` | CRUD thuyền viên đầy đủ, hồ sơ cá nhân (`GET me`, `GET me/certificates`), upload avatar, quản lý 4 loại tài liệu tuỳ thân (travel/seafarer/employment/health documents) kèm file, tiện ích đồng bộ user-account, và **quy trình duyệt crew mới từ Shore** (xem mục riêng bên dưới). |
| `CertificatesController.cs` | `api/certificates` | Danh mục loại chứng chỉ (master data) + thống kê số thuyền viên còn hạn/sắp hết hạn/hết hạn theo từng loại + gán chứng chỉ cho thuyền viên cụ thể (crew-certificates) kèm upload file scan. |
| `CountriesController.cs` | `api/countries` | CRUD quốc gia (master data) — controller đơn giản nhất nhóm, chưa có soft-delete. |
| `RanksController.cs` | `api/ranks` | CRUD chức danh (master data) — có cả soft-delete (`DELETE {id}`, set `IsActive=false`) VÀ hard-delete (`DELETE {id}/permanent`) tách biệt rõ ràng. |
| `RankCertificatesController.cs` | `api/rank-certificates` | Bảng nối: chứng chỉ nào bắt buộc theo từng chức danh (STCW). |
| `CountryCertificatesController.cs` | `api/country-certificates` | Bảng nối: chứng chỉ nào bắt buộc theo từng quốc tịch — có 2 route khác nhau trả cùng 1 kết quả (`by-certificate/{id}` và `certificate/{id}` — route sau ghi chú "alias for frontend compatibility"). |
| `LogbookController.cs` | `api/crew/{crewMemberId}/logbook` | Nhật ký cá nhân thuyền viên (huấn luyện, sự kiện nghề nghiệp) — KHÁC HẲN các "logbook" SOLAS/MARPOL ở `Controllers/Logbooks/`. Tự tay thao tác `SyncQueue` trực tiếp (không qua service riêng) — xem `Services/Core/README.md`. |

## Luồng hoạt động chính

### A. Quy trình duyệt thuyền viên mới từ Shore (`CrewController`)

```
Shore tạo/gửi CrewMember mới với OnboardStatus = "PendingReview"
  → SyncConflictHandler (Services/Core) nhận qua Pull, ép IsOnboard = false
  → Crew xuất hiện trong danh sách chờ duyệt:

GET  /api/crew/pending                    -- liệt kê crew đang PendingReview hoặc OnHold

POST /api/crew/{id}/approve  { reviewChecklist }
  → OnboardStatus = "Approved", IsOnboard = true, EmbarkDate = now (nếu chưa có)
  → Tự động tạo User account (CreateUserForCrewMemberAsync) nếu chưa có

POST /api/crew/{id}/hold  { reviewChecklist, reviewNotes }
  → OnboardStatus = "OnHold" — báo hiệu thiếu thông tin, chờ Shore bổ sung

POST /api/crew/{id}/reject  { reason }
  → OnboardStatus = "Rejected", ghi lý do vào Notes

Mọi thao tác đều set IsSynced = false → outbox tự động đẩy trạng thái mới về Shore
```

`approver`/`reviewer`/`rejector` được trích ra từ chính chuỗi access-token (`access_{userId}_{crewId}_{timestamp}_{random}`, lấy phần tử thứ 3) thay vì từ `HttpContext.GetUsername()` — khác với cách "chuẩn" mà `SessionAuthMiddleware` cung cấp.

### B. Tài liệu tuỳ thân — 4 loại dùng chung 1 endpoint tổng quát

```
POST /api/crew/{id}/identity-documents  [FromForm] { targetTable, documentType, documentNumber, file? }
  targetTable ∈ { travel_documents | seafarer_documents | employment_documents | health_documents }
  → Lưu file (nếu có) vào uploads/crew/documents/{loại}/ → tạo entity tương ứng → enqueue SyncQueue thủ công

PUT  /api/crew/identity-documents/{documentId}/file   -- thay file cho 1 tài liệu đã có, cùng cơ chế targetTable
GET  /api/crew/{id}/travel-documents | seafarer-documents | employment-documents | health-documents
```

### C. Upload avatar và file chứng chỉ

`PUT /api/crew/{id}/avatar` (giới hạn 5MB, jpg/jpeg/png/gif) và `PUT /api/certificates/crew-certificates/{id}/file` (giới hạn 10MB, thêm cả pdf) đều: lưu file vật lý → xoá file cũ → cập nhật entity → `IsSynced=false` → **tự tay thêm `SyncQueue`** (action UPDATE, priority Operational) — đây là ví dụ khác về enqueue thủ công song song với outbox tự động của `EdgeDbContext` (2 file này set `IsSynced=false` trên field KHÔNG bị loại trừ, nên cả outbox tự động và enqueue thủ công đều tạo `SyncQueue` — cùng dạng trùng lặp tiềm ẩn như đã nêu ở `Data/README.md`).

## Liên kết với phần khác

- **`Services/Core/SyncConflictHandler`** — quy tắc merge field-level cho `crew_member` (Shore thắng field HR, Edge giữ field vận hành) là "nửa kia" của quy trình duyệt crew ở mục A.
- **`Data/README.md`** — outbox tự động xử lý phần lớn thay đổi `CrewMember`/`CrewCertificate`, nhưng `LogbookController` và các endpoint upload file ở đây vẫn tự enqueue thủ công thêm.
- **`Maritime.Shared`** — entity `CrewMember`, `Certificate`, `CrewCertificate`, `Country`, `Rank`, `RankCertificate`, `CountryCertificate` đều định nghĩa ở thư viện dùng chung, không phải trong `Models/` của Edge.
- **`Controllers/Safety/ComplianceController`... không liên quan trực tiếp**, nhưng đáng đối chiếu: `CertificatesController.GetCertificatesWithCrewCount` tính sẵn số lượng "sắp hết hạn trong 90 ngày" — cùng logic cảnh báo hạn chứng chỉ mà `Controllers/Testing/TestDataController` (đã tắt) từng được thiết kế để seed dữ liệu test.

## Ghi chú khi đọc/dạy

- **`CrewController.cs` là file controller dài nhất dự án (1930 dòng)** — nên đọc theo từng nhóm chức năng (CRUD cơ bản → tài liệu tuỳ thân → workflow duyệt) thay vì tuyến tính. Nhiều đoạn code cũ bị comment lại thay vì xoá hẳn (`GetMyCertificates` còn nguyên khối comment tham chiếu field `CertificateNumber`/`MedicalExpiry` đã bị xoá khỏi `CrewMember` — dấu vết của việc migrate sang hệ thống Certificate/CrewCertificate mới, tương tự lý do `TestDataController` bị vô hiệu hoá ở `Controllers/Testing/`).
- **`UpdateCrew` có cơ chế "diff tracking" thủ công** (`TrackStr`/`TrackDate`/`TrackNum`/`TrackInt`/`TrackBool`) ghi lại danh sách field đã đổi vào `existing.EdgeChanges` (JSON) — đây là cơ chế RIÊNG của Edge để ghi nhớ "Edge đã sửa gì" (khác với field-diff mà `SyncConflictHandler` tạo khi NHẬN thay đổi từ Shore) — dễ nhầm lẫn 2 chiều nếu không đọc kỹ.
- **`CountryCertificatesController` có 2 route trả kết quả giống hệt nhau** (`by-certificate/{id}` và `certificate/{id}`) — comment ghi rõ route sau là "alias" giữ lại vì tương thích ngược với frontend, một ví dụ thực tế về nợ kỹ thuật API do thay đổi hợp đồng giữa các version.
- **`RanksController` là 1 trong số ít controller phân biệt rõ soft-delete và hard-delete qua 2 route riêng** (`DELETE {id}` vs `DELETE {id}/permanent`) — đáng dùng làm ví dụ chuẩn khi dạy REST API design cho thao tác xoá có/không thể khôi phục.
