import { useState, useEffect, useCallback, useRef } from 'react'

export interface FetchState<T> {
  data: T | null
  loading: boolean
  error: Error | null
}

export interface UseFetchOptions extends RequestInit {
  skip?: boolean
}

export function useFetch<T>(
  url: string,
  options?: UseFetchOptions
): FetchState<T> & { refetch: () => Promise<void> } {
  const [state, setState] = useState<FetchState<T>>({
    data: null,
    loading: !options?.skip,
    error: null,
  })

  // Store options in ref to avoid unnecessary re-renders
  const optionsRef = useRef(options)
  
  useEffect(() => {
    optionsRef.current = options
  }, [options])

  const fetchData = useCallback(async () => {
    if (optionsRef.current?.skip) return

    setState((prev) => ({ ...prev, loading: true, error: null }))
    try {
      const response = await fetch(url, optionsRef.current)
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }
      const data = await response.json()
      setState({ data, loading: false, error: null })
    } catch (error) {
      setState({ data: null, loading: false, error: error as Error })
    }
  }, [url]) // Only depend on url

  useEffect(() => {
    fetchData()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [url, options?.skip]) // Fetch when url or skip flag changes

  return { ...state, refetch: fetchData }
}
