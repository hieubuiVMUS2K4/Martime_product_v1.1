# data/ — Models, API & Repositories

## Mục đích

Tầng `data/` chịu trách nhiệm: định nghĩa hình dạng dữ liệu (`models/`), gọi Edge API bằng Retrofit (`api/`, `data_sources/remote/`), và điều phối online/offline + chuẩn hoá dữ liệu cho tầng UI (`repositories/`). Trong Clean Architecture rút gọn của app này, **repository đóng luôn vai trò "domain logic"** — không có tầng use-case riêng biệt.

## Cấu trúc & vai trò

| Thư mục | Số file | Vai trò | README chi tiết |
|---|---|---|---|
| `api/` | 2 (`watchkeeping_api.dart` + `.g.dart` sinh tự động) | Định nghĩa Retrofit cho Watchkeeping — **đặt sai chỗ theo quy ước chung** (xem Ghi chú) | — |
| `data_sources/remote/` | 8 (4 API × 2 file `.dart` + `.g.dart`) | Định nghĩa Retrofit cho Alarm, Auth, Crew, Task | — |
| `models/` | 25 (22 model viết tay + 3 file `.g.dart` sinh tự động) | DTO/entity — request, response, domain model | mô tả tại đây |
| `repositories/` | 6 | Điều phối online/offline, cache, sync queue | [`repositories/README.md`](repositories/README.md) |

### API layer (Retrofit) — `api/` + `data_sources/remote/`

Tất cả dùng chung 1 pattern: `abstract class XApi { factory XApi(Dio dio) = _XApi; @GET/@POST(...) ... }`. Code gọi HTTP thật được sinh bởi `retrofit_generator` vào file `.g.dart` (chạy `flutter pub run build_runner build --delete-conflicting-outputs`).

| File | Class | Endpoint chính (tiền tố) |
|---|---|---|
| `data_sources/remote/auth_api.dart` | `AuthApi` | `/api/auth/login`, `/login-legacy`, `/refresh`, `/logout`, `/change-password` |
| `data_sources/remote/crew_api.dart` | `CrewApi` | `/api/crew/me`, `/api/crew/me/certificates` |
| `data_sources/remote/task_api.dart` | `TaskApi` | `/api/maintenance/tasks/*`, `/api/tasks/{id}/start\|submit`, `/api/deferral-requests` — endpoint nhiều & phức tạp nhất (workflow v2.0) |
| `data_sources/remote/alarm_api.dart` | `AlarmApi` | `/api/alarms/active\|history\|statistics\|{id}/acknowledge\|resolve` |
| `api/watchkeeping_api.dart` | `WatchkeepingApi` | `/api/watchkeeping/*` |

### `models/` (25 file) — nhóm theo nghiệp vụ

| Nhóm | File | Ghi chú |
|---|---|---|
| **Auth** | `login_request.dart`, `login_response.dart`, `refresh_token_request.dart` | `LoginResponse.fromJson` tự nhận diện **2 định dạng response khác nhau** (mới: field `user` lồng nhau; cũ: field phẳng) để tương thích ngược với backend |
| **Task / PMS Workflow (đang dùng)** | `maintenance_task.dart`, `task_checklist_item.dart`, `task_progress.dart`, `start_task_dto.dart`, `submit_task_dto.dart`, `task_deferral_request.dart`, `create_deferral_request_dto.dart`, `complete_task_checklist_item_request.dart`, `update_task_checklist_item_request.dart` | `MaintenanceTask` là model lớn nhất (~30 field) — chứa cả field workflow v2 (rejection, deferral, verification) lẫn nhiều getter tính toán (`isOverdue`, `canStart`, `statusColor`, `statusText`...) |
| **Task (mồ côi — đã xác minh không còn được import ở bất kỳ đâu khác ngoài chính file đó)** | `task_detail.dart` (`TaskDetail`), `maintenance_task_detail.dart` (`MaintenanceTaskDetail`), `task_type.dart` (`TaskType`), `task_complete_request.dart` (`TaskCompleteRequest`), `complete_checklist_item_request.dart` (`CompleteChecklistItemRequest`) | 5 file thuộc về 1 phiên bản checklist/task cũ hơn (trước khi có `TaskChecklistItem`/`UpdateTaskChecklistItemRequest` hiện tại) — không bị `data_sources/remote/task_api.dart` hay bất kỳ repository/provider nào tham chiếu. Ứng viên tốt để xoá khi dọn dẹp code, nhưng nên xác nhận lại với backend trước vì có thể phản ánh 1 API cũ vẫn còn tồn tại phía server |
| **Crew** | `crew_member.dart` | Có các getter kiểm tra hạn chứng chỉ/passport/medical (`isCertificateExpiring`, `isCertificateExpired`..., ngưỡng 90 ngày cho chứng chỉ/y tế, 180 ngày cho hộ chiếu) |
| **Alarm** | `safety_alarm.dart` (gồm cả `AlarmStatistics`, `SeverityCount`, `TypeCount`, `LocationCount`) | Dùng `json_serializable` (`@JsonSerializable`, cần `build_runner`) — khác đa số model khác (viết `fromJson`/`toJson` thủ công) |
| **Chat** | `chat_message.dart` | Có field `source` phân biệt `gemini`/`knowledge_base`/`fallback`/`user` — cho thấy backend Chat dùng LLM kèm cơ chế fallback |
| **Sync** | `sync_item.dart` | Model Hive (`@HiveType`) cho hàng đợi đồng bộ — xem `core/cache/README.md` |
| **Watchkeeping** | `watchkeeping_log.dart` | Model khá đầy đủ, bám STCW/SOLAS Chapter V/28/MLC 2006 (rest-hour compliance, GMDSS, fatigue management, bridge manning) — dùng cho module lạc chuẩn, xem Ghi chú |

