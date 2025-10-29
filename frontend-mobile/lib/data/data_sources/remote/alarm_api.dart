import 'package:dio/dio.dart';
import 'package:retrofit/retrofit.dart';
import '../../models/safety_alarm.dart';

part 'alarm_api.g.dart';

@RestApi()
abstract class AlarmApi {
  factory AlarmApi(Dio dio, {String baseUrl}) = _AlarmApi;

  @GET('/api/alarms/active')
  Future<List<SafetyAlarm>> getActiveAlarms();

  @GET('/api/alarms/history')
  Future<List<SafetyAlarm>> getAlarmHistory({
    @Query('days') int days = 7,
  });

  @POST('/api/alarms/{id}/acknowledge')
  Future<void> acknowledgeAlarm(
    @Path('id') int id,
    @Body() Map<String, String> request,
  );

  @POST('/api/alarms/{id}/resolve')
  Future<void> resolveAlarm(@Path('id') int id);

  @GET('/api/alarms/statistics')
  Future<AlarmStatistics> getStatistics({
    @Query('days') int days = 30,
  });

  @POST('/api/alarms/test/generate-sample')
  @DioResponseType(ResponseType.json)
  Future<HttpResponse<void>> generateSampleAlarms();
}
