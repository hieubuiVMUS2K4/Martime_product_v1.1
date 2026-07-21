# core/network — Lớp giao tiếp HTTP (Dio + Retrofit)

## Mục đích

Định nghĩa & cấu hình client HTTP dùng chung để gọi Edge API (ASP.NET Core, mặc định cổng **5001**): base URL có thể đổi lúc runtime, tự gắn JWT + header định danh thiết bị vào mọi request, log request/response, và phát hiện tình trạng kết nối mạng để các Repository quyết định online/offline.

## Cấu trúc & vai trò

| File | Vai trò |
|---|---|
| `api_client.dart` | `ApiClient` — singleton bọc `Dio`, đọc/ghi base URL |
| `api_interceptor.dart` | `ApiInterceptor extends Interceptor` — gắn header, xử lý lỗi 401 |
| `network_info.dart` | `NetworkInfo` — bọc `connectivity_plus`, expose `isConnected` và stream `onConnectivityChanged` |

### `ApiClient`

- Factory constructor (`factory ApiClient()`) giữ 1 `_instance` tĩnh → **singleton dù không đăng ký qua GetIt cũng vẫn an toàn** (nhưng vẫn nên lấy qua `sl<ApiClient>()` để nhất quán với phần còn lại của app).
- `_initDio()` tạo `Dio` với `baseUrl: ApiConstants.baseUrl`, `connectTimeout`/`receiveTimeout` = 60 giây (khá dài, cố ý để chịu được việc upload ảnh trong deferral request), và 2 interceptor theo đúng thứ tự: `ApiInterceptor()` rồi `PrettyDioLogger()` (log gọn, có màu, request/response).
- `initialize()` (gọi 1 lần trong `setupServiceLocator()`) đọc `ServerConfigStorage.getServerUrl()` (Hive) rồi gọi `updateBaseUrl()` — **đây là baseUrl thực sự được dùng lúc app chạy**, ghi đè giá trị tĩnh ban đầu của `ApiConstants.baseUrl`.
- `updateBaseUrl(String newBaseUrl)` set cả `_dio.options.baseUrl` lẫn `ApiConstants.baseUrl` — dùng khi người dùng đổi server qua `ServerConfigDialog`.

### `ApiInterceptor`

- `onRequest`: đọc `crew_id` và `access_token` từ `TokenStorage` (tự tạo instance `TokenStorage()` riêng, không qua `sl`), gắn header `X-User-Id`, `X-Device-Type: MOBILE`, `Authorization: Bearer <token>`. Có `print()` log 20 ký tự đầu của token ra console — cần lưu ý khi bàn giao build demo/production vì lộ 1 phần token vào log.
- `onError`: nếu gặp `401`, gọi `_refreshToken()` để thử làm mới token rồi retry request — nhưng **`_refreshToken()` hiện chỉ là `TODO`, luôn `return false`**. Do đó mọi lỗi 401 hiện nay đều rơi vào nhánh `else` (xoá token cục bộ; dòng "Navigate to login screen" cũng mới chỉ là comment, chưa code thật). Nghĩa là **refresh-token tự động chưa hoạt động ở tầng interceptor** — refresh chỉ tồn tại như 1 method riêng (`AuthRepository.refreshToken()`) phải được gọi thủ công từ nơi khác trong app.

### `NetworkInfo`

- `isConnected` trả `true` nếu `ConnectivityResult` hiện tại chứa `wifi`, `ethernet`, hoặc `mobile`. Lưu ý: đây chỉ là **kiểm tra có kết nối vật lý** (đã bắt được sóng WiFi/4G), **không kiểm tra Edge server có phản hồi hay không** — 1 thiết bị có thể được coi là "online" theo định nghĩa này dù Edge server đang tắt hoặc app đang trỏ sai IP.

## Luồng hoạt động chính

```
Repository → ApiClient (lấy qua sl<ApiClient>()) → .dio → Retrofit API class (VD: TaskApi)
                 │                                              │
                 ▼                                              ▼
        ApiInterceptor gắn JWT + header                gọi Edge API port 5001
                 │
                 ▼
        PrettyDioLogger in log request/response ra console
```

Trước khi gọi API, hầu hết Repository tự kiểm tra `await networkInfo.isConnected` để quyết định gọi API hay đọc cache/queue offline — xem `../../data/repositories/README.md`.

## Liên kết với phần khác

- `core/di/service_locator.dart` — đăng ký `ApiClient`, `NetworkInfo` là singleton, và gọi `ApiClient().initialize()` cuối cùng.
- `core/storage/server_config_storage.dart` — nguồn sự thật cho base URL thực tế lúc runtime.
- `core/auth/token_storage.dart` — nguồn JWT cho interceptor.
- `data/data_sources/remote/*` và `data/api/watchkeeping_api.dart` — các interface Retrofit dùng `ApiClient().dio` làm client nền.

## Ghi chú khi đọc/dạy

- **`ApiConstants.baseUrl` (giá trị code cứng `http://192.168.1.81:5001`) hầu như không bao giờ là baseUrl thực tế lúc app chạy** — nó luôn bị `ApiClient.initialize()` ghi đè bằng giá trị lấy từ `ServerConfigStorage` (mặc định `http://10.0.2.2:5001` cho Android Emulator). Dev mới rất dễ nhầm khi grep thấy `192.168.1.81` trong code và tưởng đó là URL đang thực sự được dùng.
- Refresh-token tự động ở `ApiInterceptor` **chưa hoàn thiện** (luôn trả `false`) — đây là khoảng trống thật trong code, nên nêu rõ nếu có nhiệm vụ liên quan tới xử lý phiên đăng nhập hết hạn.
- `print()` dùng để log JWT token và crew_id ra console (kể cả 1 phần token) ở nhiều nơi trong `core/network` và `data/repositories` — cân nhắc dọn dẹp trước khi bàn giao bản demo/production cho khách hàng thật.
