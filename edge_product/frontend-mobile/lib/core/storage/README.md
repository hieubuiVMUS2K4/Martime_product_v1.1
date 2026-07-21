# core/storage — Cấu hình địa chỉ Edge Server

## Mục đích

Cho phép người dùng (hoặc người triển khai) đổi địa chỉ IP/port của Edge Server **ngay trong app**, không cần build lại APK — quan trọng vì mỗi tàu chạy 1 Edge server riêng với IP LAN khác nhau.

## Cấu trúc & vai trò

| File | Lớp | Vai trò |
|---|---|---|
| `server_config_storage.dart` | `ServerConfigStorage` (toàn static method) | Đọc/ghi URL server vào Hive box `server_config`, key `server_url` |

Các hàm chính:

| Hàm | Mô tả |
|---|---|
| `getServerUrl()` | Trả URL đã lưu, mặc định `http://10.0.2.2:5001` (địa chỉ loopback đặc biệt của Android Emulator, trỏ về `localhost` của máy host) |
| `saveServerUrl(url)` | Ghi URL mới vào Hive |
| `resetToDefault()` | Reset về `10.0.2.2:5001` |
| `isValidUrl(url)` | Validate: phải có scheme `http`/`https` và có `host` |
| `getExampleUrls()` | 4 URL mẫu hiển thị trong dialog trợ giúp |

## Luồng hoạt động chính

Có **2 giao diện** cho phép đổi server URL trong app — nhưng chỉ **1 trong 2** thực sự hoạt động:

| Giao diện | Cơ chế lưu | Có gọi `ApiClient.updateBaseUrl()`? | Kết quả |
|---|---|---|---|
| `ServerConfigDialog` (`presentation/widgets/server_config_dialog.dart`), mở từ nút "Server Settings" ở `LoginScreen` | `ServerConfigStorage` (Hive) | **Có** — áp dụng ngay lập tức, không cần khởi động lại app | Hoạt động đúng, kèm nút "Test" gọi `/api/health` trước khi lưu, và tự `logout()` sau khi đổi server |
| `ServerConfigScreen` (`presentation/screens/settings/server_config_screen.dart`), mở từ Settings → "Server Configuration" | `SharedPreferences` key `'api_base_url'` (hoàn toàn khác key/cơ chế của `ServerConfigStorage`) | **Không** | Lưu xong hiện thông báo "Server URL saved successfully! Please restart the app." — nhưng **khởi động lại app cũng không có tác dụng**, vì `ApiClient.initialize()` chỉ đọc từ `ServerConfigStorage`, không bao giờ đọc lại `SharedPreferences['api_base_url']` |

```
LoginScreen ── "Server Settings" ──► ServerConfigDialog ──► ServerConfigStorage.saveServerUrl()
                                                         └─► ApiClient().updateBaseUrl()   ✅ áp dụng ngay

SettingsScreen ── "Server Configuration" ──► ServerConfigScreen ──► SharedPreferences['api_base_url']
                                                                  └─► (không có nơi nào đọc lại giá trị này)   ❌ vô tác dụng
```

## Liên kết với phần khác

- `core/network/api_client.dart` — `initialize()` đọc `ServerConfigStorage.getServerUrl()` lúc app khởi động; đây là nơi DUY NHẤT baseURL thực sự được nạp vào `Dio`.
- `presentation/screens/auth/login_screen.dart` — hiển thị URL hiện tại (đọc từ `ServerConfigStorage`) và mở `ServerConfigDialog`.
- `presentation/screens/settings/server_config_screen.dart` — **phiên bản không hoạt động**, xem Ghi chú.

## Ghi chú khi đọc/dạy

- Đây là ví dụ thực tế rất tốt để dạy về **"2 cách làm 1 việc" (duplicate implementation) dẫn tới tính năng "ma"**: `ServerConfigScreen` trông như hoạt động hoàn chỉnh (có form, có validate URL, có thông báo thành công) nhưng **không có tác dụng thật** vì ghi dữ liệu sai chỗ so với nơi `ApiClient` đọc lại. Nếu được giao sửa bug "đổi server trong Settings không có tác dụng", đây chính là nguyên nhân gốc — cách sửa đúng là cho `ServerConfigScreen` dùng chung `ServerConfigStorage` + gọi `ApiClient().updateBaseUrl()` giống hệt `ServerConfigDialog`, hoặc đơn giản là bỏ bớt 1 trong 2 màn hình trùng chức năng.
- Giá trị mặc định `10.0.2.2:5001` chỉ đúng khi chạy **Android Emulator**; trên thiết bị thật hoặc iOS Simulator phải đổi thủ công sang IP LAN thật của máy đang chạy Edge backend (`dotnet run --urls "http://0.0.0.0:5001"`).
