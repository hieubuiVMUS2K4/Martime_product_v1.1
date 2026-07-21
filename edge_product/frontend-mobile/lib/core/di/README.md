# core/di — Dependency Injection với GetIt

## Mục đích

Thay thế việc mỗi class tự `new` dependency của mình bằng một **Service Locator** duy nhất (package `get_it`), đảm bảo các service dùng chung (Dio client, Hive cache, hàng đợi sync...) chỉ có **1 instance** trong suốt vòng đời app. Comment gốc trong code ghi rõ lý do: "avoid creating duplicate instances (which was causing performance issues)".

## Cấu trúc & vai trò

| File | Vai trò |
|---|---|
| `service_locator.dart` | Khai báo `final GetIt sl = GetIt.instance;`, alias `GetIt get ServiceLocator => sl;`, hàm `setupServiceLocator()` (đăng ký toàn bộ) và `resetServiceLocator()` (dùng cho test) |

Toàn bộ đăng ký dùng `registerLazySingleton` (chỉ khởi tạo khi lần đầu `sl<T>()` được gọi), theo đúng thứ tự khai báo trong file:

| # | Kiểu (`T`) | Khởi tạo | Ghi chú |
|---|---|---|---|
| 1 | `NetworkInfo` | `NetworkInfo()` | Không phụ thuộc gì |
| 2 | `ApiClient` | `ApiClient()` | Bản thân đã là singleton nội bộ (factory constructor riêng) — gọi `sl<ApiClient>()` nhiều lần vẫn ra cùng 1 Dio |
| 3 | `TokenStorage` | `TokenStorage()` | Bọc Flutter Secure Storage |
| 4 | `CacheManager` | `CacheManager()` | Bọc Hive box `cache_box` |
| 5 | `SyncQueue` | `SyncQueue(sl<NetworkInfo>())` | Phụ thuộc `NetworkInfo` |
| 6 | `TaskApi` | `TaskApi(sl<ApiClient>().dio)` | API Retrofit **duy nhất** được đăng ký như 1 singleton độc lập trong `sl` |
| 7 | `TaskRepository` | inject `apiClient, networkInfo, cacheManager, syncQueue, tokenStorage` | Repository "nặng" nhất, dùng toàn bộ core services |
| 8 | `AuthRepository` | inject `apiClient, tokenStorage, networkInfo` | |
| 9 | `AlarmRepository` | `AlarmRepository(AlarmApi(sl<ApiClient>().dio))` | `AlarmApi` được tạo **inline**, không đăng ký riêng trong `sl` |
| 10 | `ChatRepository` | inject `apiClient` | Không có Retrofit API riêng — gọi thẳng `dio.get/post` |

Cuối `setupServiceLocator()`, gọi `await sl<ApiClient>().initialize()` để nạp base URL đã lưu (xem `../storage/README.md`) — **thứ tự này quan trọng**: nếu thiếu dòng này, app sẽ luôn gọi vào `ApiConstants.baseUrl` mặc định thay vì URL người dùng đã cấu hình.

## Luồng hoạt động chính

1. `main.dart` gọi `await setupServiceLocator()` **trước** `runApp()`.
2. Mọi Provider trong `presentation/providers/` gọi `sl<XRepository>()`/`sl<NetworkInfo>()`/... ngay trong constructor của chính nó (`AuthProvider`, `TaskProvider`, `SyncProvider`), hoặc nhận sẵn qua tham số được resolve tại nơi khởi tạo (`AlarmProvider`, `ChatProvider` trong `app.dart`) — xem `presentation/providers/README.md`.
3. `core/cache/sync_queue.dart` cũng tự gọi `sl<TaskApi>()` (cache lại trong field riêng `_taskApi`) để gọi API trực tiếp khi retry 1 item, tránh vòng lặp gọi lại qua repository.

## Liên kết với phần khác

- `main.dart` — nơi duy nhất gọi `setupServiceLocator()`.
- `presentation/providers/*` — người tiêu thụ chính của `sl<T>()`.
- `core/cache/sync_queue.dart` — dùng `sl<TaskApi>()` để gửi lại item khi có mạng.

## Ghi chú khi đọc/dạy

- **Không phải mọi thứ đều đăng ký trong GetIt.** `CrewRepository` và `CrewApi` **hoàn toàn vắng mặt** khỏi `service_locator.dart`. `ProfileScreen` và `CertificatesScreen` tự `new CrewRepository(apiClient: ApiClient(), networkInfo: NetworkInfo(), cacheManager: CacheManager())` ngay trong `initState()`. Vì `ApiClient()` có factory constructor trả về singleton nội bộ nên vẫn dùng chung 1 Dio, nhưng `NetworkInfo()`/`CacheManager()` bị tạo **instance mới** mỗi lần — không gây lỗi chức năng thực tế (2 class này không giữ state riêng ngoài việc trỏ tới cùng 1 Hive box theo tên cố định), nhưng là điểm không nhất quán so với quy ước "luôn lấy qua `sl<T>()`" mà chính comment trong `service_locator.dart` đề ra. Khi hướng dẫn dev mới thêm 1 repository mới, nên yêu cầu họ đăng ký vào đây thay vì lặp lại pattern cũ này.
- `AuthApi`/`CrewApi` cũng không được đăng ký độc lập trong `sl` — chúng được tạo bên trong constructor của repository tương ứng (`AuthRepository._authApi = AuthApi(_apiClient.dio)`, `CrewRepository._crewApi = CrewApi(_apiClient.dio)`). Chỉ `TaskApi` được đăng ký như 1 singleton độc lập vì `SyncQueue` cần gọi lại nó trực tiếp từ bên ngoài repository.
- `lib/providers/watchkeeping_provider.dart` **không dùng GetIt** — tự tạo `Dio` riêng, nằm hoàn toàn ngoài hệ thống DI này (xem cảnh báo trong `../../README.md`).
