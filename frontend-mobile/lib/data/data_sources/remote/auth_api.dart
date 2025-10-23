import 'package:dio/dio.dart';
import 'package:retrofit/retrofit.dart';
import '../../models/login_request.dart';
import '../../models/login_response.dart';
import '../../models/refresh_token_request.dart';

part 'auth_api.g.dart';

@RestApi()
abstract class AuthApi {
  factory AuthApi(Dio dio, {String baseUrl}) = _AuthApi;

  @POST('/api/auth/login')
  Future<LoginResponse> login(@Body() LoginRequest request);

  @POST('/api/auth/refresh')
  Future<LoginResponse> refreshToken(@Body() RefreshTokenRequest request);

  @POST('/api/auth/logout')
  Future<void> logout();

  @POST('/api/auth/change-password')
  Future<void> changePassword(@Body() Map<String, dynamic> request);
}
