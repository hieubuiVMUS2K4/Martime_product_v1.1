import 'package:dio/dio.dart';
import '../../core/network/api_client.dart';
import '../../core/network/network_info.dart';
import '../../core/cache/cache_manager.dart';
import '../../core/constants/cache_keys.dart';
import '../../core/auth/token_storage.dart';
import '../data_sources/remote/crew_api.dart';
import '../models/crew_member.dart';

class CrewRepository {
  final ApiClient _apiClient;
  final NetworkInfo _networkInfo;
  final CacheManager _cacheManager;
  final TokenStorage _tokenStorage = TokenStorage();
  late final CrewApi _crewApi;

  CrewRepository({
    required ApiClient apiClient,
    required NetworkInfo networkInfo,
    required CacheManager cacheManager,
  })  : _apiClient = apiClient,
        _networkInfo = networkInfo,
        _cacheManager = cacheManager {
    _crewApi = CrewApi(_apiClient.dio);
  }

  /// Get current crewId from storage
  Future<String> _getCrewId() async {
    final crewId = await _tokenStorage.getCrewId();
    if (crewId == null || crewId.isEmpty) {
      throw Exception('Crew ID not found. Please login again.');
    }
    return crewId;
  }

  /// Get my profile - Online-first when forceRefresh, Offline-first otherwise
  Future<CrewMember> getMyProfile({bool forceRefresh = false}) async {
    try {
      final crewId = await _getCrewId();
      print('🔍 CrewRepository: Getting profile for crewId: $crewId, forceRefresh: $forceRefresh');
      
      // If online and forceRefresh, always fetch from API first
      if (await _networkInfo.isConnected) {
        if (forceRefresh) {
          print('🌐 CrewRepository: Fetching fresh profile from API...');
          final profile = await _crewApi.getMyProfile(crewId);
          print('✅ CrewRepository: Profile fetched successfully: ${profile.fullName}');

          // Cache the result
          await _cacheManager.saveData(
            CacheKeys.userProfile,
            profile.toJson(),
          );

          return profile;
        }
        
        // Not forceRefresh - try cache first, then API
        final cached = await _cacheManager.getData(CacheKeys.userProfile);
        if (cached != null) {
          print('📦 CrewRepository: Using cached profile');
          return CrewMember.fromJson(cached);
        }
        
        // No cache, fetch from API
        print('🌐 CrewRepository: No cache, fetching from API...');
        final profile = await _crewApi.getMyProfile(crewId);
        await _cacheManager.saveData(
          CacheKeys.userProfile,
          profile.toJson(),
        );
        return profile;
      }

      // Offline - try cache
      final cached = await _cacheManager.getData(CacheKeys.userProfile);
      if (cached != null) {
        print('📦 CrewRepository: Offline - using cached profile');
        return CrewMember.fromJson(cached);
      }

      // No cache and offline
      throw Exception('No cached data available. Please connect to internet');
    } on DioException catch (e) {
      print('❌ CrewRepository: API error: ${e.message}');
      // On API error, try to return cached data
      final cached = await _cacheManager.getData(CacheKeys.userProfile);
      if (cached != null) {
        print('📦 CrewRepository: API failed, using cached profile');
        return CrewMember.fromJson(cached);
      }
      throw Exception('Failed to fetch profile: ${e.message}');
    } catch (e) {
      print('❌ CrewRepository: Error: $e');
      rethrow;
    }
  }

  /// Get my certificates
  Future<Map<String, dynamic>> getMyCertificates() async {
    try {
      final crewId = await _getCrewId();
      
      if (!await _networkInfo.isConnected) {
        // Try to get from profile cache
        final cached = await _cacheManager.getData(CacheKeys.userProfile);
        if (cached != null) {
          final profile = CrewMember.fromJson(cached);
          return {
            'stcw': {
              'number': profile.certificateNumber,
              'issue': profile.certificateIssue,
              'expiry': profile.certificateExpiry,
              'isExpiring': profile.isCertificateExpiring,
              'isExpired': profile.isCertificateExpired,
            },
            'medical': {
              'issue': profile.medicalIssue,
              'expiry': profile.medicalExpiry,
              'isExpiring': profile.isMedicalExpiring,
              'isExpired': profile.isMedicalExpired,
            },
            'passport': {
              'number': profile.passportNumber,
              'expiry': profile.passportExpiry,
              'isExpiring': profile.isPassportExpiring,
              'isExpired': profile.isPassportExpired,
            },
            'visa': {
              'number': profile.visaNumber,
              'expiry': profile.visaExpiry,
            },
          };
        }
        throw Exception('No cached data available');
      }

      final response = await _crewApi.getMyCertificates(crewId);
      return response.data as Map<String, dynamic>;
    } catch (e) {
      throw Exception('Failed to fetch certificates: $e');
    }
  }
}
