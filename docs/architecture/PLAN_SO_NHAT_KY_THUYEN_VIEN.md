# Kế hoạch phát triển tính năng Sổ nhật ký thuyền viên

## 1. Mục tiêu

- Bổ sung một phần mới trong trang chi tiết thuyền viên để quản lý sổ nhật ký.
- Hỗ trợ cả hai luồng dữ liệu `shore` (trên bờ) và `edge` (trên tàu / môi trường kết nối hạn chế).
- Cho phép tạo, xem, chỉnh sửa và xóa các bản ghi nhật ký.
- Đảm bảo đồng bộ dữ liệu cho các bản ghi `edge` khi có kết nối.

## 2. Phạm vi tính năng

- Thiết kế UI cho phần Sổ nhật ký trong trang chi tiết thuyền viên.
- Xây dựng mô hình dữ liệu trong cơ sở dữ liệu.
- Triển khai API CRUD cho nhật ký thuyền viên.
- Định nghĩa cơ chế đồng bộ cho entry `edge`.
- Hỗ trợ phân quyền xem và thao tác.

## 3. Yêu cầu chức năng chính

1. Hiển thị danh sách bản ghi nhật ký của thuyền viên.
2. Tạo mới bản ghi với các trường chung và trường riêng cho `shore`/`edge`.
3. Sửa/xóa bản ghi theo quyền người dùng.
4. Phân biệt rõ nguồn dữ liệu (Shore / Edge) trong UI.
5. Đồng bộ entry `edge` khi có kết nối.
6. Tìm kiếm, lọc theo nguồn, loại và khoảng thời gian.

## 4. Thiết kế dữ liệu

- Tạo bảng mới `crew_logbook_entry`.
- Quan hệ với bảng thuyền viên hiện tại qua `crew_member_id`.
- Các trường chính:
  - `id`, `crew_member_id`, `entry_origin`, `entry_type`, `title`, `description`, `entry_date`
  - `created_by`, `created_at`, `updated_at`, `status`, `notes`
  - `shore_activity`, `training_course`, `shore_location`, `supervisor`
  - `watch_duty`, `navigation_phase`, `incident_type`, `weather_conditions`, `vessel_position`, `operational_notes`
  - `sync_status`, `edge_device_id`, `edge_local_created_at`
- Thêm index cho `crew_member_id`, `entry_origin`, `entry_date`, `sync_status`.

## 5. API cần triển khai

- `GET /api/crew/{crewMemberId}/logbook`
- `POST /api/crew/{crewMemberId}/logbook`
- `PUT /api/crew/{crewMemberId}/logbook/{entryId}`
- `DELETE /api/crew/{crewMemberId}/logbook/{entryId}`
- `POST /api/crew/{crewMemberId}/logbook/sync`
- `GET /api/crew/{crewMemberId}/logbook/pending-sync`

## 6. Thiết kế frontend

- Ứng dụng chính: React + Vite + TypeScript.
- Component đề xuất:
  - `CrewLogbookSection`
  - `CrewLogbookToolbar`
  - `CrewLogbookHistoryList`
  - `CrewLogbookEntryCard`
  - `CrewLogbookDetail`
  - `CrewLogbookForm`
- State management: `Zustand` hoặc store hiện có.
- Tương tác API qua `axios`.
- UI phân biệt nguồn `shore`/`edge` bằng badge màu và block form điều kiện.

## 7. Đồng bộ và offline

- Với app `frontend-edge`, cần hỗ trợ trạng thái `pending_sync`.
- Lưu tạm bản ghi trên client nếu mất kết nối.
- Khi có mạng, gọi API `sync` để gửi batch.
- Hiển thị trạng thái đồng bộ rõ ràng trong danh sách.

## 8. Kiểm tra và chất lượng

- Viết test đơn vị cho model và API.
- Kiểm tra validation trên client và server.
- Xác thực quyền truy cập và thao tác.
- Kiểm thử UI trên cả môi trường shore và edge.

## 9. Bước triển khai đề xuất

