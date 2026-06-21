# 🚢 TÀI LIỆU PHÂN TÍCH KỸ THUẬT CHI TIẾT LUỒNG HOẠT ĐỘNG & KIẾN TRÚC HỆ THỐNG
## Hệ Thống Quản Lý Đội Tàu Biển — Mô Hình Shore-Edge Computing
*(Tài liệu hướng dẫn chuyên sâu cấp mã nguồn - Dành cho Lập trình viên & Kiến trúc sư hệ thống)*

---

## I. TỔNG QUAN KIẾN TRÚC & NỀN TẢNG CÔNG NGHỆ

Hệ thống hoạt động theo mô hình **Edge-Shore Computing** (Tính toán Biên - Trung tâm trên bờ) để đối phó với điều kiện kết nối Internet không liên tục trên đại dương.

```
┌──────────────────────────────────┐         📡 VSAT / 4G / IRIDIUM         ┌──────────────────────────────────┐
│      SHORE SYSTEM (Trên bờ)      │ ◄────────────────────────────────────► │      EDGE SYSTEM (Trên tàu)      │
│  - ASP.NET Core 8 Web API        │        Đồng bộ hai chiều (Sync)        │  - ASP.NET Core 8 Web API        │
│  - React 19 Frontend Dashboard   │       Idempotency & Delta-Sync        │  - React 19 Frontend Dashboard   │
│  - Database PostgreSQL (5434)    │                                        │  - Flutter Mobile (Crew App)     │
│  - Cache & Session Redis         │                                        │  - Database PostgreSQL (5433)    │
└──────────────────────────────────┘                                        └──────────────────────────────────┘
```

### 1. Phân tích nền tảng công nghệ
*   **Backend (Edge & Shore)**: ASP.NET Core 8.0, Entity Framework Core 8, Npgsql (PostgreSQL provider). Ngôn ngữ sử dụng là C# với cú pháp hiện đại (pattern matching, record types, nullable reference types).
*   **Frontend (Edge & Shore)**: React 19, TypeScript 5.x, Vite, Tailwind CSS 3.4.
*   **State Management (Edge)**: `Zustand` phục vụ persistence lưu offline ngay trên trình duyệt của thuyền viên.
*   **Database**: PostgreSQL 15, sử dụng cơ chế lưu trữ quan hệ chuẩn hóa cao kết hợp với JSONB cho các payloads động.

---

## II. QUY ƯỚC LẬP TRÌNH & NỀN TẢNG KỸ THUẬT (UNDERLYING LOGIC)

Dưới đây là các kỹ thuật lập trình và thiết kế lõi được áp dụng xuyên suốt hệ thống để giải quyết các vấn đề về hiệu năng và đặc thù môi trường hàng hải.

### 1. Dependency Injection (DI) & Biến Immutable
Trong toàn bộ các file C# (Services, Controllers), các đối tượng phụ thuộc đều được tiêm qua Constructor và khai báo dưới dạng:
```csharp
private readonly EdgeDbContext _context;
private readonly ILogger<WatchkeepingService> _logger;
```
*   **`private`**: Hạn chế quyền truy cập, chỉ cho phép nội bộ class này sử dụng biến.
*   **`readonly`**: Đảm bảo biến chỉ được gán giá trị **một lần duy nhất** trong Constructor. Tránh việc vô tình gán lại giá trị khác trong quá trình thực thi, bảo vệ an toàn luồng (thread-safety).
*   **Dấu gạch dưới `_`**: Tiền tố quy ước chỉ định đây là một biến thành viên (field) nội bộ lớp, phân biệt với tham số truyền vào constructor hoặc biến local.

