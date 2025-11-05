# 🌍 INTERNATIONALIZATION (i18n) IMPLEMENTATION GUIDE

## 📋 Tổng quan

Hệ thống đa ngôn ngữ được triển khai trên **3 nền tảng**:
1. **Flutter Mobile** (ưu tiên cao)
2. **React Frontend (Edge Dashboard)**
3. **ASP.NET Core Backend API**

---

## 1️⃣ FLUTTER MOBILE APP (HOÀN THÀNH)

### ✅ Đã triển khai:

#### **A. Cấu hình cơ bản**
```yaml
# pubspec.yaml
flutter:
  generate: true  # Enable l10n
  
# l10n.yaml
arb-dir: lib/l10n
template-arb-file: app_en.arb
output-localization-file: app_localizations.dart
```

#### **B. Ngôn ngữ hỗ trợ**
- 🇬🇧 English (IMO Standard - mặc định)
- 🇻🇳 Vietnamese (Tiếng Việt)
- 🇵🇭 Filipino/Tagalog
- 🇮🇳 Hindi (हिंदी)
- 🇨🇳 Chinese (简体中文)
- 🇯🇵 Japanese (日本語)
- 🇰🇷 Korean (한국어)

#### **C. Cách sử dụng trong code**

**ĐÚNG ✅ - Dùng AppLocalizations:**
```dart
import 'package:flutter_gen/gen_l10n/app_localizations.dart';

Widget build(BuildContext context) {
  final l10n = AppLocalizations.of(context)!;
  
  return Text(l10n.taskDetails);  // ✅ Dùng key
  return Text(l10n.statusPending);  // ✅ Dùng key
  return Text(l10n.priorityHigh);   // ✅ Dùng key
}
```

**SAI ❌ - Hardcode text:**
```dart
return Text('Task Details');  // ❌ KHÔNG BAO GIỜ HARDCODE
return Text('Pending');       // ❌ KHÔNG BAO GIỜ HARDCODE
```

#### **D. Thêm text mới**

**Bước 1: Thêm vào tất cả file .arb**
```json
// app_en.arb
{
  "newFeatureTitle": "New Feature",
  "newFeatureDescription": "This is a new feature description"
}

// app_vi.arb
{
  "newFeatureTitle": "Tính năng mới",
  "newFeatureDescription": "Đây là mô tả tính năng mới"
}

// app_fil.arb
{
  "newFeatureTitle": "Bagong Feature",
  "newFeatureDescription": "Ito ay bagong feature description"
}
```

**Bước 2: Generate code**
```bash
flutter pub get
# Code tự động generate vào .dart_tool/flutter_gen/gen_l10n/
```

**Bước 3: Sử dụng**
```dart
Text(l10n.newFeatureTitle)
Text(l10n.newFeatureDescription)
```

#### **E. Text với tham số động**

**Trong file .arb:**
```json
{
  "taskAssignedTo": "Task assigned to {name}",
  "@taskAssignedTo": {
    "placeholders": {
      "name": {
        "type": "String"
      }
    }
  },
  
  "itemsCount": "{count} items",
  "@itemsCount": {
    "placeholders": {
      "count": {
        "type": "int"
      }
    }
  }
}
```

**Sử dụng:**
```dart
Text(l10n.taskAssignedTo('John Smith'))
Text(l10n.itemsCount(5))  // "5 items"
```

#### **F. Chuyển đổi ngôn ngữ**

Settings → Language Settings → Chọn ngôn ngữ → App tự động cập nhật

---

## 2️⃣ REACT FRONTEND (Edge Dashboard)

### 🎯 Khuyến nghị: **react-i18next**

#### **A. Cài đặt**
```bash
cd frontend-edge
npm install i18next react-i18next i18next-browser-languagedetector
```

#### **B. Cấu trúc thư mục**
```
frontend-edge/
├── src/
│   ├── i18n/
│   │   ├── locales/
│   │   │   ├── en/
│   │   │   │   ├── common.json
│   │   │   │   ├── dashboard.json
│   │   │   │   ├── maintenance.json
│   │   │   │   └── navigation.json
│   │   │   ├── vi/
│   │   │   │   └── ...
│   │   │   └── fil/
│   │   │       └── ...
│   │   └── config.ts
```

#### **C. File cấu hình**
```typescript
// src/i18n/config.ts
import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';

import enCommon from './locales/en/common.json';
import viCommon from './locales/vi/common.json';

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources: {
      en: { common: enCommon },
      vi: { common: viCommon },
    },
    fallbackLng: 'en',
    defaultNS: 'common',
    interpolation: {
      escapeValue: false,
    },
  });

export default i18n;
```

#### **D. Sử dụng trong component**
```typescript
import { useTranslation } from 'react-i18next';

function DashboardPage() {
  const { t } = useTranslation();
  
  return (
    <div>
      <h1>{t('dashboard.title')}</h1>
      <p>{t('dashboard.welcome', { name: 'Captain' })}</p>
    </div>
  );
}
```

