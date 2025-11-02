import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class TokenStorage {
  final FlutterSecureStorage _storage = const FlutterSecureStorage();
  
  static const String _accessTokenKey = 'access_token';
  static const String _refreshTokenKey = 'refresh_token';
  static const String _userIdKey = 'user_id';
  static const String _crewIdKey = 'crew_id';
  static const String _fullNameKey = 'full_name';
  static const String _positionKey = 'position';
  
  // Save tokens after login
  Future<void> saveTokens({
    required String accessToken,
    required String refreshToken,
    required int userId,
    required String crewId,
    String? fullName,
    String? position,
  }) async {
    try {
      print('💾 TokenStorage: Starting to save tokens...');
      print('   - Access Token: ${accessToken.substring(0, 30)}...');
      print('   - Crew ID: $crewId');
      
      await _storage.write(key: _accessTokenKey, value: accessToken);
      print('✅ TokenStorage: Access token saved');
      
      await _storage.write(key: _refreshTokenKey, value: refreshToken);
      print('✅ TokenStorage: Refresh token saved');
      
      await _storage.write(key: _userIdKey, value: userId.toString());
      print('✅ TokenStorage: User ID saved');
      
      await _storage.write(key: _crewIdKey, value: crewId);
      print('✅ TokenStorage: Crew ID saved');
      
      if (fullName != null) {
        await _storage.write(key: _fullNameKey, value: fullName);
        print('✅ TokenStorage: Full name saved');
      }
      
      if (position != null) {
        await _storage.write(key: _positionKey, value: position);
        print('✅ TokenStorage: Position saved');
      }
      
      print('✅ TokenStorage: All tokens saved successfully');
      
      // Verify immediately
      final savedToken = await _storage.read(key: _accessTokenKey);
      final savedCrewId = await _storage.read(key: _crewIdKey);
      print('🔍 TokenStorage: Verification - Token exists: ${savedToken != null}, CrewId: $savedCrewId');
    } catch (e) {
      print('❌ TokenStorage: Error saving tokens: $e');
      rethrow;
    }
  }
  
  Future<String?> getAccessToken() async {
    final token = await _storage.read(key: _accessTokenKey);
    print('🔑 TokenStorage.getAccessToken(): ${token != null ? "Found" : "NOT FOUND"}');
    return token;
  }
  
  Future<String?> getRefreshToken() async {
    return await _storage.read(key: _refreshTokenKey);
  }
  
  Future<String?> getUserId() async {
    return await _storage.read(key: _userIdKey);
  }
  
  Future<String?> getCrewId() async {
    final crewId = await _storage.read(key: _crewIdKey);
    print('👤 TokenStorage.getCrewId(): ${crewId ?? "NOT FOUND"}');
    return crewId;
  }
  
  Future<String?> getFullName() async {
    return await _storage.read(key: _fullNameKey);
  }
  
  Future<String?> getPosition() async {
    return await _storage.read(key: _positionKey);
  }
  
  Future<bool> hasValidToken() async {
    final token = await getAccessToken();
    return token != null && token.isNotEmpty;
  }
  
  Future<void> clearTokens() async {
    await _storage.deleteAll();
  }
}
