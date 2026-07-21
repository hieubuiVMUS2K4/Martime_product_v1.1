# Services/Voyage — Business logic Voyage + TOÀN BỘ background service telemetry

## Mục đích

Đây là thư mục "hai vai trò" quan trọng nhất để hiểu Edge backend sau `Services/Core`: vừa chứa business logic quản lý chuyến đi (Management/Cockpit/Financial/Efficiency/Context), **vừa là nơi thực sự đặt 7 background service** mà root README của dự án liệt kê rải rác dưới tên gọi khác nhau (Telemetry Simulator, SignalK Collector, GPS Collector, NMEA Playback, và 3 Sync Enqueuer). Tên thư mục "Voyage" hơi gây hiểu lầm vì phần lớn các background service ở đây thực chất phục vụ **telemetry/cảm biến**, không phải riêng nghiệp vụ chuyến đi.

## Cấu trúc & vai trò

### Nhóm A — Background Service thu thập/mô phỏng cảm biến (4 file, cả 4 đều là `BackgroundService`)

| File | Bật/tắt qua config (mặc định) | Vai trò |
|---|---|---|
| `TelemetrySimulatorService.cs` | `TelemetrySimulator:Enabled` (**false**) | Sinh số ngẫu nhiên quanh baseline hợp lý cho Position/Navigation/Engine/Generator/Environmental mỗi 60s (Environmental mỗi 5 tick = 300s) — dùng khi demo/dev không có tàu thật. |
| `SignalKDataCollectorService.cs` | `SignalK:Enabled` (**false**) | Gọi HTTP tới server SignalK (chuẩn dữ liệu hàng hải mở, mặc định demo server `demo.signalk.org`) lấy Navigation/Environment/Propulsion, tự quy đổi đơn vị (rad→độ, m/s→knot, Kelvin→Celsius) qua `UnitConversionHelper`. |
| `GpsCollectorService.cs` | `GpsCollector:Enabled` (**false**) | Kết nối TCP thật tới thiết bị GPS/gpsd, đọc stream NMEA thô, tự ghép buffer bị phân mảnh, parse qua `NmeaParser`, có auto-reconnect exponential backoff (1s→60s). Đây là con đường dữ liệu thật khi lắp phần cứng lên tàu. |
| `NmeaPlaybackService.cs` | `NmeaPlayback:Enabled` (**true — ĐANG BẬT trong appsettings mẫu**) | Đọc lại file NMEA đã ghi sẵn (`TestData/nmea/vietnam-coastal-route.nmea`, sinh bằng script Node trong `package.json` ở gốc `edge-services/`) để giả lập tàu đang di chuyển dọc bờ biển Việt Nam. Có **resume logic**: khi restart, tự tìm dòng NMEA có toạ độ gần nhất với vị trí `PositionData` cuối cùng trong DB thay vì luôn phát lại từ đầu file. |

**Chỉ 1 trong 4 service này thực sự chạy trong cấu hình mẫu của dự án — `NmeaPlaybackService`.** 3 service còn lại đều tồn tại đầy đủ code nhưng bị tắt qua config.

### Nhóm B — Sync Enqueuer thủ công (3 file, cũng là `BackgroundService`, KHÔNG kế thừa `Services/Sync/BaseSyncEnqueuerService`)

| File | Interval mặc định | Enqueue bảng nào | Priority |
|---|---|---|---|
| `PositionSyncEnqueuerService.cs` | 30s, batch 100 | `PositionData` | Operational |
| `EngineSyncEnqueuerService.cs` | 30s, batch 100 | `EngineData` | Operational |
| `AlertSyncEnqueuerService.cs` | 10s, batch 50 | `SafetyAlarm` **và** `EngineEvent` (cùng 1 vòng lặp) | Critical |

