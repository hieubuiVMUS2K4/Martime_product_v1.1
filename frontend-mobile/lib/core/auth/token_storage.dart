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
    await Future.wait([
      _storage.write(key: _accessTokenKey, value: accessToken),
      _storage.write(key: _refreshTokenKey, value: refreshToken),
      _storage.write(key: _userIdKey, value: userId.toString()),
      _storage.write(key: _crewIdKey, value: crewId),
      if (fullName != null) _storage.write(key: _fullNameKey, value: fullName),
      if (position != null) _storage.write(key: _positionKey, value: position),
    ]);
  }
  
  Future<String?> getAccessToken() async {
    return await _storage.read(key: _accessTokenKey);
  }
  
  Future<String?> getRefreshToken() async {
    return await _storage.read(key: _refreshTokenKey);
  }
  
  Future<String?> getUserId() async {
    return await _storage.read(key: _userIdKey);
  }
  
  Future<String?> getCrewId() async {
    return await _storage.read(key: _crewIdKey);
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
