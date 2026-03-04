import { Settings } from 'lucide-react';
import { useSettingsStore } from '@/stores/settings.store';
import { useTranslationSafe } from '@/contexts/I18nContext';

/**
 * Settings button for the header
 * Uses useTranslationSafe to handle cases where it renders before I18nProvider
 */
export function SettingsButton() {
  const openSettings = useSettingsStore((state) => state.openSettings);
  const { t } = useTranslationSafe();

  return (
    <button
      type="button"
      onClick={openSettings}
      className="p-2 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-md transition-colors
        focus:outline-none focus:ring-2 focus:ring-blue-500"
      aria-label={t('settings.title')}
      title={t('settings.title')}
    >
      <Settings className="w-5 h-5 text-gray-600 dark:text-gray-300" />
    </button>
  );
}

export default SettingsButton;
