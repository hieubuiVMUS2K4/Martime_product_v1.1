# l10n — Đa ngôn ngữ (Flutter gen-l10n)

## Mục đích

Chứa chuỗi văn bản đa ngôn ngữ, sinh tự động bởi công cụ `flutter gen-l10n` (bật qua `generate: true` trong `pubspec.yaml`) — cơ chế localization **chuẩn của Flutter**, khác với `core/localization/` (nơi chứa state/provider để **chọn** ngôn ngữ, không chứa bản dịch nào).

## Cấu trúc & vai trò

| File | Sinh tự động? | Vai trò |
|---|---|---|
| `app_en.arb` | Viết tay (nguồn) | Chuỗi gốc tiếng Anh, mỗi key có thể kèm `@key` mô tả |
| `app_vi.arb` | Viết tay (nguồn) | Bản dịch tiếng Việt, cùng bộ key với `app_en.arb` |
| `app_localizations.dart` | **Sinh tự động** (~2738 dòng) | Abstract class `AppLocalizations` (getter cho mỗi chuỗi), `_AppLocalizationsDelegate`, hàm `lookupAppLocalizations()` |
| `app_localizations_en.dart` | **Sinh tự động** (~1435 dòng) | `AppLocalizationsEn extends AppLocalizations` |
| `app_localizations_vi.dart` | **Sinh tự động** (~1437 dòng) | `AppLocalizationsVi extends AppLocalizations` |

**Không bao giờ sửa tay** 3 file `.dart` — sửa `app_en.arb`/`app_vi.arb` rồi chạy lại `flutter gen-l10n` (hoặc đơn giản là `flutter run`/`flutter build`, vì `generate: true` khiến Flutter tự chạy lại công cụ này).

## Luồng hoạt động chính

```
app_en.arb + app_vi.arb  ──flutter gen-l10n──►  app_localizations*.dart (sinh tự động)
                                                          │
                                                          ▼
app.dart: localizationsDelegates: [AppLocalizations.delegate, ...]
                                                          │
                                                          ▼
                    context.l10n (extension trong core/localization/localization_helper.dart)
                                                          │
                                                          ▼
                                    Text(l10n.someKey)  trong mọi screen
```

## Liên kết với phần khác

- `core/localization/locale_provider.dart` — `LocaleProvider` quyết định `Locale` hiện tại (lưu trong `SharedPreferences`), độc lập với việc bản dịch có thật sự tồn tại hay không.
- `core/localization/localization_helper.dart` — extension `context.l10n` mà toàn bộ screen dùng để đọc chuỗi.
- `app.dart` — khai báo `AppLocalizations.delegate` + `supportedLocales` (7 locale) + `localeResolutionCallback`.

## Ghi chú khi đọc/dạy

- **Gotcha quan trọng nhất của toàn bộ phần đa ngôn ngữ**: `_AppLocalizationsDelegate.isSupported()` (trong `app_localizations.dart`, gần cuối file) chỉ trả `true` cho `'en'` và `'vi'`:
  ```dart
  bool isSupported(Locale locale) =>
      <String>['en', 'vi'].contains(locale.languageCode);
  ```
  Trong khi đó `LocaleProvider.supportedLocales` (7 ngôn ngữ) và `MaterialApp.supportedLocales` trong `app.dart` (cũng 7 locale: en, vi, fil, hi, zh, ja, ko) đều "hứa hẹn" hỗ trợ Filipino/Hindi/Trung/Nhật/Hàn. Vì **không có file `app_fil.arb`/`app_hi.arb`/`app_zh.arb`/`app_ja.arb`/`app_ko.arb`**, nếu người dùng chọn 1 trong 5 ngôn ngữ này ở `LanguageSelectionScreen`, `Localizations` widget của Flutter sẽ không nạp được `AppLocalizations` cho locale đó → **mọi lời gọi `context.l10n`/`AppLocalizations.of(context)` sau đó sẽ throw runtime error** (assertion "No AppLocalizations found").
- **Cách khắc phục đúng khi được giao xử lý việc này**: hoặc (a) thêm đủ 5 file `.arb` còn thiếu rồi chạy lại `flutter gen-l10n`, hoặc (b) thu gọn `LocaleProvider.supportedLocales` và danh sách trong `app.dart` xuống đúng 2 ngôn ngữ đang có bản dịch thật (`en`, `vi`) cho tới khi có bản dịch đầy đủ cho các ngôn ngữ còn lại. Đừng chỉ bọc try-catch để "ẩn" crash — gốc vấn đề là thiếu file `.arb`, phải xử lý từ đó.
- Khi thêm 1 chuỗi mới: phải thêm key vào **cả 2** file `.arb`. Thiếu 1 trong 2 không gây lỗi build ngay mà chỉ lặng lẽ thiếu bản dịch ở 1 ngôn ngữ — nên kiểm tra kỹ khi review PR thêm chuỗi mới.
