# Repositories — Repository Pattern (chỉ dùng cho 1 entity duy nhất)

## Mục đích

Thư mục này chứng minh Shore Backend đã **thử áp dụng** Repository Pattern (Controller → Service → Repository → DbContext) nhưng chỉ dừng lại ở đúng 1 entity demo (`Ship`) rồi không mở rộng tiếp cho bất kỳ domain nghiệp vụ thật nào khác. Đọc thư mục này để hiểu rõ ranh giới giữa "pattern lẽ ra nên dùng" và "pattern thực tế đang dùng" trong toàn bộ codebase.

## Cấu trúc & vai trò

| File | Vai trò |
|---|---|
| `IShipRepository.cs` | Interface 2 phương thức: `GetAllAsync()`, `AddAsync(Ship ship)` |
| `ShipRepository.cs` | Triển khai bằng EF Core: `GetAllAsync` dùng `_db.Ships.AsNoTracking().ToListAsync()`, `AddAsync` dùng `_db.Ships.Add(...)` + `SaveChangesAsync()` |

## Luồng hoạt động chính

```
Controllers/ShipsController.cs
        │
        ▼
Services/IShipService.cs → Services/ShipService.cs
        │
        ▼
Repositories/IShipRepository.cs → Repositories/ShipRepository.cs
        │
        ▼
Data/AppDbContext.cs (DbSet<Ship> Ships)
```

Đây là chuỗi 4 lớp "sách giáo khoa" — nhưng chỉ áp dụng cho `Ship`, một entity demo 4 trường (`Id`, `Name`, `IMO`, `Capacity`), KHÔNG phải `Vessel` (entity tàu thật của hệ thống).

## Liên kết với phần khác

- Đăng ký DI ở `Program.cs`: `AddScoped<IShipService, ShipService>()` và `AddScoped<IShipRepository, ShipRepository>()`.
- Không có liên kết nào khác — đây là nhánh code biệt lập, không entity/service/controller nào khác trong toàn backend tham chiếu tới `Repositories/`.

## Ghi chú khi đọc/dạy

- **Đây KHÔNG phải quy ước của dự án — đừng dạy nhầm.** Đã xác minh bằng cách tìm từ khoá "Repository" trong toàn bộ `Controllers/` (24 file) và `Services/` (~40 file): chỉ xuất hiện đúng 1 lần bên ngoài chính thư mục này (`Services/ShipService.cs`). Ngược lại, `AppDbContext` được inject trực tiếp vào **27 file Service** và **24 file Controller** — nghĩa là mọi domain thật (Vessel, Voyage, CrewManagement, Materials, Pms, Sync, AI...) đều bỏ qua Repository, gọi thẳng `AppDbContext` từ Service (và nhiều khi cả từ Controller).
- Nhiều khả năng đây là tàn dư của một scaffold/tutorial ban đầu khi dự án mới bắt đầu, trước khi các domain nghiệp vụ thật được thêm vào theo lối gọi thẳng `AppDbContext` nhanh hơn. Khi tài liệu hoá kiến trúc dự án cho người mới, nói rõ: **"Repository pattern tồn tại trong code nhưng không phải là quy ước cần tuân theo khi thêm tính năng mới"** — thêm tính năng mới nên theo pattern phổ biến thật sự: Controller/Service gọi thẳng `AppDbContext`.
- Nếu một ngày nhóm quyết định chuẩn hoá lại theo Repository Pattern thật sự, đây là nơi duy nhất đã có sẵn để tham khảo cấu trúc, nhưng cần nhân rộng cho toàn bộ ~30 entity nghiệp vụ khác — khối lượng công việc không nhỏ.
