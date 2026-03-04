import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';

/// Provider quản lý ngôn ngữ ứng dụng
/// Hỗ trợ: English, Vietnamese, Filipino, Hindi, Chinese, Japanese, Korean
class LocaleProvider with ChangeNotifier {
  static const String _localeKey = 'app_locale';
  
  Locale _locale = const Locale('en'); // Default: English (IMO Standard)
  
  Locale get locale => _locale;
  
  /// Danh sách ngôn ngữ được hỗ trợ
  static const List<LocaleInfo> supportedLocales = [
    LocaleInfo(locale: Locale('en'), name: 'English', flag: '🇬🇧'),
    LocaleInfo(locale: Locale('vi'), name: 'Tiếng Việt', flag: '🇻🇳'),
    LocaleInfo(locale: Locale('fil'), name: 'Filipino', flag: '🇵🇭'),
    LocaleInfo(locale: Locale('hi'), name: 'हिंदी', flag: '🇮🇳'),
    LocaleInfo(locale: Locale('zh'), name: '简体中文', flag: '🇨🇳'),
    LocaleInfo(locale: Locale('ja'), name: '日本語', flag: '🇯🇵'),
    LocaleInfo(locale: Locale('ko'), name: '한국어', flag: '🇰🇷'),
  ];
  
  /// Khởi tạo và load ngôn ngữ đã lưu
  Future<void> initialize() async {
    final prefs = await SharedPreferences.getInstance();
    final localeCode = prefs.getString(_localeKey);
    
    if (localeCode != null) {
      _locale = Locale(localeCode);
      notifyListeners();
    }
  }
  
  /// Thay đổi ngôn ngữ
  Future<void> setLocale(Locale locale) async {
    if (_locale == locale) return;
    
    _locale = locale;
    
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_localeKey, locale.languageCode);
    
    notifyListeners();
  }
  
  /// Lấy tên ngôn ngữ hiện tại
  String get currentLanguageName {
    final localeInfo = supportedLocales.firstWhere(
      (l) => l.locale.languageCode == _locale.languageCode,
      orElse: () => supportedLocales.first,
    );
    return localeInfo.name;
  }
  
  /// Lấy cờ quốc gia hiện tại
  String get currentLanguageFlag {
    final localeInfo = supportedLocales.firstWhere(
      (l) => l.locale.languageCode == _locale.languageCode,
      orElse: () => supportedLocales.first,
    );
    return localeInfo.flag;
  }
}

/// Thông tin ngôn ngữ
class LocaleInfo {
  final Locale locale;
  final String name;
  final String flag;
  
  const LocaleInfo({
    required this.locale,
    required this.name,
    required this.flag,
  });
}
