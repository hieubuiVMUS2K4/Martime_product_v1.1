import 'package:dio/dio.dart';
import 'package:retrofit/retrofit.dart';
import '../../models/maintenance_task.dart';
import '../../models/task_complete_request.dart';
import '../../models/task_checklist_item.dart';
import '../../models/task_progress.dart';
import '../../models/complete_checklist_item_request.dart';

part 'task_api.g.dart';

@RestApi()
abstract class TaskApi {
  factory TaskApi(Dio dio, {String baseUrl}) = _TaskApi;

  /// Get tasks assigned to specific crew member
  @GET('/api/maintenance/tasks/my-tasks')
  Future<List<MaintenanceTask>> getMyTasks({
    @Query('crewId') String? crewId,
    @Query('includeCompleted') bool? includeCompleted,
  });

  @GET('/api/maintenance/tasks/{id}')
  Future<MaintenanceTask> getTaskById(@Path('id') int id);

  @POST('/api/maintenance/tasks/{id}/start')
  Future<MaintenanceTask> startTask(@Path('id') int id);

  @POST('/api/maintenance/tasks/{id}/complete')
  Future<MaintenanceTask> completeTask(
    @Path('id') int id,
    @Body() TaskCompleteRequest request,
  );

  @GET('/api/maintenance/tasks/upcoming')
  Future<List<MaintenanceTask>> getUpcomingTasks();

  /// Get task checklist with execution status
  @GET('/api/maintenance/tasks/{taskId}/checklist')
  Future<List<TaskChecklistItem>> getTaskChecklist(@Path('taskId') int taskId);

  /// Complete a checklist item
  @POST('/api/maintenance/tasks/{taskId}/checklist/{detailId}/complete')
  Future<void> completeChecklistItem(
    @Path('taskId') int taskId,
    @Path('detailId') int detailId,
    @Body() CompleteChecklistItemRequest request,
  );

  /// Get task progress
  @GET('/api/maintenance/tasks/{taskId}/progress')
  Future<TaskProgress> getTaskProgress(@Path('taskId') int taskId);
}
