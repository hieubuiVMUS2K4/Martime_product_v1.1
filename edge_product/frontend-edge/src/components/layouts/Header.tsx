import { useMaritimeStore } from '@/stores/maritime.store'
import { Wifi, WifiOff, RefreshCw } from 'lucide-react'
import { format } from 'date-fns'
import { SettingsButton } from '@/components/settings'
import { useTranslationSafe } from '@/contexts/I18nContext'
import { UserMenu } from './UserMenu'

export function Header() {
  const { isOnline, isSyncing, lastSyncTime } = useMaritimeStore()
  const { t } = useTranslationSafe()

  return (
    <header className="h-16 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 flex items-center justify-between px-6">
      <div className="flex items-center space-x-4">
        <h1 className="text-xl font-semibold text-gray-800 dark:text-white">
          {t('header.title')}
        </h1>
      </div>

      <div className="flex items-center space-x-4">
        {/* Sync Status */}
        <div className="flex items-center space-x-2 text-sm">
          {isSyncing ? (
            <>
              <RefreshCw className="w-4 h-4 animate-spin text-yellow-500" />
              <span className="text-gray-600 dark:text-gray-300">{t('header.syncing')}</span>
            </>
          ) : lastSyncTime ? (
            <span className="text-gray-600 dark:text-gray-300">
              {t('header.lastSync', { time: format(lastSyncTime, 'HH:mm:ss') })}
            </span>
          ) : null}
        </div>

        {/* Connection Status */}
        <div className="flex items-center space-x-2">
          {isOnline ? (
            <>
              <Wifi className="w-5 h-5 text-green-500" />
              <span className="text-sm text-gray-600 dark:text-gray-300">{t('header.online')}</span>
            </>
          ) : (
            <>
              <WifiOff className="w-5 h-5 text-red-500" />
              <span className="text-sm text-gray-600 dark:text-gray-300">{t('header.offline')}</span>
            </>
          )}
        </div>

        {/* Settings Button */}
        <SettingsButton />

        {/* Current Time */}
        <div className="text-sm text-gray-600 dark:text-gray-300">
          {format(new Date(), 'dd MMM yyyy HH:mm')}
        </div>

        {/* User Menu / Logout */}
        <div className="border-l border-gray-200 dark:border-gray-700 pl-4 ml-1">
          <UserMenu />
        </div>
      </div>
    </header>
  )
}
