# Mobile Code Flow Trace

Tài liệu này giải thích luồng code chính của app mobile trong `edge_product/frontend-mobile`, đi từ màn hình Flutter -> Provider/Repository -> API client -> backend ASP.NET -> database.

## 1. Cấu trúc lớp mobile

Mobile đang đi theo mô hình khá rõ:

- UI/screen: `edge_product/frontend-mobile/lib/presentation/screens/...`
- Provider/state: `edge_product/frontend-mobile/lib/presentation/providers/...`
- Repository: `edge_product/frontend-mobile/lib/data/repositories/...`
- Remote API Retrofit/Dio: `edge_product/frontend-mobile/lib/data/data_sources/remote/...`
- Network/token/cache/sync queue: `edge_product/frontend-mobile/lib/core/...`
- Model DTO: `edge_product/frontend-mobile/lib/data/models/...`

Nói ngắn gọn:

```text
Screen -> Provider -> Repository -> Retrofit API -> Dio -> ASP.NET Controller -> DbContext -> Database
```

## 2. Cấu hình API gốc và token

### API base URL

File: `edge_product/frontend-mobile/lib/core/network/api_client.dart`

- Dòng 21: khởi tạo `Dio`.
- Dòng 34-36: gắn interceptor gồm `ApiInterceptor` và logger.
- Dòng 56-58: `updateBaseUrl()` đổi base URL runtime:

```dart
void updateBaseUrl(String newBaseUrl) {
  _dio.options.baseUrl = newBaseUrl;
  ApiConstants.baseUrl = newBaseUrl;
}
```

Khi đổi server trong app mobile, Dio sẽ gọi API theo base URL mới.

### Token storage

File: `edge_product/frontend-mobile/lib/core/auth/token_storage.dart`

- Dòng 14: `saveTokens(...)` lưu `accessToken`, `refreshToken`, `userId`, `crewId`.
- Dòng 27: ghi access token vào secure storage.
- Dòng 62: `getAccessToken()` đọc token.
- Dòng 77: `getCrewId()` đọc mã thuyền viên.

### Gắn Authorization header

File: `edge_product/frontend-mobile/lib/core/network/api_interceptor.dart`

- Dòng 19: thêm header:

```dart
options.headers['Authorization'] = 'Bearer $token';
```

Vì vậy mọi request mobile đi qua Dio sẽ tự kèm token nếu đã đăng nhập.

## 3. Flow đăng nhập mobile

### UI bấm đăng nhập

File: `edge_product/frontend-mobile/lib/presentation/screens/auth/login_screen.dart`

- Dòng 52: hàm `_login()`.
- Dòng 57: gọi `authProvider.login(...)`.
- Dòng 63: nếu thành công thì chuyển sang `/home`.

```text
LoginScreen._login()
  -> AuthProvider.login()
  -> AuthRepository.login()
  -> AuthApi.login()
  -> POST /api/auth/login
```

### Provider

File: `edge_product/frontend-mobile/lib/presentation/providers/auth_provider.dart`

- Dòng 56: `Future<bool> login(...)`.

Provider nhận username/password từ UI, gọi repository, sau đó cập nhật state đăng nhập.

### Repository

File: `edge_product/frontend-mobile/lib/data/repositories/auth_repository.dart`

- Dòng 42: nhận `accessToken` từ response.
- Dòng 49-50: gọi `_tokenStorage.saveTokens(...)`.
- Dòng 69-70: bắt lỗi `DioException`.

### API interface

File: `edge_product/frontend-mobile/lib/data/data_sources/remote/auth_api.dart`

- Dòng 14: endpoint `@POST('/api/auth/login')`.
- Dòng 15: hàm `login(@Body() LoginRequest request)`.

## 4. Flow tải danh sách công việc của tôi

### UI danh sách task

File: `edge_product/frontend-mobile/lib/presentation/screens/tasks/task_list_screen.dart`

- Dòng 363: render `TaskCard`.
- Dòng 365: xử lý `onTap`.
- Dòng 388: chặn bấm task đã hoàn thành.
- Dòng 399-408: điều hướng:
  - task chưa bắt đầu -> `TaskDetailScreen`
  - task đang chạy hoặc sâu hơn -> `CompleteTaskScreen`

### Provider lấy task

File: `edge_product/frontend-mobile/lib/presentation/providers/task_provider.dart`

- Dòng 149: `_tasks = await _taskRepository.getMyTasks(...)`.
- Dòng 80-135: các getter lọc task theo trạng thái: chưa bắt đầu, sắp tới, đang làm, quá hạn, hoàn thành...

### Repository lấy task

