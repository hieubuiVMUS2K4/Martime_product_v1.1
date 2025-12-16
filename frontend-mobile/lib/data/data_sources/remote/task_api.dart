import 'package:dio/dio.dart';
import 'package:retrofit/retrofit.dart';
import '../../models/maintenance_task.dart';
import '../../models/start_task_dto.dart';
import '../../models/submit_task_dto.dart';
import '../../models/create_deferral_request_dto.dart';
import '../../models/task_checklist_item.dart';
import '../../models/update_task_checklist_item_request.dart';
import '../../models/complete_task_checklist_item_request.dart';
import '../../models/task_progress.dart';

part 'task_api.g.dart';

@RestApi()
abstract class TaskApi {
  factory TaskApi(Dio dio, {String baseUrl}) = _TaskApi;

  // === CREW TASK LIST ===
  @GET('/api/maintenance/tasks/my-tasks')
  Future<List<MaintenanceTask>> getMyTasks({
    @Query('crewId') String? crewId,
    @Query('includeCompleted') bool? includeCompleted,
  });

  @GET('/api/maintenance/tasks/{id}')
  Future<MaintenanceTask> getTaskById(@Path('id') String id);

  // === WORKFLOW v2.0 (MUST USE) ===
  @GET('/api/tasks/{id}/details')
  @DioResponseType(ResponseType.json)
  Future<HttpResponse<dynamic>> getTaskDetails(@Path('id') String id);

  @POST('/api/tasks/{id}/start')
  @DioResponseType(ResponseType.json)
  Future<HttpResponse<dynamic>> startTask(
    @Path('id') String id,
    @Body() StartTaskDto dto,
  );

  @POST('/api/tasks/{id}/submit')
  @DioResponseType(ResponseType.json)
  Future<HttpResponse<dynamic>> submitTask(
    @Path('id') String id,
    @Body() SubmitTaskDto dto,
  );

  // === CHECKLIST (TaskChecklistItemsController) ===
  // NOTE: taskId here is the task code string (NOT UUID)
  @GET('/api/maintenance/tasks/{taskId}/checklist')
  Future<List<TaskChecklistItem>> getTaskChecklist(@Path('taskId') String taskId);

  @PUT('/api/maintenance/tasks/{taskId}/checklist/{itemId}')
  Future<TaskChecklistItem> updateChecklistItem(
    @Path('taskId') String taskId,
    @Path('itemId') String itemId,
    @Body() UpdateTaskChecklistItemRequest request,
  );

  @POST('/api/maintenance/tasks/{taskId}/checklist/{itemId}/complete')
  Future<TaskChecklistItem> completeChecklistItem(
    @Path('taskId') String taskId,
    @Path('itemId') String itemId,
    @Body() CompleteTaskChecklistItemRequest request,
  );

  @GET('/api/maintenance/tasks/{taskId}/checklist/summary')
  @DioResponseType(ResponseType.json)
  Future<HttpResponse<dynamic>> getChecklistSummary(@Path('taskId') String taskId);

  /// Get task progress
  @GET('/api/maintenance/tasks/{taskId}/progress')
  Future<TaskProgress> getTaskProgress(@Path('taskId') int taskId);

  // === DEFERRALS ===
  @POST('/api/deferral-requests')
  @DioResponseType(ResponseType.json)
  Future<HttpResponse<dynamic>> createDeferralRequest(
    @Body() CreateDeferralRequestDto dto,
  );

  @GET('/api/deferral-requests')
  @DioResponseType(ResponseType.json)
  Future<HttpResponse<dynamic>> getDeferralRequests({
    @Query('status') String? status,
    @Query('taskId') String? taskId,
    @Query('page') int? page,
    @Query('pageSize') int? pageSize,
  });

  @DELETE('/api/deferral-requests/{id}')
  @DioResponseType(ResponseType.json)
  Future<HttpResponse<dynamic>> cancelDeferralRequest(@Path('id') String id);

  @GET('/api/maintenance/tasks/upcoming')
  Future<List<MaintenanceTask>> getUpcomingTasks();
}