1. Khảo sát cấu trúc dữ liệu thuyền viên hiện tại.
2. Thiết kế bảng `crew_logbook_entry` và migration SQL.
3. Xây dựng API CRUD và validation backend.
4. Thêm phần UI vào trang chi tiết thuyền viên.
5. Tích hợp state, fetch dữ liệu và form.
6. Triển khai đồng bộ edge/shore.
7. Kiểm thử toàn diện và hoàn thiện tài liệu.

## 10. Ghi chú

- Ưu tiên rõ ràng giữa `shore` và `edge` để tránh nhầm lẫn dữ liệu.
- Nếu cần, có thể mở rộng model để lưu file đính kèm sau.
- Dự án đã có React + Tailwind, nên tận dụng cùng mô hình hiện có.


Implementation Plan: Crew Member Logbook Feature (Sổ nhật ký thuyền viên)
This plan details the technical steps to design, implement, and verify the Crew Member Logbook (Sổ nhật ký thuyền viên) feature. It covers shared models, backend REST APIs, conflict resolution, sync infrastructure, and React Vite frontends for both Shore and Edge environments.

User Review Required
IMPORTANT

Schema Changes: We are introducing a new table crew_logbook_entries on both Shore and Edge. Migrations must be run on both databases. Bidirectional Sync: Since logs can be created in both environments, we will register the sync mapping so Edge records sync to Shore, and Shore records sync to Edge automatically. Access Permissions: Logbook endpoints will be protected by InternalAccess authorization policy on both sides.

Proposed Changes
1. Shared Models (Maritime.Shared)
We will define the CrewLogbookEntry model in the shared library, which is duplicated in shore_product/shared and edge_product/shared.

[NEW] 
CrewLogbookEntry.cs
 and 
CrewLogbookEntry.cs
Create the core entity schema with fields for:
Base: Id, CrewMemberId, EntryOrigin (SHORE/EDGE), EntryType (SHORE/WATCH/INCIDENT/TRAINING), Title, Description, EntryDate, CreatedBy, Status, Notes.
Shore Activity: ShoreActivity, TrainingCourse, ShoreLocation, Supervisor.
Edge Activity: WatchDuty, NavigationPhase, IncidentType, WeatherConditions, VesselPosition, OperationalNotes.
Sync metadata: EdgeDeviceId, EdgeLocalCreatedAt, IsSynced, OriginNode, SyncVersion, CreatedAt, UpdatedAt.
Inherits from ISyncableEntity.
[MODIFY] 
SharedTypeAliases.cs
 and 
SharedTypeAliases.cs
Register global using CrewLogbookEntry = Maritime.Shared.Models.Crew.CrewLogbookEntry;.
2. Backend Databases & Migrations
[MODIFY] 
AppDbContext.cs
Add public DbSet<CrewLogbookEntry> CrewLogbookEntries { get; set; } = null!;.
Configure CrewLogbookEntry inside OnModelCreating:
Table name: crew_logbook_entries.
Indexes on: CrewMemberId, EntryOrigin, EntryDate, IsSynced.
Cascade delete relationship to CrewMember.
[MODIFY] 
EdgeDbContext.cs
Add public DbSet<CrewLogbookEntry> CrewLogbookEntries { get; set; } = null!;.
Configure CrewLogbookEntry inside OnModelCreating to register table name crew_logbook_entries and indices (will automatically maps to snake_case crew_logbook_entries table in PostgreSQL).
3. Backend REST APIs & Controllers
[NEW] 
LogbookController.cs
Implement the REST controller on Shore:

GET /api/crew/{crewMemberId}/logbook - Search and paginated list with filter.
POST /api/crew/{crewMemberId}/logbook - Create new log entry. Broadcasts to _syncOutbox.
PUT /api/crew/{crewMemberId}/logbook/{entryId} - Update log entry. Broadcasts to _syncOutbox.
DELETE /api/crew/{crewMemberId}/logbook/{entryId} - Delete log entry. Broadcasts to _syncOutbox.
[NEW] 
LogbookController.cs
Implement the REST controller on Edge:

