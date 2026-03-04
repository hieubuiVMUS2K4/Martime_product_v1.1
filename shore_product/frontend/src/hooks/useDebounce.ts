import { useState, useEffect } from 'react';

/**
 * Debounce a value — returns the debounced value after `delay` ms of inactivity.
 * Perfect for search inputs to avoid calling API on every keystroke.
 */
export function useDebounce<T>(value: T, delay = 300): T {
  const [debouncedValue, setDebouncedValue] = useState<T>(value);

  useEffect(() => {
    const timer = setTimeout(() => setDebouncedValue(value), delay);
    return () => clearTimeout(timer);
  }, [value, delay]);

  return debouncedValue;
}
