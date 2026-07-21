# lib/ — Toàn cảnh kiến trúc Mobile App (Flutter)

## Mục đích

Đây là thư mục mã nguồn chính của **Maritime Crew App** (tên package `maritime_crew_app`, xem `pubspec.yaml`) — ứng dụng Flutter dành cho thuyền viên trên tàu, đóng vai trò "đầu cuối di động" của **Edge System** trong kiến trúc Edge-Shore của dự án. App gọi trực tiếp Edge API (ASP.NET Core, mặc định cổng **5001**) và bắt buộc phải hoạt động được cả khi tàu **mất kết nối Internet** hàng giờ/hàng ngày giữa biển.

README này giải thích cách `lib/` được tổ chức theo **Clean Architecture rút gọn 2 tầng** — không có tầng "domain" riêng với entity/use-case như Clean Architecture kinh điển. Đây là bài học thực tế tốt: dự án thật thường đơn giản hoá kiến trúc sách vở cho phù hợp quy mô đội ngũ, và ở đây **repository đảm nhiệm luôn vai trò "domain logic"**.

```
presentation/            (UI + state — Provider/ChangeNotifier)
     │  gọi trực tiếp (không qua use-case riêng)
     ▼
data/repositories/        (gộp cả điều phối online/offline lẫn "domain logic")
     │  dùng
     ▼
data/data_sources (remote)  +  core/cache (Hive)  +  core/network (Dio)
     │
     ▼
Edge API (ASP.NET Core, port 5001)   hoặc   Hive local storage khi offline
```

## Cấu trúc & vai trò

| Thư mục / file | Vai trò | README chi tiết |
|---|---|---|
| `main.dart` | Entry point: khởi tạo Hive, đăng ký adapter, mở box, gọi `setupServiceLocator()`, khởi tạo `LocaleProvider`, `runApp()` | — |
| `app.dart` | Khai báo `MyApp`: `MultiProvider` (6 provider) bọc `MaterialApp`, cấu hình localization (7 ngôn ngữ khai báo), theme, `routes` (6 route đặt tên) | — |
| `core/` | Hạ tầng dùng chung: DI, network, cache, storage, auth, hằng số, theme, đa ngôn ngữ | [`core/README.md`](core/README.md) |
| `data/` | Models, API (Retrofit), repositories — lớp truy xuất & chuẩn hoá dữ liệu | [`data/README.md`](data/README.md) |
| `presentation/` | Providers (state) + Screens + Widgets — toàn bộ UI | [`presentation/README.md`](presentation/README.md) |
| `providers/` | **1 file lẻ** `watchkeeping_provider.dart` nằm ngoài `presentation/providers/` — xem Ghi chú bên dưới | — |
| `l10n/` | Chuỗi đa ngôn ngữ sinh tự động bởi `flutter gen-l10n` từ `app_en.arb`/`app_vi.arb` | [`l10n/README.md`](l10n/README.md) |

## Luồng hoạt động chính

1. `main()` khởi tạo Hive (`Hive.initFlutter()`), đăng ký `SyncItemAdapter`/`SyncItemTypeAdapter`, mở 2 box (`cache_box`, `sync_queue`), rồi gọi `await setupServiceLocator()` (đăng ký toàn bộ singleton qua GetIt — xem `core/di/README.md`).
2. `runApp()` bọc `MyApp` bằng `ChangeNotifierProvider<LocaleProvider>` (đã gọi `initialize()` để đọc ngôn ngữ đã lưu từ `SharedPreferences`).
3. `app.dart` tạo `MultiProvider` với **6 provider**: `AuthProvider`, `TaskProvider`, `SyncProvider`, `AlarmProvider(sl<AlarmRepository>())`, `WatchkeepingProvider()`, `ChatProvider(sl<ChatRepository>())` — mỗi provider tự lấy dependency qua Service Locator (`sl<T>()`) theo 2 kiểu khác nhau (xem `presentation/providers/README.md`).
4. `MaterialApp` khởi động ở `LoginScreen` (`home: const LoginScreen()`). Sau khi đăng nhập thành công (`AuthProvider.login()` → `AuthRepository` → Edge API `/api/auth/login`), điều hướng tới `HomeScreen` — nơi chứa `Drawer` dẫn tới Tasks, Schedule, Profile, Settings.
5. Mọi thao tác dữ liệu đi qua **Provider → Repository → (ApiClient.dio hoặc Retrofit API) → Edge API**; khi mất mạng, Repository tự chuyển sang đọc `CacheManager` (Hive) hoặc ghi vào `SyncQueue` để đồng bộ sau — đây là **trục xương sống offline-first** của toàn app (xem `core/cache/README.md` và `data/repositories/README.md`).

