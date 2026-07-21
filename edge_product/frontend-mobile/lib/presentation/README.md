# presentation/ — Giao diện & quản lý trạng thái (Provider)

## Mục đích

Toàn bộ UI của app: state management (`providers/`), màn hình (`screens/`), và widget tái sử dụng (`widgets/`). Dùng package **`provider` (^6.1.1)** theo mô hình `ChangeNotifier` + `MultiProvider` — không dùng Bloc/Riverpod/GetX.

## Cấu trúc & vai trò

| Thư mục | Số file | Vai trò | README chi tiết |
|---|---|---|---|
| `providers/` | 5 | State management: `AuthProvider`, `TaskProvider`, `SyncProvider`, `AlarmProvider`, `ChatProvider` | [`providers/README.md`](providers/README.md) |
| `screens/` | 20 (chia 8 thư mục con) | Toàn bộ màn hình theo tính năng: alarms, auth, chat, home, profile, schedule, settings, tasks | [`screens/README.md`](screens/README.md) |
| `widgets/` | 7 (chia `common/`, `task/`, + 1 file gốc) | Widget dùng lại nhiều nơi | mô tả tại đây |

### `widgets/`

| File | Widget | Dùng ở |
|---|---|---|
| `common/loading_widget.dart` | `LoadingWidget`, `LoadingOverlay` | Hầu hết màn hình có gọi API |
| `common/error_widget.dart` | `ErrorDisplayWidget` | Hiển thị lỗi kèm nút Retry |
| `common/empty_state_widget.dart` | `EmptyStateWidget` | Danh sách rỗng (task, alarm...) |
| `task/task_card.dart` | `TaskCard` | `TaskListScreen`, `ScheduleScreen` |
| `task/priority_badge.dart` | `PriorityBadge` | Hiển thị CRITICAL/HIGH/NORMAL/LOW |
| `task/status_badge.dart` | `StatusBadge` | Hiển thị trạng thái task (dùng `task.statusColor`/`task.statusText` từ model) |
| `server_config_dialog.dart` | `ServerConfigDialog` | `LoginScreen` — cơ chế đổi server URL **thực sự hoạt động** (xem `../core/storage/README.md`) |

Lưu ý: `server_config_dialog.dart` nằm trực tiếp trong `widgets/`, không theo cấu trúc con `common/`/`task/`, vì nó không thuộc nhóm nào trong 2 nhóm đó.

## Luồng hoạt động chính

`app.dart` khai báo 1 `MultiProvider` bọc toàn bộ `MaterialApp`, đăng ký 6 `ChangeNotifierProvider` (5 provider trong `presentation/providers/` + 1 `WatchkeepingProvider` lạc chuẩn từ `lib/providers/`). Mỗi Screen truy cập state qua `Provider.of<X>(context)`, `context.read<X>()`, hoặc `Consumer<X>`. Xem `providers/README.md` cho chi tiết vòng đời từng provider và `screens/README.md` cho bảng toàn bộ màn hình.

## Liên kết với phần khác

- `data/repositories/*` — nguồn dữ liệu cho mọi provider.
- `core/di` — nơi cung cấp instance repository cho provider qua `sl<T>()`.
- `l10n/` — hầu hết screen dùng `AppLocalizations.of(context)` (qua `context.l10n`, xem `core/localization`) để lấy chuỗi hiển thị.

## Ghi chú khi đọc/dạy

- **Không phải mọi màn hình chỉ nói chuyện qua Provider.** Một số screen tự `import` thẳng `core/network`, `core/cache`, `core/di` và gọi trực tiếp:
  - `presentation/screens/tasks/complete_task_screen.dart` gọi thẳng `sl<TaskRepository>()`, `sl<CacheManager>()`, `sl<SyncQueue>()` ở nhiều chỗ (lưu draft form, spare parts nháp) thay vì luôn đi qua `TaskProvider`.
  - `presentation/screens/profile/profile_screen.dart` và `certificates_screen.dart` tự `new CrewRepository(ApiClient(), NetworkInfo(), CacheManager())` — không có `CrewProvider` nào cả.
  - `presentation/screens/settings/settings_screen.dart` tự `new CacheManager()` để xử lý nút "Clear Cache".

  Khi dạy kiến trúc Provider "chuẩn", nên chỉ rõ đây là các ngoại lệ thực tế trong code, không phải quy tắc — dev mới không nên mặc định mọi thao tác dữ liệu đều phải có provider tương ứng khi đọc code cũ, nhưng nên ưu tiên pattern chuẩn (đi qua Provider) khi viết code mới.
- Xem `providers/README.md` mục Ghi chú để biết vì sao `WatchkeepingProvider` bị liệt vào diện "lạc chuẩn" dù vẫn được đăng ký sống trong `MultiProvider` của `app.dart`.