Cả 3 đều: quét `WHERE !IsSynced` → tự build payload JSON thủ công (không dùng chung logic) → **chủ động ghi đè `OriginNode = _vesselImo`** (đọc từ `SyncSecurity:NodeId`/`Vessel:IMO`) trước khi enqueue — comment trong code giải thích đây là fix quan trọng vì giá trị mặc định "SHIP_01" không khớp bộ lọc phía Shore.

### Nhóm C — Business logic Voyage (5 service — Management/Cockpit/Financial/Efficiency/Context)

| Interface / Implementation | Vai trò |
|---|---|
| `IVoyageManagementService` / `VoyageManagementService.cs` (file lớn nhất nhóm) | CRUD Voyage, Port Call, Crew Assignment (bao gồm `BulkAssignCrewAsync`); sinh **FAL Form 5** (`GenerateFalForm5Async`) — Crew List theo Công ước IMO FAL. |
| `IVoyageContextService` / `VoyageContextService.cs` (nhỏ nhất — 1 method) | `ResolveActiveVoyageAsync(recordDateTime)` — tìm voyage/leg đang active tại 1 mốc thời gian. Được **8 Logbook Service** (`Services/Logbooks/`) tiêm vào để tự động gắn `VoyageId`/`VoyagePlanLegId` khi tạo log mới ("Phase 6 — auto-link"). |
| `IVoyageCockpitService` / `VoyageCockpitService.cs` | `GetCockpitAsync` (tổng hợp kế hoạch-vs-thực-tế 1 voyage) + `GetTimelineAsync` (timeline hợp nhất nhiều nguồn sự kiện). |
| `IVoyageEfficiencyService` / `VoyageEfficiencyService.cs` | `GetEfficiencyReportAsync` — so sánh ước tính vs thực tế sau khi đóng voyage, điểm số 0-100 theo từng "dimension". |
| `IVoyageFinancialService` / `VoyageFinancialService.cs` (nhiều method nhất nhóm) | Toàn bộ vòng đời tài chính chuyến đi: Expense Request → Advance Payment → Disbursement/Actual Revenue → Settlement → đóng sổ (`CloseVoyageFinancialsAsync`). |

### Nhóm D — Tiện ích

| File | Vai trò |
|---|---|
| `SignalKHttpClient.cs` (`ISignalKHttpClient`) | Bọc `IHttpClientFactory` gọi SignalK API — `GetAsync<T>`, `GetRawAsync`, `HealthCheckAsync`. |
| `UnitConversionHelper.cs` | `static class` thuần hàm chuyển đổi đơn vị hàng hải: m/s↔knot, rad↔độ, K↔°C, Pa↔bar/hPa, m³/s↔lít/giờ... — dùng bởi `SignalKDataCollectorService` và các nơi khác cần chuẩn hoá đơn vị. |

## Luồng hoạt động chính

### A. Tại sao chỉ `NmeaPlaybackService` chạy trong môi trường mẫu?

`Program.cs` đăng ký cả 4 service Nhóm A dưới dạng `AddHostedService` **có điều kiện** (đọc cờ `*:Enabled`), và `appsettings.json` mẫu chỉ bật `NmeaPlayback:Enabled = true` — 3 service còn lại đều `false`. Đây là lựa chọn demo hợp lý: dữ liệu NMEA phát lại từ file có tính xác định (deterministic) và "trông giống tàu thật đang di chuyển ở Việt Nam" hơn là số ngẫu nhiên thuần tuý của `TelemetrySimulatorService`. Khi triển khai lên tàu thật, thứ tự chuyển đổi dự kiến: tắt `NmeaPlayback` → bật `GpsCollector` (đọc phần cứng GPS/gpsd thật qua TCP).

### B. Từ cảm biến tới Shore — chuỗi đầy đủ cho vị trí GPS

