import { useState, useEffect, useCallback, useRef } from 'react';
import { draftsDb } from '../lib/draftsDb';
import { toast } from 'sonner';

/**
 * Custom hook for offline draft auto-save inside large forms.
 * Periodically saves the form state to IndexedDB to protect against browser crashes or network dropouts.
 * 
 * @param key Unique key for the draft, e.g. 'shore-crew-form'
 * @param currentState Current React state of the form
 * @param onRestore Callback when user chooses to restore the draft
 */
export function useOfflineDraft<T>(
  key: string,
  currentState: T,
  onRestore?: (restoredState: T) => void
) {
  const [hasDraft, setHasDraft] = useState(false);
  const [restoredDraft, setRestoredDraft] = useState<T | null>(null);
  const [lastSaved, setLastSaved] = useState<number | null>(null);
  const stateRef = useRef<T>(currentState);

  // Keep ref up to date
  useEffect(() => {
    stateRef.current = currentState;
  }, [currentState]);

  // Check for existing draft on mount
  useEffect(() => {
    async function checkDraft() {
      try {
        const record = await draftsDb.drafts.get(key);
        if (record && record.data) {
          setRestoredDraft(record.data);
          setHasDraft(true);
          setLastSaved(record.updatedAt);
        }
      } catch (err) {
        console.warn('Failed to read draft from IndexedDB:', err);
      }
    }
    checkDraft();
  }, [key]);

  // Periodic Auto-Save every 5 seconds (only saves if form contains values)
  useEffect(() => {
    const timer = setInterval(async () => {
      const current = stateRef.current;
      
      if (current && typeof current === 'object' && Object.keys(current).length > 0) {
        try {
          const now = Date.now();
          await draftsDb.drafts.put({
            key,
            data: current,
            updatedAt: now,
          });
          setLastSaved(now);
          setHasDraft(true);
        } catch (err) {
          console.error('Auto-save draft error:', err);
        }
      }
    }, 5000);

    return () => clearInterval(timer);
  }, [key]);

  // Perform manual restore
  const restoreDraft = useCallback(() => {
    if (restoredDraft && onRestore) {
      onRestore(restoredDraft);
      toast.success('Bản nháp tự động lưu đã được phục hồi thành công!');
      setHasDraft(false);
    }
  }, [restoredDraft, onRestore]);

  // Explicitly clear draft on successful form submission
  const clearDraft = useCallback(async () => {
    try {
      await draftsDb.drafts.delete(key);
      setRestoredDraft(null);
      setHasDraft(false);
      setLastSaved(null);
    } catch (err) {
      console.warn('Failed to clear draft from IndexedDB:', err);
    }
  }, [key]);

  return {
    hasDraft,
    lastSaved,
    restoreDraft,
    clearDraft,
    restoredDraft,
  };
}

export default useOfflineDraft;