#### **E. Chuyển ngôn ngữ**
```typescript
import { useTranslation } from 'react-i18next';

function LanguageSelector() {
  const { i18n } = useTranslation();
  
  return (
    <select 
      value={i18n.language}
      onChange={(e) => i18n.changeLanguage(e.target.value)}
    >
      <option value="en">🇬🇧 English</option>
      <option value="vi">🇻🇳 Tiếng Việt</option>
      <option value="fil">🇵🇭 Filipino</option>
    </select>
  );
}
```

---

## 3️⃣ ASP.NET CORE BACKEND

### 🎯 Khuyến nghị: **Built-in Localization**

#### **A. Cấu hình Program.cs**
```csharp
// Program.cs
builder.Services.AddLocalization(options => 
    options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en", "vi", "fil", "hi" };
    options.SetDefaultCulture("en")
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

var app = builder.Build();
app.UseRequestLocalization();
```

#### **B. Cấu trúc Resource Files**
```
edge-services/
├── Resources/
│   ├── Controllers/
│   │   ├── MaintenanceController.en.resx
│   │   ├── MaintenanceController.vi.resx
│   │   └── MaintenanceController.fil.resx
│   └── Shared/
│       ├── SharedResources.en.resx
│       └── SharedResources.vi.resx
```

#### **C. Sử dụng trong Controller**
```csharp
using Microsoft.Extensions.Localization;

[ApiController]
[Route("api/maintenance")]
public class MaintenanceController : ControllerBase
{
    private readonly IStringLocalizer<MaintenanceController> _localizer;
    
    public MaintenanceController(IStringLocalizer<MaintenanceController> localizer)
    {
        _localizer = localizer;
    }
    
    [HttpGet]
    public IActionResult GetTasks()
    {
        return Ok(new { 
            message = _localizer["TasksRetrievedSuccessfully"],
            title = _localizer["MyTasks"]
        });
    }
}
```

#### **D. Lấy ngôn ngữ từ Header**
```csharp
// Client gửi:
// Accept-Language: vi

// hoặc Custom header:
// X-Locale: fil
```

---

## 🔄 WORKFLOW THÊM TEXT MỚI

### **Khi thêm feature mới:**

#### 1. **Mobile App (Flutter)**
```bash
1. Thêm key vào lib/l10n/app_en.arb
2. Thêm key vào lib/l10n/app_vi.arb
3. Thêm key vào các file .arb khác
4. Run: flutter pub get
5. Dùng: l10n.yourNewKey
```

#### 2. **Frontend (React)**
```bash
1. Thêm key vào src/i18n/locales/en/common.json
2. Thêm key vào src/i18n/locales/vi/common.json
3. Dùng: t('yourNewKey')
```

#### 3. **Backend (.NET)**
```bash
1. Thêm key vào Resources/Controller.en.resx
2. Thêm key vào Resources/Controller.vi.resx
3. Dùng: _localizer["YourNewKey"]
```

---

## 📝 QUY TẮC ĐẶT TÊN KEY

### ✅ ĐÚNG:
```
taskDetails
statusPending
priorityHigh
confirmDeleteMessage
errorServerConnection
```

### ❌ SAI:
```
Task_Details        // không dùng underscore
taskdetails         // không viết liền
TASK_DETAILS        // không viết hoa
task-details        // không dùng dash
```

---

## 🎨 UI/UX BEST PRACTICES

### 1. **Language Selector Position**
- Mobile: Settings → Language
- Web: Header → Profile Menu → Language
- Backend: Accept-Language header

### 2. **Default Language**
- **English** (IMO Standard cho hàng hải)
- Fallback nếu ngôn ngữ không hỗ trợ

### 3. **Persist User Choice**
- Mobile: SharedPreferences
- Web: LocalStorage
- Backend: User profile

### 4. **RTL Languages (Tương lai)**
- Arabic: `Locale('ar')` với `textDirection: TextDirection.rtl`

---

## 🧪 TESTING

### Mobile:
```dart
// Test với ngôn ngữ khác nhau
testWidgets('displays correct language', (tester) async {
  await tester.pumpWidget(
    MaterialApp(
      locale: Locale('vi'),
      localizationsDelegates: AppLocalizations.localizationsDelegates,
      home: MyWidget(),
    ),
  );
});
```

### React:
```typescript
// Test i18n
import { I18nextProvider } from 'react-i18next';
import i18n from './i18n/config';

<I18nextProvider i18n={i18n}>
  <YourComponent />
</I18nextProvider>
```

---

## 🚀 DEPLOYMENT CHECKLIST

- [ ] Tất cả hardcoded text đã được thay bằng i18n keys
- [ ] Tất cả ngôn ngữ có đầy đủ translations
- [ ] Date/time format theo locale
- [ ] Number format theo locale (1,000.00 vs 1.000,00)
- [ ] Currency format phù hợp
- [ ] Error messages đa ngôn ngữ
- [ ] API response messages đa ngôn ngữ

---

## 📚 TÀI LIỆU THAM KHẢO

- Flutter i18n: https://docs.flutter.dev/development/accessibility-and-localization/internationalization
- react-i18next: https://react.i18next.com/
- ASP.NET Localization: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization

---

**🎯 Ưu tiên:**
1. **Mobile App** (đã hoàn thành)
2. **Frontend Edge Dashboard** (khuyến nghị triển khai tiếp theo)
3. **Backend API** (optional - tùy nhu cầu)