Đa số model dùng `Equatable` (so sánh theo giá trị) + `fromJson`/`toJson` viết tay; 3 ngoại lệ dùng code-gen: `SafetyAlarm` (`json_serializable`), `SyncItem` và `WatchkeepingLog` (đều có `.g.dart` riêng).

## Luồng hoạt động chính

```
Provider (presentation/providers/*)
     │  gọi
     ▼
Repository (data/repositories/*)
     │
     ├─ Online:  gọi XApi(ApiClient().dio).method() ──► Edge API (port 5001) ──► parse JSON qua Model.fromJson()
     │                                                                                │
     │                                                                                ▼
     │                                                              CacheManager.saveData() để dùng offline sau
     │
     └─ Offline: CacheManager.getData() ──► Model.fromJson() từ cache
                 (nếu là thao tác ghi) SyncQueue.addToQueue(SyncItem(...))
```

## Liên kết với phần khác

- `core/network/` — cung cấp `ApiClient` cho mọi Retrofit API class.
- `core/cache/` — `CacheManager` (đọc) và `SyncQueue` (ghi offline) được hầu hết repository sử dụng, trừ `AlarmRepository`/`ChatRepository`/`CrewRepository` (xem `repositories/README.md`).
- `presentation/providers/*` — tiêu thụ trực tiếp repository (thường 1 provider ↔ 1 repository).

## Ghi chú khi đọc/dạy

- **`data/api/` chỉ có đúng 1 file** (`watchkeeping_api.dart`) trong khi quy ước chung của dự án đặt API Retrofit trong `data/data_sources/remote/`. Đây là dấu hiệu `watchkeeping_api.dart` được thêm sau, không theo đúng convention — cùng với việc `WatchkeepingProvider` cũng nằm sai chỗ (`lib/providers/` thay vì `presentation/providers/`, xem `../README.md`), gần như chắc chắn cả cụm tính năng Watchkeeping (model, api, repository, provider, 3 screen trong `presentation/screens/schedule/`) được phát triển tách rời/muộn hơn và chưa được dọn dẹp theo chuẩn chung của app.
- Một số model có tên rất giống nhau nhưng khác mục đích, dễ nhầm khi tìm kiếm: `complete_checklist_item_request.dart` (field `measuredValue`, `checkResult` — thuộc nhóm model mồ côi, API cũ) so với `complete_task_checklist_item_request.dart` (field `readingValue`, `isAbnormal` — khớp đúng với `TaskApi.completeChecklistItem` hiện đang dùng thật). Khi cần sửa luồng checklist, kiểm tra kỹ đang đọc đúng DTO nào trước khi sửa.
- Xem `repositories/README.md` để hiểu sâu phần quan trọng nhất của tầng data: chiến lược offline-first khác nhau giữa các repository.
