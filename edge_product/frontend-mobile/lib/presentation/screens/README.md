# presentation/screens — Toàn bộ màn hình theo tính năng

## Mục đích

Liệt kê toàn bộ 20 screen của app, nhóm theo 8 thư mục tính năng, cùng với route (nếu có đăng ký trong `app.dart`) và provider/repository chính mỗi màn dùng.

## Cấu trúc & vai trò

| Thư mục | Screen | Route (`app.dart`) | Provider/Repository chính | Vai trò |
|---|---|---|---|---|
| `auth/` | `LoginScreen` | `/login` (và mặc định `home:`) | `AuthProvider` | Đăng nhập bằng Crew ID + password; có nút "Server Settings" mở `ServerConfigDialog` |
| `home/` | `HomeScreen` | `/home` | `AuthProvider`, `TaskProvider`, `SyncProvider` | Khung điều hướng chính: `Drawer` + nội dung đổi theo tab (Home/Tasks/Schedule/Profile), hiển thị icon online/offline + badge số item chờ sync |
| `tasks/` | `TaskListScreen` | *(nhúng trong Home, không có route riêng)* | `TaskProvider` | Danh sách task, **7 tab** lọc theo trạng thái, tự động refresh mỗi 30 giây khi app ở foreground (`PerformanceConstants.taskListRefreshInterval`), tìm kiếm có debounce 300ms |
| | `TaskDetailScreen` | — | `TaskProvider` | Chi tiết 1 `MaintenanceTask` (nhận thẳng object, không load lại theo ID), load checklist + lịch sử trạng thái |
| | `CompleteTaskScreen` | — | `TaskProvider`, và gọi thẳng `sl<TaskRepository>()`/`sl<CacheManager>()`/`sl<SyncQueue>()` | Form hoàn thành task: giờ chạy máy, phụ tùng thay thế, ghi chú, checklist, ảnh — màn hình phức tạp nhất app |
| | `CreateDeferralScreen` | — | `TaskProvider` | Form xin dời hạn task (lý do, ngày đề xuất, root cause bắt buộc cho task quá hạn) |
| `schedule/` | `ScheduleScreen` | *(nhúng trong Home)* | `TaskProvider` | Lịch task lọc theo `upcoming/week/month/all`, có tab con `WatchScheduleTab` |
| | `WatchScheduleTab` | — | `WatchkeepingProvider` (lạc chuẩn — xem cảnh báo trong `../../README.md`) | Danh sách ca trực (watch log), lọc theo period/type |
| | `WatchLogDetailScreen`, `CreateWatchLogScreen` | — | `WatchkeepingProvider` | Xem / tạo watch log |
| `alarms/` | `AlarmListScreen` | `/alarms` | `AlarmProvider` | Danh sách cảnh báo an toàn đang active |
| | `AlarmDetailScreen` | — | `AlarmProvider` | Chi tiết 1 alarm, acknowledge/resolve |
| | `AlarmHistoryScreen` | `/alarms/history` | `AlarmProvider` | Lịch sử alarm theo số ngày lọc |
| | `AlarmStatisticsScreen` | `/alarms/statistics` | `AlarmProvider` | Thống kê theo severity/type/location |
| `chat/` | `ChatScreen` | `/chat` | `ChatProvider` | Chat hỏi-đáp hàng hải (COLREGs, MARPOL...), có gợi ý câu hỏi |
| `profile/` | `ProfileScreen` | *(nhúng trong Home)* | tự khởi tạo `CrewRepository` (không qua Provider) | Thông tin cá nhân thuyền viên |
| | `CertificatesScreen` | — | tự khởi tạo `CrewRepository` | Chứng chỉ + hạn dùng (màu theo mức cảnh báo) |
| `settings/` | `SettingsScreen` | *(nhúng trong Home)* | `AuthProvider`, `SyncProvider`, `LocaleProvider` | Ngôn ngữ, thông tin tài khoản, trạng thái sync, cấu hình server (hỏng — xem dưới), clear cache, logout |
| | `ServerConfigScreen` | — | *(không — tự đọc/ghi `SharedPreferences`, xem `../../core/storage/README.md`)* | **Không có tác dụng thực tế** — lưu sai chỗ so với nơi `ApiClient` đọc lại |
| | `LanguageSelectionScreen` | — | `LocaleProvider` | Chọn 1 trong 7 ngôn ngữ khai báo — **5/7 sẽ crash khi chọn** (xem `../../core/README.md` và `../../l10n/README.md`) |

## Luồng hoạt động chính

Điều hướng chủ yếu qua `Navigator.push(MaterialPageRoute(...))`, truyền thẳng object đã có sẵn (VD `TaskDetailScreen({required this.task})` nhận trực tiếp 1 `MaintenanceTask` thay vì load lại theo ID) — chỉ **6 route "phẳng"** được đăng ký tên trong `MaterialApp.routes` (`/login`, `/home`, `/alarms`, `/alarms/statistics`, `/alarms/history`, `/chat`); phần lớn điều hướng còn lại (Tasks, Schedule, Profile, Settings...) không dùng named route.

## Liên kết với phần khác

- `presentation/providers/` — hầu hết screen inject qua `Provider.of`/`context.read`/`Consumer`.
- `presentation/widgets/` — `TaskCard`, `StatusBadge`, `PriorityBadge`, `LoadingWidget`, `ErrorDisplayWidget`, `EmptyStateWidget` dùng lặp lại ở nhiều screen.
- `l10n/` — hầu hết text hiển thị qua `AppLocalizations.of(context)`.
- `data/repositories/` — `profile/` gọi thẳng `CrewRepository`, `tasks/complete_task_screen.dart` gọi thẳng `TaskRepository` qua `sl<>()`.

## Ghi chú khi đọc/dạy

- **Bắt đầu đọc từ `tasks/`** khi cần hiểu độ phức tạp thực tế của app — đây là cụm nghiệp vụ chính (PMS Workflow v2.0: start → checklist → submit → deferral); phần còn lại (Alarms, Chat, Schedule) đơn giản hơn nhiều.
- Với dev mới, nên thử luồng thật trên emulator theo thứ tự: Login → Home → Tasks (mở 1 task DUE → Start → tick checklist → Complete) để thấy trực quan toàn bộ optimistic-update + sync-queue hoạt động, thay vì chỉ đọc code.
- `ServerConfigScreen` và `LanguageSelectionScreen` (mục "5/7 ngôn ngữ crash") là 2 điểm nên tránh demo trước khách hàng cho tới khi được sửa.
