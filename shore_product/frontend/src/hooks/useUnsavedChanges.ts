import { useEffect } from 'react';

/**
 * Hook to prevent accidental tab closing, page reload, or navigation when form is dirty.
 * @param isDirty Boolean indicating whether the form has unsaved modifications
 * @param message Optional custom warning message
 */
export function useUnsavedChanges(isDirty: boolean, message: string = 'Bạn có dữ liệu chưa lưu. Bạn có chắc chắn muốn rời khỏi trang không?') {
  useEffect(() => {
    const handleBeforeUnload = (event: BeforeUnloadEvent) => {
      if (!isDirty) return;

      event.preventDefault();
      event.returnValue = message; // Standard browser prompt
      return message;
    };

    if (isDirty) {
      window.addEventListener('beforeunload', handleBeforeUnload);
    }

    return () => {
      window.removeEventListener('beforeunload', handleBeforeUnload);
    };
  }, [isDirty, message]);
}

export default useUnsavedChanges;
