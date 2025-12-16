import 'package:dio/dio.dart';
import 'package:retrofit/retrofit.dart';
import '../../models/crew_member.dart';

part 'crew_api.g.dart';

@RestApi()
abstract class CrewApi {
  factory CrewApi(Dio dio, {String baseUrl}) = _CrewApi;

  @GET('/api/crew/me')
  Future<CrewMember> getMyProfile(@Query('crewId') String crewId);

  @GET('/api/crew/me/certificates')
  Future<HttpResponse<dynamic>> getMyCertificates(@Query('crewId') String crewId);
}