File: `edge_product/frontend-mobile/lib/data/repositories/task_repository.dart`

- Dòng 54: `getMyTasks(...)`.
- Dòng 57: lấy `crewId` từ `TokenStorage`.
- Dòng 67: gọi `_taskApi.getMyTasks(...)`.
- Dòng 75: lưu danh sách task vào cache.
- Dòng 83-87: nếu offline hoặc lỗi thì đọc lại cache.

### API mobile

File: `edge_product/frontend-mobile/lib/data/data_sources/remote/task_api.dart`

- Dòng 19: `@GET('/api/maintenance/tasks/my-tasks')`.

### Backend nhận request

File: `edge_product/edge-services/Controllers/Maintenance/MaintenanceController.cs`

- Dòng 320: `[HttpGet("tasks/my-tasks")]`.
- Dòng 322: `GetMyTasks(...)`.
- Dòng 333: cảnh báo nếu không có `crewId` hoặc `assignedTo`.
- Dòng 389: select field tối ưu để trả về mobile.
- Dòng 469: chỉ include checklist summary, không kéo toàn bộ checklist nặng.

Backend đọc bảng chính:

- `maintenance_tasks`
- `task_checklist_items`, dùng để tính summary/checklist count.

## 5. Flow mở chi tiết công việc

### UI

File: `edge_product/frontend-mobile/lib/presentation/screens/tasks/task_detail_screen.dart`

- Dòng 40: class `TaskDetailScreen`.
- Dòng 2182-2185: bấm sang màn hình báo cáo hoàn thành `CompleteTaskScreen`.

### Provider

File: `edge_product/frontend-mobile/lib/presentation/providers/task_provider.dart`

- Dòng 231: `return await _taskRepository.getTaskDetails(id);`

### Repository

File: `edge_product/frontend-mobile/lib/data/repositories/task_repository.dart`

- Dòng 145: `getTaskDetails(String id)`.
- Dòng 150: gọi `_taskApi.getTaskDetails(id)`.

### API mobile

File: `edge_product/frontend-mobile/lib/data/data_sources/remote/task_api.dart`

- Dòng 29: `@GET('/api/tasks/{id}/details')`.

### Backend

File: `edge_product/edge-services/Controllers/Maintenance/TaskWorkflowController.cs`

- Dòng 14: route gốc `[Route("api/tasks")]`.
- Dòng 607: `[HttpGet("{id:guid}/details")]`.
- Dòng 608: `GetTaskDetails(Guid id)`.
- Dòng 612: query `_context.MaintenanceTasks`.

## 6. Flow bắt đầu công việc

### UI

File: `edge_product/frontend-mobile/lib/presentation/screens/tasks/task_detail_screen.dart`

- Dòng 2042 và 2127: gọi `taskProvider.startTask(widget.task.id)`.

### Provider

File: `edge_product/frontend-mobile/lib/presentation/providers/task_provider.dart`

- Dòng 180: `startTask(String taskId)`.
- Dòng 186: gọi `_taskRepository.startTask(taskId)`.

### Repository

File: `edge_product/frontend-mobile/lib/data/repositories/task_repository.dart`

- Dòng 158: `startTask(String taskId)`.
- Dòng 165: gọi `_taskApi.startTask(taskId, dto)`.
- Dòng 167-169: clear cache checklist/progress để lần sau refresh.

### API mobile

File: `edge_product/frontend-mobile/lib/data/data_sources/remote/task_api.dart`

- Dòng 33: `@POST('/api/tasks/{id}/start')`.
- Dòng 35: `startTask(...)`.

### Backend

File: `edge_product/edge-services/Controllers/Maintenance/TaskWorkflowController.cs`

- Dòng 36: `[HttpPost("{id:guid}/start")]`.
- Dòng 37: `StartTask(...)`.
- Dòng 47: query `_context.MaintenanceTasks`.
- Dòng 67 và 115: `SaveChangesAsync()`.

Backend cập nhật các field trong `maintenance_tasks`, ví dụ status, started time, started by.

## 7. Flow tick checklist

### UI

File: `edge_product/frontend-mobile/lib/presentation/screens/tasks/task_detail_screen.dart`

- Dòng 1673: gọi `taskProvider.completeChecklistItem(...)`.

File: `edge_product/frontend-mobile/lib/presentation/screens/tasks/complete_task_screen.dart`

- Dòng 1260 và 1390: cũng gọi `taskProvider.completeChecklistItem(...)` từ màn hình báo cáo.

### Provider

File: `edge_product/frontend-mobile/lib/presentation/providers/task_provider.dart`

