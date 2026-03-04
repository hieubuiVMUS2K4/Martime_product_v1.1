/**
 * Reusable Error Message Component
 * Displays user-friendly error messages with icons
 */

import { AlertCircle, AlertTriangle, WifiOff, Lock, Ban, Server, Database } from 'lucide-react';

interface ErrorMessageProps {
  message: string;
  onRetry?: () => void;
  className?: string;
}

export function ErrorMessage({ message, onRetry, className = '' }: ErrorMessageProps) {
  // Determine icon based on error type
  const getIcon = () => {
    const msg = message.toLowerCase();
    
    if (msg.includes('🌐') || msg.includes('network') || msg.includes('kết nối')) {
      return <WifiOff className="h-5 w-5 text-red-600 flex-shrink-0" />;
    }
    if (msg.includes('🔒') || msg.includes('unauthorized') || msg.includes('đăng nhập')) {
      return <Lock className="h-5 w-5 text-red-600 flex-shrink-0" />;
    }
    if (msg.includes('⛔') || msg.includes('forbidden') || msg.includes('quyền')) {
      return <Ban className="h-5 w-5 text-red-600 flex-shrink-0" />;
    }
    if (msg.includes('⚠️') || msg.includes('500') || msg.includes('máy chủ')) {
      return <Server className="h-5 w-5 text-red-600 flex-shrink-0" />;
    }
    if (msg.includes('🗄️') || msg.includes('database') || msg.includes('cơ sở dữ liệu')) {
      return <Database className="h-5 w-5 text-red-600 flex-shrink-0" />;
    }
    
    return <AlertCircle className="h-5 w-5 text-red-600 flex-shrink-0" />;
  };

  return (
    <div className={`bg-red-50 border border-red-200 rounded-lg p-4 ${className}`}>
      <div className="flex items-start gap-3">
        {getIcon()}
        <div className="flex-1 min-w-0">
          <p className="text-sm font-medium text-red-800 mb-1">Lỗi</p>
          <p className="text-sm text-red-700 whitespace-pre-line leading-relaxed">
            {message}
          </p>
          {onRetry && (
            <button
              onClick={onRetry}
              className="mt-3 px-4 py-2 bg-red-600 text-white text-sm font-medium rounded-lg 
                       hover:bg-red-700 transition-colors focus:outline-none focus:ring-2 
                       focus:ring-red-500 focus:ring-offset-2"
            >
              Thử lại
            </button>
          )}
        </div>
      </div>
    </div>
  );
}

/**
 * Success Message Component
 */
interface SuccessMessageProps {
  message: string;
  onClose?: () => void;
  className?: string;
}

export function SuccessMessage({ message, onClose, className = '' }: SuccessMessageProps) {
  return (
    <div className={`bg-green-50 border border-green-200 rounded-lg p-4 ${className}`}>
      <div className="flex items-start gap-3">
        <AlertCircle className="h-5 w-5 text-green-600 flex-shrink-0" />
        <div className="flex-1 min-w-0">
          <p className="text-sm font-medium text-green-800 mb-1">Thành công</p>
          <p className="text-sm text-green-700 whitespace-pre-line leading-relaxed">
            {message}
          </p>
        </div>
        {onClose && (
          <button
            onClick={onClose}
            className="text-green-600 hover:text-green-800 transition-colors"
            aria-label="Close"
          >
            ×
          </button>
        )}
      </div>
    </div>
  );
}

/**
 * Warning Message Component
 */
interface WarningMessageProps {
  message: string;
  className?: string;
}

export function WarningMessage({ message, className = '' }: WarningMessageProps) {
  return (
    <div className={`bg-yellow-50 border border-yellow-200 rounded-lg p-4 ${className}`}>
      <div className="flex items-start gap-3">
        <AlertTriangle className="h-5 w-5 text-yellow-600 flex-shrink-0" />
        <div className="flex-1 min-w-0">
          <p className="text-sm font-medium text-yellow-800 mb-1">Cảnh báo</p>
          <p className="text-sm text-yellow-700 whitespace-pre-line leading-relaxed">
            {message}
          </p>
        </div>
      </div>
    </div>
  );
}
