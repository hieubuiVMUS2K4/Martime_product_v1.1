import { useMaritimeStore } from '@/stores/maritime.store'
import { Wifi, WifiOff, RefreshCw } from 'lucide-react'
import { format } from 'date-fns'

export function Header() {
  const { isOnline, isSyncing, lastSyncTime } = useMaritimeStore()

  return (
    <header className="h-16 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 flex items-center justify-between px-6">
      <div className="flex items-center space-x-4">
        <h1 className="text-xl font-semibold text-gray-800 dark:text-white">
          Vessel Operations
        </h1>
      </div>

      <div className="flex items-center space-x-4">
        {/* Sync Status */}
        <div className="flex items-center space-x-2 text-sm">
          {isSyncing ? (
            <>
              <RefreshCw className="w-4 h-4 animate-spin text-yellow-500" />
              <span className="text-gray-600 dark:text-gray-300">Syncing...</span>
            </>
          ) : lastSyncTime ? (
            <span className="text-gray-600 dark:text-gray-300">
              Last sync: {format(lastSyncTime, 'HH:mm:ss')}
            </span>
          ) : null}
        </div>

        {/* Connection Status */}
        <div className="flex items-center space-x-2">
          {isOnline ? (
            <>
              <Wifi className="w-5 h-5 text-green-500" />
              <span className="text-sm text-gray-600 dark:text-gray-300">Online</span>
            </>
          ) : (
            <>
              <WifiOff className="w-5 h-5 text-red-500" />
              <span className="text-sm text-gray-600 dark:text-gray-300">Offline</span>
            </>
          )}
        </div>

        {/* Current Time */}
        <div className="text-sm text-gray-600 dark:text-gray-300">
          {format(new Date(), 'dd MMM yyyy HH:mm')}
        </div>
      </div>
    </header>
  )
}
