import 'package:dio/dio.dart';
import '../../core/network/api_client.dart';
import '../../core/network/network_info.dart';
import '../../core/cache/cache_manager.dart';
import '../../core/constants/cache_keys.dart';
import '../data_sources/remote/crew_api.dart';
import '../models/crew_member.dart';

class CrewRepository {
  final ApiClient _apiClient;
  final NetworkInfo _networkInfo;
  final CacheManager _cacheManager;
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

  /// Get my profile - Offline-first
  Future<CrewMember> getMyProfile({bool forceRefresh = false}) async {
    try {
      // If online, fetch from API
      if (await _networkInfo.isConnected && forceRefresh) {
        final profile = await _crewApi.getMyProfile();

        // Cache the result
        await _cacheManager.saveData(
          CacheKeys.userProfile,
          profile.toJson(),
        );

        return profile;
      }

      // Try to load from cache
      final cached = await _cacheManager.getData(CacheKeys.userProfile);
      if (cached != null) {
        return CrewMember.fromJson(cached);
      }

      // If no cache and online, fetch from API
      if (await _networkInfo.isConnected) {
        final profile = await _crewApi.getMyProfile();
        await _cacheManager.saveData(
          CacheKeys.userProfile,
          profile.toJson(),
        );
        return profile;
      }

      // No cache and offline
      throw Exception('No cached data available. Please connect to internet');
    } on DioException catch (e) {
      // On API error, try to return cached data
      final cached = await _cacheManager.getData(CacheKeys.userProfile);
      if (cached != null) {
        return CrewMember.fromJson(cached);
      }
      throw Exception('Failed to fetch profile: ${e.message}');
    }
  }

  /// Get my certificates
  Future<Map<String, dynamic>> getMyCertificates() async {
    try {
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

      final response = await _crewApi.getMyCertificates();
      return response.data as Map<String, dynamic>;
    } catch (e) {
      throw Exception('Failed to fetch certificates: $e');
    }
  }
}
