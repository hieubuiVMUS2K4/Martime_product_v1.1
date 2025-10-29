import 'package:dio/dio.dart';
import 'package:retrofit/retrofit.dart';
import '../models/watchkeeping_log.dart';

part 'watchkeeping_api.g.dart';

@RestApi()
abstract class WatchkeepingApi {
  factory WatchkeepingApi(Dio dio, {String baseUrl}) = _WatchkeepingApi;

  @GET('/api/watchkeeping/active')
  Future<List<WatchkeepingLog>> getActiveWatchLogs();

  @GET('/api/watchkeeping/history')
  Future<List<WatchkeepingLog>> getWatchHistory(@Query('days') int days);

  @GET('/api/watchkeeping/{id}')
  Future<WatchkeepingLog> getWatchLogById(@Path('id') int id);

  @POST('/api/watchkeeping/create')
  Future<WatchkeepingLog> createWatchLog(@Body() WatchkeepingLog log);

  @PUT('/api/watchkeeping/{id}/sign')
  Future<WatchkeepingLog> signWatchLog(
    @Path('id') int id,
    @Body() Map<String, String> signature,
  );

  @GET('/api/watchkeeping/officer/{officerName}')
  Future<List<WatchkeepingLog>> getLogsByOfficer(@Path('officerName') String officerName);

  @GET('/api/watchkeeping/period/{period}')
  Future<List<WatchkeepingLog>> getLogsByPeriod(@Path('period') String period);
}
