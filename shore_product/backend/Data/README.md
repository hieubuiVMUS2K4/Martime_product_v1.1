# Data — EF Core DbContext & Cấu hình Database

## Mục đích

Đây là điểm nối duy nhất giữa entity C# (`Models/`) và PostgreSQL. Toàn bộ index, độ chính xác số thập phân, quan hệ khoá ngoại, tên bảng thật (snake_case hay PascalCase), và dữ liệu seed ban đầu đều khai báo tập trung ở đây — **không** nằm rải rác trong các file `Models/*.cs` (khác với một số dự án EF Core khác dùng attribute trên entity là chính).

## Cấu trúc & vai trò

| File | Vai trò |
|---|---|
| `AppDbContext.cs` (1878 dòng — file cấu hình lớn nhất backend) | `DbContext` chính: khai báo ~120 `DbSet<T>` (khớp con số "110+ bảng" nêu trong README gốc dự án) + `OnModelCreating` cấu hình từng entity |
| `AppDbContextDesignTimeFactory.cs` | `IDesignTimeDbContextFactory<AppDbContext>` — cho phép chạy `dotnet ef migrations add ...` từ dòng lệnh mà không cần khởi động toàn bộ ứng dụng/DI container thật |

## Luồng hoạt động chính

```
appsettings.json / appsettings.Development.json / biến môi trường
   ConnectionStrings:DefaultConnection
        │
        ▼
Program.cs: builder.Services.AddDbContext<AppDbContext>(...)
   .UseNpgsql(conn).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
        │                     ▲
        │                     └─ MẶC ĐỊNH TOÀN BỘ QUERY LÀ NoTracking — muốn sửa
        │                        rồi SaveChanges phải tự .AsTracking() (xem ghi chú)
        ▼
Khi app khởi động (Database:AutoMigrate=true, mặc định true):
   db.Database.Migrate()                          — chạy migration EF chuẩn (thư mục Migrations/)
   + hàng loạt db.Database.ExecuteSqlRawAsync(...) — patch tay ngay trong Program.cs, KHÔNG
                                                      qua migration (tạo bảng voyage_reviews,
                                                      thêm cột IsRunning/VesselId, seed user admin...)
        │
        ▼
AppDbContext instance (Scoped — 1 instance/request) được inject vào Controllers/Services
        │
        ▼
OnModelCreating(modelBuilder) áp dụng MỖI LẦN app khởi động (không phải mỗi request) —
   quyết định tên bảng thật, index, precision, cascade/restrict/set-null khi xoá cha
```

## Liên kết với phần khác

