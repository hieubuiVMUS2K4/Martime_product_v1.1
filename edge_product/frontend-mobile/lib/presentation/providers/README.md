# presentation/providers — State Management (ChangeNotifier + Provider)

## Mục đích

Cầu nối giữa UI (`screens/`) và dữ liệu (`data/repositories/`). Mỗi class kế thừa `ChangeNotifier`, expose getter cho state + method `Future<void>` cho thao tác, gọi `notifyListeners()` sau khi state đổi để rebuild UI đang lắng nghe.

## Cấu trúc & vai trò

| File | Class | Repository dùng | Cách lấy dependency |
|---|---|---|---|
| `auth_provider.dart` | `AuthProvider` | `AuthRepository` | Tự gọi `sl<T>()` trong constructor không tham số |
| `task_provider.dart` | `TaskProvider` | `TaskRepository` | Tự gọi `sl<T>()` trong constructor không tham số (có thêm constructor phụ `TaskProvider.withDependencies()` cho unit test) |
| `sync_provider.dart` | `SyncProvider` | *(không dùng repository — dùng thẳng `SyncQueue` + `NetworkInfo`)* | Tự gọi `sl<T>()` trong initializer list |
| `alarm_provider.dart` | `AlarmProvider` | `AlarmRepository` | **Nhận qua constructor** — `AlarmProvider(this._repository)`, giá trị được truyền từ `app.dart`: `AlarmProvider(sl<AlarmRepository>())` |
| `chat_provider.dart` | `ChatProvider` | `ChatRepository` | **Nhận qua constructor**, tương tự — `ChatProvider(sl<ChatRepository>())` trong `app.dart` |

→ 2 phong cách lấy dependency cùng tồn tại: (a) tự "service-locate" bên trong provider (Auth/Task/Sync), (b) constructor injection với giá trị resolve sẵn tại nơi khởi tạo Provider (Alarm/Chat). Cả 2 đều dùng chung `sl` cuối cùng, chỉ khác **nơi gọi**.

### Từng provider

| Provider | State chính | Điểm đáng chú ý |
|---|---|---|
| `AuthProvider` | `isLoggedIn`, `isLoading`, `error`, `crewId`, `fullName`, `position` | `login()` gọi `_cacheManager.clearAllCache()` **trước khi** set state đăng nhập thành công — xoá cache cũ để tránh hiện dữ liệu của user trước đó |
| `TaskProvider` | `tasks` (list gốc) + khoảng 10 getter lọc theo tab (`notStartedTasks`, `dueTasks`, `overdueTasks`, `rectifyTasks`, `pendingApprovalTasks`...) | Có **cache getter thủ công** (`_cachedDueTasks`, v.v.) để tránh lọc lại list trên mỗi lần build — phải gọi `_invalidateFilterCaches()` mỗi khi `_tasks` đổi (dễ quên khi thêm tính năng mới, gây bug hiển thị dữ liệu cũ ở 1 tab) |
| `SyncProvider` | `isSyncing`, `queueSize`, `isOnline`, `lastSyncTime` | Chạy **song song 2 cơ chế trigger sync**: lắng nghe `networkInfo.onConnectivityChanged` VÀ `Timer.periodic(30s)` — cả 2 đều có thể gọi `syncQueue()`, nhưng mutex `SyncQueue._isProcessing` đảm bảo không chạy chồng nhau |
| `AlarmProvider` | `activeAlarms`, `alarmHistory`, `statistics` + đếm `criticalCount`/`warningCount`/`unacknowledgedCount` | `fetchActiveAlarms()` tự sort: critical trước, rồi warning, rồi theo thời gian mới nhất |
| `ChatProvider` | `messages` (immutable qua `List.unmodifiable`), `suggestions` | Khi API suggestions lỗi, dùng **danh sách câu hỏi mẫu code cứng** (fallback: COLREGs, MARPOL, an toàn, giờ nghỉ...) thay vì để trống |

## Luồng hoạt động chính

```
Screen (initState / callback)
   │  Provider.of<X>(context, listen:false).method()  hoặc  context.read<X>().method()
   ▼
XProvider  ── set _isLoading=true, notifyListeners() ──► UI hiện LoadingWidget
   │  gọi
   ▼
XRepository.method()
   │  trả về data hoặc throw
   ▼
XProvider  ── cập nhật state, _isLoading=false, notifyListeners() ──► UI rebuild (Consumer<X>/context.watch)
```

## Liên kết với phần khác

- `core/di/service_locator.dart` — nguồn `sl<T>()` cho tất cả provider.
- `app.dart` — nơi khai báo `MultiProvider`, quyết định provider nào inject theo kiểu nào (constructor param vs tự locate).
- `presentation/screens/*` — nơi tiêu thụ, thường 1 provider phục vụ nhiều screen (VD: `TaskProvider` dùng ở cả `TaskListScreen`, `TaskDetailScreen`, `CompleteTaskScreen`, `ScheduleScreen`, `HomeScreen`).
- **Không có** `CrewProvider` — màn hình Profile/Certificates tự quản lý `Future<CrewMember?>` cục bộ bằng `FutureBuilder` + `CrewRepository` tự khởi tạo, không qua `ChangeNotifier` chung (xem `../../data/repositories/README.md`).
- **Không nằm trong thư mục này**: `WatchkeepingProvider` — về mặt chức năng đây là provider thứ 6 của app (đăng ký trong `MultiProvider` ở `app.dart` y hệt 5 provider trên), nhưng lại nằm ở `lib/providers/watchkeeping_provider.dart`, ngoài thư mục `presentation/`. Xem cảnh báo tại `../../README.md`.

## Ghi chú khi đọc/dạy

- Khi dạy "vòng đời 1 request" trong app, nên đi theo đúng 1 luồng cụ thể từ Screen → Provider → Repository → Api/Cache, ví dụ dễ hiểu nhất: `TaskListScreen` → `TaskProvider.fetchMyTasks()` → `TaskRepository.getMyTasks()`.
- Cẩn thận với **cache filter thủ công** trong `TaskProvider` — nếu thêm 1 method mới làm thay đổi `_tasks` mà quên gọi `_invalidateFilterCaches()`, các tab (Due/Overdue/Completed...) sẽ hiển thị dữ liệu cũ dù `_tasks` đã cập nhật.
- `SyncProvider.dispose()` đã huỷ `_connectivitySubscription` và `_periodicSyncTimer` đúng cách — nhưng nếu thêm state/subscription mới cho provider này, cần nhớ thêm luôn vào `dispose()` để tránh memory leak, vì đây là provider sống lâu nhất (không bị rebuild/dispose cho tới khi app đóng).