### 2. Chuẩn hóa DateTime UTC toàn cục
PostgreSQL với thư viện Npgsql 6.0+ yêu cầu kiểu dữ liệu `timestamp with time zone` phải nhận giá trị `DateTime` có thuộc tính `Kind` là `DateTimeKind.Utc`. Nếu truyền kiểu `Unspecified` hoặc `Local`, hệ thống sẽ ném lỗi runtime.
Để giải quyết triệt để, hệ thống ghi đè phương thức `SaveChanges` và `SaveChangesAsync` trong [EdgeDbContext.cs](file:///f:/NCKH/Product/Martime_product_v1.1/edge_product/edge-services/Data/EdgeDbContext.cs):

```csharp
private void NormalizeDateTimesToUtc()
{
    var entries = ChangeTracker.Entries()
        .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
        .ToList();

    foreach (var entry in entries)
    {
        foreach (var property in entry.Properties)
        {
            if (property.Metadata.ClrType == typeof(DateTime))
            {
                if (property.CurrentValue is DateTime dateTime && dateTime.Kind == DateTimeKind.Unspecified)
                {
                    property.CurrentValue = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                }
            }
            else if (property.Metadata.ClrType == typeof(DateTime?))
            {
                if (property.CurrentValue is DateTime dateTime && dateTime.Kind == DateTimeKind.Unspecified)
                {
                    property.CurrentValue = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                }
            }
        }
    }
}
```
*   **`ChangeTracker.Entries()`**: Truy cập danh sách các thực thể đang được EF Core theo dõi trạng thái.
*   **`EntityState.Added` / `Modified`**: Chỉ chuẩn hóa những dữ liệu chuẩn bị được chèn mới hoặc cập nhật.
*   **`DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)`**: Thiết lập lại cờ Metadata của biến thời gian sang UTC mà không làm thay đổi giá trị giờ thực tế của nó.

Đồng thời, hệ thống đăng ký một `ValueConverter` trong `OnModelCreating` để chuyển đổi tự động khi đọc/ghi dữ liệu từ database:
```csharp
property.SetValueConverter(
    new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
        v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc)));
```

### 3. Tự động ánh xạ snake_case Database Mapping
Quy ước đặt tên C# là PascalCase (ví dụ: `WatchkeepingLog`), trong khi PostgreSQL ưa chuộng snake_case (ví dụ: `watchkeeping_logs`). Trong [EdgeDbContext.cs](file:///f:/NCKH/Product/Martime_product_v1.1/edge_product/edge-services/Data/EdgeDbContext.cs):
```csharp
foreach (var entity in modelBuilder.Model.GetEntityTypes())
{
    // Convert table names to snake_case
    entity.SetTableName(ToSnakeCase(entity.GetTableName() ?? entity.ClrType.Name));

    // Convert column names to snake_case
    foreach (var property in entity.GetProperties())
    {
        var member = property.PropertyInfo ?? (System.Reflection.MemberInfo?)property.FieldInfo;
        var colAttr = member?.GetCustomAttributes(typeof(ColumnAttribute), true)
                            .OfType<ColumnAttribute>()
                            .FirstOrDefault();
        if (colAttr?.Name != null)
            property.SetColumnName(colAttr.Name);
        else
            property.SetColumnName(ToSnakeCase(property.Name));
    }
}
```
*   **`modelBuilder.Model.GetEntityTypes()`**: Lấy toàn bộ danh sách các Class thực thể được khai báo thông qua `DbSet<T>`.
*   **`entity.SetTableName(...)`**: Thiết lập lại tên bảng thực tế trong SQL Database.
*   **`ColumnAttribute`**: Kiểm tra xem cột đó có được lập trình viên ghi đè đặt tên thủ công bằng Attribute `[Column("tên_cột")]` hay không. Nếu không, gọi hàm `ToSnakeCase()` để tự động đổi tên cột.
*   **`ToSnakeCase`**: Hàm chuyển chuỗi, chèn ký tự gạch dưới trước các chữ in hoa và chuyển toàn bộ chuỗi về chữ thường.

---

## III. PHÂN TÍCH LUỒNG HOẠT ĐỘNG CHI TIẾT CỦA 5 CHỨC NĂNG

### 1. Sổ Nhật Ký Vận Hành Hàng Hải (Logbooks - 8 loại)

Hệ thống quản lý 8 loại nhật ký chuyên biệt giúp tàu vận hành tuân thủ luật pháp quốc tế.

#### A. Danh sách 8 loại Logbooks
1.  **Deck Log Book (`DeckLogBook`)**: Sổ nhật ký hành hải (SOLAS Chapter V/28). Ghi chép chạy tàu, vị trí, gió, thời tiết, các sự kiện đặc biệt.
2.  **Engine Log Book (`EngineLogBook`)**: Sổ nhật ký máy (ISM Code). Ghi thông số RPM máy chính, áp suất dầu nhờn, lò hơi, nhiên liệu ROB.
3.  **Oil Record Book Part I (`OilRecordBook`)**: Nhật ký dầu buồng máy (MARPOL Annex I). Ghi việc xử lý nước bilge nhiễm dầu, chuyển dầu cặn cặn bùn (sludge).
4.  **Ballast Water Record Book (`BallastWaterRecordBook`)**: Nhật ký nước dằn (BWM Convention). Ghi việc hút/xả, trao đổi nước dằn ngoài đại dương.
5.  **Garbage Record Book (`GarbageRecordBook`)**: Nhật ký rác chung.
6.  **Garbage Record Part I (`GarbageRecordPartI`)**: Nhật ký rác sinh hoạt thông thường (MARPOL Annex V) bao gồm rác thực phẩm, nhựa, cooking oil.
7.  **Garbage Record Part II (`GarbageRecordPartII`)**: Nhật ký cặn hàng hóa rắn và nước rửa hầm hàng (cấm xả cặn nhóm K - HME ra biển).
8.  **Watchkeeping Log (`WatchkeepingLog`)**: Nhật ký ca trực buồng lái/buồng máy và kiểm soát giờ nghỉ ngơi (STCW/MLC 2006).

#### B. Khai báo mô hình dữ liệu chính (C# Models)
Trong file [EdgeModels.cs](file:///f:/NCKH/Product/Martime_product_v1.1/edge_product/edge-services/Models/EdgeModels.cs):

```csharp
public class DeckLogBook : ISyncableEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public DateTime LogDateTime { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string WatchPeriod { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string OfficerOnWatch { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(30)]
    public string EntryType { get; set; } = string.Empty; // ROUTINE, NAVIGATION, WEATHER, SAFETY...
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? CourseOverGround { get; set; }
    public double? SpeedOverGround { get; set; }
    public double? Heading { get; set; }
    
    // Khóa ngoại liên kết chuyến đi phục vụ phân tích
    public Guid? VoyageId { get; set; }
    public Guid? VoyagePlanLegId { get; set; }
    
    [ForeignKey("VoyageId")]
    [JsonIgnore]
    public virtual VoyageRecord? Voyage { get; set; }

    public bool IsSynced { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string OriginNode { get; set; } = "SHIP_01";
    
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
```
*   **`[Key]`**: Xác định trường `Id` là khóa chính (Primary Key).
*   **`Guid` (Globally Unique Identifier)**: Kiểu dữ liệu định danh duy nhất toàn cầu 128-bit. Việc dùng GUID thay vì `int` tự tăng (1, 2, 3...) giúp ngăn ngừa xung đột khóa chính khi đồng bộ dữ liệu tạo ra từ nhiều tàu (Edge) khác nhau về bờ (Shore).
*   **`double?`**: Kiểu số thực dấu phẩy động cho phép nhận giá trị `null`. Dùng dấu hỏi `?` vì tọa độ GPS hay thời tiết có thể không khả dụng lúc ghi chép ngoại tuyến (ví dụ thiết bị đo bị hỏng).
*   **`virtual`**: Từ khóa khai báo Property điều hướng (Navigation Property). EF Core dùng tính năng này để hỗ trợ cơ chế Lazy Loading hoặc Proxy Creation khi truy vấn thực thể liên kết.
*   **`[JsonIgnore]`**: Ngăn bộ tuần tự hóa System.Text.Json chuyển đổi thuộc tính `Voyage` sang JSON, tránh lỗi tham chiếu vòng lặp vô hạn (circular reference loops) khi serialize dữ liệu gửi qua API.
*   **`IsDeleted`, `DeletedAt`**: Dùng cho cơ chế **Soft Delete (Xóa mềm)**. Thay vì xóa vật lý dữ liệu khỏi đĩa cứng, hệ thống chỉ đổi trạng thái cờ `IsDeleted = true`.

#### C. Chi tiết Luồng Tạo Mới Nhật Ký (Create Entry Flow)
Khi thuyền viên thao tác trên UI [DeckLogPage.tsx](file:///f:/NCKH/Product/Martime_product_v1.1/edge_product/frontend-edge/src/pages/logbooks/DeckLogPage.tsx):
1.  Frontend thu thập thông tin form và gửi HTTP POST chứa JSON Payload đến `/api/logbooks/deck-logbook`.
2.  [DeckLogbookController.cs](file:///f:/NCKH/Product/Martime_product_v1.1/edge_product/edge-services/Controllers/Logbooks/DeckLogbookController.cs) tiếp nhận và chuyển cho Service:

```csharp
[HttpPost]
public async Task<IActionResult> CreateEntry([FromBody] CreateDeckLogEntryDto dto)
{
    var username = HttpContext.GetUsername() ?? "Unknown";
    var (success, id, error) = await _service.CreateEntryAsync(dto, username);
    if (!success) return BadRequest(new { message = error });
    return CreatedAtAction(nameof(GetEntry), new { id }, new { id, message = "Created" });
}
```
*   **`[FromBody]`**: Ép kiểu ASP.NET Core đọc và giải mã chuỗi JSON từ Body của HTTP Request thành DTO Object.
*   **`CreatedAtAction`**: Trả về mã HTTP 201 Created đi kèm Header `Location` chứa đường link GET chi tiết của bản ghi vừa tạo.

3.  Trong Service [DeckLogbookService.cs](file:///f:/NCKH/Product/Martime_product_v1.1/edge_product/edge-services/Services/Logbooks/DeckLogbookService.cs):

```csharp
public async Task<(bool Success, Guid? Id, string? Error)> CreateEntryAsync(CreateDeckLogEntryDto dto, string? username = null)
{
    try
    {
        var entry = new DeckLogBook
        {
            Id = Guid.NewGuid(),
            LogDateTime = dto.LogDateTime,
            WatchPeriod = dto.WatchPeriod,
            OfficerOnWatch = dto.OfficerOnWatch,
            EntryType = dto.EntryType,
            Description = dto.Description,
            // ... copy các thuộc tính đo đạc khác
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsSynced = false
        };

        // Liên kết chuyến đi tự động dựa vào thời gian ghi nhật ký
        var (voyageId, legId) = await _voyageContext.ResolveActiveVoyageAsync(entry.LogDateTime);
        entry.VoyageId = voyageId;
        entry.VoyagePlanLegId = legId;

        _context.DeckLogBooks.Add(entry);
        await _context.SaveChangesAsync();

        return (true, entry.Id, null);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating deck log entry");
        return (false, null, "An error occurred.");
    }
}
```
*   **`_voyageContext.ResolveActiveVoyageAsync`**: Hàm phân giải chuyến đi đang kích hoạt tại mốc thời gian nhật ký ghi nhận. Nhờ đó, nhật ký buồng lái tự động liên kết với mã chuyến đi mà không bắt thuyền viên phải chọn thủ công.
*   **`_context.DeckLogBooks.Add(entry)`**: Đánh dấu thực thể mang trạng thái `EntityState.Added` trong bộ nhớ của DbContext Change Tracker.
*   **`await _context.SaveChangesAsync()`**: Kích hoạt lưu xuống DB. Trong quá trình lưu, phương thức `ProcessSyncQueue()` nội bộ của DbContext tự động được gọi, quét qua thực thể vừa thêm này và tạo ra một bản ghi đồng bộ tại bảng `sync_queue` dưới dạng transaction đồng thời.

---

### 2. Quản Lý Hành Trình (Voyage Management)

Quản lý thông tin thương mại, chặng đi, mớn nước tại các cảng, danh sách thuyền viên trên chuyến tàu.

#### A. Tối ưu hóa truy vấn chi tiết hành trình
Trong [VoyageManagementService.cs](file:///f:/NCKH/Product/Martime_product_v1.1/edge_product/edge-services/Services/Voyage/VoyageManagementService.cs):

```csharp
public async Task<VoyageDetailDto?> GetVoyageDetailAsync(Guid voyageId)
{
    var voyage = await _context.VoyageRecords
        .AsNoTracking()
        .AsSplitQuery() 
        .Include(v => v.PortCalls.OrderBy(p => p.Sequence))
        .Include(v => v.PlanLegs.OrderBy(p => p.Sequence))
        .Include(v => v.StatusHistory.OrderByDescending(h => h.ChangedAt))
        .Include(v => v.CargoPlans.OrderBy(cp => cp.Sequence))
        .Include(v => v.BunkerPlans.OrderBy(bp => bp.Sequence))
        .Include(v => v.CrewChangePlans.OrderBy(ccp => ccp.Sequence))
            .ThenInclude(ccp => ccp.CrewMember)
        .Include(v => v.CrewAssignments)
            .ThenInclude(a => a.CrewMember)
        .FirstOrDefaultAsync(v => v.Id == voyageId);

    if (voyage == null) return null;

    var counts = await _context.VoyageRecords
        .Where(v => v.Id == voyageId)
        .Select(v => new {
            LogCount = _context.VoyageLogEntries.Count(l => l.VoyageId == voyageId),
            CargoCount = _context.CargoOperations.Count(c => c.VoyageId == voyageId)
        })
        .FirstOrDefaultAsync();

    return MapToDetailDto(voyage, counts?.LogCount ?? 0, counts?.CargoCount ?? 0);
}
```
*   **`.AsNoTracking()`**: Tắt tính năng theo dõi trạng thái đối tượng. Khi cần truy vấn dữ liệu chi tiết cực kỳ phức tạp để hiển thị lên UI và không có mục đích chỉnh sửa ghi đè ngay lập tức, tắt Change Tracker giúp tiết kiệm bộ nhớ RAM và tăng hiệu năng xử lý đáng kể.
*   **`.AsSplitQuery()`**: **Câu lệnh phân tách truy vấn**. Mặc định EF Core sẽ dùng SQL JOIN để kéo toàn bộ dữ liệu từ bảng `PortCalls`, `PlanLegs`, `CargoPlans`, `BunkerPlans`, `CrewAssignments` trong một câu lệnh duy nhất. Việc này sinh ra tích Descartes khổng lồ khiến lượng dữ liệu thừa truyền từ Postgres về API phình to. Sử dụng `.AsSplitQuery()` sẽ ép EF Core tách thành các câu lệnh SQL nhỏ gọn chạy song song, giảm lượng dữ liệu thừa truyền qua mạng.
*   **`.ThenInclude()`**: Nối tiếp truy vấn lồng (Eager loading cấp độ 2), lấy thông tin chi tiết của thuyền viên thông qua thực thể kế hoạch thay đổi thuyền viên `CrewChangePlans`.

#### B. Quy tắc khóa dữ liệu theo vòng đời hành trình
Trong phương thức cập nhật `UpdateVoyageAsync`:
```csharp
var currentStatus = voyage.VoyageStatus;

// 1. Kiểm tra chuyển đổi trạng thái hợp lệ
if (dto.VoyageStatus != null && dto.VoyageStatus != currentStatus)
{
    var nextStatus = NormalizeVoyageStatus(dto.VoyageStatus);
    if (!VoyageStatus.IsValidTransition(currentStatus, nextStatus))
        throw new InvalidOperationException($"Invalid status transition: {currentStatus} → {nextStatus}.");
}

// 2. Khóa ghi chép dựa trên trạng thái
if (VoyageStatus.ReadOnlyStatuses.Contains(currentStatus))
{
    if (nextStatus != null && nextStatus != currentStatus)
    {
        voyage.VoyageStatus = nextStatus;
        // Chỉ cho phép cập nhật trạng thái chứ không cho sửa thông tin thương mại khác
        await _context.SaveChangesAsync();
        return voyage;
    }
    throw new InvalidOperationException($"Voyage is {currentStatus} and cannot be modified.");
}
```
*   **`VoyageStatus.ReadOnlyStatuses`**: Chứa hai trạng thái hoàn thành (`COMPLETED`) và hủy (`CANCELLED`).
*   **`ReadOnly`**: Khi tàu cập cảng an toàn và chuyến đi đóng lại, toàn bộ dữ liệu lịch trình, hàng hóa, mớn nước của chuyến đi đó lập tức bị đóng băng (lock) để phục vụ thanh tra cảng biển (Port State Control) và kiểm toán hàng hải, ngăn chặn tuyệt đối chỉnh sửa sai lệch hồi tố.

---

### 3. Hệ Thống Báo Cáo Tuân Thủ (Reporting System)

Thu thập thông số vận hành hàng ngày (Noon Report), lúc xuất bến (Departure) và cập bến (Arrival).

#### A. Luồng tạo báo cáo Noon Report (Sử dụng Transaction an toàn)
Trong [ReportingService.cs](file:///f:/NCKH/Product/Martime_product_v1.1/edge_product/edge-services/Services/Reporting/ReportingService.cs):

```csharp
public async Task<(bool Success, string ReportNumber, Guid? ReportId, string? Error)> CreateNoonReportAsync(
    CreateNoonReportDto dto, string? username = null)
{
    // ... Kiểm tra logic hàng hải chéo
    var reportType = await GetReportTypeByCodeAsync("NOON");
    var reportNumber = await GenerateReportNumberAsync("NOON");

    // Khởi tạo Transaction đảm bảo ghi đồng thời cả bảng cha và con hoặc không ghi gì cả
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        // 1. Tạo bảng cha lưu thông tin hành chính
        var maritimeReport = new MaritimeReport
        {
            Id = Guid.NewGuid(),
            ReportNumber = reportNumber,
            ReportTypeId = reportType.Id,
            ReportDateTime = dto.ReportDate,
            VoyageId = dto.VoyageId,
            Status = "DRAFT",
            PreparedBy = username ?? dto.PreparedBy,
            ReportData = JsonSerializer.Serialize(dto), // Lưu backup dạng chuỗi JSON
            IsTransmitted = false,
            IsSynced = false
        };

        _context.MaritimeReports.Add(maritimeReport);
        await _context.SaveChangesAsync(); // Cần lưu trước để tự tăng ID của cha phục vụ FK của con

        // 2. Tạo bảng con Noon Report chứa thông số kỹ thuật tiêu thụ
        var noonReport = new NoonReport
        {
            MaritimeReportId = maritimeReport.Id,
            ReportDate = dto.ReportDate,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            FuelOilConsumed = dto.FuelOilConsumed,
            DieselOilConsumed = dto.DieselOilConsumed,
            FuelOilROB = dto.FuelOilROB,
            DieselOilROB = dto.DieselOilROB,
            // ...
        };

        _context.NoonReports.Add(noonReport);
        await _context.SaveChangesAsync();

        // 3. Commit toàn bộ transaction xuống đĩa cứng
        await transaction.CommitAsync();
        return (true, reportNumber, maritimeReport.Id, null);
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync(); // Thu hồi lại nếu có lỗi
        _logger.LogError(ex, "Transaction failed while creating noon report");
        throw;
    }
}
```
*   **`BeginTransactionAsync()`**: Bắt đầu một tiến trình giao dịch cơ sở dữ liệu. Báo cáo hàng hải gồm 2 thực thể độc lập (`MaritimeReport` - cha và `NoonReport` - con). Nếu không dùng transaction, trong tình huống máy chủ trên tàu bị mất nguồn điện giữa chừng khi mới chỉ ghi xong bảng cha mà chưa ghi bảng con, hệ thống sẽ rơi vào trạng thái dữ liệu rác (orphan record). Transaction đảm bảo tính toàn vẹn tuyệt đối.
*   **`JsonSerializer.Serialize(dto)`**: Lưu trữ toàn bộ payload gửi lên từ client dưới dạng text JSON trong trường `ReportData` của bảng cha. Đây là giải pháp dự phòng cấu trúc động (schema-less design), giúp bảo toàn nguyên vẹn số liệu khai báo gốc của sĩ quan gửi lên ngay cả khi cấu trúc bảng con có sự nâng cấp/thay đổi sau này.

#### B. Luồng Truyền Báo Cáo Tuân Thủ (Direct HTTP & Fallback)
Tại hàm `TransmitReportAsync`:
```csharp
// 1. Chuyển trạng thái sang TRANSMITTED
report.Status = "TRANSMITTED";
report.IsTransmitted = true;
report.TransmittedAt = DateTime.UtcNow;

var syncItems = await BuildReportSyncItemsAsync(report);

// 2. Cố gắng kết nối trực tiếp gửi về Shore qua API HTTP
var (httpSuccess, httpError) = await SendReportToShoreAsync(syncItems);

var log = new ReportTransmissionLog
{
    MaritimeReportId = reportId,
    TransmissionDateTime = DateTime.UtcNow,
    Status = httpSuccess ? "SUCCESS" : "FAILED"
};
_context.ReportTransmissionLogs.Add(log);

if (!httpSuccess)
{
    // 3. Nếu thất bại (tàu mất sóng), tự động chuyển đổi ném vào sync_queue làm dự phòng
    foreach (var item in syncItems)
    {
        _context.SyncQueue.Add(new SyncQueue
        {
            TableName = item.TableName,
            RecordKey = item.RecordKey,
            ActionType = SyncActionType.CREATE,
            Payload = item.Payload,
            Priority = SyncPriority.Operational,
            CreatedAt = DateTime.UtcNow
        });
    }
    log.Status = "QUEUED"; // Chuyển trạng thái truyền tin sang Chờ gửi trong hàng đợi
}
await _context.SaveChangesAsync();
```
*   **`SendReportToShoreAsync`**: Thao tác gọi trực tiếp Web API trên bờ. Do báo cáo là dữ liệu quan trọng có thời hạn nộp cố định (vd: Báo cáo trưa luôn phải gửi trước 12:00 UTC), hệ thống ưu tiên bắn thẳng HTTP trực tiếp về đất liền để lấy phản hồi tức thì.
*   **`Fallback`**: Nếu mất kết nối, hệ thống biến đổi gói tin báo cáo thành một tác vụ đồng bộ ngầm chèn vào bảng `sync_queue`. Trạng thái báo cáo chuyển thành `QUEUED` thay vì ném lỗi về UI. Thuyền viên vẫn tiếp tục vận hành bình thường, tiến trình ngầm `SyncBackgroundWorker` sẽ tự gửi đi khi tàu kết nối lại vệ tinh.

---

### 4. Quản Lý Hàng Đợi Đồng Bộ (Queue Management & Sync Protocol)

Tiến trình điều phối và đẩy dữ liệu ngoại tuyến từ biên lên bờ.

#### A. Phân tích chi tiết thuật toán đồng bộ (Sync Loop)
Trong file [SyncService.cs](file:///f:/NCKH/Product/Martime_product_v1.1/edge_product/edge-services/Services/Core/SyncService.cs):

```csharp
public async Task ExecuteSyncAsync(CancellationToken cancellationToken)
{
    var retryPolicy = LoadRetryPolicyConfig();
    
    // Cổng kiểm soát số lượng batch xử lý đồng thời để tránh làm nghẽn CPU biên
    if (!await TryEnterPushGateAsync(retryPolicy.MaxInFlightBatches, cancellationToken))
    {
        return;
    }

    using var scope = _serviceProvider.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<EdgeDbContext>();

    try
    {
        var networkType = await GetCurrentNetworkStatusAsync();
        var allowedPriorities = GetAllowedPriorities(networkType);
        var nodeId = _configuration["SyncSecurity:NodeId"] ?? "UNKNOWN";
        var nowUtc = DateTime.UtcNow;

        // 1. Kiểm tra cửa sổ chờ tránh bão kết nối sau khi khôi phục mạng
        if (ShouldDelayForReconnectWarmup(nowUtc, nodeId))
        {
            await ProcessFileTransferCycleAsync(context, cancellationToken);
            return;
        }

        if (allowedPriorities.Count == 0) return;

        // 2. Lấy danh sách chênh lệch dữ liệu (Outbox Pattern)
        var batchSize = GetAdaptiveBatchSize(networkType);
        var pendingItems = await context.SyncQueue
            .Where(q => q.SyncedAt == null)
            .Where(q => allowedPriorities.Contains(q.Priority))
            .Where(q => q.RetryCount < q.MaxRetries)
            .Where(q => q.NextRetryAt == null || q.NextRetryAt <= nowUtc)
            .OrderBy(q => q.Priority) 
            .ThenBy(q => q.CreatedAt) 
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        if (pendingItems.Count == 0) return;

        // 3. Áp dụng Token Bucket rate-limiting ngăn ngừa quá tải API
        if (!TryConsumeToken(nodeId, retryPolicy, nowUtc))
        {
            var tokenDelaySeconds = GetTokenBucketDeferralSeconds(nodeId);
            foreach (var item in pendingItems)
            {
                item.NextRetryAt = nowUtc.AddSeconds(tokenDelaySeconds);
                item.LastError = $"Deferred by token bucket ({tokenDelaySeconds:F2}s)";
            }
            await context.SaveChangesAsync(cancellationToken);
            return;
        }

        // 4. Gửi batch lên Shore
        await SendBatchToShoreAsync(pendingItems, context, cancellationToken);
    }
    finally
    {
        _pushGate.Release(); // Giải phóng cổng kiểm soát batch
    }
}
```

##### Phân tích chi tiết biến & câu lệnh trong Sync Loop:
*   **`_pushGate` (`SemaphoreSlim`)**: Cú pháp khóa đồng bộ dị bộ giới hạn số lượng tác vụ đồng thời trong bộ nhớ.
    *   *Vì sao sử dụng*: `SyncBackgroundWorker` được kích hoạt liên tục. Nếu một vòng đồng bộ trước bị treo/chạy quá chậm do mạng yếu mà vòng sau đã nhảy vào chạy tiếp, việc này sẽ dẫn đến việc gửi trùng lặp dữ liệu hoặc chiếm dụng hết CPU. `SemaphoreSlim` với tham số `(1, 1)` đóng vai trò là một Mutex khóa chặt chỉ cho đúng **1 luồng chạy tại một thời điểm**.
*   **`GetAllowedPriorities(networkType)`**:
    *   *Ý nghĩa nghiệp vụ*: Ánh xạ loại mạng vệ tinh với mức ưu tiên dữ liệu được phép đồng bộ. Nếu mạng là `Iridium` (băng thông cực kỳ đắt đỏ và chậm chạp ~2.4kbps), hàm chỉ trả về `[SyncPriority.Critical]`. Điều này ngăn cản việc đồng bộ các dữ liệu rác thải, nhật ký thông thường hoặc file ảnh scan chứng chỉ, đảm bảo dành toàn bộ tài nguyên đường truyền vệ tinh cho việc gửi tín hiệu khẩn nguy hoặc tọa độ SOS của tàu.
*   **`GetAdaptiveBatchSize(networkType)`**:
    *   *Cơ chế hoạt động*: Kích thước lô gửi (Batch Size) tự động co giãn theo băng thông. Nếu dùng Shore WiFi ở cảng, batch là 200 bản ghi. Nếu dùng VSAT biển khơi, co về 50. Nếu dùng Iridium, co về 5. Tránh việc gói tin HTTP Payload quá lớn bị drop giữa chừng trên đường truyền vệ tinh chập chờn.
*   **`TryConsumeToken()` (Thuật toán Token Bucket)**:
    *   *Mục đích*: Ngăn ngừa hiện tượng **"Herding Effect"** (Bão request). Khi hàng trăm tàu đồng loạt mất kết nối rồi có mạng trở lại, việc tất cả các tàu ngay lập tức gửi hàng ngàn bản ghi dồn ứ sẽ làm sập máy chủ Shore. Thuật toán Token Bucket cung cấp một lượng "Token" (hạn ngạch) nhất định cho mỗi tàu. Mỗi lượt gửi batch phải tiêu tốn 1 Token. Khi hết Token, tàu phải chờ Token tự động nạp lại theo tốc độ cấu hình (`TokenBucketRatePerSecond`).
*   **`ShouldDelayForReconnectWarmup()`**:
    *   *Cơ chế*: Khi khôi phục kết nối vật lý, hệ thống trì hoãn (Warm-up delay) một khoảng thời gian ngẫu nhiên từ 5 đến 120 giây dựa trên mã băm Hash của định danh tàu (`NodeId`). Điều này đảm bảo việc đồng bộ của cả đội tàu được giãn cách tự động trên phổ thời gian (staggered window), giảm tải đỉnh (peak load) cho hệ thống Shore.

#### B. Cơ chế Exponential Backoff & Decorrelated Jitter
Nếu một gói tin đồng bộ bị Shore từ chối hoặc lỗi mạng vệ tinh, hệ thống tính toán thời điểm gửi lại tiếp theo tại hàm `ComputeJitterDelaySeconds`:

```csharp
private double ComputeJitterDelaySeconds(SyncQueue item, RetryPolicyConfig policy)
{
    var exponent = Math.Max(0, item.RetryCount - 1);
    var ceiling = Math.Min(policy.CapDelaySeconds, policy.BaseDelaySeconds * Math.Pow(2, exponent));
    ceiling = Math.Max(policy.BaseDelaySeconds, ceiling);
    
    if (ShouldUseDecorrelatedJitter(policy.Strategy))
    {
        var previous = _previousRetryDelaySeconds.TryGetValue(item.Id, out var prevDelay)
            ? Math.Max(policy.BaseDelaySeconds, prevDelay)
            : policy.BaseDelaySeconds;
        var upperBound = Math.Min(policy.CapDelaySeconds, 3 * previous);
        var delay = NextRandomDouble(policy.BaseDelaySeconds, Math.Max(policy.BaseDelaySeconds, upperBound));
        _previousRetryDelaySeconds[item.Id] = delay;
        return delay;
    }

    var jitterCeiling = Math.Max(policy.BaseDelaySeconds, ceiling * policy.JitterMultiplier);
    return NextRandomDouble(0, jitterCeiling);
}
```
*   **`Math.Pow(2, exponent)`**: Tính toán thời gian giãn cách theo hàm mũ lũy thừa cơ số 2. Lần lỗi thứ 1 đợi 2s, lần thứ 2 đợi 4s, lần 3 đợi 8s, lần 4 đợi 16s... Tránh việc liên tục kết nối lại khi mạng vệ tinh đang mất ổn định kéo dài.
*   **`Decorrelated Jitter`**: Công thức sinh số ngẫu nhiên không tương quan dựa trên thời gian trễ của lần thử lại trước đó. Điều này phá vỡ tính tuần hoàn của các đợt thử lại lỗi giữa các tàu khác nhau, đảm bảo lịch sử thử lại của các tàu bị rải ngẫu nhiên, loại bỏ hoàn toàn khả năng cộng hưởng request gây nghẽn máy chủ Shore.

---

### 5. Nhật Ký Hệ Thống & Audit Log (System Log & Audit Log)

Ghi nhận vết mọi hoạt động chỉnh sửa dữ liệu nhạy cảm trên tàu để đáp ứng chương trình quản lý an toàn hàng hải ISM Code.

#### A. Cấu trúc bộ đánh chặn tự động SaveChanges (Audit Interceptor)
Hệ thống sử dụng cơ chế đánh chặn dị bộ lõi của EF Core trong [AuditInterceptor.cs](file:///f:/NCKH/Product/Martime_product_v1.1/edge_product/edge-services/Services/Core/AuditInterceptor.cs):

```csharp
public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditInterceptor> _logger;

    // Loại trừ các bảng cảm biến/telemetry tần suất ghi cực cao
    private static readonly HashSet<string> ExcludedEntities = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(NmeaRawData), nameof(PositionData), nameof(AisData),
        nameof(NavigationData), nameof(EngineData), nameof(GeneratorData),
        nameof(TankLevel), nameof(FuelConsumption), nameof(SystemLog),
        nameof(SyncQueue), nameof(UserSession)
    };

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        // 1. Quét tracker lấy các thực thể bị biến đổi dữ liệu
        var entries = eventData.Context.ChangeTracker
            .Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => !ExcludedEntities.Contains(e.Entity.GetType().Name))
            .ToList();

        if (entries.Count == 0) return base.SavingChangesAsync(eventData, result, cancellationToken);

        // 2. Phân giải thông tin người dùng từ HttpContext
        var (userId, username) = ResolveCurrentUser();
        var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        foreach (var entry in entries)
        {
            try
            {
                var entityName = entry.Entity.GetType().Name;
                var entityId = GetPrimaryKeyValue(entry);
                var action = entry.State switch
                {
                    EntityState.Added => "RECORD_CREATED",
                    EntityState.Modified => "RECORD_UPDATED",
                    EntityState.Deleted => "RECORD_DELETED",
                    _ => "UNKNOWN"
                };

                string? oldValues = null;
                string? newValues = null;

                // 3. Phân tích so sánh giá trị cũ và mới
                if (entry.State == EntityState.Modified)
                {
                    var changed = new Dictionary<string, object?>();
                    var original = new Dictionary<string, object?>();

                    foreach (var prop in entry.Properties.Where(p => p.IsModified))
                    {
                        original[prop.Metadata.Name] = prop.OriginalValue;
                        changed[prop.Metadata.Name] = prop.CurrentValue;
                    }

                    if (changed.Count > 0)
                    {
                        oldValues = SafeSerialize(original);
                        newValues = SafeSerialize(changed);
                    }
                    else continue; // Bỏ qua nếu không có cột nào đổi giá trị thực tế
                }
                else if (entry.State == EntityState.Added)
                {
                    var values = entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                    newValues = SafeSerialize(values);
                }
                else if (entry.State == EntityState.Deleted)
                {
                    var values = entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
                    oldValues = SafeSerialize(values);
                }

                // 4. Tạo thực thể Log chèn trực tiếp vào DbContext hiện tại
                var log = new SystemLog
                {
                    Timestamp = DateTime.UtcNow,
                    Category = "DATA",
                    Action = action,
                    Level = entry.State == EntityState.Deleted ? "WARNING" : "INFO",
                    Message = $"{action}: {entityName} [{entityId}]",
                    UserId = userId,
                    Username = username,
                    IpAddress = ipAddress,
                    EntityType = entityName,
                    EntityId = entityId,
                    OldValues = oldValues,
                    NewValues = newValues,
                    Result = "SUCCESS",
                    IsSynced = false
                };

                eventData.Context.Set<SystemLog>().Add(log);
            }
            catch (Exception ex)
            {
                // ISM Code: Lỗi hệ thống audit không bao giờ được phép làm sập/treo ứng dụng chính
                _logger.LogWarning(ex, "Failed to create audit entry");
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
```

##### Chi tiết lệnh và thiết kế logic của Audit Log:
*   **`SavingChangesAsync()`**: Phương thức đánh chặn (interceptor method) chạy ngầm trước khi dữ liệu được commit vật lý xuống DB. Nó chạy dị bộ hoàn toàn không cản trở luồng chính.
*   **`ExcludedEntities` (`HashSet`)**:
    *   *Vì sao sử dụng HashSet*: Hệ thống loại trừ các bảng Sensor/NMEA ghi dữ liệu liên tục 1 giây/lần. Nếu không có bộ lọc này, ổ cứng máy chủ trên tàu sẽ bị đầy tràn trong vòng vài tuần và CPU luôn quá tải do xử lý chuỗi JSON của hàng triệu bản ghi Sensor. Sử dụng kiểu `HashSet` cho phép thực hiện kiểm tra `Contains()` với độ phức tạp thuật toán tối ưu là **$O(1)$**, nhanh hơn rất nhiều so với kiểm tra trên kiểu List có độ phức tạp $O(N)$.
*   **`SafeSerialize`**: Chuyển đổi dữ liệu Dictionary sang chuỗi JSON để lưu trữ dạng cột văn bản.
    *   *Cú pháp giới hạn*: Chuỗi JSON được giới hạn cứng tối đa 4000 ký tự (`json.Length > 4000 ? json[..4000] + "..." : json`). Điều này bảo vệ cơ sở dữ liệu khỏi nguy cơ tràn bộ nhớ đệm hoặc vượt quá kích thước giới hạn của cột dữ liệu văn bản khi thực thể có chứa các cột Blob dài.
*   **`ResolveCurrentUser`**: Tự động giải mã Session/Token trong `IHttpContextAccessor` để lấy ra tài khoản đang thực hiện thao tác sửa đổi. Giải pháp này giúp tách biệt hoàn toàn mã nguồn ghi log khỏi tầng Controller, lập trình viên viết code CRUD nghiệp vụ không cần phải viết thủ công bất cứ dòng code ghi log nào.

---

## IV. MÔ HÌNH FLOW CHI TIẾT: TỪ FRONTEND ĐẾN DATABASE (CHI TIẾT 5 KỊCH BẢN CHÍNH)

Phần này phân tích tường tận quy trình luân chuyển dữ liệu từ các thao tác bấm chuột trên giao diện người dùng (React), qua các hàm API Client, chuyển tiếp đến API Controller của C#, xử lý nghiệp vụ tại Service, ghi nhận vào DbContext, tự động kích hoạt tiến trình ném gói tin đồng bộ và ghi logs, rồi ghi vật lý xuống các bảng trong database PostgreSQL.

---

### Luồng 1: Quy trình Thêm Mới Nhật Ký Trực Ca (Watchkeeping Log)

Khi Sĩ quan hoàn thành ca trực và ghi chép nhật ký ca trực hàng hải trên màn hình buồng lái:

```
┌────────────────────────────────────────────────────────────────────────┐
│ UI [WatchkeepingPage.tsx]                                              │
│ - Thu thập dữ liệu form nhập liệu                                      │
└──────────────────────────────────┬─────────────────────────────────────┘
                                   │
                                   ▼ (Gọi hàm logbookService.createWatchkeepingEntry)
┌────────────────────────────────────────────────────────────────────────┐
│ Service [logbook.service.ts]                                           │
│ - Gọi: apiClient.post('/logbooks/watchkeeping', formData)              │
└──────────────────────────────────┬─────────────────────────────────────┘
                                   │
                                   ▼ (Gửi HTTP POST Request kèm JSON Payload)
┌────────────────────────────────────────────────────────────────────────┐
│ Controller [WatchkeepingController.cs]                                 │
│ - Endpoint: [HttpPost] "api/logbooks/watchkeeping"                     │
│ - Tiếp nhận: CreateEntry([FromBody] CreateWatchkeepingLogDto dto)      │
└──────────────────────────────────┬─────────────────────────────────────┘
                                   │
                                   ▼ (Gọi _service.CreateEntryAsync(dto, username))
┌────────────────────────────────────────────────────────────────────────┐
│ Service [WatchkeepingService.cs]                                       │
│ - Hàm: CreateEntryAsync                                                │
│ - Tự động phân giải VoyageId & LegId dựa trên thời gian ca trực        │
│ - Ánh xạ DTO sang entity WatchkeepingLog                               │
│ - Gọi _context.WatchkeepingLogs.Add(entry)                             │
└──────────────────────────────────┬─────────────────────────────────────┘
                                   │
                                   ▼ (Gọi await _context.SaveChangesAsync())
┌────────────────────────────────────────────────────────────────────────┐
│ DbContext Interception [EdgeDbContext.cs]                              │
│ - 1. NormalizeDateTimesToUtc(): Chuẩn hóa DateTime sang UTC            │
│ - 2. ProcessSyncQueue(): Quét ChangeTracker, phát hiện Added           │
│      -> Tạo bản ghi SyncQueue mới chứa serialized JSON payload         │
└──────────────────────────────────┬─────────────────────────────────────┘
                                   │
                                   ▼ (Lưu vật lý trong một Transaction duy nhất)
┌────────────────────────────────────────────────────────────────────────┐
│ PostgreSQL Database (Cổng 5433)                                        │
│ - Insert bản ghi gốc vào bảng: watchkeeping_logs                       │
│ - Insert bản ghi Outbox vào bảng: sync_queue                           │
└────────────────────────────────────────────────────────────────────────┘
```

#### Bước 1.1: Gửi Request từ Frontend
*   **File giao diện**: [WatchkeepingPage.tsx](file:///f:/NCKH/Product/Martime_product_v1.1/edge_product/frontend-edge/src/pages/logbooks/WatchkeepingPage.tsx)
*   **File gọi API**: [logbook.service.ts](file:///f:/NCKH/Product/Martime_product_v1.1/edge_product/frontend-edge/src/services/logbook.service.ts)
*   **Chi tiết luồng chạy**:
    Khi sĩ quan điền đầy đủ thông tin ca trực và nhấn nút **"Lưu ca trực"**, hàm `handleSave` được kích hoạt. Dữ liệu form sẽ được đóng gói và chuyển sang hàm `createWatchkeepingEntry` của service:
    ```typescript
    // Tại WatchkeepingPage.tsx
    const handleSave = async (formData: WatchkeepingFormValues) => {
      try {
        setLoading(true);
        await logbookService.createWatchkeepingEntry(formData);
        showNotification("Lưu nhật ký ca trực thành công", "success");
      } catch (err) {
        showError(err);
      } finally {
        setLoading(false);
      }
    };

    // Tại logbook.service.ts
    async createWatchkeepingEntry(data: CreateWatchkeepingLogDto) {
      return await apiClient.post<WatchkeepingLogResponseDto>('/logbooks/watchkeeping', data);
    }
    ```
*   **Ví dụ JSON Request Payload gửi đi**:
    ```json
    {
      "watchDate": "2026-06-11T00:00:00Z",
      "watchPeriod": "0000-0400",
      "watchType": "BRIDGE",
      "officerOnWatch": "Nguyen Van A",
      "reliefOfficer": "Tran Van B",
      "lookout": "Le Van C",
      "workHours": 4.0,
      "restHoursLast24h": 12.0,
      "restHoursLast7Days": 84.0,
      "restHoursCompliant": true,
      "restHoursException": false,
      "weatherConditions": "Clear sky",
      "seaState": "Calm",
      "visibility": "Good",
      "courseLogged": 95.0,
      "speedLogged": 14.5,
      "positionLat": 10.234567,
      "positionLon": 107.890123,
      "distanceRun": 58.0,
      "engineStatus": "Normal",
      "radarOperational": true,
      "ecdisOperational": true,
      "aisOperational": true,
      "gyroOperational": true,
      "autopilotEngaged": true,
      "handoverChecklistCompleted": true,
      "bridgeManningLevel": "Routine"
    }
    ```

#### Bước 1.2: Tiếp nhận và kiểm thử tại API Controller
*   **File xử lý**: [WatchkeepingController.cs](file:///f:/NCKH/Product/Martime_product_v1.1/edge_product/edge-services/Controllers/Logbooks/WatchkeepingController.cs)
*   **Chi tiết luồng chạy**:
    Định tuyến API `[Route("api/logbooks/watchkeeping")]` của controller sẽ đón nhận request HTTP POST. ASP.NET Core tự động gọi cơ chế Model Binding để giải mã JSON thành DTO:
    ```csharp
    [HttpPost]
    public async Task<IActionResult> CreateEntry([FromBody] CreateWatchkeepingLogDto dto)
    {
        // Kiểm tra hợp lệ dữ liệu bằng các thuộc tính Validation Attributes trong DTO
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Lấy tên tài khoản hiện tại từ Token xác thực JWT / Cookie Session
        var username = User.Identity?.Name ?? "OOW_System";
        
        // Gọi Service xử lý nghiệp vụ
        var result = await _service.CreateEntryAsync(dto, username);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        // Trả về mã 201 Created kèm theo đường dẫn lấy chi tiết bản ghi
        return CreatedAtAction(
            nameof(GetEntry),
            new { id = result.Id },
            new { id = result.Id, message = "Watchkeeping log entry created successfully" });
    }
    ```

#### Bước 1.3: Thực hiện Nghiệp Vụ tại Service
*   **File xử lý**: [WatchkeepingService.cs](file:///f:/NCKH/Product/Martime_product_v1.1/edge_product/edge-services/Services/Logbooks/WatchkeepingService.cs)
*   **Chi tiết luồng chạy**:
    Hàm `CreateEntryAsync` sẽ thực hiện ánh xạ từ DTO sang Model thực thể DB (`WatchkeepingLog`), tự động truy vấn tìm chuyến đi (`VoyageId`) đang kích hoạt tương ứng với thời điểm trực ca thông qua `_voyageContext.ResolveActiveVoyageAsync` để liên kết khóa ngoại. Cuối cùng, thực thể được đưa vào DB Set:
    ```csharp
    public async Task<(bool Success, Guid? Id, string? Error)> CreateEntryAsync(CreateWatchkeepingLogDto dto, string? username = null)
    {
        try
        {
            var entry = new WatchkeepingLog
            {
                Id = Guid.NewGuid(),
                WatchDate = dto.WatchDate,
                WatchPeriod = dto.WatchPeriod,
                WatchType = dto.WatchType,
                OfficerOnWatch = dto.OfficerOnWatch,
                ReliefOfficer = dto.ReliefOfficer,
                Lookout = dto.Lookout,
                WorkHours = dto.WorkHours,
                RestHoursLast24h = dto.RestHoursLast24h,
                RestHoursLast7Days = dto.RestHoursLast7Days,
                RestHoursCompliant = dto.RestHoursCompliant,
                RestHoursException = dto.RestHoursException,
                WeatherConditions = dto.WeatherConditions,
                SeaState = dto.SeaState,
                Visibility = dto.Visibility,
                CourseLogged = dto.CourseLogged,
                SpeedLogged = dto.SpeedLogged,
                PositionLat = dto.PositionLat,
                PositionLon = dto.PositionLon,
                DistanceRun = dto.DistanceRun,
                EngineStatus = dto.EngineStatus,
                RadarOperational = dto.RadarOperational,
                ECDISOperational = dto.ECDISOperational,
                AISOperational = dto.AISOperational,
                GyroOperational = dto.GyroOperational,
                AutopilotEngaged = dto.AutopilotEngaged,
                HandoverChecklistCompleted = dto.HandoverChecklistCompleted,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OriginNode = Environment.MachineName, // SHIP_EDGE_01
                IsSynced = false
            };

            // Phân giải tự động VoyageId và VoyagePlanLegId
            var (voyageId, legId) = await _voyageContext.ResolveActiveVoyageAsync(entry.WatchDate);
            entry.VoyageId = voyageId;
            entry.VoyagePlanLegId = legId;

            // Đưa thực thể vào Change Tracker với trạng thái EntityState.Added
            _context.WatchkeepingLogs.Add(entry);
            
            // Thực hiện ghi nhận và lưu thay đổi xuống Database
            await _context.SaveChangesAsync();

            return (true, entry.Id, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating watchkeeping log entry");
            return (false, null, "An error occurred while creating the entry.");
        }
    }
    ```

#### Bước 1.4: Đánh Chặn & Lưu Trữ Vật Lý Xuống Database
*   **File xử lý**: [EdgeDbContext.cs](file:///f:/NCKH/Product/Martime_product_v1.1/edge_product/edge-services/Data/EdgeDbContext.cs)
*   **Chi tiết luồng chạy**:
    Khi gọi `SaveChangesAsync()`, DbContext thực hiện chuỗi hành vi được ghi đè:
    1.  **`NormalizeDateTimesToUtc()`**: Quét qua tất cả thuộc tính `DateTime` trong thực thể chuẩn bị ghi và gắn cờ `DateTimeKind.Utc` cho chúng.
    2.  **`ProcessSyncQueue()`**:
        *   Change Tracker phát hiện ra một thực thể mới (`WatchkeepingLog`) có thuộc tính `IsSynced` và trạng thái `EntityState.Added`.
        *   Tạo ra một bản ghi đồng bộ mới trong bảng `SyncQueue` với `TableName = "watchkeeping_log"`, `ActionType = SyncActionType.CREATE`.
        *   Tuần tự hóa toàn bộ thông tin của `WatchkeepingLog` thành một chuỗi JSON dẹt để lưu vào trường `Payload`.
    3.  **Database Transaction**: Entity Framework Core mở một Database Connection tới PostgreSQL Biên (Edge DB), tạo một transaction chứa 2 lệnh chèn SQL và thực thi nguyên khối (Atomic Transaction):
        ```sql
        -- Lệnh 1: Thêm mới bản ghi nhật ký gốc
        INSERT INTO watchkeeping_logs (id, watch_date, watch_period, watch_type, officer_on_watch, ..., is_synced, created_at)
        VALUES ('3fa85f64-5717-4562-b3fc-2c963f66afa6', '2026-06-11 00:00:00+00', '0000-0400', 'BRIDGE', 'Nguyen Van A', ..., false, '2026-06-11 13:16:43+00');

        -- Lệnh 2: Thêm mới tác vụ Outbox vào hàng đợi đồng bộ
        INSERT INTO sync_queue (id, table_name, record_key, action_type, payload, priority, retry_count, max_retries, created_at)
        VALUES ('a1b2c3d4-e5f6-7a8b-9c0d-e1f2a3b4c5d6', 'watchkeeping_log', '3fa85f64-5717-4562-b3fc-2c963f66afa6', 'CREATE', '{"Id":"3fa85f64-5717-4562-b3fc-2c963f66afa6","WatchDate":"2026-06-11T00:00:00Z",...}', 2, 0, 5, '2026-06-11 13:16:43+00');
        ```

---

### Luồng 2: Cập Nhật Trạng Thái Hành Trình (Voyage Status Update)

Khi Thuyền trưởng tiến hành điều khiển tàu rời cảng, đổi trạng thái chuyến đi từ `PLANNING` sang `UNDERWAY` trên bảng điều khiển:

```
┌────────────────────────────────────────────────────────────────────────┐
│ UI [CockpitTab.tsx]                                                    │
│ - Người dùng click "Start Voyage" trên giao diện                       │
└──────────────────────────────────┬─────────────────────────────────────┘
                                   │
                                   ▼ (Gọi hàm voyageService.updateVoyage)
┌────────────────────────────────────────────────────────────────────────┐
│ Service [voyage.service.ts]                                            │
│ - Gọi: apiClient.put(`/voyages/${voyageId}`, { voyageStatus: 'UNDERWAY' })
└──────────────────────────────────┬─────────────────────────────────────┘
                                   │
                                   ▼ (Gửi HTTP PUT Request kèm JSON Payload)
┌────────────────────────────────────────────────────────────────────────┐
│ Controller [VoyageController.cs]                                       │
│ - Endpoint: [HttpPut] "api/voyages/{id}"                               │
│ - Tiếp nhận: UpdateEntry(id, [FromBody] UpdateVoyageDto dto)           │
└──────────────────────────────────┬─────────────────────────────────────┘
                                   │
                                   ▼ (Gọi _service.UpdateVoyageAsync(id, dto))
┌────────────────────────────────────────────────────────────────────────┐
│ Service [VoyageManagementService.cs]                                   │
│ - Tìm bản ghi VoyageRecord hiện tại bằng FindAsync()                   │
│ - Kiểm tra chuyển trạng thái hợp lệ (PLANNING -> UNDERWAY)             │
│ - Áp dụng mốc thời gian xuất bến thực tế (ActualDepartureTime)         │
│ - Đánh dấu: voyage.IsSynced = false                                    │
└──────────────────────────────────┬─────────────────────────────────────┘
                                   │
                                   ▼ (Gọi await _context.SaveChangesAsync())
┌────────────────────────────────────────────────────────────────────────┐
│ DbContext Interception [EdgeDbContext.cs]                              │
│ - ProcessSyncQueue() phát hiện thực thể VoyageRecord bị Modified.      │
│ - Lọc các cột nghiệp vụ bị sửa đổi (IsModified == true)                │
│ - Sinh ra Delta JSON Payload chứa DUY NHẤT các trường thay đổi        │
│ - Thêm bản ghi SyncQueue với ActionType = SyncActionType.UPDATE        │
└──────────────────────────────────┬─────────────────────────────────────┘
                                   │
                                   ▼ (Lưu transaction xuống DB PostgreSQL)
┌────────────────────────────────────────────────────────────────────────┐
│ PostgreSQL Database (Cổng 5433)                                        │
│ - Execute UPDATE voyage_records SET voyage_status = 'UNDERWAY',...     │
│ - Execute INSERT INTO sync_queue (Outbox delta update)                 │
└────────────────────────────────────────────────────────────────────────┘
```

#### Bước 2.1: Gọi API từ UI Frontend
*   **File giao diện**: [CockpitTab.tsx](file:///f:/NCKH/Product/Martime_product_v1.1/edge_product/frontend-edge/src/pages/Voyage/CockpitTab.tsx)
*   **File gọi API**: [voyage.service.ts](file:///f:/NCKH/Product/Martime_product_v1.1/edge_product/frontend-edge/src/services/voyage.service.ts)
*   **Chi tiết luồng chạy**:
    Khi nhấn "Start Voyage", UI sẽ gọi hàm của API client gửi yêu cầu PUT cập nhật trạng thái:
    ```typescript
    // Tại CockpitTab.tsx
    const handleStartVoyage = async () => {
      try {
        await voyageService.updateVoyage(voyageId, { voyageStatus: 'UNDERWAY' });
        showNotification("Đã bắt đầu hành trình tàu chạy!", "success");
      } catch (err) {
        showNotification("Lỗi cập nhật hành trình", "error");
      }
    };

    // Tại voyage.service.ts
    async updateVoyage(id: string, data: UpdateVoyageDto) {
      return await apiClient.put<VoyageResponseDto>(`/voyages/${id}`, data);
    }
    ```

#### Bước 2.2: Xử lý tại Backend Service
*   **File xử lý**: [VoyageManagementService.cs](file:///f:/NCKH/Product/Martime_product_v1.1/edge_product/edge-services/Services/Voyage/VoyageManagementService.cs)
*   **Chi tiết luồng chạy**:
    Hàm `UpdateVoyageAsync` chịu trách nhiệm chạy các business rules xác thực trạng thái. Nếu trạng thái chuyến đi đã ở mức đóng băng (`COMPLETED` hoặc `CANCELLED`), mọi chỉnh sửa đều bị ném biệt lệ từ chối.
    ```csharp
    public async Task<VoyageRecord?> UpdateVoyageAsync(Guid voyageId, UpdateVoyageDto dto)
    {
        var voyage = await _context.VoyageRecords.FindAsync(voyageId);
        if (voyage == null) return null;

        var currentStatus = voyage.VoyageStatus;
        string? nextStatus = null;

        // 1. Kiểm tra điều kiện chuyển đổi trạng thái hợp lệ
        if (dto.VoyageStatus != null && dto.VoyageStatus != currentStatus)
        {
            nextStatus = NormalizeVoyageStatus(dto.VoyageStatus);
            // Kiểm tra theo ma trận trạng thái: PLANNING -> UNDERWAY -> ARRIVED -> COMPLETED
            if (!VoyageStatus.IsValidTransition(currentStatus, nextStatus))
                throw new InvalidOperationException($"Invalid status transition: {currentStatus} -> {nextStatus}");
        }

        // 2. Cập nhật các trường thông tin thay đổi
        if (nextStatus != null)
        {
            voyage.VoyageStatus = nextStatus;
            ApplyLifecycleMilestones(voyage, nextStatus); // Ghi nhận ActualDepartureTime = DateTime.UtcNow
            AddStatusHistory(voyage, currentStatus, nextStatus, "Status changed via cockpit");
        }

        voyage.UpdatedAt = DateTime.UtcNow;
        voyage.IsSynced = false; // Đặt về false để hệ thống hiểu cần đồng bộ

        await _context.SaveChangesAsync();
        return voyage;
    }
    ```

#### Bước 2.3: Tạo thông tin đồng bộ chênh lệch (Smart Delta Payload)
*   **File xử lý**: [EdgeDbContext.cs](file:///f:/NCKH/Product/Martime_product_v1.1/edge_product/edge-services/Data/EdgeDbContext.cs) -> Hàm `ProcessSyncQueue()`
*   **Cơ chế hoạt động**:
    Đối với hành động `UPDATE`, để tối ưu hóa băng thông truyền tin qua vệ tinh, hệ thống không gửi toàn bộ cả bản ghi (có thể lên tới hàng chục KB nếu chứa mớn nước, danh sách kế hoạch dài). Thay vào đó, hàm `ProcessSyncQueue` thực hiện so sánh chênh lệch:
    *   Nó duyệt qua tất cả thuộc tính của đối tượng `VoyageRecord` trong ChangeTracker.
    *   Chỉ chọn các trường có cờ `IsModified == true`.
    *   Loại trừ các trường siêu dữ liệu nội bộ tàu (`UpdatedAt`, `CreatedAt`, `IsSynced`, `OriginNode`).
    *   Tự động bổ sung các trường nhận diện thiết yếu (ví dụ: `ImoNumber` của tàu) để Shore có thể tra cứu nhanh.
    
    *Ví dụ Delta Payload được tạo ra trong cột `Payload` của `sync_queue`:*
    ```json
    {
      "VoyageStatus": "UNDERWAY",
      "ActualDepartureTime": "2026-06-11T13:16:43Z"
    }
    ```
    *Lệnh SQL thực thi trên PostgreSQL Biên:*
    ```sql
    UPDATE voyage_records 
    SET voyage_status = 'UNDERWAY', actual_departure_time = '2026-06-11 13:16:43+00', updated_at = '2026-06-11 13:16:43+00', is_synced = false 
    WHERE id = '5e9b8f2a-8c9d-4e5f-a0b1-c2d3e4f5a6b7';

    INSERT INTO sync_queue (id, table_name, record_key, action_type, payload, priority, retry_count, max_retries, created_at)
    VALUES ('f8e7d6c5-b4a3-9281-706f-5e4d3c2b1a0f', 'voyage_record', '5e9b8f2a-8c9d-4e5f-a0b1-c2d3e4f5a6b7', 'UPDATE', '{"VoyageStatus":"UNDERWAY","ActualDepartureTime":"2026-06-11T13:16:43Z"}', 2, 0, 5, '2026-06-11 13:16:43+00');
    ```

---

### Luồng 3: Duyệt và Truyền Noon Report Về Đất Liền (Direct HTTP + Fallback Queue)

Quy trình Thuyền trưởng duyệt và đẩy trực tiếp một bản báo cáo Noon Report về văn phòng Shore, sử dụng cơ chế truyền trực tiếp HTTP và tự động hạ cấp về Sync Queue Outbox nếu đường truyền bị ngắt kết nối:

```
┌────────────────────────────────────────────────────────────────────────┐
│ UI [ReportDetailPage.tsx]                                              │
│ - Thuyền trưởng xem chi tiết báo cáo APPROVED                          │
│ - Nhấn nút "Duyệt & Gửi về bờ"                                         │
└──────────────────────────────────┬─────────────────────────────────────┘
                                   │
                                   ▼ (Gọi hàm reportingService.transmitReport)
┌────────────────────────────────────────────────────────────────────────┐
│ Service [reporting.service.ts]                                         │
│ - Gọi: apiClient.post(`/reports/${reportId}/transmit`, params)         │
└──────────────────────────────────┬─────────────────────────────────────┘
                                   │
                                   ▼ (Gửi HTTP POST Request)
┌────────────────────────────────────────────────────────────────────────┐
│ Controller [ReportingController.cs]                                    │
│ - Endpoint: [HttpPost] "api/reports/{id}/transmit"                     │
└──────────────────────────────────┬─────────────────────────────────────┘
                                   │
                                   ▼ (Gọi _reportingService.TransmitReportAsync())
┌────────────────────────────────────────────────────────────────────────┐
│ Service [ReportingService.cs]                                          │
│ - Đổi trạng thái báo cáo gốc sang TRANSMITTED                          │
│ - Gọi BuildReportSyncItemsAsync() đóng gói dữ liệu cha/con thành JSON  │
│ - Thực thi gọi HTTP POST trực tiếp tới Shore API qua mạng vệ tinh      │
└──────────────────────────────────┬─────────────────────────────────────┘
                                   │
          ┌────────────────────────┴────────────────────────┐
          ▼ (TH 1: ONLINE - Shore phản hồi Ok)              ▼ (TH 2: OFFLINE - Gửi lỗi / Timeout)
┌──────────────────────────────────────┐          ┌──────────────────────────────────────┐
│ - Tạo ReportTransmissionLog: SUCCESS │          │ - Tạo ReportTransmissionLog: QUEUED  │
│ - Không chèn sync_queue (đã truyền)  │          │ - Đóng gói dữ liệu chèn vào bảng     │
│                                      │          │   sync_queue để gửi ngầm sau         │
└──────────────────┬───────────────────┘          └──────────────────┬───────────────────┘
                   │                                                 │
                   └───────────────────────┬─────────────────────────┘
                                           │
                                           ▼ (Lưu thay đổi)
┌────────────────────────────────────────────────────────────────────────┐
│ PostgreSQL Database (Cổng 5433)                                        │
│ - Lưu trạng thái báo cáo đã cập nhật và log truyền tin                 │
└────────────────────────────────────────────────────────────────────────┘
```

#### Bước 3.1: Thao tác phê duyệt trên UI
*   **File giao diện**: [ReportDetailPage.tsx](file:///f:/NCKH/Product/Martime_product_v1.1/edge_product/frontend-edge/src/pages/Reporting/ReportDetailPage.tsx)
*   **Chi tiết luồng chạy**:
    Báo cáo Noon Report sau khi được tạo ở trạng thái nháp (`DRAFT`), được ký duyệt sẽ lên trạng thái `APPROVED`. Thuyền trưởng nhấn nút gửi để truyền dữ liệu về đất liền:
    ```typescript
    const handleTransmit = async (method: string) => {
      try {
        setTransmitting(true);
        // Gửi qua HTTP vệ tinh hoặc các phương thức email/file dự phòng khác
        const res = await reportingService.transmitReport(reportId, {
          transmissionMethod: method,
          recipientEmails: ["fleet-noon@company.com"]
        });
        showToast("Đã xử lý truyền tin báo cáo");
      } catch (err) {
        showToast("Lỗi truyền tin");
      } finally {
        setTransmitting(false);
      }
    };
    ```

#### Bước 3.2: Logic Phân Tách Kết Nối và Đồng Bộ Dự Phòng tại Backend Service
*   **File xử lý**: [ReportingService.cs](file:///f:/NCKH/Product/Martime_product_v1.1/edge_product/edge-services/Services/Reporting/ReportingService.cs)
*   **Chi tiết luồng chạy**:
    Vì báo cáo là tài liệu pháp lý mang tính thời gian nghiêm ngặt, hệ thống không áp dụng cơ chế đồng bộ ngầm tự động (auto-sync) ngay từ đầu để tránh độ trễ. Nó thực hiện kết nối trực tiếp đến Shore. Nếu lỗi (ví dụ tàu đi vào vùng mất sóng vệ tinh), hệ thống sẽ tự động chuyển đổi cấu trúc sang Outbox và chèn vào bảng hàng đợi đồng bộ làm phương án dự phòng.
    ```csharp
    public async Task<(bool Success, string? Error)> TransmitReportAsync(Guid reportId, TransmitReportDto dto, string? username = null)
    {
        var report = await _context.MaritimeReports.FindAsync(reportId);
        if (report == null || report.Status != "APPROVED")
        {
            return (false, "Report must be APPROVED before transmission.");
        }

        // 1. Chuyển trạng thái báo cáo
        report.Status = "TRANSMITTED";
        report.IsTransmitted = true;
        report.TransmittedAt = DateTime.UtcNow;

        // 2. Đóng gói dữ liệu báo cáo cha (MaritimeReport) và báo cáo con (NoonReport) thành DTO
        var syncItems = await BuildReportSyncItemsAsync(report);

        // 3. Tiến hành gửi trực tiếp bằng HTTP client qua vệ tinh tới Shore
        var (httpSuccess, httpError) = await SendReportToShoreAsync(syncItems);

        // 4. Ghi nhận nhật ký truyền tin
        var log = new ReportTransmissionLog
        {
            Id = Guid.NewGuid(),
            MaritimeReportId = reportId,
            TransmissionDateTime = DateTime.UtcNow,
            TransmissionMethod = dto.TransmissionMethod,
            Status = httpSuccess ? "SUCCESS" : "FAILED",
            ErrorMessage = httpError
        };
        _context.ReportTransmissionLogs.Add(log);

        // 5. Cơ chế Fallback dự phòng khi mất kết nối mạng
        if (!httpSuccess)
        {
            _logger.LogWarning("Direct report transmission failed. Fallback: Queueing into Sync Queue. Error: {Error}", httpError);
            
            foreach (var item in syncItems)
            {
                _context.SyncQueue.Add(new SyncQueue
                {
                    Id = Guid.NewGuid(),
                    TableName = item.TableName,
                    RecordKey = item.RecordKey,
                    ActionType = SyncActionType.CREATE,
                    Payload = item.Payload,
                    Priority = SyncPriority.Operational,
                    RetryCount = 0,
                    MaxRetries = 5,
                    CreatedAt = DateTime.UtcNow
                });
            }
            log.Status = "QUEUED"; // Đánh dấu là đang xếp hàng đợi đồng bộ ngầm
        }

        await _context.SaveChangesAsync();
        return (true, null);
    }
    ```

---

### Luồng 4: Quy Trình Gửi Dữ Liệu Đồng Bộ Ngầm (Push Sync Queue)

Tiến trình ngầm liên tục chạy quét hàng đợi và đẩy dữ liệu chênh lệch lên đất liền:

#### Bước 4.1: Background Worker kích hoạt
*   **File xử lý**: [SyncBackgroundWorker.cs](file:///f:/NCKH/Product/Martime_product_v1.1/edge_product/edge-services/Services/Core/SyncBackgroundWorker.cs)
*   **Chi tiết luồng chạy**:
    Lớp này kế thừa `BackgroundService` của .NET Core, chạy như một Daemon Service suốt vòng đời ứng dụng. Mỗi 30 giây (hoặc cấu hình tùy chỉnh), nó sẽ thức dậy và kích hoạt luồng đồng bộ:
    ```csharp
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Gọi tiến trình đẩy dữ liệu lên Shore (Push) và kéo dữ liệu về (Pull)
                await _syncService.ExecuteSyncAsync(stoppingToken);
                await _syncService.PullFromShoreAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in sync loop execution");
            }
            
            // Chờ 30 giây cho chu kỳ tiếp theo
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
    ```

#### Bước 4.2: Phân tích Logic Đẩy Dữ Liệu Lên Shore (Push Sync)
*   **File xử lý**: [SyncService.cs](file:///f:/NCKH/Product/Martime_product_v1.1/edge_product/edge-services/Services/Core/SyncService.cs)
*   **Cơ chế hoạt động**:
    Hàm `ExecuteSyncAsync` thực hiện các bước kiểm tra mạng, giới hạn hạn ngạch, đóng gói batch và gửi đi:

    ```
    +-------------------------------------------------------------+
    |                     ExecuteSyncAsync()                      |
    +------------------------------+------------------------------+
                                   |
                                   ▼
    +-------------------------------------------------------------+
    | Kiểm tra Semaphore lock (_pushGate) chống chạy song song    |
    +------------------------------+------------------------------+
                                   |
                                   ▼
    +-------------------------------------------------------------+
    | Đọc loại mạng kết nối hiện tại (WiFi / VSAT / Iridium)     |
    +------------------------------+------------------------------+
                                   |
                                   ▼
    +-------------------------------------------------------------+
    | Lọc các bản ghi sync_queue theo:                            |
    | - SyncedAt IS NULL                                          |
    | - Mức độ ưu tiên được phép của loại mạng hiện tại           |
    | - NextRetryAt <= Now                                        |
    | - RetryCount < MaxRetries                                   |
    | - Giới hạn kích thước Batch size phù hợp với mạng           |
    +------------------------------+------------------------------+
                                   |
                                   ▼
    +-------------------------------------------------------------+
    | Kiểm tra Token Bucket rate-limiting                         |
    +------------------------------+------------------------------+
                                   |
                                   ▼
    +-------------------------------------------------------------+
    | Ký số gói tin (HMAC-SHA256) & Gửi HTTP POST tới Shore API   |
    +------------------------------+------------------------------+
                                   |
                                   ▼
    +-------------------------------------------------------------+
    | Đánh dấu SyncedAt = UtcNow ở hàng đợi                       |
    | Cập nhật is_synced = true ở bảng gốc (bằng SQL thuần)       |
    +-------------------------------------------------------------+
    ```

    1.  **Ma Trận Loại Mạng & Độ Ưu Tiên (Network Priority Grid)**:
        Hệ thống tự động phát hiện loại kết nối thông qua Gateway Router API và áp dụng bộ lọc ưu tiên nhằm tiết kiệm băng thông vệ tinh đắt đỏ:
        
        | Loại Kết Nối (Connection Type) | Mức Ưu Tiên Dữ Liệu Được Cho Phép (Allowed Priorities) | Kích Thước Lô Gửi (Batch Size) |
        | :--- | :--- | :--- |
        | **WiFi (Tại cảng / Staging)** | `Critical`, `Operational`, `Telemetry` (Toàn bộ) | 200 bản ghi / batch |
        | **VSAT (Vệ tinh băng rộng)** | `Critical`, `Operational` | 50 bản ghi / batch |
        | **Iridium (Vệ tinh băng hẹp)** | `Critical` (Chỉ báo động, SOS, vị trí khẩn nguy) | 5 bản ghi / batch |

    2.  **Cơ chế Token Bucket Rate Limiting**:
        Để tránh hiện tượng dồn ứ request (Herding Effect) khi hàng chục tàu cùng lúc kết nối lại mạng làm nghẽn Shore API, mỗi tàu được cấp hạn ngạch truyền tin:
        *   Tối đa chứa 10 token. Tái tạo 1 token mỗi 5 giây.
        *   Mỗi lượt truyền 1 batch tiêu thụ 1 token.
        *   Nếu hết token, quá trình truyền của batch hiện tại sẽ bị trì hoãn và các bản ghi trong batch được cập nhật `NextRetryAt = DateTime.UtcNow.AddSeconds(15)`.

    3.  **Bảo mật HMAC-SHA256 ký số yêu cầu**:
        Mỗi node tàu biên được cấu hình một cặp `NodeId` và `PrivateKey` trong file cấu hình bảo mật. Request trước khi gửi đi được ký xác thực bằng mã băm HMAC-SHA256 trên chuỗi JSON payload:
        ```csharp
        // Mã hóa ký số gói tin gửi đi
        var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(privateKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(jsonPayload));
        var signature = Convert.ToBase64String(hash);
        request.Headers.Add("X-Sync-Signature", signature);
        request.Headers.Add("X-Sync-NodeId", nodeId);
        ```

#### Bước 4.3: Ingestion & Đánh giá Xung Đột tại Shore Backend
*   **File tiếp nhận**: `SyncController.cs` (Shore) -> `SyncInboxService.cs` (Shore)
*   **Các bước xử lý**:
    1.  **Idempotency Check**: Shore kiểm tra trường `SyncVersion` (chính là ID của bản ghi hàng đợi Edge gửi lên) đối chiếu với bảng `sync_idempotency_records`. Nếu đã tồn tại, Shore bỏ qua xử lý và trả về HTTP 200 Ok ngay lập tức để tránh ghi trùng lặp dữ liệu do cơ chế retry mạng.
    2.  **Conflict Resolution (Giải quyết xung đột ghi đè)**:
        Hệ thống áp dụng chính sách phân mảnh sở hữu dữ liệu (Domain-based ownership rules) tại [ConflictResolverService.cs](file:///f:/NCKH/Product/Martime_product_v1.1/edge_product/edge-services/Services/Core/SyncConflictHandler.cs) hoặc Shore tương đương:
        *   **Quy tắc "Shore Authority"**: Các bảng danh mục gốc, thông tin nhân sự, chứng chỉ thuyền viên (`ranks`, `certificates`, `crew_members`) do Shore làm chủ. Nếu có xung đột dữ liệu giữa Shore và Edge, dữ liệu Shore luôn chiến thắng. Edge sẽ bị cưỡng chế ghi đè dữ liệu Shore kéo về.
        *   **Quy tắc "Edge Authority"**: Các bảng nhật ký vận hành trực tiếp, báo cáo kỹ thuật (`deck_log_books`, `watchkeeping_logs`, `noon_reports`, `maintenance_tasks`) do Edge làm chủ. Shore sẽ ghi đè dữ liệu nhận từ tàu lên database của mình mà không cần kiểm tra thời gian.
        *   **Quy tắc "Last-Write-Wins"**: Dành cho các thực thể dùng chung (ví dụ: cập nhật kho vật tư `inventory_stocks`). So sánh trường thời gian cập nhật `UpdatedAt` giữa bản ghi trên Shore và bản ghi Edge gửi lên. Bản ghi nào có mốc thời gian lớn hơn (mới hơn) sẽ được ghi nhận.

#### Bước 4.4: Phản hồi và Cập nhật Trạng thái phía Edge
Sau khi Shore xử lý xong bản ghi và trả về HTTP 200 Ok:
*   Edge cập nhật cột `synced_at = DateTime.UtcNow` cho bản ghi trong bảng `sync_queue`.
*   Để cập nhật trạng thái `is_synced = true` trên bảng dữ liệu gốc nhanh nhất mà không làm phình Change Tracker trong bộ nhớ, Edge thực thi lệnh SQL trực tiếp:
    ```csharp
    await context.Database.ExecuteSqlRawAsync(
        $"UPDATE watchkeeping_logs SET is_synced = true WHERE id = {0}", 
        Guid.Parse(recordKey));
    ```

---

### Luồng 5: Ghi Nhật Ký Hoạt Động (Audit Log) Tự Động

Mọi tác vụ tạo mới, sửa đổi hoặc xóa dữ liệu nghiệp vụ nhạy cảm đều được tự động lưu vết mà không cần can thiệp code tay tại Controller:

```
[Bất kỳ Nghiệp vụ nào gọi SaveChangesAsync()]
      │
      ▼ (Kích hoạt bộ đánh chặn EF Core trước khi lưu)
[AuditInterceptor.cs: SavingChangesAsync()]
      │
      ├─► (ChangeTracker quét danh sách thực thể có trạng thái thay đổi)
      ├─► (Kiểm tra loại trừ: Nếu thực thể thuộc bảng Sensor/Telemetry -> Bỏ qua)
      ├─► (Truy vấn HttpContextAccessor lấy UserId, Username, IP thực hiện)
      │
      ▼ (Tiến hành so sánh tìm trường dữ liệu bị biến đổi)
[AuditInterceptor.cs: Phân tích EntityState]
      │
      ├───► (Added): Chụp toàn bộ thuộc tính -> gán vào NewValues
      │
      ├───► (Deleted): Chụp toàn bộ thuộc tính ban đầu -> gán vào OldValues
      │
      └───► (Modified):
            - Chạy vòng lặp duyệt qua các thuộc tính của thực thể
            - Lọc các thuộc tính bị biến đổi (IsModified == true)
            - Bỏ qua các trường metadata (UpdatedAt, IsSynced...)
            - Ghi giá trị cũ vào OldValues, giá trị mới vào NewValues
            - Nếu không có trường nghiệp vụ nào thay đổi thực tế -> Bỏ qua
      │
      ▼ (Khởi tạo thực thể SystemLog chèn vào DB)
[SystemLog entity -> Set<SystemLog>().Add(log)]
      │
      ▼ (Lưu vật lý cùng transaction với bản ghi gốc)
[Postgres: Ghi đồng thời dữ liệu nghiệp vụ chính & dữ liệu audit logs]
```

#### Bước 5.1: Đánh chặn thay đổi tại EF Core level
*   **File xử lý**: [AuditInterceptor.cs](file:///f:/NCKH/Product/Martime_product_v1.1/edge_product/edge-services/Services/Core/AuditInterceptor.cs)
*   **Chi tiết luồng chạy**:
    Khi bất kỳ Service nào thực hiện chỉnh sửa dữ liệu nghiệp vụ chính và gọi `SaveChangesAsync()`, EF Core tự động chuyển hướng dòng điều khiển qua phương thức `SavingChangesAsync` của Interceptor trước khi gửi lệnh xuống Postgres:
    ```csharp
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        // Lọc ra các thực thể bị biến đổi dữ liệu, bỏ qua các bảng Sensor/Logs loại trừ
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => !ExcludedEntities.Contains(e.Entity.GetType().Name))
            .ToList();

        if (entries.Count == 0) return base.SavingChangesAsync(eventData, result, cancellationToken);

        var (userId, username) = ResolveCurrentUser(); // Đọc HttpContext lấy tài khoản
        var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        foreach (var entry in entries)
        {
            var entityName = entry.Entity.GetType().Name;
            var entityId = GetPrimaryKeyValue(entry);
            string? oldJson = null;
            string? newJson = null;

            if (entry.State == EntityState.Modified)
            {
                var originalValues = new Dictionary<string, object?>();
                var currentValues = new Dictionary<string, object?>();

                foreach (var prop in entry.Properties)
                {
                    if (prop.IsModified)
                    {
                        if (prop.Metadata.Name is "UpdatedAt" or "IsSynced" or "SyncVersion") continue;
                        originalValues[prop.Metadata.Name] = prop.OriginalValue;
                        currentValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }

                if (originalValues.Count > 0)
                {
                    oldJson = SafeSerialize(originalValues);
                    newJson = SafeSerialize(currentValues);
                }
                else continue;
            }
            else if (entry.State == EntityState.Added)
            {
                var values = entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                newJson = SafeSerialize(values);
            }
            else if (entry.State == EntityState.Deleted)
            {
                var values = entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
                oldJson = SafeSerialize(values);
            }

            var log = new SystemLog
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Category = "AUDIT_DATA",
                Action = entry.State.ToString().ToUpper(), // ADDED, MODIFIED, DELETED
                Level = entry.State == EntityState.Deleted ? "WARNING" : "INFO",
                Message = $"User {username} performed {entry.State} on {entityName} [{entityId}]",
                UserId = userId,
                Username = username,
                IpAddress = ipAddress,
                EntityType = entityName,
                EntityId = entityId,
                OldValues = oldJson,
                NewValues = newJson,
                Result = "SUCCESS"
            };
            context.Set<SystemLog>().Add(log);
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
    ```
*   **`SavingChangesAsync`**: Bắt vết thay đổi ở mức cơ sở dữ liệu. Nhờ cơ chế này, toàn bộ lịch sử thay đổi (Ví dụ: Thủy thủ sửa tên thuốc trong tủ y tế, Sĩ quan sửa mớn nước hầm hàng) đều được lưu lại giá trị cũ và mới chi tiết tới từng trường thông tin, đáp ứng tuyệt đối yêu cầu ISM Code hàng hải.

#### Bước 5.2: Ví dụ Cấu trúc Bản Ghi Audit Log trong Database
*   **Tên bảng lưu trữ**: `system_logs` (Snake-case map)
*   **Ví dụ dữ liệu một dòng log sửa đổi thông số ca trực:**
    ```
    - id: "e9d8c7b6-a5b4-c3d2-e1f0-9876543210ab"
    - timestamp: "2026-06-11 13:16:43+00"
    - category: "AUDIT_DATA"
    - action: "MODIFIED"
    - level: "INFO"
    - message: "User Captain_Nguyen performed Modified on WatchkeepingLog [3fa85f64-5717-4562-b3fc-2c963f66afa6]"
    - username: "Captain_Nguyen"
    - ip_address: "192.168.1.105"
    - entity_type: "WatchkeepingLog"
    - entity_id: "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    - old_values: "{\"ReliefOfficer\":\"Tran Van B\",\"SeaState\":\"Calm\"}"
    - new_values: "{\"ReliefOfficer\":\"Le Quoc D\",\"SeaState\":\"Rough Sea\"}"
    - result: "SUCCESS"
    - is_synced: false
    ```

---

## V. CHI TIẾT GIAO THỨC ĐỒNG BỘ HAI CHIỀU (PULL SHORE -> EDGE)

Bên cạnh việc Edge tự động đẩy dữ liệu (Push) lên bờ, tàu cũng phải kéo thông tin (Pull) từ Shore về để nhận các chỉ thị, danh sách thuyền viên mới từ phòng nhân sự, hoặc master data.

```
                    EDGE (Tàu)                              SHORE (Bờ)
                    ═══════════                              ═══════════

   [Tàu gọi API kéo tin]
   GET /api/sync/pull?nodeId=SHIP_01
            │
            ▼
   [Đọc danh sách outbox gửi tàu] ──────────────────────► [Truy vấn bảng sync_outbox]
                                                                  │
                                                                  ▼
   [Nhận danh sách gói tin đồng bộ] ◄──────────────────── [Lọc DeliveredAt IS NULL]
            │
            ▼
   [Duyệt qua từng gói tin đồng bộ]
            │
            ├───► (Xử lý xung đột ghi đè: SyncConflictHandler.cs)
            │           │
            │           ├─► (HR / Master Data): Shore thắng -> Chấp nhận ghi đè dữ liệu cục bộ
            │           │
            │           └─► (Service Record / Ops): Edge thắng -> Từ chối ghi đè dữ liệu dưới tàu
            │
            ▼
   [Gửi tín hiệu xác nhận đã nhận tin]
   POST /api/sync/acknowledge
            │
            └───────────────────────────────────────────► [Đánh dấu DeliveredAt = UtcNow]
                                                          (Ngăn Shore gửi lại tin này ở lần sau)
```

### 1. Luồng xử lý kéo tin (Pulling Flow)
Tiến trình ngầm `SyncBackgroundWorker.cs` gọi hàm `PullFromShoreAsync` định kỳ 30 giây:
1.  **Gửi Request Pull**: Tàu gọi HTTP GET tới Shore API `/api/sync/pull?nodeId=SHIP_01`.
2.  **Shore Lọc Tin**: Shore quét bảng `sync_outbox` tìm các gói tin có đích đến là tàu này (`TargetNode = "SHIP_01"` hoặc `TargetNode = "*"`) và trường thời gian xác nhận nhận tin `DeliveredAt` đang ở trạng thái trống (`NULL`).
3.  **Xử lý tại Edge**: Tàu nhận về danh sách gói tin, chạy qua lớp giải quyết xung đột [SyncConflictHandler.cs](file:///f:/NCKH/Product/Martime_product_v1.1/edge_product/edge-services/Services/Core/SyncConflictHandler.cs).
4.  **Gửi ACK**: Sau khi lưu thành công dưới tàu, Edge gửi gói xác nhận `/api/sync/acknowledge` chứa danh sách ID đã nhận về Shore. Shore lập tức cập nhật `DeliveredAt = DateTime.UtcNow` để đánh dấu gói tin đã gửi xong, không gửi lại ở các phiên sau.

---

## VI. BẢNG TỔNG HỢP CÂU LỆNH VÀ DATA FLOW HỆ THỐNG

Dưới đây là bảng tóm tắt luồng dữ liệu của 5 chức năng, chỉ rõ file chịu trách nhiệm xử lý logic chính ở cả Edge và Shore:

| Chức Năng | Điểm bắt đầu (UI Edge) | Xử lý API cục bộ (Edge Services) | Điểm đến Database (Edge) | Cơ chế luân chuyển dữ liệu | Tiếp nhận Shore API (Shore Backend) |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **1. Logbooks** | `WatchkeepingPage.tsx`<br>`DeckLogPage.tsx` | `WatchkeepingController.cs`<br>`DeckLogbookService.cs` | `watchkeeping_logs`<br>`deck_log_books` | **Tự động**: Interceptor phát hiện ghi chép -> ném vào `sync_queue` -> `SyncBackgroundWorker` đẩy batch. | `SyncController.cs`<br>`ConflictResolverService.cs` |
| **2. Voyage** | `VoyagePage.tsx`<br>`CockpitTab.tsx` | `VoyageController.cs`<br>`VoyageManagementService.cs` | `voyage_records`<br>`port_calls`<br>`voyage_crew_assignments` | **Tự động**: Auto-sync khi lưu hành trình. Tự động tạo mốc `service_records` cho thuyền viên khi lên tàu. | `SyncController.cs`<br>`ConflictResolverService.cs` |
| **3. Reports** | `NoonReportForm.tsx`<br>`BunkerReportForm.tsx`| `ReportingController.cs`<br>`ReportingService.cs` | `maritime_reports`<br>`noon_reports`<br>`bunker_reports` | **Thủ công (Transmit)**: Thuyền trưởng ký duyệt -> gọi HTTP Client gửi trực tiếp -> nếu lỗi mạng mới ném vào `sync_queue` làm phương án dự phòng. | `ReportsController.cs` |
| **4. Sync Queue** | `SyncPage.tsx` | `SyncController.cs`<br>`SyncService.cs` | `sync_queue` | **Ngầm (Background Process)**: `SyncBackgroundWorker` chạy tuần kỳ 30s kiểm tra mác mạng để đóng gói đẩy đi. | `SyncController.cs` |
| **5. Audit Log** | `AuditLogPage.tsx` | `AuditLogController.cs`<br>`AuditInterceptor.cs` | `system_logs` | **Nội bộ Edge**: Không đồng bộ ngược lên bờ nhằm tối ưu tài nguyên lưu trữ vệ tinh (Trừ các trường hợp yêu cầu đặc biệt). | Không có (chỉ lưu cục bộ ở mỗi nút biên tàu). |

---

## VII. TỔNG HỢP CÁC PHƯƠNG PHÁP KHAI BÁO BIẾN ĐẶC THÙ HÀNG HẢI

1.  **Múi giờ trong khai báo `DateTime`**:
    *   *Khai báo*: `DateTime.UtcNow` thay vì `DateTime.Now`.
    *   *Mục đích*: Tàu liên tục di chuyển qua các kinh tuyến trái đất (múi giờ thay đổi liên tục). Ghi nhận thời gian UTC giúp đồng bộ dữ liệu chuẩn hóa, văn phòng trên bờ ở múi giờ khác vẫn đối chiếu chính xác lịch trình di chuyển của tàu.
2.  **Định dạng kiểu dữ liệu Tọa độ GPS**:
    *   *Khai báo*: `numeric(10,7)` trong Database và `double` trong C# class.
    *   *Mục đích*: Độ dài phần thập phân 7 chữ số cung cấp độ chính xác định vị địa lý dưới **1.1 cm** trên thực địa. Thích hợp cho các báo cáo cập cảng và theo dõi chênh lệch dòng chảy khi chạy tàu.
3.  **Khóa đồng bộ dị bộ `SemaphoreSlim`**:
    *   *Khai báo*: `private static readonly SemaphoreSlim _pushGate = new SemaphoreSlim(1, 1);`
    *   *Mục đích*: Biến tĩnh (`static`) dùng chung trên toàn bộ các instances của class Service. Giới hạn đúng 1 luồng truy cập đồng thời để chống hiện tượng tranh chấp (race conditions) dữ liệu trong hàng đợi đồng bộ.
4.  **Sử dụng `volatile` / `volatile bool`**:
    *   *Khai báo*: `private static volatile bool _shoreReachable = true;`
    *   *Mục đích*: Từ khóa `volatile` báo cho trình biên dịch C# biết thuộc tính này có thể bị thay đổi bởi nhiều luồng chạy song song (luồng kiểm tra kết nối mạng và luồng đẩy dữ liệu). Nó ép CPU luôn đọc giá trị từ bộ nhớ vật lý (RAM) thay vì lưu tạm vào bộ nhớ đệm (Cache thanh ghi CPU), đảm bảo thông tin trạng thái kết nối mạng luôn được đồng nhất tức thì giữa các tiểu tiến trình ngầm.
