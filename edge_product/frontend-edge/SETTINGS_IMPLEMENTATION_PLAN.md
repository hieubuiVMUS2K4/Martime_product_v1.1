# 🎯 Settings & Configuration Implementation Plan

## Maritime Edge Dashboard - User Preferences System

**Version:** 1.1  
**Date:** 2026-01-12  
**Status:** Planning (Reviewed & Hardened)  

---

## 📋 Table of Contents

1. [Overview](#1-overview)
2. [Features Scope](#2-features-scope)
3. [Architecture Design](#3-architecture-design)
4. [File Structure](#4-file-structure)
5. [Implementation Details](#5-implementation-details)
6. [Translation Keys](#6-translation-keys)
7. [UI/UX Design](#7-uiux-design)
8. [Testing Plan](#8-testing-plan)
9. [Migration Strategy](#9-migration-strategy)
10. [Timeline](#10-timeline)
11. [⚠️ Known Issues & Mitigations](#11-known-issues--mitigations)
12. [🛡️ Error Handling Strategy](#12-error-handling-strategy)

---

## 1. Overview

### 1.1 Mục tiêu
Xây dựng hệ thống cài đặt người dùng cho Maritime Edge Dashboard, cho phép crew members tùy chỉnh giao diện theo sở thích cá nhân trong điều kiện làm việc trên tàu.

### 1.2 Yêu cầu đặc biệt cho Maritime
- **Night Mode**: Quan trọng cho bridge operations ban đêm (IMO COLREG)
- **Large Font**: Dễ đọc trong điều kiện rung lắc
- **High Contrast**: Đọc được dưới ánh sáng mặt trời trực tiếp
- **Offline-first**: Settings phải hoạt động khi không có mạng

### 1.3 Tech Stack hiện tại
- React 19.x + TypeScript
- Zustand 5.x (State Management)
- Tailwind CSS 3.x với CSS Variables
- Radix UI (Dialog, Select, Tabs)
- Vite (Build tool)

---

## 2. Features Scope

### 2.1 Phase 1 - Core Settings (MVP)

| Feature | Description | Priority |
|---------|-------------|----------|
| 🌐 Language | English ↔ Vietnamese | P0 |
| 🎨 Theme | Light / Dark / System | P0 |
| 🔤 Font Size | Small / Medium / Large / Extra Large | P0 |
| 💾 Persistence | localStorage with Zustand persist | P0 |

### 2.2 Phase 2 - Advanced Settings (Future)

| Feature | Description | Priority |
|---------|-------------|----------|
| 🔔 Notifications | Sound on/off, vibration | P1 |
| 📊 Dashboard Layout | Customize widgets | P1 |
| ⌨️ Keyboard Shortcuts | Custom keybindings | P2 |
| 🔄 Sync Settings | Sync across devices | P2 |
| 🖥️ Display Density | Compact / Normal / Comfortable | P2 |

### 2.3 Out of Scope (Phase 1)
- User profile management
- Server-side settings sync
- Custom color themes
- Accessibility features (screen reader, etc.)

---

## 3. Architecture Design

### 3.1 Data Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           SETTINGS ARCHITECTURE                              │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  User Action                                                                 │
│      │                                                                       │
│      ▼                                                                       │
│  ┌─────────────────┐                                                        │
│  │ SettingsDialog  │  ← UI Layer (React Components)                         │
│  │ (Radix Dialog)  │                                                        │
│  └────────┬────────┘                                                        │
│           │                                                                  │
│           ▼                                                                  │
│  ┌─────────────────┐     ┌─────────────────┐                               │
│  │ useSettings()   │ ──▶ │ settings.store  │  ← State Layer (Zustand)      │
│  │ useTranslation()│     │ with persist    │                               │
│  └────────┬────────┘     └────────┬────────┘                               │
│           │                       │                                         │
│           ▼                       ▼                                         │
│  ┌─────────────────┐     ┌─────────────────┐                               │
│  │ i18n Context    │     │  localStorage   │  ← Persistence Layer          │
│  │ (Translations)  │     │  'edge-settings'│                               │
│  └────────┬────────┘     └─────────────────┘                               │
│           │                                                                  │
│           ▼                                                                  │
│  ┌─────────────────────────────────────────┐                               │
│  │         CSS Variables (Real-time)        │  ← Styling Layer             │
│  │  --font-scale, dark class, etc.          │                               │
│  └─────────────────────────────────────────┘                               │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 3.2 State Structure

```typescript
interface SettingsState {
  // Appearance
  theme: 'light' | 'dark' | 'system';
  fontSize: 'small' | 'medium' | 'large' | 'xlarge';
  
  // Localization
  language: 'en' | 'vi';
  
  // UI State
  isSettingsOpen: boolean;
  
  // Actions
  setTheme: (theme: SettingsState['theme']) => void;
  setFontSize: (size: SettingsState['fontSize']) => void;
  setLanguage: (lang: SettingsState['language']) => void;
  toggleSettings: () => void;
  resetToDefaults: () => void;
}
```

### 3.3 i18n Structure

```typescript
interface TranslationContext {
  locale: 'en' | 'vi';
  t: (key: string, params?: Record<string, string>) => string;
  setLocale: (locale: 'en' | 'vi') => void;
}
```

---

## 4. File Structure

### 4.1 New Files to Create

```
frontend-edge/src/
├── stores/
│   └── settings.store.ts          # NEW: Zustand store with persist + error handling
│
├── contexts/
│   └── I18nContext.tsx            # NEW: Translation context with fallbacks
│
├── locales/
│   ├── en.json                    # NEW: English translations
│   └── vi.json                    # NEW: Vietnamese translations
│
├── components/
│   └── settings/
│       ├── SettingsDialog.tsx     # NEW: Main settings modal
│       └── SettingsButton.tsx     # NEW: Trigger button for header
│
└── styles/
    └── globals.css                # UPDATE: Add font-scale + overflow handling
```

**Note:** Removed separate hook files - hooks are exported directly from contexts/I18nContext.tsx to reduce file count and improve cohesion.

### 4.2 Files to Modify

| File | Changes | Risk Level |
|------|---------|------------|
| `src/main.tsx` | Wrap app with I18nProvider | Low |
| `src/App.tsx` | Add SettingsDialog component | Low |
| `src/components/layouts/Header.tsx` | Add SettingsButton | Low |
| `src/styles/globals.css` | Add font-scale CSS variables + overflow handling | Medium |

### 4.3 Files NOT to Modify (Phase 1)

To minimize risk, Phase 1 will NOT modify these files:
- Individual page components (Dashboard, Maintenance, etc.)
- Sidebar navigation (translations added but hardcoded strings kept as fallback)
- Form components
- Table components

These will be migrated to use translations in Phase 2.

---

## 5. Implementation Details

### 5.1 Settings Store (`settings.store.ts`)

```typescript
import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';

type Theme = 'light' | 'dark' | 'system';
type FontSize = 'small' | 'medium' | 'large' | 'xlarge';
type Language = 'en' | 'vi';

interface SettingsState {
  // Settings
  theme: Theme;
  fontSize: FontSize;
  language: Language;
  
  // UI State
  isSettingsOpen: boolean;
  
  // Initialization flag
  _hasHydrated: boolean;
  
  // Actions
  setTheme: (theme: Theme) => void;
  setFontSize: (size: FontSize) => void;
  setLanguage: (lang: Language) => void;
  openSettings: () => void;
  closeSettings: () => void;
  resetToDefaults: () => void;
  
  // Internal
  setHasHydrated: (state: boolean) => void;
}

const DEFAULT_SETTINGS = {
  theme: 'system' as Theme,
  fontSize: 'medium' as FontSize,
  language: 'en' as Language,
};

// Font scale mapping with CSS clamp for safety
const FONT_SCALES: Record<FontSize, number> = {
  small: 0.875,
  medium: 1,
  large: 1.125,
  xlarge: 1.25,
};

// Safe localStorage wrapper for environments where it may not be available
const safeStorage = {
  getItem: (name: string): string | null => {
    try {
      return localStorage.getItem(name);
    } catch (error) {
      console.warn('[Settings] localStorage not available:', error);
      return null;
    }
  },
  setItem: (name: string, value: string): void => {
    try {
      localStorage.setItem(name, value);
    } catch (error) {
      console.warn('[Settings] Failed to save to localStorage:', error);
    }
  },
  removeItem: (name: string): void => {
    try {
      localStorage.removeItem(name);
    } catch (error) {
      console.warn('[Settings] Failed to remove from localStorage:', error);
    }
  },
};

export const useSettingsStore = create<SettingsState>()(
  persist(
    (set, get) => ({
      ...DEFAULT_SETTINGS,
      isSettingsOpen: false,
      _hasHydrated: false,
      
      setTheme: (theme) => {
        set({ theme });
        applyTheme(theme);
      },
      
      setFontSize: (fontSize) => {
        // Validate input
        if (!FONT_SCALES[fontSize]) {
          console.warn(`[Settings] Invalid font size: ${fontSize}, using medium`);
          fontSize = 'medium';
        }
        set({ fontSize });
        applyFontSize(fontSize);
      },
      
      setLanguage: (language) => {
        // Validate input
        if (!['en', 'vi'].includes(language)) {
          console.warn(`[Settings] Invalid language: ${language}, using en`);
          language = 'en';
        }
        set({ language });
      },
      
      openSettings: () => set({ isSettingsOpen: true }),
      closeSettings: () => set({ isSettingsOpen: false }),
      
      resetToDefaults: () => {
        set({ ...DEFAULT_SETTINGS });
        applyTheme(DEFAULT_SETTINGS.theme);
        applyFontSize(DEFAULT_SETTINGS.fontSize);
      },
      
      setHasHydrated: (state) => set({ _hasHydrated: state }),
    }),
    {
      name: 'maritime-edge-settings',
      storage: createJSONStorage(() => safeStorage),
      partialize: (state) => ({
        theme: state.theme,
        fontSize: state.fontSize,
        language: state.language,
      }),
      onRehydrateStorage: () => (state) => {
        // Called when store is rehydrated from localStorage
        if (state) {
          state.setHasHydrated(true);
          // Apply settings after hydration
          applyTheme(state.theme);
          applyFontSize(state.fontSize);
        }
      },
    }
  )
);

// ============================================
// Helper functions with error handling
// ============================================

let systemThemeListener: MediaQueryList | null = null;

function applyTheme(theme: Theme): void {
  try {
    const root = document.documentElement;
    
    // Clean up previous system theme listener
    if (systemThemeListener) {
      systemThemeListener.removeEventListener('change', handleSystemThemeChange);
      systemThemeListener = null;
    }
    
    if (theme === 'system') {
      // Set up listener for system theme changes
      systemThemeListener = window.matchMedia('(prefers-color-scheme: dark)');
      systemThemeListener.addEventListener('change', handleSystemThemeChange);
      
      const isDark = systemThemeListener.matches;
      root.classList.toggle('dark', isDark);
    } else {
      root.classList.toggle('dark', theme === 'dark');
    }
  } catch (error) {
    console.error('[Settings] Failed to apply theme:', error);
  }
}

function handleSystemThemeChange(e: MediaQueryListEvent): void {
  const currentTheme = useSettingsStore.getState().theme;
  if (currentTheme === 'system') {
    document.documentElement.classList.toggle('dark', e.matches);
  }
}

function applyFontSize(size: FontSize): void {
  try {
    const scale = FONT_SCALES[size] ?? 1;
    document.documentElement.style.setProperty('--font-scale', String(scale));
    // Also set a data attribute for CSS selectors
    document.documentElement.dataset.fontSize = size;
  } catch (error) {
    console.error('[Settings] Failed to apply font size:', error);
  }
}

// Initialize settings on module load (for SSR safety)
if (typeof window !== 'undefined') {
  // Apply default settings immediately to prevent flash
  applyTheme(DEFAULT_SETTINGS.theme);
  applyFontSize(DEFAULT_SETTINGS.fontSize);
}
```

### 5.2 Translation Files

**`locales/en.json`** (excerpt)
```json
{
  "common": {
    "save": "Save",
    "cancel": "Cancel",
    "reset": "Reset to Defaults",
    "loading": "Loading...",
    "error": "Error",
    "success": "Success"
  },
  "settings": {
    "title": "Settings",
    "appearance": "Appearance",
    "theme": {
      "label": "Theme",
      "light": "Light",
      "dark": "Dark",
      "system": "System"
    },
    "fontSize": {
      "label": "Font Size",
      "small": "Small",
      "medium": "Medium",
      "large": "Large",
      "xlarge": "Extra Large"
    },
    "language": {
      "label": "Language",
      "en": "English",
      "vi": "Tiếng Việt"
    }
  },
  "nav": {
    "dashboard": "Dashboard",
    "navigation": "Navigation",
    "engine": "Engine Room",
    "alarms": "Alarms",
    "crew": "Crew Management",
    "maintenance": "Maintenance",
    "voyage": "Voyage",
    "compliance": "Compliance",
    "materials": "Materials",
    "fuelAnalytics": "Fuel Analytics",
    "logbooks": "Logbooks",
    "reporting": "Reporting",
    "pms": "PMS",
    "sync": "Sync Status"
  },
  "dashboard": {
    "title": "Vessel Dashboard",
    "stats": {
      "crewOnboard": "Crew Onboard",
      "activeAlarms": "Active Alarms",
      "pendingMaintenance": "Pending Maintenance",
      "fuelLevel": "Fuel Level"
    }
  }
}
```

**`locales/vi.json`** (excerpt)
```json
{
  "common": {
    "save": "Lưu",
    "cancel": "Hủy",
    "reset": "Khôi phục mặc định",
    "loading": "Đang tải...",
    "error": "Lỗi",
    "success": "Thành công"
  },
  "settings": {
    "title": "Cài đặt",
    "appearance": "Giao diện",
    "theme": {
      "label": "Chủ đề",
      "light": "Sáng",
      "dark": "Tối",
      "system": "Theo hệ thống"
    },
    "fontSize": {
      "label": "Cỡ chữ",
      "small": "Nhỏ",
      "medium": "Vừa",
      "large": "Lớn",
      "xlarge": "Rất lớn"
    },
    "language": {
      "label": "Ngôn ngữ",
      "en": "English",
      "vi": "Tiếng Việt"
    }
  },
  "nav": {
    "dashboard": "Bảng điều khiển",
    "navigation": "Hàng hải",
    "engine": "Buồng máy",
    "alarms": "Cảnh báo",
    "crew": "Quản lý thuyền viên",
    "maintenance": "Bảo trì",
    "voyage": "Hành trình",
    "compliance": "Tuân thủ",
    "materials": "Vật tư",
    "fuelAnalytics": "Phân tích nhiên liệu",
    "logbooks": "Nhật ký",
    "reporting": "Báo cáo",
    "pms": "Hệ thống bảo trì",
    "sync": "Trạng thái đồng bộ"
  },
  "dashboard": {
    "title": "Bảng điều khiển tàu",
    "stats": {
      "crewOnboard": "Thuyền viên trên tàu",
      "activeAlarms": "Cảnh báo đang hoạt động",
      "pendingMaintenance": "Bảo trì đang chờ",
      "fuelLevel": "Mức nhiên liệu"
    }
  }
}
```

### 5.3 I18n Context Provider

```typescript
// contexts/I18nContext.tsx
import { createContext, useContext, useMemo, ReactNode } from 'react';
import { useSettingsStore } from '@/stores/settings.store';

// Import translations statically (small files, no need for lazy load)
import en from '@/locales/en.json';
import vi from '@/locales/vi.json';

type Language = 'en' | 'vi';
type TranslationData = typeof en;

interface I18nContextType {
  locale: Language;
  t: (key: string, params?: Record<string, string | number>) => string;
}

const translations: Record<Language, TranslationData> = { en, vi };

const I18nContext = createContext<I18nContextType | null>(null);

/**
 * Get nested value from object using dot notation
 * Returns undefined if path doesn't exist
 */
function getNestedValue(obj: any, path: string): string | undefined {
  const keys = path.split('.');
  let value = obj;
  
  for (const key of keys) {
    if (value === null || value === undefined || typeof value !== 'object') {
      return undefined;
    }
    value = value[key];
  }
  
  return typeof value === 'string' ? value : undefined;
}

export function I18nProvider({ children }: { children: ReactNode }) {
  const language = useSettingsStore((state) => state.language);
  
  // Memoize translation function to prevent unnecessary re-renders
  const contextValue = useMemo<I18nContextType>(() => {
    const currentTranslations = translations[language] ?? translations.en;
    const fallbackTranslations = translations.en;
    
    const t = (key: string, params?: Record<string, string | number>): string => {
      // Try current language first
      let value = getNestedValue(currentTranslations, key);
      
      // Fallback to English if not found
      if (value === undefined && language !== 'en') {
        value = getNestedValue(fallbackTranslations, key);
        if (value !== undefined) {
          console.debug(`[i18n] Using fallback for key: ${key}`);
        }
      }
      
      // If still not found, return key as-is (development helper)
      if (value === undefined) {
        if (process.env.NODE_ENV === 'development') {
          console.warn(`[i18n] Missing translation: ${key}`);
        }
        return key;
      }
      
      // Replace params like {name} with actual values
      if (params) {
        return value.replace(/\{(\w+)\}/g, (match, paramKey) => {
          const paramValue = params[paramKey];
          return paramValue !== undefined ? String(paramValue) : match;
        });
      }
      
      return value;
    };
    
    return { locale: language, t };
  }, [language]);
  
  return (
    <I18nContext.Provider value={contextValue}>
      {children}
    </I18nContext.Provider>
  );
}

/**
 * Hook to access translations
 * @throws Error if used outside I18nProvider
 */
export function useTranslation(): I18nContextType {
  const context = useContext(I18nContext);
  if (!context) {
    throw new Error(
      '[useTranslation] Must be used within I18nProvider. ' +
      'Wrap your app with <I18nProvider> in main.tsx'
    );
  }
  return context;
}

/**
 * Safe version that returns fallback instead of throwing
 * Useful for components that may render before provider is ready
 */
export function useTranslationSafe(): I18nContextType {
  const context = useContext(I18nContext);
  if (!context) {
    return {
      locale: 'en',
      t: (key: string) => key, // Return key as fallback
    };
  }
  return context;
}
```

### 5.4 Settings Dialog Component

```typescript
// components/settings/SettingsDialog.tsx
import * as Dialog from '@radix-ui/react-dialog';
import { X, Settings, Sun, Moon, Monitor } from 'lucide-react';
import { useSettingsStore } from '@/stores/settings.store';
import { useTranslation } from '@/contexts/I18nContext';
import { useCallback } from 'react';

type Theme = 'light' | 'dark' | 'system';
type FontSize = 'small' | 'medium' | 'large' | 'xlarge';

const THEME_OPTIONS: { value: Theme; icon: typeof Sun }[] = [
  { value: 'light', icon: Sun },
  { value: 'dark', icon: Moon },
  { value: 'system', icon: Monitor },
];

const FONT_SIZE_OPTIONS: FontSize[] = ['small', 'medium', 'large', 'xlarge'];

export function SettingsDialog() {
  const { t } = useTranslation();
  const { 
    isSettingsOpen, 
    closeSettings,
    theme, 
    setTheme,
    fontSize, 
    setFontSize,
    language, 
    setLanguage,
    resetToDefaults
  } = useSettingsStore();
  
  // Memoize handlers to prevent unnecessary re-renders
  const handleThemeChange = useCallback((newTheme: Theme) => {
    setTheme(newTheme);
  }, [setTheme]);
  
  const handleFontSizeChange = useCallback((e: React.ChangeEvent<HTMLSelectElement>) => {
    setFontSize(e.target.value as FontSize);
  }, [setFontSize]);
  
  const handleLanguageChange = useCallback((e: React.ChangeEvent<HTMLSelectElement>) => {
    setLanguage(e.target.value as 'en' | 'vi');
  }, [setLanguage]);
  
  const handleReset = useCallback(() => {
    if (window.confirm(t('settings.confirmReset') || 'Reset all settings to defaults?')) {
      resetToDefaults();
    }
  }, [resetToDefaults, t]);
  
  return (
    <Dialog.Root open={isSettingsOpen} onOpenChange={(open) => !open && closeSettings()}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 bg-black/50 z-50 animate-in fade-in" />
        <Dialog.Content 
          className="fixed top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 
            bg-white dark:bg-gray-800 rounded-lg shadow-xl z-50 w-full max-w-md p-6
            animate-in fade-in zoom-in-95"
          onEscapeKeyDown={closeSettings}
        >
          {/* Header */}
          <div className="flex items-center justify-between mb-6">
            <Dialog.Title className="text-xl font-semibold flex items-center gap-2 text-gray-900 dark:text-white">
              <Settings className="w-5 h-5" />
              {t('settings.title')}
            </Dialog.Title>
            <Dialog.Close 
              className="p-2 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-md transition-colors"
              aria-label={t('common.close')}
            >
              <X className="w-4 h-4" />
            </Dialog.Close>
          </div>
          
          <div className="space-y-6">
            {/* Theme Selection */}
            <div>
              <label className="text-sm font-medium mb-3 block text-gray-700 dark:text-gray-300">
                {t('settings.theme.label')}
              </label>
              <div className="grid grid-cols-3 gap-2">
                {THEME_OPTIONS.map(({ value, icon: Icon }) => (
                  <button
                    key={value}
                    type="button"
                    onClick={() => handleThemeChange(value)}
                    className={`p-3 rounded-md border-2 flex flex-col items-center gap-2 transition-all
                      ${theme === value 
                        ? 'border-blue-500 bg-blue-50 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300' 
                        : 'border-gray-200 dark:border-gray-600 hover:border-gray-300 dark:hover:border-gray-500'
                      }`}
                    aria-pressed={theme === value}
                  >
                    <Icon className="w-5 h-5" />
                    <span className="text-xs font-medium">{t(`settings.theme.${value}`)}</span>
                  </button>
                ))}
              </div>
            </div>
            
            {/* Font Size */}
            <div>
              <label 
                htmlFor="fontSize" 
                className="text-sm font-medium mb-2 block text-gray-700 dark:text-gray-300"
              >
                {t('settings.fontSize.label')}
              </label>
              <select 
                id="fontSize"
                value={fontSize}
                onChange={handleFontSizeChange}
                className="w-full p-2.5 border border-gray-300 dark:border-gray-600 rounded-md 
                  bg-white dark:bg-gray-700 text-gray-900 dark:text-white
                  focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors"
              >
                {FONT_SIZE_OPTIONS.map((size) => (
                  <option key={size} value={size}>
                    {t(`settings.fontSize.${size}`)}
                  </option>
                ))}
              </select>
              {/* Preview */}
              <p className="mt-2 text-gray-500 dark:text-gray-400" style={{ fontSize: 'var(--preview-size, 14px)' }}>
                {t('settings.fontSize.preview') || 'Preview text'}
              </p>
            </div>
            
            {/* Language */}
            <div>
              <label 
                htmlFor="language" 
                className="text-sm font-medium mb-2 block text-gray-700 dark:text-gray-300"
              >
                {t('settings.language.label')}
              </label>
              <select 
                id="language"
                value={language}
                onChange={handleLanguageChange}
                className="w-full p-2.5 border border-gray-300 dark:border-gray-600 rounded-md 
                  bg-white dark:bg-gray-700 text-gray-900 dark:text-white
                  focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors"
              >
                <option value="en">🇬🇧 English</option>
                <option value="vi">🇻🇳 Tiếng Việt</option>
              </select>
            </div>
          </div>
          
          {/* Footer */}
          <div className="mt-6 pt-4 border-t border-gray-200 dark:border-gray-700 flex justify-between items-center">
            <button 
              type="button"
              onClick={handleReset}
              className="text-sm text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200 transition-colors"
            >
              {t('common.reset')}
            </button>
            <button 
              type="button"
              onClick={closeSettings}
              className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-md font-medium transition-colors"
            >
              {t('common.done') || 'Done'}
            </button>
          </div>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
```

### 5.5 Settings Button Component

```typescript
// components/settings/SettingsButton.tsx
import { Settings } from 'lucide-react';
import { useSettingsStore } from '@/stores/settings.store';
import { useTranslationSafe } from '@/contexts/I18nContext';

export function SettingsButton() {
  const openSettings = useSettingsStore((state) => state.openSettings);
  const { t } = useTranslationSafe();
  
  return (
    <button
      type="button"
      onClick={openSettings}
      className="p-2 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-md transition-colors"
      aria-label={t('settings.title')}
      title={t('settings.title')}
    >
      <Settings className="w-5 h-5 text-gray-600 dark:text-gray-300" />
    </button>
  );
}
```

### 5.6 CSS Variables Update

```css
/* Add to globals.css - BEFORE @tailwind directives */

:root {
  /* Font scaling - default 1 (16px base) */
  --font-scale: 1;
}

/* ============================================
   Font Scaling System
   ============================================ */

/* 
 * Apply font scaling to root element
 * This affects all rem-based sizes in Tailwind
 */
html {
  font-size: calc(16px * var(--font-scale));
}

/* 
 * Elements that should NOT scale (icons, fixed UI elements)
 * Use this class sparingly
 */
.font-fixed {
  font-size: 16px !important;
}

/* 
 * Prevent text overflow at large font sizes
 * Add to containers that may overflow
 */
.text-safe {
  overflow-wrap: break-word;
  word-wrap: break-word;
  hyphens: auto;
}

/* 
 * Font size specific adjustments
 * Use data-font-size attribute set by settings store
 */
html[data-font-size="xlarge"] {
  /* Reduce padding/margins slightly at xlarge to prevent overflow */
  --spacing-scale: 0.9;
}

html[data-font-size="xlarge"] .sidebar-nav {
  /* Sidebar text may need truncation at xlarge */
  overflow: hidden;
  text-overflow: ellipsis;
}

/* ============================================
   Theme Transition (prevent flash)
   ============================================ */

/* Smooth transition when theme changes */
html {
  transition: background-color 0.2s ease, color 0.2s ease;
}

/* Disable transition on initial load to prevent flash */
html.no-transition,
html.no-transition * {
  transition: none !important;
}

/* ============================================
   Accessibility Improvements
   ============================================ */

/* Ensure focus is visible */
*:focus-visible {
  outline: 2px solid hsl(var(--ring));
  outline-offset: 2px;
}

/* Reduce motion for users who prefer it */
@media (prefers-reduced-motion: reduce) {
  *,
  *::before,
  *::after {
    animation-duration: 0.01ms !important;
    animation-iteration-count: 1 !important;
    transition-duration: 0.01ms !important;
  }
}
```

### 5.7 Main Entry Point Update

```typescript
// main.tsx - Updated with proper provider order
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { I18nProvider } from '@/contexts/I18nContext'
import './styles/globals.css'
import App from './App.tsx'

// Remove no-transition class after initial render to enable smooth transitions
const enableTransitions = () => {
  document.documentElement.classList.remove('no-transition');
};

// Add no-transition class initially to prevent theme flash
document.documentElement.classList.add('no-transition');

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <I18nProvider>
        <App />
      </I18nProvider>
    </BrowserRouter>
  </StrictMode>,
)

// Enable transitions after a short delay
requestAnimationFrame(() => {
  requestAnimationFrame(enableTransitions);
});
```

### 5.8 App Component Update

```typescript
// App.tsx - Add SettingsDialog
import { Routes, Route, Navigate } from 'react-router-dom'
import { Toaster } from 'sonner'
import { MainLayout } from './components/layouts/MainLayout'
import { SettingsDialog } from './components/settings/SettingsDialog'

// ... existing imports ...

function App() {
  return (
    <>
      {/* Global toast provider */}
      <Toaster position="top-right" />
      
      {/* Settings Dialog (renders as portal) */}
      <SettingsDialog />

      <Routes>
        {/* ... existing routes ... */}
      </Routes>
    </>
  )
}
```

---

## 6. Translation Keys

### 6.1 Key Naming Convention

```
{module}.{section}.{key}

Examples:
- nav.dashboard           → "Dashboard"
- settings.theme.dark     → "Dark"
- dashboard.stats.crewOnboard → "Crew Onboard"
- alarms.severity.critical → "Critical"
```

### 6.2 Full Translation Key Map

| Module | Keys Count (Est.) | Priority |
|--------|-------------------|----------|
| `common` | ~20 | P0 |
| `settings` | ~15 | P0 |
| `nav` | ~20 | P0 |
| `dashboard` | ~30 | P0 |
| `alarms` | ~25 | P1 |
| `maintenance` | ~50 | P1 |
| `crew` | ~40 | P1 |
| `logbooks` | ~60 | P2 |
| `reporting` | ~50 | P2 |

**Total Estimated: ~310 keys**

---

## 7. UI/UX Design

### 7.1 Settings Dialog Layout

```
┌──────────────────────────────────────────────┐
│ ⚙️ Settings                              [X] │
├──────────────────────────────────────────────┤
│                                              │
│ Theme                                        │
│ ┌──────────┬──────────┬──────────┐          │
│ │  ☀️      │  🌙      │  💻      │          │
│ │  Light   │  Dark    │  System  │          │
│ └──────────┴──────────┴──────────┘          │
│                                              │
│ Font Size                                    │
│ ┌────────────────────────────────────┐      │
│ │ Medium                           ▼ │      │
│ └────────────────────────────────────┘      │
│                                              │
│ Language                                     │
│ ┌────────────────────────────────────┐      │
│ │ 🇬🇧 English                       ▼ │      │
│ └────────────────────────────────────┘      │
│                                              │
├──────────────────────────────────────────────┤
│ Reset to Defaults              [ Save ]      │
└──────────────────────────────────────────────┘
```

### 7.2 Settings Button Location

```
┌─────────────────────────────────────────────────────────────────┐
│ 🚢 Maritime Edge         [Status]     [🔔] [⚙️] [👤 User]     │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│                    Main Content Area                             │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
                                           ↑
                                    Settings Button
```

---

## 8. Testing Plan

### 8.1 Unit Tests

| Test Case | Description | Expected |
|-----------|-------------|----------|
| Settings Store - setTheme | Call setTheme('dark') | theme === 'dark', dark class on html |
| Settings Store - setFontSize | Call setFontSize('large') | fontSize === 'large', CSS var updated |
| Settings Store - invalid input | Call setFontSize('invalid') | Fallback to 'medium', no crash |
| Settings Store - resetToDefaults | Call after changes | All values reset to defaults |
| Persistence - save | Change settings | localStorage updated |
| Persistence - load | Refresh page | Settings restored |
| Persistence - corrupt data | Corrupt localStorage | Fallback to defaults, no crash |
| Translation - valid key | t('common.save') | Returns "Save" or "Lưu" |
| Translation - missing key | t('invalid.key') | Returns "invalid.key", console warn |
| Translation - params | t('greeting', {name: 'John'}) | Returns "Hello John" |

### 8.2 Integration Tests

| Test Case | Steps | Expected |
|-----------|-------|----------|
| Theme Persistence | 1. Set dark → 2. Refresh → 3. Check | Still dark theme |
| System Theme | 1. Set system → 2. Change OS theme | UI follows OS |
| Language Switch | 1. Switch to VI → 2. Check nav | All nav items in Vietnamese |
| Font Scale | 1. Set xlarge → 2. Check all pages | Text scaled, no overflow |
| localStorage disabled | 1. Block localStorage → 2. Use app | Works with defaults |

### 8.3 Manual Testing Checklist

**Theme Testing:**
- [ ] Light theme applies correctly
- [ ] Dark theme applies correctly
- [ ] System theme follows OS preference
- [ ] System theme updates when OS changes (without refresh)
- [ ] No flash of wrong theme on page load
- [ ] Theme persists after refresh
- [ ] Theme persists after browser close/reopen

**Font Size Testing:**
- [ ] Small (0.875x) - all text readable
- [ ] Medium (1x) - baseline, matches current design
- [ ] Large (1.125x) - no layout overflow
- [ ] Extra Large (1.25x) - no layout overflow
- [ ] Test on Dashboard page at xlarge
- [ ] Test on Maintenance page at xlarge
- [ ] Test on Forms at xlarge
- [ ] Test Sidebar text at xlarge (check truncation)
- [ ] Tables don't break at xlarge

**Language Testing:**
- [ ] English translations complete for nav
- [ ] Vietnamese translations complete for nav
- [ ] Settings dialog fully translated
- [ ] Language change is instant (no refresh)
- [ ] Fallback to English for missing keys
- [ ] No "undefined" or blank text

**Accessibility:**
- [ ] Settings dialog keyboard accessible
- [ ] Tab order makes sense
- [ ] Escape closes dialog
- [ ] Focus trap inside dialog
- [ ] Screen reader announces dialog title
- [ ] Color contrast sufficient

**Error Handling:**
- [ ] App works in private/incognito mode
- [ ] App works with localStorage disabled
- [ ] App works with corrupt localStorage data
- [ ] No console errors in normal usage

### 8.4 Edge Cases to Test

```typescript
// Test these scenarios:

// 1. Corrupt localStorage
localStorage.setItem('maritime-edge-settings', 'not valid json');
// Expected: App loads with defaults, no crash

// 2. Partial localStorage
localStorage.setItem('maritime-edge-settings', '{"theme":"dark"}');
// Expected: theme=dark, fontSize/language=defaults

// 3. Invalid values in localStorage
localStorage.setItem('maritime-edge-settings', '{"theme":"purple","fontSize":"huge"}');
// Expected: Falls back to valid defaults

// 4. localStorage quota exceeded
// Fill localStorage to quota, then try to save settings
// Expected: Warning in console, app continues working

// 5. Concurrent tabs
// Open 2 tabs, change settings in one
// Expected: Other tab should NOT break (may need refresh to sync)
```

---

## 9. Migration Strategy

### 9.1 Gradual i18n Migration

**Phase 1: Infrastructure (This PR)**
- Create settings store and i18n context
- Add translation files with common keys
- Translate navigation and settings UI

**Phase 2: Core Pages**
- Dashboard translations
- Alarms page translations
- Maintenance page translations

**Phase 3: Complete Coverage**
- All remaining pages
- Error messages
- Toast notifications
- Form validation messages

### 9.2 Code Migration Pattern

```typescript
// Before
<h1>Dashboard</h1>

// After
const { t } = useTranslation();
<h1>{t('dashboard.title')}</h1>
```

---

## 10. Timeline

### 10.1 Estimated Effort

| Task | Hours | Notes |
|------|-------|-------|
| Settings Store (with error handling) | 1.5h | Includes safe storage wrapper |
| I18n Context + Hooks | 1.5h | Includes fallback mechanisms |
| Translation Files (Core) | 2h | nav, settings, common modules |
| SettingsDialog Component | 2h | With accessibility |
| SettingsButton + Header integration | 0.5h | Simple addition |
| CSS Variables + Overflow fixes | 1h | Test at all font sizes |
| main.tsx + App.tsx updates | 0.5h | Provider wrapping |
| Testing & Bug Fixes | 2h | Extended for edge cases |
| **Total** | **~11 hours** | +2h buffer from original |

### 10.2 Implementation Order

```
Day 1 (4h):
├── 1. Update globals.css with font-scale variables
├── 2. Create settings.store.ts (with safe storage)
├── 3. Create I18nContext.tsx (with fallbacks)
└── 4. Create locales/en.json, vi.json (core keys only)

Day 2 (4h):
├── 5. Create SettingsDialog.tsx
├── 6. Create SettingsButton.tsx  
├── 7. Update Header.tsx (add SettingsButton)
├── 8. Update main.tsx (wrap with I18nProvider)
└── 9. Update App.tsx (add SettingsDialog)

Day 3 (3h):
├── 10. Test all features thoroughly
├── 11. Test edge cases (corrupt data, no localStorage)
├── 12. Fix any overflow issues at xlarge
└── 13. Final review & documentation
```

---

## 11. ⚠️ Known Issues & Mitigations

### 11.1 Issue: Variable Shadowing Bug (FIXED)

**Original Code (BUGGY):**
```typescript
// ❌ BUG: 't' shadows the translation function!
{(['light', 'dark', 'system'] as const).map((t) => (
  <span>{t(`settings.theme.${t}`)}</span>  // t is now 'light'|'dark'|'system', not function!
))}
```

**Fixed Code:**
```typescript
// ✅ FIXED: Use different variable name
{THEME_OPTIONS.map(({ value, icon: Icon }) => (
  <span>{t(`settings.theme.${value}`)}</span>  // t is still the translation function
))}
```

### 11.2 Issue: Font Scaling with Fixed Tailwind Classes

**Problem:** Components use `text-sm`, `text-lg` etc. which are rem-based BUT tied to Tailwind's 16px base.

**Solution:** Our approach DOES work because:
1. We change `html { font-size: calc(16px * var(--font-scale)) }`
2. Tailwind's `text-sm` = `0.875rem` = `0.875 * html font-size`
3. So when `--font-scale: 1.25`, `text-sm` becomes `0.875 * 20px = 17.5px`

**However**, watch out for:
- `px` values (icons, borders) - these won't scale
- Hardcoded inline styles - these won't scale
- Fixed-width containers - may overflow

### 11.3 Issue: System Theme Listener Cleanup

**Problem:** If not cleaned up, multiple listeners accumulate on theme changes.

**Solution:** Store reference and remove before adding new:
```typescript
// Clean up previous listener
if (systemThemeListener) {
  systemThemeListener.removeEventListener('change', handleSystemThemeChange);
}
// Add new listener only for 'system' theme
```

### 11.4 Issue: Hydration Mismatch (SSR)

**Problem:** If using SSR/SSG, server renders with defaults, client has stored settings.

**Solution:** 
1. Add `_hasHydrated` flag to store
2. Use `onRehydrateStorage` callback
3. Apply settings only after hydration

**Note:** Current app is SPA (Vite), so this is low risk, but code is future-proof.

### 11.5 Issue: localStorage Unavailable

**Scenarios:**
- Private/Incognito mode (some browsers)
- Storage quota exceeded
- User disabled storage
- Safari with "Block all cookies"

**Solution:** Safe storage wrapper that catches all errors and returns defaults.

---

## 12. 🛡️ Error Handling Strategy

### 12.1 Defense Layers

```
┌─────────────────────────────────────────────────────────────┐
│                    ERROR HANDLING LAYERS                     │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Layer 1: Input Validation                                   │
│  ├── Validate theme values ('light'|'dark'|'system')        │
│  ├── Validate fontSize values ('small'|'medium'|'large'|'xlarge')
│  └── Validate language values ('en'|'vi')                   │
│                                                              │
│  Layer 2: Storage Safety                                     │
│  ├── Try/catch around localStorage operations               │
│  ├── JSON parse with try/catch                              │
│  └── Fallback to defaults on any error                      │
│                                                              │
│  Layer 3: Runtime Safety                                     │
│  ├── Check typeof window !== 'undefined'                    │
│  ├── Check document.documentElement exists                  │
│  └── Graceful degradation if DOM not available              │
│                                                              │
│  Layer 4: Translation Fallbacks                             │
│  ├── Missing key → return key string                        │
│  ├── Missing language → fallback to English                 │
│  └── Invalid params → preserve placeholder                  │
│                                                              │
│  Layer 5: UI Feedback                                        │
│  ├── Console warnings in development                        │
│  ├── No user-facing errors for settings                     │
│  └── App continues working with defaults                    │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### 12.2 Error Recovery Matrix

| Error Type | Detection | Recovery | User Impact |
|------------|-----------|----------|-------------|
| Invalid theme value | Type check | Use 'system' | None |
| Invalid fontSize | Type check | Use 'medium' | None |
| Invalid language | Type check | Use 'en' | None |
| localStorage blocked | Try/catch | Use memory only | Settings don't persist |
| Corrupt JSON | JSON.parse catch | Clear & use defaults | Settings reset |
| Missing translation | Key lookup | Return key string | See raw key |
| DOM not ready | typeof check | Defer application | Brief flash |

### 12.3 Logging Strategy

```typescript
// Development only - don't spam production logs
if (process.env.NODE_ENV === 'development') {
  console.warn('[Settings] Invalid value:', value);
  console.debug('[i18n] Fallback used for:', key);
}

// Always log critical errors
console.error('[Settings] Critical: Unable to apply theme');
```

---

## 13. Approval

| Role | Name | Status |
|------|------|--------|
| Developer | | Pending |
| Reviewer | | Pending |
| QA | | Pending |

---

**Next Step:** Review this hardened plan and confirm to start implementation.
