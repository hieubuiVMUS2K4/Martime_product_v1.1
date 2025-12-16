import 'package:dio/dio.dart';
import '../auth/token_storage.dart';

class ApiInterceptor extends Interceptor {
  final TokenStorage _tokenStorage = TokenStorage();
  
  @override
  Future<void> onRequest(RequestOptions options, RequestInterceptorHandler handler) async {
    // Required edge-services headers
    final crewId = await _tokenStorage.getCrewId();
    if (crewId != null && crewId.isNotEmpty) {
      options.headers['X-User-Id'] = crewId;
    }
    options.headers['X-Device-Type'] = 'MOBILE';

    // Add JWT token to header if exists
    final token = await _tokenStorage.getAccessToken();
    if (token != null) {
      options.headers['Authorization'] = 'Bearer $token';
      print('🔑 ApiInterceptor: Added Authorization header with token: ${token.substring(0, 20)}...');
    } else {
      print('⚠️ ApiInterceptor: No token found!');
    }
    
    handler.next(options);
  }
  
  @override
  Future<void> onError(DioException err, ErrorInterceptorHandler handler) async {
    // Handle 401 Unauthorized - Refresh token or logout
    if (err.response?.statusCode == 401) {
      // Try to refresh token
      final refreshed = await _refreshToken();
      if (refreshed) {
        // Retry the request
        final opts = err.requestOptions;
        final token = await _tokenStorage.getAccessToken();
        opts.headers['Authorization'] = 'Bearer $token';
        
        try {
          final response = await Dio().fetch(opts);
          return handler.resolve(response);
        } catch (e) {
          return handler.reject(err);
        }
      } else {
        // Logout user
        await _tokenStorage.clearTokens();
        // Navigate to login screen
      }
    }
    
    handler.next(err);
  }
  
  Future<bool> _refreshToken() async {
    // TODO: Implement token refresh logic
    // This should call /api/auth/refresh endpoint
    return false;
  }
}
