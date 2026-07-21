# core/ — Hạ tầng dùng chung (cross-cutting infrastructure)

## Mục đích

`core/` chứa toàn bộ thành phần **không thuộc riêng một tính năng nghiệp vụ nào** mà được nhiều nơi trong `data/` và `presentation/` dùng chung: dependency injection, gọi mạng, cache/offline, lưu trữ bảo mật, hằng số, theme, đa ngôn ngữ. Đây là tầng thấp nhất trong sơ đồ phụ thuộc của app — không phụ thuộc ngược lên `data/` hay `presentation/`.

## Cấu trúc & vai trò

| Thư mục | File chính | Vai trò | Độ sâu tài liệu |
|---|---|---|---|
| `di/` | `service_locator.dart` | Đăng ký & khởi tạo toàn bộ singleton bằng **GetIt** | [`di/README.md`](di/README.md) |
| `network/` | `api_client.dart`, `api_interceptor.dart`, `network_info.dart` | Cấu hình Dio, interceptor JWT, kiểm tra kết nối mạng | [`network/README.md`](network/README.md) |
| `cache/` | `cache_manager.dart`, `sync_queue.dart` | Cache Hive có hạn dùng + hàng đợi đồng bộ store-and-forward | [`cache/README.md`](cache/README.md) |
| `storage/` | `server_config_storage.dart` | Lưu URL Edge Server (Hive) để đổi server không cần build lại app | [`storage/README.md`](storage/README.md) |
| `auth/` | `token_storage.dart` | Lưu JWT access/refresh token bằng `flutter_secure_storage` (Keychain/Keystore) | mô tả tại đây |
| `constants/` | `api_constants.dart`, `app_constants.dart`, `cache_keys.dart` | Hằng số: endpoint, tên cache key, ngưỡng UI/hiệu năng | mô tả tại đây |
| `localization/` | `locale_provider.dart`, `localization_helper.dart` | `ChangeNotifier` chọn ngôn ngữ + extension `context.l10n` | mô tả tại đây |
| `theme/` | `maritime_theme.dart` | Bảng màu & `ThemeData` "Maritime Professional" | mô tả tại đây |

4 thư mục đầu (`di`, `network`, `cache`, `storage`) có README riêng vì đây là nơi tập trung phần lớn logic quan trọng và các "gotcha" cấu hình của app. 4 thư mục còn lại (`auth`, `constants`, `localization`, `theme`) chỉ gồm 1-3 file nhỏ, được mô tả ngay dưới đây.

### `auth/token_storage.dart`

`TokenStorage` bọc `FlutterSecureStorage` (Keychain trên iOS, Keystore/EncryptedSharedPreferences trên Android) để lưu 6 khóa: `access_token`, `refresh_token`, `user_id`, `crew_id`, `full_name`, `position`. Được `ApiInterceptor` (mỗi request, để gắn JWT + `X-User-Id`), `AuthRepository`, `TaskRepository`, `CrewRepository`, `AuthProvider` dùng. **Không** có refresh-token-rotation tự động phía interceptor — `ApiInterceptor._refreshToken()` hiện là `TODO`, luôn trả `false` (xem `network/README.md`).

### `constants/`

- **`ApiConstants`**: base URL mặc định (`http://192.168.1.81:5001` — sẽ bị ghi đè lúc khởi động, xem `storage/README.md`), toàn bộ path endpoint dạng string, timeout (60s — cố ý dài để chịu upload ảnh), `cacheDuration`.
- **`AppConstants`**: tên/version app, các hằng string status/priority/task-type (dùng song song với logic tương đương trong `MaintenanceTask`), format ngày giờ. File này còn chứa 3 class riêng: `PerformanceConstants` (chu kỳ auto-refresh 30s, các mốc timeout theo mức ưu tiên), `UIBreakpoints` (ngưỡng responsive), `AnimationDurations`.
- **`CacheKeys`**: toàn bộ key dùng với `CacheManager`/Hive (`myTasks`, `userProfile`, các prefix `draft_`/`pending_` được `CacheManager.clearAllCache()` bảo vệ khi logout — xem `cache/README.md`).

### `localization/`

