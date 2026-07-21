# core/cache — Offline Cache & Sync Queue (Hive)

## Mục đích

Đây là **trái tim của khả năng offline-first** — yêu cầu bắt buộc với app chạy trên tàu, nơi kết nối mạng có thể mất hàng giờ liền. Thư mục này cung cấp 2 cơ chế bổ trợ nhau, cả hai đều dùng **Hive** (NoSQL, lưu trên đĩa) làm nền:

1. **`CacheManager`**: cache dữ liệu đọc (GET) có hạn dùng, dùng khi offline hoặc để tăng tốc.
2. **`SyncQueue`**: hàng đợi lưu tạm các thao tác ghi (POST/PUT) chưa gửi được lên server, tự động gửi lại khi có mạng — mô hình **store-and-forward**, cùng triết lý với `SyncQueue`/`SyncBackgroundWorker` phía Edge backend (`edge-services`, xem README gốc dự án mục 5), chỉ khác là chạy trên chính thiết bị di động và đơn giản hơn nhiều.

## Cấu trúc & vai trò

| File | Lớp chính | Vai trò |
|---|---|---|
| `cache_manager.dart` | `CacheManager` | Đọc/ghi Hive box `cache_box`, tự kiểm tra hết hạn (`expiryMs`) |
| `sync_queue.dart` | `SyncQueue`, `NonRetryableSyncException` | Hàng đợi Hive box `sync_queue` (kiểu `Box<SyncItem>`), xử lý gửi lại + retry |

### `CacheManager`

- Lưu mỗi entry dưới dạng chuỗi JSON: `{ data, timestamp, expiryMs }` (dùng `jsonEncode`/`jsonDecode` thủ công, không qua Hive TypeAdapter riêng).
- 2 mức hạn dùng: `defaultExpiryMs` = 1 giờ; `offlineExpiryMs` = **7 ngày** (dùng cho dữ liệu cần giữ lâu để xem offline, qua `saveDataForOffline()`).
- `getData()` tự xoá entry nếu hết hạn hoặc JSON hỏng; `getDataNoExpiry()` bỏ qua kiểm tra hạn — dùng làm "phao cứu sinh" cuối cùng khi API lỗi (xem cách `TaskRepository` fallback trong `../../data/repositories/README.md`).
- `clearAllCache()` **cố tình giữ lại** các key bắt đầu bằng `pending_`, `draft_`, `sync_queue` khi logout — tránh mất dữ liệu người dùng đã nhập nhưng chưa gửi đi. `clearAllCacheForced()` mới xoá sạch hoàn toàn (dùng khi bấm "Clear Cache" thủ công trong Settings).

### `SyncQueue`

- Model lưu trong queue là `SyncItem` (`data/models/sync_item.dart`, đối tượng Hive qua `@HiveType`): `id` (uuid), `type` (`SyncItemType`), `data` (`Map`), `createdAt`, `retryCount`.
- 8 loại `SyncItemType`: `taskComplete`, `taskStart`, `profileUpdate` (2 loại đầu đã **deprecated** — xử lý bằng cách xoá khỏi hàng đợi mà không gọi API gì cả), `checklistComplete`, `taskSubmit`, `deferralCreate`, `deferralCancel`, `sparePartsSync`.
- `processSyncQueue()`:
  - Có **mutex flag** `_isProcessing` chống chạy chồng — quan trọng vì `SyncProvider` có 2 nguồn trigger độc lập (connectivity event + timer định kỳ) có thể gọi hàm này gần như cùng lúc.
  - Bỏ qua và **xoá vĩnh viễn** item nếu `retryCount >= 5` (`_maxRetries`) — không có cơ chế "dead-letter queue" lưu lại để xem sau.
  - Kiểm tra lại kết nối mạng **trước mỗi item** (không chỉ 1 lần đầu tiên), vì mạng có thể rớt giữa chừng khi xử lý hàng loạt item.
  - Với mỗi item, gọi thẳng `TaskApi` (qua `sl<TaskApi>()`, cache lại trong field riêng) — **không gọi qua `TaskRepository`** để tránh vòng lặp vô hạn (nếu gọi lại repository, repository có thể add lại chính item đó vào sync queue khi gặp lỗi).
  - Lỗi HTTP `< 500` (400, 409...) được coi là **lỗi nghiệp vụ không thể retry** → ném `NonRetryableSyncException` và xoá khỏi hàng đợi ngay lập tức (ví dụ mã lỗi thật từ backend: `RUNNING_HOURS_BELOW_CURRENT`, `INSUFFICIENT_STOCK`). Lỗi khác (mất mạng, lỗi 5xx) thì tăng `retryCount` và giữ lại trong hàng đợi để thử lại sau.