Same CRUD endpoints, but writes to EdgeDbContext and enqueues tasks to _context.SyncQueue instead of using the Shore _syncOutbox service.
Special endpoints for offline/pending sync:
GET /api/crew/{crewMemberId}/logbook/pending-sync - Get records that have not been synced yet (IsSynced == false).
POST /api/crew/{crewMemberId}/logbook/sync - Batch triggers manual sync.
4. Sync Configuration & Conflict Resolution
[MODIFY] 
SyncInboxService.cs
Register crew_logbook_entry in _tableEntityMap: ["crew_logbook_entry"] = typeof(CrewLogbookEntry),
Silently skip plural alias or canonicalize table names if needed.
[MODIFY] 
ConflictResolverService.cs
Rely on standard Last-Write-Wins (LWW) timestamp matching since logs can be updated in either environment independently.
[MODIFY] 
SyncController.cs
Include crew_logbook_entries in full snapshot synchronization queue (POST /api/sync/snapshot and POST /api/sync/snapshot-crew):
csharp

var logbookEntries = await _context.CrewLogbookEntries.AsNoTracking().ToListAsync();
foreach (var x in logbookEntries) Enqueue("crew_logbook_entry", x.Id.ToString(), x);
5. Frontend UI Development (React + Tailwind CSS)
We will build the Sổ nhật ký UI section inside the Crew Details page in both frontend portals. To maintain visual excellence, the design will employ modern curated colors, clean glassmorphism styling, responsive layouts, micro-animations, and descriptive badges.

[MODIFY] 
CrewDetailPage.tsx
Integrate a new "Sổ nhật ký / Logbook" tab or sidebar section.
Design:
Toolbar with search input, filter by entry type (Shore / Watch Duty / Incident / Training), and origin type.
History Timeline view showing list of log cards with descriptive icons:
Shore activity: Green activity badge, location/supervisor text.
Watch duty: Blue badge, navigation phase, weather summary.
Incident: Amber/Red alert icon with description details.
Interactive Action buttons (Add, Edit, Delete).
Modern Slide-Over / Modal form for creating/updating entries. The form dynamically switches input blocks depending on the selected Entry Type.
[MODIFY] 
CrewDetailPage.tsx
Build the matching UI on the Edge frontend.
Added visual indicator showing if a log is Đã đồng bộ (Green cloud icon) or Chờ đồng bộ (Yellow warning cloud icon).
Add support for local offline saving and batch-sync triggers.
Verification Plan
Automated Tests
Migration Verification:
Run dotnet ef migrations add AddCrewLogbookEntries on Shore backend.
Run dotnet ef migrations add AddCrewLogbookEntries on Edge backend.
Run database update and check tables structure in PostgreSQL.
API Verification:
Run local servers and test log CRUD APIs with Postman/Curl.
Sync Engine Verification:
Create a log on Edge, trigger manual sync, and verify it successfully updates on Shore.
Create a log on Shore, trigger manual sync, and verify it propagates to Edge.
Manual Verification
Visual layout check on both Shore and Edge Crew Detail interfaces.
Perform form conditional rendering checks by toggling entry types.


  Dưới đây là chi tiết tất cả các trường thông tin được in và cần điền trong Sổ thuyền viên:

1. Trang thông tin cá nhân (Trang chính)
   Trang này thường được bọc plastic hoặc in trên giấy bảo an, chứa các thông tin định danh cốt lõi của thuyền viên, bao gồm cả tiếng Việt và tiếng Anh:

Số sổ (Book No.): Mã số định danh duy nhất của Sổ thuyền viên.

Họ và tên (Full name): Viết in hoa, đúng theo Giấy khai sinh/Căn cước công dân.

Ngày, tháng, năm sinh (Date of birth): Định dạng ngày/tháng/năm.

Nơi sinh (Place of birth): Tỉnh/Thành phố quốc gia.

Quốc tịch (Nationality): Ví dụ: VIỆT NAM / VIETNAMESE.

Giới tính (Sex): Nam (M) hoặc Nữ (F).

