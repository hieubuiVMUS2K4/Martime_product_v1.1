import 'package:dio/dio.dart';
import '../auth/token_storage.dart';

class ApiInterceptor extends Interceptor {
  final TokenStorage _tokenStorage = TokenStorage();
  
  @override
  void onRequest(RequestOptions options, RequestInterceptorHandler handler) async {
    // Add JWT token to header if exists
    final token = await _tokenStorage.getAccessToken();
    if (token != null) {
      options.headers['Authorization'] = 'Bearer $token';
    }
    
    super.onRequest(options, handler);
  }
  
  @override
  void onError(DioException err, ErrorInterceptorHandler handler) async {
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
    
    super.onError(err, handler);
  }
  
  Future<bool> _refreshToken() async {
    // TODO: Implement token refresh logic
    // This should call /api/auth/refresh endpoint
    return false;
  }
}
