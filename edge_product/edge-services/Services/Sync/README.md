# Services/Sync — Base class cho "enqueuer" đồng bộ (mẫu thiết kế chưa được dùng)

## Mục đích

Thư mục này chỉ có **một file**: `BaseSyncEnqueuerService.cs`. Vai trò dự kiến của nó (theo comment trong chính file) là làm **lớp cha dùng chung** cho các background service có nhiệm vụ "quét bảng dữ liệu cảm biến/sự kiện chưa đồng bộ (`IsSynced = false`) → đóng gói thành bản ghi `SyncQueue` → đánh dấu đã enqueue", để 3 service `AlertSyncEnqueuerService`, `PositionSyncEnqueuerService`, `EngineSyncEnqueuerService` không phải viết lại cùng một logic ba lần.

**Quan trọng khi dạy**: đây là ví dụ thực tế và hiếm gặp — một abstraction được thiết kế đúng đắn (Template Method pattern, generic `TEntity : ISyncableEntity`) nhưng **hiện không được bất kỳ class nào trong repo kế thừa**. Ba service kể trên (nằm trong `Services/Voyage/`, không phải ở đây) đều tự viết lại logic tương tự một cách độc lập. Đã xác minh bằng cách tìm kiếm toàn bộ mã nguồn: `BaseSyncEnqueuerService` chỉ xuất hiện trong chính file định nghĩa nó.

## Cấu trúc & vai trò

| File | Vai trò |
|---|---|
| `BaseSyncEnqueuerService.cs` | `public abstract class BaseSyncEnqueuerService<TEntity> : BackgroundService where TEntity : class, ISyncableEntity` — vòng lặp chuẩn: đọc cấu hình `{ConfigPrefix}:Enabled/IntervalSeconds/BatchSize` → mỗi chu kỳ gọi `EnqueueUnsyncedAsync()` → lấy tối đa `BatchSize` bản ghi `IsSynced=false` (mặc định order theo `CreatedAt`) → với mỗi bản ghi, gọi phương thức trừu tượng `SerializeEntity()` (subclass override) để tạo payload JSON → tạo `SyncQueue` với `Priority = Critical` cố định, `MaxRetries = 5` → lưu hàng loạt bằng `AddRangeAsync` + `SaveChangesAsync` → đặt `entity.IsSynced = true`. |

## Luồng hoạt động chính (dự kiến theo thiết kế, KHÔNG phải thực tế đang chạy)

```
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    while (!stoppingToken.IsCancellationRequested)
    {
        await EnqueueUnsyncedAsync(stoppingToken);
        await Task.Delay(IntervalSeconds, stoppingToken);
    }

EnqueueUnsyncedAsync():
    var unsynced = await GetUnsyncedRecordsAsync(dbContext, ct);   // ảo, mặc định: Where(!IsSynced).Take(BatchSize)
    foreach (entity in unsynced):
        payload = SerializeEntity(entity);        // trừu tượng — subclass tự định nghĩa field nào serialize
        SyncQueue.Add(new SyncQueue {
            TableName = EntityTableName,           // trừu tượng, vd "safety_alarm"
            RecordKey = GetEntityId(entity),       // dùng reflection lấy property "Id"
            Priority  = Critical,                  // CỐ ĐỊNH — không phân biệt loại entity
            ...
        });
        entity.IsSynced = true;
    SaveChangesAsync();
```

So sánh với 3 service thật đang chạy production (`Services/Voyage/PositionSyncEnqueuerService.cs`, `EngineSyncEnqueuerService.cs`, `AlertSyncEnqueuerService.cs`):

| Khía cạnh | `BaseSyncEnqueuerService<T>` (chưa dùng) | 3 service thật (Services/Voyage) |
|---|---|---|
| Kế thừa | Generic, dùng `ISyncableEntity` | Không kế thừa gì — mỗi service tự viết `BackgroundService` riêng |
| Priority | Luôn `Critical` | `PositionSyncEnqueuerService`/`EngineSyncEnqueuerService` dùng `Operational`; `AlertSyncEnqueuerService` dùng `Critical` |
| Serialize payload | Trừu tượng, subclass tự quyết | Mỗi service inline `JsonSerializer.Serialize(new { ... })` thủ công với danh sách field cụ thể |
| Set `OriginNode` | Không thấy xử lý trong base class | Cả 3 service thật đều CHỦ ĐỘNG ghi đè `OriginNode = _vesselImo` (đọc từ `SyncSecurity:NodeId`/`Vessel:IMO`) — có comment giải thích đây là fix quan trọng để khớp filter phía Shore |
| Số entity xử lý / chu kỳ | 1 loại entity | `AlertSyncEnqueuerService` xử lý CẢ `SafetyAlarm` VÀ `EngineEvent` trong cùng 1 vòng lặp |

## Liên kết với phần khác

- **`Services/Voyage/`** — nơi thực sự chứa 3 service enqueuer đang chạy (`PositionSyncEnqueuerService`, `EngineSyncEnqueuerService`, `AlertSyncEnqueuerService`), được đăng ký làm `IHostedService` trong `Program.cs` (mặc định cả 3 đều `Enabled = true`). Xem `Services/Voyage/README.md`.
- **`Services/Core/SyncBackgroundWorker.cs`** — người tiêu thụ cuối cùng của bảng `SyncQueue` mà các enqueuer (dù kế thừa base class này hay tự viết) đều ghi vào. Xem `Services/Core/README.md`.
- **`Maritime.Shared.Interfaces.ISyncableEntity`** — ràng buộc generic `where TEntity : ISyncableEntity` (yêu cầu có `IsSynced`, `OriginNode`, `SyncVersion`, `CreatedAt`, `UpdatedAt`).

## Ghi chú khi đọc/dạy

- Đừng dạy `BaseSyncEnqueuerService` như "class đang điều khiển việc enqueue Position/Engine/Alert" — **nó không hề chạy**. Cách kiểm chứng nhanh: tìm kiếm toàn văn (`grep`/tìm kiếm trong IDE) tên class này trong cả repo — chỉ có 1 kết quả (chính nó).
- Đây là ví dụ tốt để thảo luận về **refactor dở dang**: có thể một lập trình viên đã viết class này với ý định thay thế 3 service trùng lặp, nhưng chưa hoàn thành việc migrate (`PositionSyncEnqueuerService`... vẫn extend `BackgroundService` trực tiếp thay vì `BaseSyncEnqueuerService<PositionData>`).
- Nếu giao bài tập cho sinh viên, đây là một bài thực hành refactor tự nhiên: "Hãy sửa `PositionSyncEnqueuerService` để kế thừa `BaseSyncEnqueuerService<PositionData>` thay vì viết lại logic" — vừa dạy Template Method pattern, vừa dạy kỹ năng đọc hiểu code cũ trước khi refactor.
