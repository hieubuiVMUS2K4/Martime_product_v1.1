# Services/Background — Tiến trình chạy nền (ngoài Sync)

## Mục đích

Thư mục này chứa các `BackgroundService`/hosted-service **không thuộc trực tiếp về đồng bộ Edge-Shore** (những cái đó nằm trong `Services/Sync/`, cộng `AlertBackgroundService.cs` ở thư mục cha — xem `Services/README.md`). Có 2 nhóm việc: dò chất lượng mạng cục bộ, và pipeline hàng đợi để tự động đánh giá Noon Report bằng AI mà không làm chậm luồng nhận sync.

## Cấu trúc & vai trò

| File | Loại | Vai trò |
|---|---|---|
| `NetworkAwareSyncBackgroundService.cs` | `BackgroundService`, đăng ký `AddHostedService` trong `Program.cs` | Vòng lặp định kỳ đọc `INetworkDetectionService.GetNetworkQualityAsync()`, log chất lượng mạng, và điều chỉnh chu kỳ kiểm tra tiếp theo (5s Shore_WiFi / 30s Cellular_4G / 180s VSAT / 180s Iridium / 60s khi None) |
| `ReportEvaluationQueue.cs` | Lớp thường (Singleton) | Hàng đợi in-memory (`System.Threading.Channels.Channel<Guid>`, bounded 1000) chuyển tiếp `ReportId` của các Noon Report cần AI đánh giá, từ nơi sản xuất (`SyncInboxService`) sang nơi tiêu thụ (`ReportEvaluationWorker`) |
| `ReportEvaluationWorker.cs` | `BackgroundService`, đăng ký `AddHostedService` | Đọc từng `ReportId` từ hàng đợi, dựng dữ liệu "hôm nay so với trung bình 7 ngày", gọi `IGeminiEvaluationService.EvaluateReportAsync`, lưu kết quả vào bảng `ReportEvaluation` |

## Luồng hoạt động chính

**Pipeline đánh giá AI tự động cho Noon Report** (luồng quan trọng nhất trong thư mục này):

```
Edge POST /api/sync (chứa noon_report)
        │
        ▼
SyncInboxService.ProcessBatchAsync (Services/Sync/)
        │  sau khi lưu thành công các NoonReport mới
        ▼
ReportEvaluationQueue.EnqueueAsync(reportId)   ◄── Singleton, dùng chung toàn app
        │  (Channel bounded 1000 — nếu đầy, EnqueueAsync sẽ CHỜ, gây nghẽn ngược lên SyncInboxService)
        ▼
ReportEvaluationWorker.ExecuteAsync
   await foreach (reportId in _queue.ReadAllAsync(...))
        │  xử lý TUẦN TỰ từng cái một (không song song)
        ▼
   1. Tải NoonReport + 7 ngày trước đó (cùng voyage nếu có)
   2. Map sang ShipMetricsDto (EngineTemp lấy từ SeaTemperature — KHÔNG phải nhiệt độ máy thật!)
   3. Gọi IGeminiEvaluationService.EvaluateReportAsync(...)   (Services/AI/)
   4. Lưu ReportEvaluation { ReportId, Status, ContentVi }
   5. await Task.Delay(10 giây)   ★ giới hạn cứng ~6 báo cáo/phút, bất kể provider cho phép nhanh hơn ★
```

**Vòng lặp dò mạng** (`NetworkAwareSyncBackgroundService`): tạo scope DI mỗi lượt, gọi `INetworkDetectionService`, log kết quả, chờ theo interval tương ứng loại mạng — xem ghi chú quan trọng bên dưới về việc vòng lặp này **chưa** thực sự điều khiển sync.

## Liên kết với phần khác

- `ReportEvaluationQueue` được `Services/Sync/SyncInboxService.cs` gọi (producer) và `ReportEvaluationWorker` tiêu thụ (consumer) — cầu nối trực tiếp giữa `Services/Sync/` và `Services/AI/`.
- `NetworkAwareSyncBackgroundService` phụ thuộc `Services/Network/INetworkDetectionService` (xem `Services/README.md` — chỉ 1 file nên không tách README riêng).
- Cả hai hosted service đăng ký qua `builder.Services.AddHostedService<...>()` trong `Program.cs`.

## Ghi chú khi đọc/dạy

- **`NetworkAwareSyncBackgroundService` KHÔNG thực sự thực hiện đồng bộ nào cả — đây là điểm quan trọng nhất cần biết.** Phương thức `PerformNetworkAwareSyncAsync` chỉ tính `maxSyncPriority` từ loại mạng rồi log ra, có nguyên văn comment `// TODO: Integrate with actual ISyncOutboxService`. Nó không gọi `ISyncOutboxService` hay `ISyncInboxService` ở bất kỳ đâu. Lý do hợp lý: Shore nhận dữ liệu qua **Edge chủ động push** (`POST /api/sync`) và **Edge chủ động pull** (`GET /api/sync/pull`) — không có vòng lặp "Shore chủ động gửi" nào để cái service này cắm vào một cách tự nhiên; nó giống một bản scaffold mô phỏng theo khái niệm bên Edge (nơi có `SyncBackgroundWorker` chủ động đẩy dữ liệu qua vệ tinh) nhưng chưa hoàn thiện phía Shore. Khi dạy, gọi đây là **"trình theo dõi chất lượng mạng, chưa phải cỗ máy sync thích ứng"**, đừng mô tả nó như một tính năng đang hoạt động đầy đủ.
- **`ReportEvaluationQueue` là in-memory, KHÔNG bền vững** — nếu process Shore restart giữa lúc còn item trong hàng đợi (đã enqueue nhưng chưa dequeue), các `ReportId` đó biến mất, sẽ không bao giờ được đánh giá AI cho tới khi có thay đổi khác kích hoạt lại. Khác hẳn với `SyncOutbox`/`SyncDlq` (Services/Sync) là bảng DB, sống sót qua restart.
- **Giới hạn 10 giây/item trong `ReportEvaluationWorker`** là hard-code để tôn trọng rate-limit miễn phí của Groq — một batch sync lớn (ví dụ 50 Noon Report cùng lúc) sẽ mất hơn 8 phút để đánh giá xong hết, xử lý tuần tự từng cái một, không song song dù `Channel` có thể hỗ trợ nhiều consumer.
- Nếu `NoonReports.FindAsync` không tìm thấy report (bị xóa giữa lúc enqueue và lúc worker xử lý), worker chỉ log warning và bỏ qua — không có cơ chế báo lỗi nào khác.