Số Căn cước công dân / Chứng minh nhân dân / Hộ chiếu (ID Card / Passport No.).

Chiều cao (Height): Tính bằng cm.

Màu mắt (Color of eyes): (Ví dụ: Đen/Black, Nâu/Brown).

Đặc điểm nhận dạng (Distinguishing marks): Ghi chú các vết sẹo, nốt ruồi dễ nhận biết.

Ảnh chân dung: Ảnh thẻ cỡ 4x6 hoặc 3x4 (tùy chuẩn quốc gia), phông nền trắng, mặc áo sơ mi hoặc đồng phục hàng hải, có đóng dấu giáp lai của cơ quan cấp.

Chữ ký của người mang sổ (Signature of Bearer).

2. Trang thông tin cơ quan cấp sổ
   Cơ quan cấp (Issuing Authority): Tại Việt Nam thường là Cục Hàng hải Việt Nam (Vinamarine) hoặc các Cảng vụ hàng hải khu vực được ủy quyền.

Nơi cấp (Place of issue).

Ngày cấp (Date of issue).

Ngày hết hạn (Date of expiry): Thường Sổ thuyền viên có thời hạn 5 năm.

Chữ ký, họ tên người có thẩm quyền và con dấu của cơ quan cấp sổ.

3. Trang thông tin liên hệ khẩn cấp (Next of Kin)
   Trang này do thuyền viên tự điền bằng bút bi, dùng để liên lạc trong trường hợp khẩn cấp, tai nạn trên biển:

Họ và tên người báo tin (Name of Next of Kin).

Mối quan hệ (Relationship): (Vợ, chồng, bố, mẹ...).

Địa chỉ liên hệ (Address).

Số điện thoại (Telephone/Mobile).

4. Các trang quá trình công tác (Record of Employment / Sea Service)
   Đây là phần chiếm nhiều trang nhất trong Sổ thuyền viên, dùng để ghi lại toàn bộ "lý lịch đi biển" của thuyền viên. Mỗi lần lên hoặc xuống tàu, Thuyền trưởng (hoặc Đại lý tàu biển) phải điền đầy đủ và đóng dấu vào các cột sau:

Tên tàu (Name of ship).

Hô hiệu (Call sign): Mã liên lạc vô tuyến của tàu.

Số phân cấp của Tổ chức Hàng hải Quốc tế (IMO Number): Mã số duy nhất của tàu không bao giờ thay đổi.

Quốc tịch tàu (Flag State): Tàu mang cờ nước nào.

Tổng dung tích (Gross Tonnage - GT): Dùng để xác định quy mô tàu.

Công suất máy chính (Engine Power - kW): Đặc biệt quan trọng đối với thuyền viên bộ phận Máy để tính thời gian thực tập và nâng hạng bằng cấp.

Chức danh trên tàu (Capacity / Rank): (Ví dụ: Thủy thủ trực ca, Đại phó, Thợ máy, Bếp trưởng...).

Ngày và Cảng lên tàu (Date and Port of joining / Sign-on).

Ngày và Cảng rời tàu (Date and Port of discharge / Sign-off).

Đánh giá năng lực/Hành vi (Conduct / Ability): Có thể ghi "Tốt", "Khá" hoặc để trống tùy quy định từng tàu.

Chữ ký và con dấu của Thuyền trưởng (Signature and stamp of Master): Đóng dấu tàu (Ship's stamp) để xác nhận thông tin là chính xác.

5. Trang gia hạn và ghi chú (Extensions and Remarks)
   Dành cho cơ quan quản lý nhà nước (Cảng vụ hàng hải) ghi chú các thay đổi đặc biệt hoặc gia hạn thời hạn của Sổ thuyền viên (nếu có chính sách cho phép gia hạn thay vì cấp mới).

Lưu ý: Không được tự ý tẩy xóa, sửa chữa bất kỳ thông tin nào (đặc biệt là quá trình công tác) trong Sổ thuyền viên. Hành vi này có thể dẫn đến việc Sổ thuyền viên bị tịch thu và thuyền viên bị cấm đi biển.
