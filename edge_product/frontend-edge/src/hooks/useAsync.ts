import { useState, useCallback } from 'react'

export interface AsyncState<T> {
  data: T | null
  loading: boolean
  error: Error | null
}

/**
 * Custom hook for handling async operations
 * Note: This hook does NOT automatically execute on mount
 * Call the `execute` function manually when needed
 */
export function useAsync<T>(): AsyncState<T> & { 
  execute: (asyncFunction: () => Promise<T>) => Promise<void>
  reset: () => void 
} {
  const [state, setState] = useState<AsyncState<T>>({
    data: null,
    loading: false,
    error: null,
  })

  const execute = useCallback(async (asyncFunction: () => Promise<T>) => {
    setState({ data: null, loading: true, error: null })
    try {
      const data = await asyncFunction()
      setState({ data, loading: false, error: null })
    } catch (error) {
      setState({ data: null, loading: false, error: error as Error })
    }
  }, [])

  const reset = useCallback(() => {
    setState({ data: null, loading: false, error: null })
  }, [])

  return { ...state, execute, reset }
}
