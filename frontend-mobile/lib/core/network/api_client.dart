import 'package:dio/dio.dart';
import 'package:pretty_dio_logger/pretty_dio_logger.dart';
import '../constants/api_constants.dart';
import '../storage/server_config_storage.dart';
import 'api_interceptor.dart';

class ApiClient {
  late Dio _dio;
  static ApiClient? _instance;
  
  ApiClient._internal() {
    _initDio();
  }
  
  factory ApiClient() {
    _instance ??= ApiClient._internal();
    return _instance!;
  }
  
  void _initDio() {
    _dio = Dio(
      BaseOptions(
        baseUrl: ApiConstants.baseUrl,
        connectTimeout: const Duration(milliseconds: ApiConstants.connectionTimeout),
        receiveTimeout: const Duration(milliseconds: ApiConstants.receiveTimeout),
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json',
        },
      ),
    );
    
    // Add interceptors
    _dio.interceptors.addAll([
      ApiInterceptor(), // Custom interceptor for auth
      PrettyDioLogger(
        requestHeader: true,
        requestBody: true,
        responseBody: true,
        responseHeader: false,
        error: true,
        compact: true,
      ),
    ]);
  }
  
  /// Initialize with saved server URL from storage
  Future<void> initialize() async {
    final savedUrl = await ServerConfigStorage.getServerUrl();
    updateBaseUrl(savedUrl);
  }
  
  Dio get dio => _dio;
  
  // Update base URL (when user changes server settings)
  void updateBaseUrl(String newBaseUrl) {
    _dio.options.baseUrl = newBaseUrl;
    ApiConstants.baseUrl = newBaseUrl;
  }
}