- Dòng 268: `completeChecklistItem(...)`.
- Dòng 278: gọi repository.

### Repository

File: `edge_product/frontend-mobile/lib/data/repositories/task_repository.dart`

- Dòng 306: comment nói rõ đây là offline-first.
- Dòng 316: lấy `crewId`.
- Dòng 324-335: update cache trước để UI phản hồi ngay.
- Dòng 350-353: nếu offline thì add vào sync queue với type `checklistComplete`.
- Dòng 367-387: nếu lỗi API thì vẫn đưa vào sync queue, không throw ra làm vỡ UI.

### API mobile

File: `edge_product/frontend-mobile/lib/data/data_sources/remote/task_api.dart`

- Dòng 59: `@POST('/api/maintenance/tasks/{taskId}/checklist/{itemId}/complete')`.
- Dòng 60: `completeChecklistItem(...)`.

### Backend

File: `edge_product/edge-services/Controllers/Maintenance/TaskChecklistItemsController.cs`

- Dòng 9: route gốc `[Route("api/maintenance/tasks/{taskId}/checklist")]`.
- Dòng 142: endpoint complete checklist.
- Dòng 148: nhận `CompleteChecklistItemRequest`.
- Dòng 152: query `_context.TaskChecklistItems`.
- Dòng 178: `SaveChangesAsync()`.

Backend cập nhật bảng:

- `task_checklist_items`

## 8. Flow báo cáo hoàn thành công việc

### UI nhập giờ chạy, vật tư, ghi chú

File: `edge_product/frontend-mobile/lib/presentation/screens/tasks/complete_task_screen.dart`

- Dòng 37: `_runningHoursController`.
- Dòng 62: `_actuallyUsedSpareParts`.
- Dòng 73: prefill running hours từ task.
- Dòng 388-390: serialize vật tư đã dùng thành JSON.
- Dòng 403-406: gọi `taskProvider.submitTask(...)` với:
  - `runningHours`
  - `sparePartsUsed`

Guard giờ chạy:

- Dòng 792: input running hours.
- Dòng 812-814: kiểm tra không cho nhập thấp hơn giờ hiện tại.
- Dòng 356-359: map lỗi `RUNNING_HOURS_BELOW_CURRENT` thành thông báo đúng.

### Provider

File: `edge_product/frontend-mobile/lib/presentation/providers/task_provider.dart`

- Dòng 199: `submitTask(...)`.
- Dòng 211-214: truyền xuống repository:

```dart
await _taskRepository.submitTask(
  taskId: taskId,
  completedRunningHours: runningHours,
  sparePartsUsed: sparePartsUsed,
)
```

### Repository

File: `edge_product/frontend-mobile/lib/data/repositories/task_repository.dart`

- Dòng 176: `submitTask(...)`.
- Dòng 186: tạo `SubmitTaskDto`.
- Dòng 194: online thì gọi `_taskApi.submitTask(...)`.
- Dòng 197-199: offline thì add sync queue type `taskSubmit`.
- Dòng 207: bắt `DioException`.
- Dòng 219-222: lỗi network/server có thể retry thì đưa vào sync queue.

### DTO mobile

File: `edge_product/frontend-mobile/lib/data/models/submit_task_dto.dart`

- Dòng 7: `sparePartsUsed`.
- Dòng 9: `completedRunningHours`.
- Dòng 27: ghi `sparePartsUsed` vào JSON.
- Dòng 29: ghi `completedRunningHours` vào JSON.

### API mobile

File: `edge_product/frontend-mobile/lib/data/data_sources/remote/task_api.dart`

- Dòng 40: `@POST('/api/tasks/{id}/submit')`.
- Dòng 42: `submitTask(...)`.

### Backend nhận submit

File: `edge_product/edge-services/Controllers/Maintenance/TaskWorkflowController.cs`

- Dòng 146: `[HttpPost("{id:guid}/submit")]`.
- Dòng 147: `SubmitTask(...)`.
- Dòng 154: query `_context.MaintenanceTasks`.
- Dòng 172: kiểm tra checklist đã hoàn thành chưa.
- Dòng 245: trả lỗi code `RUNNING_HOURS_BELOW_CURRENT` nếu giờ chạy gửi lên thấp hơn giờ hiện tại.
- Dòng 271: `SaveChangesAsync()`.

### Backend trừ vật tư và cập nhật lịch bảo trì

File: `edge_product/edge-services/Services/Maintenance/MaintenanceCompletionService.cs`

