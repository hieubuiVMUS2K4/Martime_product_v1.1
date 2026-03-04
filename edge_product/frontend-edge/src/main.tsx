import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { I18nProvider } from '@/contexts/I18nContext'
import './styles/globals.css'
import App from './App.tsx'

// Add no-transition class initially to prevent theme flash on load
document.documentElement.classList.add('no-transition');

// Enable transitions after initial render
const enableTransitions = () => {
  document.documentElement.classList.remove('no-transition');
};

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <I18nProvider>
        <App />
      </I18nProvider>
    </BrowserRouter>
  </StrictMode>,
)

// Enable transitions after a short delay (2 frames)
requestAnimationFrame(() => {
  requestAnimationFrame(enableTransitions);
});