```
NmeaPlaybackService (hoặc GpsCollectorService khi có phần cứng thật)
  → NmeaParser.ParseSentence() → PositionData { IsSynced = false, OriginNode = _vesselImo }
  → dbContext.PositionData.AddAsync() + SaveChangesAsync()
        → NGAY LÚC NÀY: outbox tự động trong EdgeDbContext (Data/README.md) đã tạo 1 SyncQueue
          entry cho PositionData này (PositionData KHÔNG nằm trong danh sách loại trừ)
  → (30s sau) PositionSyncEnqueuerService quét WHERE !IsSynced → tìm thấy CÙNG bản ghi
        → tạo THÊM 1 SyncQueue entry (payload khác định dạng) → set IsSynced = true
  → SyncBackgroundWorker (Services/Core) đọc SyncQueue, push cả 2 entry lên Shore
```

Xem phân tích đầy đủ về khả năng trùng lặp này tại `Services/Core/README.md` (mục "SyncQueue được nạp từ đâu?") và `Data/README.md`.

## Liên kết với phần khác

- **`Services/Core/README.md`** — người tiêu thụ cuối cùng của `SyncQueue` mà Nhóm B tạo ra; cũng là nơi giải thích cơ chế outbox tự động chồng lấn với Nhóm B.
- **`Services/Sync/README.md`** — `BaseSyncEnqueuerService<TEntity>` được thiết kế để 3 service Nhóm B kế thừa, nhưng thực tế không có service nào làm vậy.
- **`Services/Parsers/NmeaParser.cs`** — dùng bởi cả `GpsCollectorService` và `NmeaPlaybackService`.
- **`Services/Logbooks/README.md`** — cả 8 Logbook Service tiêm `IVoyageContextService` từ đây để auto-link.
- **`Controllers/Voyage/README.md`** — lớp controller mỏng gọi vào 5 service Nhóm C.
- **`appsettings.json`** — mọi cờ `*:Enabled`/`*:IntervalSeconds`/`*:BatchSize` của Nhóm A và B đều đọc từ đây; đổi cấu hình là cách nhanh nhất để "bật/tắt" một background service mà không cần sửa code.

## Ghi chú khi đọc/dạy

- **Đừng tìm background service theo tên thư mục "Voyage"** — 4/7 background service ở đây (Nhóm A) không liên quan trực tiếp đến nghiệp vụ Voyage, mà là hạ tầng thu thập cảm biến dùng chung cho toàn tàu. Cách tổ chức thư mục này (đặt cạnh Voyage business logic) có lẽ là do lịch sử phát triển hơn là ranh giới domain rõ ràng.
- **Khả năng trùng lặp SyncQueue** (mục B ở trên) là điểm đáng thảo luận sâu nhất của thư mục này — nên đối chiếu với `Data/README.md` để hiểu đầy đủ trước khi kết luận đây có phải bug hay là thiết kế cố ý (ví dụ: có thể enqueue thủ công nhằm đảm bảo retry độc lập với transaction ghi dữ liệu gốc).
- **`VoyageContextService` chỉ có 1 method** nhưng được tiêm vào rất nhiều service khác (8 Logbook Service) — minh hoạ tốt cho nguyên tắc "service nhỏ, single-responsibility, dùng lại nhiều nơi" đối lập với `VoyageFinancialService`/`VoyageManagementService` là các "service lớn, nhiều trách nhiệm".
- **`AlertSyncEnqueuerService` xử lý 2 loại entity trong 1 service** (`SafetyAlarm` và `EngineEvent`) trong khi `PositionSyncEnqueuerService`/`EngineSyncEnqueuerService` mỗi service chỉ 1 loại — không có lý do kiến trúc rõ ràng cho sự khác biệt này, có thể nêu ra như câu hỏi thảo luận.
- **`NmeaPlaybackService` có "resume logic" khá tinh vi** (tìm điểm gần nhất bằng khoảng cách Euclidean trên toạ độ, làm tròn về bội số của 3 vì mỗi mẫu là 3 dòng GGA/GSA/RMC) — đáng dùng làm ví dụ về cách xử lý "khôi phục trạng thái sau khi restart" cho một tiến trình phát lại dữ liệu tuần tự.