- Dòng 36: `CompleteTaskAsync(...)`.
- Dòng 43: mở transaction.
- Dòng 78: lặp qua `sparePartsUsed`.
- Dòng 80: query `_context.MaterialItems`.
- Dòng 91: query `_context.InventoryStocks`.
- Dòng 113: trừ kho từ các location.
- Dòng 126: đồng bộ lại `MaterialItem.OnHandQuantity`.
- Dòng 132-135: fallback nếu chưa có `InventoryStock`.
- Dòng 241: gọi `GenerateNextCycleTask(...)` nếu cần sinh chu kỳ kế tiếp.
- Dòng 246: `SaveChangesAsync()`.
- Dòng 247: `CommitAsync()`.

Backend tác động chính tới các bảng:

- `maintenance_tasks`
- `task_status_history`
- `material_items`
- `inventory_stock`
- có thể sinh task chu kỳ mới vào `maintenance_tasks`

## 9. Flow đồng bộ offline trên mobile

### Khi thao tác thất bại/offline

Repository không vứt mất dữ liệu. Nó ghi vào queue cục bộ:

File: `edge_product/frontend-mobile/lib/data/repositories/task_repository.dart`

- Dòng 197-199: queue `taskSubmit`.
- Dòng 350-353: queue `checklistComplete`.
- Dòng 493-495: queue `deferralCreate`.
- Dòng 661-663: queue `sparePartsSync`.

### SyncQueue lưu và xử lý lại

File: `edge_product/frontend-mobile/lib/core/cache/sync_queue.dart`

- Dòng 22: box cache tên `sync_queue`.
- Dòng 40: `addToQueue(...)`.
- Dòng 55: `processSyncQueue(...)`.
- Dòng 133-136: xử lý từng loại item.
- Dòng 154-159: sync `sparePartsSync`.
- Dòng 164-172: sync `taskSubmit`.
- Dòng 184-187: sync `taskStart`.
- Dòng 234: bắt `DioException`.
- Dòng 242: nếu backend trả `RUNNING_HOURS_BELOW_CURRENT` thì xử lý lỗi domain, không coi là lỗi offline chung.

### SyncProvider tự động sync

File: `edge_product/frontend-mobile/lib/presentation/providers/sync_provider.dart`

- Dòng 33: init sync queue.
- Dòng 43-47: khi online lại và queue có item thì auto sync.
- Dòng 57-59: periodic sync.
- Dòng 89: `syncQueue()`.
- Dòng 97: gọi `_syncQueue.processSyncQueue()`.

## 10. Flow đồng bộ vật tư đang dùng theo thời gian thực

Khi crew chọn vật tư đã dùng trong lúc làm task, mobile có thể sync tạm thời để web thấy tiến độ.

### Mobile

File: `edge_product/frontend-mobile/lib/data/repositories/task_repository.dart`

- Dòng 647: hàm sync vật tư đã dùng.
- Dòng 652-657: encode `sparePartsUsed` rồi gọi API.
- Dòng 661-663: offline thì queue type `sparePartsSync`.

File: `edge_product/frontend-mobile/lib/data/data_sources/remote/task_api.dart`

- Dòng 76-79: API update spare parts used.

### Backend

File: `edge_product/edge-services/Controllers/Maintenance/TaskRealTimeUpdatesController.cs`

- Dòng 10: route gốc `[Route("api/maintenance/tasks/{taskId}")]`.
- Dòng 28: `[HttpPut("spare-parts")]`.
- Dòng 47: `SaveChangesAsync()`.

Backend cập nhật field `spare_parts_used` hoặc thông tin liên quan trên `maintenance_tasks` để web dashboard/chi tiết task đọc được.

## 11. Luồng tổng quát khi bảo vệ

Nếu giảng viên hỏi: "Mobile hoàn thành bảo trì thì dữ liệu đi thế nào?"

Trả lời ngắn:

```text
CompleteTaskScreen
  -> TaskProvider.submitTask
  -> TaskRepository.submitTask
  -> TaskApi.submitTask: POST /api/tasks/{id}/submit
  -> TaskWorkflowController.SubmitTask
  -> MaintenanceCompletionService.CompleteTaskAsync
  -> DbContext: MaintenanceTasks, InventoryStocks, MaterialItems
  -> SaveChangesAsync + Commit transaction
```

Nếu offline:

```text
Repository bắt lỗi network
  -> SyncQueue.addToQueue
  -> SyncProvider thấy online lại
  -> SyncQueue.processSyncQueue
  -> gọi lại đúng TaskApi endpoint
```

Điểm quan trọng: mobile không ghi thẳng database. Mobile chỉ gọi API backend. Backend mới dùng `_context` / repository để query và `SaveChangesAsync()` để ghi database.