- **Models/**: mọi `DbSet<T>` trỏ tới 1 entity trong `Models/*.cs` — xem `Models/README.md` để biết entity nào thật sự nằm trong `shore_product/backend/`, entity nào chỉ là alias sang thư viện dùng chung `Maritime.Shared` (qua `Models/SharedTypeAliases.cs`). File này là nơi DUY NHẤT xác nhận một entity có thật sự có bảng DB hay không (nếu không có `DbSet<T>`, entity đó không tồn tại trong CSDL).
- **Services/**: mọi service inject `AppDbContext` trực tiếp — đây là điểm truy cập dữ liệu chính của toàn backend (xem `Services/README.md`, phần "Repository pattern gần như không tồn tại").
- **`Program.cs`**: chạy migration + hàng loạt patch SQL thô + seed user admin ngay khi ứng dụng khởi động (không phải tại đây, nhưng đáng biết vì nó quyết định schema thật sẽ trông ra sao trước khi `AppDbContext` được dùng lần đầu).
- **Bên ngoài `shore_product/backend/`**: việc khởi tạo container PostgreSQL cho Shore (seed dump ban đầu, script SQL bổ sung) nằm ở `shore_product/init-scripts/` và `shore_product/production/init-scripts/` (không thuộc phạm vi backend, không có README riêng ở đây) — dùng bởi `docker-compose.yml` khi tạo container Postgres lần đầu, tách biệt hoàn toàn với cơ chế `Database:AutoMigrate` chạy trong chính ứng dụng .NET.

## Ghi chú khi đọc/dạy

- **`QueryTrackingBehavior.NoTracking` là mặc định toàn cục** (cấu hình ở `Program.cs`, không phải ở đây, nhưng ảnh hưởng trực tiếp cách dùng `AppDbContext`). Điều này nghĩa là mọi LINQ query trả về entity **không được EF theo dõi thay đổi** — nếu code load 1 entity rồi sửa property rồi gọi `SaveChangesAsync()` mà quên `.AsTracking()` trước đó, thay đổi sẽ **âm thầm không được lưu**, không có exception nào cả. Rất nhiều chỗ trong `Services/Sync/`, `Services/CrewManagement/` phải nhớ gọi `.AsTracking()` một cách tường minh trước khi sửa — đây là nguồn lỗi phổ biến nhất khi mới quen codebase.
- **Nhiều thay đổi schema KHÔNG nằm trong `Migrations/`** — chúng nằm dưới dạng `ExecuteSqlRawAsync` ngay trong `Program.cs`, chạy mỗi lần app khởi động với cú pháp `IF NOT EXISTS`/`ADD COLUMN IF NOT EXISTS` (an toàn chạy lại nhiều lần). Ví dụ: bảng `voyage_reviews`, cột `EngineData.IsRunning`, cột `VesselId` trên `crew_members`/`equipment_assets`/`material_items`/`MaintenanceTasks`, và bảng `shore_notifications`. Khi tìm "cột này được tạo ở đâu", nếu không thấy trong `Migrations/`, hãy tìm tiếp trong `Program.cs`.
- **User admin mặc định được seed tự động** (`Program.cs`, `INSERT ... ON CONFLICT (Username) DO NOTHING`): username `admin`, mật khẩu lưu dạng **plain text** `Admin@123` — khớp với phát hiện ở `AuthController` rằng hệ thống chấp nhận cả mật khẩu plain-text lẫn SHA256-base64, không dùng bcrypt/salted hash. Cần biết trước khi triển khai production thật.
- **Tên bảng không nhất quán giữa PascalCase và snake_case** — ví dụ `Vessels`, `Ships`, `Users` giữ nguyên PascalCase (theo quy ước EF Core mặc định), trong khi phần lớn entity "nghiệp vụ mới hơn" (`crew_members`, `sync_outbox`, `voyage_records`, `certificates`...) được `entity.ToTable("...")` ép về snake_case tường minh trong `OnModelCreating`. Khi viết SQL thô hoặc debug trực tiếp trên Postgres, luôn tra đúng tên bảng thật trong `OnModelCreating` thay vì đoán theo tên C# class.
- **`VesselCertificate` (entity C#) ánh xạ tới bảng tên `"Certificates"`** (`entity.ToTable("Certificates")`, comment: giữ tên bảng cũ để tương thích ngược) — dễ nhầm với bảng `certificates` (chữ thường, entity `Certificate` — chứng chỉ CỦA THUYỀN VIÊN, đến từ `Maritime.Shared`). Hai bảng tên gần giống hệt nhau (chỉ khác hoa/thường) nhưng mang ý nghĩa hoàn toàn khác: `Certificates` (viết hoa) = chứng chỉ của TÀU; `certificates` (viết thường) = danh mục loại chứng chỉ của THUYỀN VIÊN.
- Toàn bộ cấu hình ép kiểu UTC cho `DateTime` (`HasConversion(v => v.ToUniversalTime(), v => DateTime.SpecifyKind(v, DateTimeKind.Utc))`) xuất hiện lặp lại rất nhiều lần trong `OnModelCreating` — đây là cách xử lý việc **Npgsql từ chối ghi `DateTime.Kind = Unspecified/Local`** vào cột `timestamptz`. Khi thêm entity/cột `DateTime` mới, phải nhớ thêm đúng cấu hình này, nếu không sẽ gặp lỗi runtime khi insert.
