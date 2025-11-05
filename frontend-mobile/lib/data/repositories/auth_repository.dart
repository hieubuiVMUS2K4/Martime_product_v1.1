import 'package:dio/dio.dart';
import '../../core/network/api_client.dart';
import '../../core/network/network_info.dart';
import '../../core/auth/token_storage.dart';
import '../data_sources/remote/auth_api.dart';
import '../models/login_request.dart';
import '../models/login_response.dart';
import '../models/refresh_token_request.dart';

class AuthRepository {
  final ApiClient _apiClient;
  final NetworkInfo _networkInfo;
  final TokenStorage _tokenStorage;
  late final AuthApi _authApi;

  AuthRepository({
    required ApiClient apiClient,
    required NetworkInfo networkInfo,
    required TokenStorage tokenStorage,
  })  : _apiClient = apiClient,
        _networkInfo = networkInfo,
        _tokenStorage = tokenStorage {
    _authApi = AuthApi(_apiClient.dio);
  }

  /// Login with crew ID and password (using new auth system)
  Future<LoginResponse> login({
    required String crewId,
    required String password,
  }) async {
    try {
      // Check network connectivity
      if (!await _networkInfo.isConnected) {
        throw Exception('No internet connection');
      }

      // Call new login API with username (crew_id is used as username)
      final request = LoginRequest(username: crewId, password: password);
      final response = await _authApi.login(request);

      print('🔐 AuthRepository: Login response received');
      print('   - Access Token: ${response.accessToken.substring(0, 30)}...');
      print('   - User ID: ${response.userId}');
      print('   - Crew ID: ${response.crewId}');
      print('   - Full Name: ${response.fullName}');
      print('   - Position: ${response.position}');

      // Save tokens to secure storage
      await _tokenStorage.saveTokens(
        accessToken: response.accessToken,
        refreshToken: response.refreshToken,
        userId: response.userId,
        crewId: response.crewId,
        fullName: response.fullName,
        position: response.position,
      );

      print('💾 AuthRepository: Tokens saved to storage');
      
      // Verify tokens were saved
      final savedToken = await _tokenStorage.getAccessToken();
      if (savedToken != null) {
        print('✅ AuthRepository: Token verification successful');
      } else {
        print('❌ AuthRepository: Token NOT found after save!');
      }

      return response;
    } on DioException catch (e) {
      if (e.response?.statusCode == 401) {
        throw Exception('Invalid crew ID or password');
      } else if (e.response?.statusCode == 400) {
        final errorMsg = e.response?.data['message'] as String? ?? 'Bad request';
        throw Exception(errorMsg);
      } else if (e.response?.statusCode == 500) {
        throw Exception('Server error. Please try again later');
      }
      throw Exception('Login failed: ${e.message}');
    } catch (e) {
      throw Exception('Login failed: $e');
    }
  }

  /// Refresh access token
  Future<LoginResponse> refreshToken() async {
    try {
      final refreshToken = await _tokenStorage.getRefreshToken();
      if (refreshToken == null) {
        throw Exception('No refresh token found');
      }

      final response = await _authApi.refreshToken(
        RefreshTokenRequest(refreshToken: refreshToken),
      );

      // Update tokens
      await _tokenStorage.saveTokens(
        accessToken: response.accessToken,
        refreshToken: response.refreshToken,
        userId: response.userId,
        crewId: response.crewId,
        fullName: response.fullName,
        position: response.position,
      );

      return response;
    } catch (e) {
      // Clear tokens if refresh fails
      await _tokenStorage.clearTokens();
      throw Exception('Session expired. Please login again');
    }
  }

  /// Logout
  Future<void> logout() async {
    try {
      if (await _networkInfo.isConnected) {
        await _authApi.logout();
      }
    } catch (e) {
      // Ignore logout API errors
    } finally {
      // Always clear local tokens
      await _tokenStorage.clearTokens();
    }
  }

  /// Check if user is logged in
  Future<bool> isLoggedIn() async {
    return await _tokenStorage.hasValidToken();
  }
}
