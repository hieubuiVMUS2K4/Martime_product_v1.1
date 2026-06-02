import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { I18nProvider } from '@/contexts/I18nContext'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import './styles/globals.css'
import App from './App.tsx'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 60 * 24, // 24 hours cache for static metadata
      gcTime: 1000 * 60 * 60 * 24 * 7, // 7 days garbage collection time
      refetchOnWindowFocus: false,
    },
  },
})

// Add no-transition class initially to prevent theme flash on load
document.documentElement.classList.add('no-transition');

// Enable transitions after initial render
const enableTransitions = () => {
  document.documentElement.classList.remove('no-transition');
};

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <I18nProvider>
          <App />
        </I18nProvider>
      </BrowserRouter>
    </QueryClientProvider>
  </StrictMode>,
)

// Enable transitions after a short delay (2 frames)
requestAnimationFrame(() => {
  requestAnimationFrame(enableTransitions);
});
