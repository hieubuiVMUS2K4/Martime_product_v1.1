import React, { createContext, useContext, useState, useCallback, useRef } from 'react';
import { AlertTriangle, HelpCircle, Trash2, Info } from 'lucide-react';
import './ConfirmDialog.css';

type DialogVariant = 'danger' | 'warning' | 'info' | 'default';

interface DialogOptions {
  title: string;
  message: string;
  confirmLabel?: string;
  confirmText?: string;
  cancelLabel?: string;
  variant?: DialogVariant;
  /** If true, show a text input for the user to provide a reason */
  withInput?: boolean;
  showInput?: boolean;
  inputLabel?: string;
  inputPlaceholder?: string;
  inputRequired?: boolean;
}

interface DialogResult {
  confirmed: boolean;
  inputValue?: string;
}

interface ConfirmDialogContextValue {
  confirm: (options: DialogOptions) => Promise<DialogResult>;
}

const ConfirmDialogContext = createContext<ConfirmDialogContextValue | null>(null);

export const useConfirmDialog = (): ConfirmDialogContextValue => {
  const ctx = useContext(ConfirmDialogContext);
  if (!ctx) throw new Error('useConfirmDialog must be used within ConfirmDialogProvider');
  return ctx;
};

const ICONS: Record<DialogVariant, React.ReactNode> = {
  danger: <Trash2 size={22} />,
  warning: <AlertTriangle size={22} />,
  info: <Info size={22} />,
  default: <HelpCircle size={22} />,
};

export const ConfirmDialogProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [dialog, setDialog] = useState<DialogOptions | null>(null);
  const [inputValue, setInputValue] = useState('');
  const resolverRef = useRef<((result: DialogResult) => void) | null>(null);

  const confirm = useCallback((options: DialogOptions): Promise<DialogResult> => {
    setDialog(options);
    setInputValue('');
    return new Promise<DialogResult>(resolve => {
      resolverRef.current = resolve;
    });
  }, []);

  const handleConfirm = () => {
    const shouldShowInput = dialog?.withInput || dialog?.showInput;
    if (shouldShowInput && dialog?.inputRequired && !inputValue.trim()) return;
    resolverRef.current?.({ confirmed: true, inputValue: inputValue.trim() || undefined });
    setDialog(null);
  };

  const handleCancel = () => {
    resolverRef.current?.({ confirmed: false });
    setDialog(null);
  };

  const variant = dialog?.variant || 'default';
  const shouldShowInput = !!(dialog?.withInput || dialog?.showInput);

  return (
    <ConfirmDialogContext.Provider value={{ confirm }}>
      {children}
      {dialog && (
        <div className="confirm-overlay" onClick={handleCancel}>
          <div className="confirm-dialog" onClick={e => e.stopPropagation()} role="alertdialog">
            <div className={`confirm-icon confirm-icon--${variant}`}>
              {ICONS[variant]}
            </div>
            <h3 className="confirm-title">{dialog.title}</h3>
            <p className="confirm-message">{dialog.message}</p>

            {shouldShowInput && (
              <div className="confirm-input-group">
                {dialog.inputLabel && <label className="confirm-input-label">{dialog.inputLabel}</label>}
                <textarea
                  className="confirm-input"
                  value={inputValue}
                  onChange={e => setInputValue(e.target.value)}
                  placeholder={dialog.inputPlaceholder || ''}
                  rows={2}
                  autoFocus
                />
              </div>
            )}

            <div className="confirm-actions">
              <button className="confirm-btn confirm-btn--cancel" onClick={handleCancel}>
                {dialog.cancelLabel || 'Hủy'}
              </button>
              <button
                className={`confirm-btn confirm-btn--${variant}`}
                onClick={handleConfirm}
                disabled={shouldShowInput && dialog.inputRequired && !inputValue.trim()}
              >
                {dialog.confirmLabel || dialog.confirmText || 'Xác nhận'}
              </button>
            </div>
          </div>
        </div>
      )}
    </ConfirmDialogContext.Provider>
  );
};
