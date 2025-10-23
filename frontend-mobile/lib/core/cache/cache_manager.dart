import 'package:hive/hive.dart';
import 'dart:convert';

class CacheManager {
  static const String _cacheBox = 'cache_box';
  
  Future<void> init() async {
    await Hive.openBox(_cacheBox);
  }
  
  // Save data to cache
  Future<void> saveData(String key, dynamic data) async {
    final box = Hive.box(_cacheBox);
    await box.put(key, jsonEncode({
      'data': data,
      'timestamp': DateTime.now().millisecondsSinceEpoch,
    }));
  }
  
  // Get cached data
  Future<dynamic> getData(String key) async {
    final box = Hive.box(_cacheBox);
    final cached = box.get(key);
    
    if (cached == null) return null;
    
    final decoded = jsonDecode(cached);
    final timestamp = decoded['timestamp'] as int;
    final now = DateTime.now().millisecondsSinceEpoch;
    
    // Check if cache is expired (1 hour)
    if (now - timestamp > 3600000) {
      await box.delete(key);
      return null;
    }
    
    return decoded['data'];
  }
  
  // Get cached data without expiry check
  Future<dynamic> getDataNoExpiry(String key) async {
    final box = Hive.box(_cacheBox);
    final cached = box.get(key);
    
    if (cached == null) return null;
    
    final decoded = jsonDecode(cached);
    return decoded['data'];
  }
  
  // Clear specific cache
  Future<void> clearCache(String key) async {
    final box = Hive.box(_cacheBox);
    await box.delete(key);
  }
  
  // Clear all cache
  Future<void> clearAllCache() async {
    final box = Hive.box(_cacheBox);
    await box.clear();
  }
  
  // Get cache size
  Future<int> getCacheSize() async {
    final box = Hive.box(_cacheBox);
    return box.length;
  }
}
