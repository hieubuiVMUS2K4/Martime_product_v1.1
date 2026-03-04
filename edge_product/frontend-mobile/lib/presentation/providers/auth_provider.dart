import 'package:flutter/material.dart';
import '../../core/auth/token_storage.dart';
import '../../core/cache/cache_manager.dart';
import '../../data/repositories/auth_repository.dart';
import '../../core/di/service_locator.dart';

class AuthProvider with ChangeNotifier {
  late final TokenStorage _tokenStorage;
  late final CacheManager _cacheManager;
  late final AuthRepository _authRepository;
  
  bool _isLoggedIn = false;
  bool _isLoading = false;
  String? _error;
  int? _userId;
  String? _crewId;
  String? _fullName;
  String? _position;

  AuthProvider() {
    _tokenStorage = sl<TokenStorage>();
    _cacheManager = sl<CacheManager>();
    _authRepository = sl<AuthRepository>();
  }
  
  bool get isLoggedIn => _isLoggedIn;
  bool get isLoading => _isLoading;
  String? get error => _error;
  String? get crewId => _crewId;
  String? get fullName => _fullName;
  String? get position => _position;
  
  // Check if user is logged in
  Future<void> checkLoginStatus() async {
    _isLoading = true;
    notifyListeners();
    
    try {
      final hasToken = await _tokenStorage.hasValidToken();
      _isLoggedIn = hasToken;
      
      if (hasToken) {
        _crewId = await _tokenStorage.getCrewId();
        _fullName = await _tokenStorage.getFullName();
        _position = await _tokenStorage.getPosition();
      }
    } catch (e) {
      _error = e.toString();
    }
    
    _isLoading = false;
    notifyListeners();
  }
  
  // Login
  Future<bool> login({
    required String crewId,
    required String password,
  }) async {
    _isLoading = true;
    _error = null;
    notifyListeners();
    
    try {
      print('🔐 AuthProvider: Logging in with Edge Server API...');
      print('   - Crew ID: $crewId');
      
      // Call real API through AuthRepository
      final response = await _authRepository.login(
        crewId: crewId,
        password: password,
      );
      
      print('✅ AuthProvider: Login successful!');
      print('   - User ID: ${response.userId}');
      print('   - Crew ID: ${response.crewId}');
      print('   - Full Name: ${response.fullName}');
      print('   - Position: ${response.position}');
      
      // Clear old cache to prevent showing stale data
      await _cacheManager.clearAllCache();
      print('🧹 AuthProvider: Cleared old cache');
      
      // Tokens are already saved by AuthRepository
      _userId = response.userId;
      _crewId = response.crewId;
      _fullName = response.fullName;
      _position = response.position;
      _isLoggedIn = true;
      
      _isLoading = false;
      notifyListeners();
      return true;
    } catch (e) {
      _error = e.toString();
      _isLoading = false;
      notifyListeners();
      return false;
    }
  }
  
  // Logout
  Future<void> logout() async {
    _isLoading = true;
    notifyListeners();
    
    try {
      // Call logout API
      await _authRepository.logout();
      await _cacheManager.clearAllCache();
      
      _isLoggedIn = false;
      _crewId = null;
      _fullName = null;
      _position = null;
      _userId = null;
      
      print('👋 AuthProvider: Logged out successfully');
    } catch (e) {
      _error = e.toString();
      print('❌ AuthProvider: Logout error: $e');
    }
    
    _isLoading = false;
    notifyListeners();
  }
}
