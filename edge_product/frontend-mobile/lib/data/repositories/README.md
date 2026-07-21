# data/repositories — Điều phối Online/Offline

## Mục đích

Là nơi duy nhất trong app quyết định: gọi API hay đọc cache, lưu gì vào hàng đợi đồng bộ khi lỗi/mất mạng, và chuẩn hoá dữ liệu trả về cho Provider. Vì dự án không có tầng "domain" riêng, các quyết định nghiệp vụ (VD: "task nào được phép bắt đầu") nằm rải rác giữa đây và các getter trong model (`data/models/maintenance_task.dart`).

## Cấu trúc & vai trò

| File | Class | Chiến lược Offline | Đăng ký trong GetIt? |
|---|---|---|---|
| `task_repository.dart` | `TaskRepository` (690 dòng — lớn nhất dự án) | **Đầy đủ**: cache-first cho đọc, sync-queue cho ghi | Có |
| `auth_repository.dart` | `AuthRepository` | **Không có offline** — login/refresh/logout đều yêu cầu mạng | Có |
| `alarm_repository.dart` | `AlarmRepository` | **Không có offline** — pass-through mỏng, chỉ bọc try/catch đổi message lỗi | Có |
| `chat_repository.dart` | `ChatRepository` | **Không có offline** — gọi thẳng `dio.get/post`, không qua Retrofit API riêng | Có |
| `crew_repository.dart` | `CrewRepository` | **Cache read-through** (đọc cache trước/sau tuỳ `forceRefresh`), không có sync queue vì không có thao tác ghi | **Không** — tự khởi tạo trong screen |
| `watchkeeping_repository.dart` | `WatchkeepingRepository` | Lưu thẳng object đầy đủ vào Hive box `watchkeeping_logs` (không qua `CacheManager`/JSON string như các repo khác) | Không (module lạc chuẩn) |

## Luồng hoạt động chính

### `TaskRepository` — mẫu offline-first đầy đủ nhất, đáng học kỹ nhất

**Đọc** (`getMyTasks`, `getTaskChecklist`, `getTaskProgress`, `getAvailableMaterials`): pattern chung là

1. Nếu online + `forceRefresh` → gọi API → lưu `CacheManager.saveData()` → trả kết quả.
2. Nếu không → thử đọc cache trước; nếu cache rỗng và có mạng → gọi API.
3. Nếu API lỗi (`DioException`) → fallback đọc cache (kể cả khi đã hết hạn dùng ở một số hàm) thay vì ném lỗi thẳng ra UI.

**Ghi** (`submitTask`, `completeChecklistItem`, `createDeferralRequest`, `cancelDeferralRequest`, `syncSparePartsUsed`):

1. Với `completeChecklistItem`: cập nhật **cache ngay lập tức** (optimistic update, hàm `_updateChecklistItemInCache`) để UI phản hồi tức thì, rồi mới thử gọi API.
2. Nếu offline hoặc API lỗi → `SyncQueue.addToQueue(SyncItem(type: ..., data: {...}))`, **không throw lỗi ra UI** trong đa số trường hợp — trừ khi là lỗi nghiệp vụ 4xx rõ ràng (`submitTask` ném `TaskSubmissionException(code, data)` khi backend trả lỗi validate như `RUNNING_HOURS_BELOW_CURRENT`).
3. `TaskSubmissionException` là exception riêng mang theo `code` + `data` gốc từ backend để UI hiển thị thông báo chính xác — khác với exception chung chung `Exception(String)` dùng ở phần lớn nơi khác.

### `AuthRepository`

- `login()` **yêu cầu mạng** (`if (!await _networkInfo.isConnected) throw Exception(...)`) — không có khái niệm "đăng nhập offline".
- `LoginResponse.fromJson` tự nhận diện 2 định dạng payload (xem `../README.md`).
- Xử lý lỗi HTTP chi tiết theo status code (401 → sai crew id/password, 400 → lấy message từ backend, 500 → lỗi server).
- `logout()` luôn xoá token cục bộ trong khối `finally` dù API logout có lỗi hay không — tránh app bị kẹt ở trạng thái "tưởng vẫn đăng nhập".

### `CrewRepository`

- Không nhận `TokenStorage`/`SyncQueue` qua constructor như `TaskRepository` — tự tạo `TokenStorage()` làm field mặc định.
- `getMyCertificates()` khi offline **tự dựng lại** cấu trúc response (`stcw`, `medical`, `passport`, `visa`) từ `CrewMember` đã cache, thay vì cache riêng response gốc của endpoint `/api/crew/me/certificates`.

### `WatchkeepingRepository`

- Dùng `Hive.openBox<Map>('watchkeeping_logs')` và lưu/đọc trực tiếp `log.toJson()`/`WatchkeepingLog.fromJson()` — **không tái sử dụng `CacheManager`** (không có TTL, không nằm trong danh sách được `clearAllCache()` bảo vệ hay xoá khi logout). Có `getUnsynced()`/`markAsSynced()` tự chế — một cơ chế "mini sync" riêng, chạy song song và hoàn toàn không liên quan tới `core/cache/SyncQueue`.

## Liên kết với phần khác

- `presentation/providers/task_provider.dart`, `auth_provider.dart` — lấy repository qua `sl<T>()`.
- `presentation/providers/alarm_provider.dart`, `chat_provider.dart` — nhận repository qua constructor (`AlarmProvider(sl<AlarmRepository>())`), được truyền từ `app.dart`.
- `presentation/screens/profile/profile_screen.dart` & `certificates_screen.dart` — tự khởi tạo `CrewRepository` trực tiếp (không qua Provider riêng, không qua GetIt).
- `presentation/screens/schedule/watch_schedule_tab.dart` (qua `WatchkeepingProvider`) — dùng `WatchkeepingRepository`.
- `core/cache/` — xem README riêng cho chi tiết `CacheManager`/`SyncQueue`.

## Ghi chú khi đọc/dạy

- Khi thêm 1 tính năng ghi dữ liệu mới cần hỗ trợ offline, **`TaskRepository.completeChecklistItem()`** là ví dụ mẫu tốt nhất để copy pattern: cập nhật cache lạc quan → thử gọi API → nếu lỗi thì đẩy vào `SyncQueue` thay vì throw lỗi thẳng ra UI.
- Không phải repository nào cũng hỗ trợ offline — `AuthRepository`, `AlarmRepository`, `ChatRepository` đều yêu cầu mạng. Đừng giả định "cả app offline-first" khi debug 1 tính năng cụ thể; phải xem đúng repository nào đang được dùng cho tính năng đó.
- `CrewRepository` không nằm trong GetIt — nếu thêm dependency mới cho nó (VD: thêm `SyncQueue` để hỗ trợ cập nhật profile khi offline), phải nhớ sửa **2 chỗ khởi tạo thủ công** trong `profile_screen.dart` và `certificates_screen.dart`, hoặc tốt hơn là refactor để đăng ký qua `core/di/service_locator.dart` trước khi mở rộng.
- 3 kiểu exception khác nhau đang tồn tại song song trong tầng data: `Exception(String)` (phổ biến nhất), `TaskSubmissionException(code, data)` (trong `task_repository.dart`), `NonRetryableSyncException(String)` (trong `core/cache/sync_queue.dart`) — khi bắt lỗi ở tầng Provider/UI cần biết đang có thể gặp loại nào để xử lý đúng cách.
