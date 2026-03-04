import { useTranslationSafe } from '@/contexts/I18nContext';

export function AlarmsPage() {
  const { t } = useTranslationSafe();
  
  return (
    <div className="h-full w-full overflow-y-auto bg-gradient-to-br from-slate-50 via-blue-50 to-slate-100 dark:from-gray-900 dark:via-gray-800 dark:to-gray-900">
      <div className="text-center py-12">
        <h2 className="text-2xl font-bold text-gray-900 dark:text-white">{t('alarms.title')}</h2>
        <p className="text-gray-600 dark:text-gray-400 mt-2">{t('common.comingSoon')}</p>
      </div>
    </div>
  )
}
