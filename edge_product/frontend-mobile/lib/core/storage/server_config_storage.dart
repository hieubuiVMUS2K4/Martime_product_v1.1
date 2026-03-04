import 'package:hive/hive.dart';

class ServerConfigStorage {
  static const String _boxName = 'server_config';
  static const String _serverUrlKey = 'server_url';
  static const String _defaultUrl = 'http://192.168.1.100:5001';

  static Future<Box> _getBox() async {
    return await Hive.openBox(_boxName);
  }

  /// Get saved server URL or default
  static Future<String> getServerUrl() async {
    try {
      final box = await _getBox();
      return box.get(_serverUrlKey, defaultValue: _defaultUrl) as String;
    } catch (e) {
      return _defaultUrl;
    }
  }

  /// Save server URL
  static Future<void> saveServerUrl(String url) async {
    try {
      final box = await _getBox();
      await box.put(_serverUrlKey, url);
    } catch (e) {
      print('Error saving server URL: $e');
    }
  }

  /// Reset to default URL
  static Future<void> resetToDefault() async {
    await saveServerUrl(_defaultUrl);
  }

  /// Validate URL format
  static bool isValidUrl(String url) {
    if (url.isEmpty) return false;
    
    try {
      final uri = Uri.parse(url);
      
      // Must have http or https scheme
      if (uri.scheme != 'http' && uri.scheme != 'https') {
        return false;
      }
      
      // Must have host
      if (uri.host.isEmpty) {
        return false;
      }
      
      return true;
    } catch (e) {
      return false;
    }
  }

  /// Get example URLs for help
  static List<String> getExampleUrls() {
    return [
      'http://localhost:5001',
      'http://192.168.1.100:5001',
      'http://192.168.0.50:5001',
      'http://10.0.0.100:5001',
    ];
  }
}