- `LocaleProvider` (`ChangeNotifier`): lưu locale đã chọn vào `SharedPreferences` (key `app_locale`), khai báo **7 ngôn ngữ** hỗ trợ (`en, vi, fil, hi, zh, ja, ko`) qua `supportedLocales`.
- `LocalizationExtension` (`context.l10n`) + `TaskStatusHelper`/`TaskPriorityHelper`: helper dịch chuỗi status/priority của task sang locale hiện tại.

> **Cảnh báo quan trọng (chi tiết đầy đủ tại `../l10n/README.md`)**: `AppLocalizations` (sinh bởi `flutter gen-l10n`) **chỉ thực sự hỗ trợ `en` và `vi`** — `isSupported()` trong `lib/l10n/app_localizations.dart` chỉ kiểm tra 2 mã ngôn ngữ này. 5 ngôn ngữ còn lại (`fil, hi, zh, ja, ko`) xuất hiện trong danh sách chọn của `LocaleProvider` và `MaterialApp.supportedLocales` nhưng **không có file `.arb` tương ứng**. Nếu người dùng chọn 1 trong 5 ngôn ngữ này ở màn hình chọn ngôn ngữ, `Localizations` widget của Flutter sẽ không nạp được delegate cho `AppLocalizations`, và bất kỳ nơi nào gọi `context.l10n`/`AppLocalizations.of(context)` sau đó sẽ **crash** (runtime assertion error). Đây là lỗi thật trong code hiện tại, không phải giả định — cần lưu ý khi review PR liên quan tới `LanguageSelectionScreen` hoặc khi thêm ngôn ngữ mới.

### `theme/`

`MaritimeTheme` là bảng màu tĩnh (Navy Blue/Sage Green + màu trạng thái CRITICAL/HIGH/NORMAL/LOW) kèm `MaritimeTheme.lightTheme` (`ThemeData` đầy đủ). Lưu ý: `app.dart` hiện **không dùng** `MaritimeTheme.lightTheme` — nó tự định nghĩa 1 `ThemeData` khác ngay trong file (seedColor `Colors.blue`, font `GoogleFonts.interTextTheme()`); và `presentation/screens/tasks/task_detail_screen.dart` còn định nghĩa lại gần như y hệt bảng màu Maritime trong 1 class cục bộ tên `MaritimeColors` riêng của file đó. Tức là có **3 nơi** định nghĩa "bảng màu maritime" gần trùng lặp nhau — ví dụ tốt để dạy về việc thiếu 1 nguồn sự thật duy nhất (single source of truth) cho design token khi codebase phát triển qua nhiều giai đoạn.

## Luồng hoạt động chính

`main()` → `setupServiceLocator()` (đăng ký `NetworkInfo`, `ApiClient`, `TokenStorage`, `CacheManager`, `SyncQueue` là singleton qua GetIt) → các Provider/Repository lấy lại các singleton này qua `sl<T>()` thay vì tự khởi tạo — mục tiêu là **1 instance duy nhất** cho mỗi service trong suốt vòng đời app (tránh nhiều `Dio`/nhiều Hive box handle chồng chéo, vốn từng gây vấn đề hiệu năng theo comment trong code).

## Liên kết với phần khác

- `data/repositories/*` phụ thuộc gần như toàn bộ `core/` (network, cache, auth, constants).
- `presentation/providers/*` phụ thuộc `core/di` để lấy repository, và trực tiếp `core/cache`, `core/network` cho `SyncProvider`.
- `presentation/screens/*` đôi khi phá vỡ quy tắc và tự `import` thẳng `core/network/api_client.dart`, `core/cache/cache_manager.dart` thay vì đi qua Provider (xem "Ghi chú khi đọc/dạy" trong `presentation/README.md`).

## Ghi chú khi đọc/dạy

- Đây là điểm bắt đầu tốt nhất cho dev mới trước khi đọc `data/` hay `presentation/`, vì gần như mọi lớp khác đều "mượn" thứ gì đó từ đây.
- 4 thư mục có README riêng (`di`, `network`, `cache`, `storage`) là nơi tập trung phần lớn "config gotchas" của app — nên đọc kỹ trước khi đổi server URL hoặc thêm API mới.
- Gotcha đa ngôn ngữ (7 ngôn ngữ khai báo, chỉ 2 hoạt động) là lỗi có khả năng gây crash thật — nên là ưu tiên đọc sớm nếu công việc liên quan tới `LocaleProvider`/`LanguageSelectionScreen`.
