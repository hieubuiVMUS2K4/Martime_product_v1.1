import 'package:hive/hive.dart';
import 'dart:convert';

class CacheManager {
  static const String _cacheBox = 'cache_box';
  
  // Cache expiry times
  static const int defaultExpiryMs = 3600000; // 1 hour
  static const int offlineExpiryMs = 86400000 * 7; // 7 days for offline mode
  
  Future<void> init() async {
    await Hive.openBox(_cacheBox);
  }
  
  // Save data to cache with optional custom expiry
  Future<void> saveData(String key, dynamic data, {int? expiryMs}) async {
    final box = Hive.box(_cacheBox);
    await box.put(key, jsonEncode({
      'data': data,
      'timestamp': DateTime.now().millisecondsSinceEpoch,
      'expiryMs': expiryMs ?? defaultExpiryMs,
    }));
  }
  
  // Save data for offline use (7 days expiry)
  Future<void> saveDataForOffline(String key, dynamic data) async {
    await saveData(key, data, expiryMs: offlineExpiryMs);
  }
  
  // Get cached data with expiry check
  Future<dynamic> getData(String key) async {
    final box = Hive.box(_cacheBox);
    final cached = box.get(key);
    
    if (cached == null) return null;
    
    try {
      final decoded = jsonDecode(cached);
      final timestamp = decoded['timestamp'] as int;
      final expiryMs = decoded['expiryMs'] as int? ?? defaultExpiryMs;
      final now = DateTime.now().millisecondsSinceEpoch;
      
      // Check if cache is expired
      if (now - timestamp > expiryMs) {
        await box.delete(key);
        return null;
      }
      
      return decoded['data'];
    } catch (e) {
      // If JSON parsing fails, delete corrupted cache
      await box.delete(key);
      return null;
    }
  }
  
  // Get cached data without expiry check (for offline fallback)
  Future<dynamic> getDataNoExpiry(String key) async {
    final box = Hive.box(_cacheBox);
    final cached = box.get(key);
    
    if (cached == null) return null;
    
    try {
      final decoded = jsonDecode(cached);
      return decoded['data'];
    } catch (e) {
      // If JSON parsing fails, delete corrupted cache
      await box.delete(key);
      return null;
    }
  }
  
  // Check if cache exists (regardless of expiry)
  Future<bool> hasCache(String key) async {
    final box = Hive.box(_cacheBox);
    return box.containsKey(key);
  }
  
  // Clear specific cache
  Future<void> clearCache(String key) async {
    final box = Hive.box(_cacheBox);
    await box.delete(key);
  }
  
  // Clear all cache EXCEPT pending/draft data (for logout)
  // This preserves offline work that hasn't been synced yet
  Future<void> clearAllCache() async {
    final box = Hive.box(_cacheBox);
    
    // Get all keys that should NOT be deleted (pending work)
    final keysToPreserve = box.keys.where((key) {
      final keyStr = key.toString();
      return keyStr.startsWith('pending_') || 
             keyStr.startsWith('draft_') ||
             keyStr.startsWith('sync_queue');
    }).toList();
    
    // Get all other keys to delete
    final keysToDelete = box.keys.where((key) {
      final keyStr = key.toString();
      return !keyStr.startsWith('pending_') && 
             !keyStr.startsWith('draft_') &&
             !keyStr.startsWith('sync_queue');
    }).toList();
    
    // Delete only non-pending keys
    for (final key in keysToDelete) {
      await box.delete(key);
    }
    
    print('🧹 CacheManager: Cleared ${keysToDelete.length} cache entries, preserved ${keysToPreserve.length} pending items');
  }
  
  // Clear ALL cache including pending (use with caution - for app reset)
  Future<void> clearAllCacheForced() async {
    final box = Hive.box(_cacheBox);
    await box.clear();
    print('🧹 CacheManager: Force cleared ALL cache');
  }
  
  // Get cache size
  Future<int> getCacheSize() async {
    final box = Hive.box(_cacheBox);
    return box.length;
  }
}