## Luồng hoạt động chính

```
Người dùng thao tác khi OFFLINE (VD: tick 1 mục checklist)
        │
        ▼
Repository cập nhật CacheManager NGAY (optimistic update — UI phản hồi tức thì)
        │
        ▼
Repository gọi SyncQueue.addToQueue(SyncItem(...))
        │
   (có mạng trở lại)
        ▼
SyncProvider phát hiện qua NetworkInfo.onConnectivityChanged HOẶC Timer 30 giây định kỳ
        │
        ▼
SyncQueue.processSyncQueue() → gọi TaskApi trực tiếp cho từng item theo `type`
        │
   thành công → xoá khỏi hàng đợi        lỗi 4xx → NonRetryableSyncException, xoá luôn
   lỗi mạng/5xx → retryCount++, giữ lại (tối đa 5 lần thử)
```

## Liên kết với phần khác

- `data/repositories/task_repository.dart` — người dùng chính của cả 2 lớp: cache-first cho đọc (`getMyTasks`, `getTaskChecklist`, `getTaskProgress`, `getAvailableMaterials`) và queue-on-failure/queue-when-offline cho ghi (`submitTask`, `completeChecklistItem`, `createDeferralRequest`, `cancelDeferralRequest`, `syncSparePartsUsed`).
- `presentation/providers/sync_provider.dart` — lớp UI-facing duy nhất "lái" `SyncQueue`: theo dõi kết nối, gọi `syncQueue()` tự động khi có mạng + hàng đợi > 0, và định kỳ mỗi 30 giây làm lưới an toàn (`_periodicSyncTimer`) phòng khi 1 event connectivity bị bỏ lỡ.
- `presentation/screens/tasks/complete_task_screen.dart` — gọi thẳng `sl<CacheManager>()`/`sl<SyncQueue>()` (bỏ qua `TaskProvider`) ở nhiều chỗ để thao tác cache chi tiết hơn (lưu draft form, spare parts nháp).
- `core/constants/cache_keys.dart` — toàn bộ tên key dùng với `CacheManager`, bao gồm các prefix `draft_`/`pending_` được `clearAllCache()` bảo vệ.

## Ghi chú khi đọc/dạy

- Đây là thư mục **quan trọng nhất để hiểu** trước khi sửa bất kỳ tính năng nào liên quan tới "làm việc khi mất mạng" — gần như mọi bài toán offline trong app đều quy về đọc kỹ 2 file này.
- `SyncQueue` xử lý sync theo **loại thao tác cứng trong switch-case** (`_syncItemToServer`), gắn chặt với `TaskApi` — muốn hỗ trợ đồng bộ offline cho 1 tính năng mới (VD: acknowledge alarm khi offline) sẽ cần thêm 1 `SyncItemType` mới + 1 nhánh switch mới; không có cơ chế generic để tái sử dụng cho API khác.
- Không có giới hạn dung lượng cho `cache_box`/`sync_queue` — về lý thuyết có thể phình to nếu thiết bị offline rất lâu; `_maxRetries = 5` là cơ chế duy nhất "dọn rác" cho hàng đợi lỗi, và khi dọn thì dữ liệu **mất hẳn**, không có nơi lưu lại để người dùng biết thao tác nào đã thất bại vĩnh viễn.
- So với cơ chế Sync phía Edge backend (`SyncQueue`/`SyncBackgroundWorker`, xem README gốc dự án mục 5): cùng triết lý store-and-forward, nhưng bản mobile **đơn giản hơn nhiều lần** — không có khái niệm mức ưu tiên (Critical/Operational/Low theo loại kết nối), không có idempotency key, không phân biệt theo loại mạng (Iridium/VSAT/4G).