## Liên kết với phần khác

- App này là **Mobile App** trong kiến trúc Edge-Shore (mục 4.3 README gốc của dự án) — chỉ nói chuyện với **Edge Backend** (`edge_product/edge-services`, cổng 5001), **không** gọi thẳng Shore.
- Dữ liệu tạo ra qua Edge Backend sẽ được đồng bộ tiếp lên Shore qua giao thức Sync riêng của Edge backend (`SyncBackgroundWorker`) — mobile app chỉ là client của Edge API, không tham gia trực tiếp giao thức Edge↔Shore.
- Toàn bộ endpoint app gọi đều nằm trong `edge-services/Controllers` (xem `core/network/README.md` để biết chi tiết baseURL).

## Ghi chú khi đọc/dạy

- **Không có tầng "domain" tách biệt**: khác với Clean Architecture sách vở (presentation → domain → data), dự án này gộp business rule vào thẳng `data/repositories/*` và vào các getter tính toán trong model (VD: `MaintenanceTask.canStart`, `isOverdue`). Khi dạy, nhấn mạnh đây là lựa chọn thực dụng (pragmatic), không phải thiếu sót cần "sửa cho đúng sách vở".
- **`lib/providers/watchkeeping_provider.dart` là module lạc chuẩn (legacy/tech debt) — ví dụ dạy học rất tốt**: nó nằm ngoài `presentation/providers/` dù đóng vai trò state-management y hệt 5 provider còn lại, và vẫn được đăng ký sống trong `MultiProvider` ở `app.dart`. Nó tự tạo `Dio` riêng trỏ tới `http://localhost:5000` — **đây là cổng của Shore backend theo README gốc dự án, không phải Edge (5001)** — nên tính năng Watchkeeping (`presentation/screens/schedule/watch_schedule_tab.dart` và 2 màn hình liên quan) gần như chắc chắn không gọi được Edge API thật khi chạy đúng theo cấu hình mặc định của repo. Nó cũng hoàn toàn không dùng GetIt (`core/di`). Đáng chú ý là model dữ liệu đứng sau nó — `WatchkeepingLog` (`data/models/watchkeeping_log.dart`) — lại được thiết kế khá kỹ, bám sát SOLAS Chapter V/28, STCW và MLC 2006 (rest-hour compliance, GMDSS, fatigue management...); vấn đề nằm ở phần **wiring** (provider đặt sai chỗ, sai cổng, ngoài DI), không phải ở chất lượng model. Đây là bài học tốt: "code có vấn đề" và "thiết kế dữ liệu tốt" có thể tồn tại trong cùng 1 tính năng.
- **File `README.md` ở gốc `frontend-mobile/`** (ngoài `lib/`) là hướng dẫn cài đặt/setup (Flutter SDK, build APK, chuyển từ mock sang backend thật...) và đã hơi lỗi thời (còn nhắc "mock data", "Settings coming soon" dù các tính năng đó đã được implement thật). README trong `lib/` (file này và các file con) tập trung vào **kiến trúc & luồng dữ liệu thực tế của code hiện tại**, không lặp lại hướng dẫn cài đặt.
- Base URL mặc định để chạy thử: Android Emulator dùng `10.0.2.2:5001` (địa chỉ loopback đặc biệt trỏ về `localhost` của máy host); thiết bị thật/iOS Simulator cần đổi thủ công sang IP LAN của máy chạy Edge backend, cấu hình qua nút "Server Settings" ở màn hình Login (xem `core/storage/README.md` — có 1 màn hình cấu hình server thứ hai trong Settings **không hoạt động**, cũng được giải thích ở đó).
