# contexts/ — React Context dùng chung

## Mục đích

Chỉ chứa **một** context: đa ngôn ngữ (i18n) Anh/Việt cho toàn dashboard. Dùng React Context (thay vì
Zustand) vì đây đúng nghĩa là "cấu hình được inject xuống toàn cây component", không cần các tính năng
persist/selector nâng cao mà `stores/` đang dùng.

## Cấu trúc & vai trò

| File | Export | Công dụng |
|---|---|---|
| `I18nContext.tsx` | `I18nProvider` | Provider bọc toàn app (đặt ở `main.tsx`, trong `BrowserRouter`) |
| | `useTranslation()` | Hook gốc — ném lỗi nếu gọi ngoài `I18nProvider` |
| | `useTranslationSafe()` | Bản "an toàn" — trả về fallback (không throw) nếu Provider chưa sẵn sàng; **đây là hook được toàn bộ page/component sử dụng trong thực tế** |

## Luồng hoạt động chính

```
stores/settings.store.ts (useSettingsStore.language: 'en' | 'vi')
   ▼
I18nProvider đọc language qua useSettingsStore((s) => s.language)
   │  chọn bộ dịch tương ứng: src/locales/en.json hoặc src/locales/vi.json (import tĩnh, không lazy-load)
   ▼
Component gọi  const { t, locale } = useTranslationSafe()
   │  t('sync.title')  → tra cứu theo dot-notation trong object JSON (getNestedValue)
   │  t('sync.minutesAgo', { mins: 5 })  → thay {mins} bằng 5 (hàm interpolate)
   ▼
Chuỗi văn bản hiển thị đúng ngôn ngữ đang chọn trong Settings
```

Đổi ngôn ngữ: `components/settings/SettingsDialog.tsx` gọi `useSettingsStore.setLanguage('vi')` →
`I18nProvider` re-render vì đang subscribe store → toàn bộ text qua `t(...)` cập nhật ngay, không cần reload
trang.

## Liên kết với phần khác

- **`stores/settings.store.ts`**: nguồn duy nhất quyết định ngôn ngữ hiện tại.
- **`src/locales/en.json` / `vi.json`**: dữ liệu dịch thật (thư mục này nằm ngoài phạm vi tài liệu này, xem
  trực tiếp 2 file JSON để biết đủ key).
- Được gọi trong **hầu hết mọi page** dưới tên `useTranslationSafe()` — là một trong số ít import xuất hiện
  lặp lại nhiều nhất toàn bộ `src/`.

## Ghi chú khi đọc/dạy

- Luôn dùng `useTranslationSafe`, KHÔNG dùng `useTranslation` trực tiếp trừ khi chắc chắn component nằm
  trong cây `I18nProvider` — nhầm lẫn phổ biến của người mới là copy nhầm hook "not-safe" rồi gặp lỗi throw
  lúc test cô lập component.
- Không phải toàn bộ UI đã được dịch 100% — nhiều đoạn JSX vẫn còn chuỗi tiếng Anh/Việt viết cứng (hard-code)
  thay vì gọi `t(...)`; khi thêm tính năng mới nên ưu tiên dùng `t()` để nhất quán dần.
- File dịch được import tĩnh (`import en from '@/locales/en.json'`) nên **cả 2 ngôn ngữ đều nằm trong bundle
  ban đầu** — chấp nhận được vì file nhỏ, nhưng nếu thêm ngôn ngữ thứ 3 nên cân nhắc lazy-load.
