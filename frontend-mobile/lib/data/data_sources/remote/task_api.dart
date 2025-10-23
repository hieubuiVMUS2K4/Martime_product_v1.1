import 'package:dio/dio.dart';
import 'package:retrofit/retrofit.dart';
import '../../models/maintenance_task.dart';
import '../../models/task_complete_request.dart';

part 'task_api.g.dart';

@RestApi()
abstract class TaskApi {
  factory TaskApi(Dio dio, {String baseUrl}) = _TaskApi;

  @GET('/api/maintenance/tasks/my-tasks')
  Future<List<MaintenanceTask>> getMyTasks();

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
}
