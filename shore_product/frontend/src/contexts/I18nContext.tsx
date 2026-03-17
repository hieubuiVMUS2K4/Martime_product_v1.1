import { createContext, useContext, type ReactNode } from 'react';

// Shore only uses Vietnamese
import vi from '@/locales/vi.json';

type Language = 'vi';

// ============================================
// Types
// ============================================

type TranslationData = typeof vi;

interface I18nContextType {
  locale: Language;
  t: (key: string, params?: Record<string, string | number>) => string;
}

// ============================================
// Translation Data
// ============================================

const translations: Record<Language, TranslationData> = { vi };

// ============================================
// Context
// ============================================

const I18nContext = createContext<I18nContextType | null>(null);

// ============================================
// Helper Functions
// ============================================

/**
 * Get nested value from object using dot notation
 * Example: getNestedValue(obj, 'settings.theme.label') 
 * Returns undefined if path doesn't exist
 */
function getNestedValue(obj: unknown, path: string): string | undefined {
  const keys = path.split('.');
  let value: unknown = obj;
  
  for (const key of keys) {
    if (value === null || value === undefined || typeof value !== 'object') {
      return undefined;
    }
    value = (value as Record<string, unknown>)[key];
  }
  
  return typeof value === 'string' ? value : undefined;
}

/**
 * Replace placeholders like {name} with actual values
 * Example: interpolate('Hello {name}!', { name: 'John' }) => 'Hello John!'
 */
function interpolate(text: string, params: Record<string, string | number>): string {
  return text.replace(/\{(\w+)\}/g, (match, paramKey) => {
    const paramValue = params[paramKey];
    return paramValue !== undefined ? String(paramValue) : match;
  });
}

// ============================================
// Provider Component
// ============================================

export function I18nProvider({ children }: { children: ReactNode }) {
  const language: Language = 'vi';
  
  const currentTranslations = translations[language];
    
  const t = (key: string, params?: Record<string, string | number>): string => {
    if (!key || typeof key !== 'string') return String(key);
    
    let value = getNestedValue(currentTranslations, key);
    
    if (value === undefined) return key;
    
    if (params && Object.keys(params).length > 0) {
      return interpolate(value, params);
    }
    
    return value;
  };

  const contextValue: I18nContextType = { locale: language, t };
  
  return (
    <I18nContext.Provider value={contextValue}>
      {children}
    </I18nContext.Provider>
  );
}

// ============================================
// Hooks
// ============================================

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
    // Return fallback that just returns the key
    return {
      locale: 'vi',
      t: (key: string) => key,
    };
  }
  return context;
}

// ============================================
// Utility Exports
// ============================================

export { I18nContext };
export type { I18nContextType };
