import { BrowserRouter } from 'react-router-dom';
import { Toaster } from 'sonner';
import { AppRoutes } from './routes/AppRoutes';
import { ToastProvider } from './components/common/Toast';
import { ConfirmDialogProvider } from './components/common/ConfirmDialog';
import { VesselProvider } from './contexts/VesselContext';
import { I18nProvider } from './contexts/I18nContext';
import './styles/variables.css';
import './styles/common.css';
import './styles/global.css';
import './App.css';

function App() {
  return (
    <BrowserRouter>
      <I18nProvider>
        <VesselProvider>
          <ToastProvider>
            <ConfirmDialogProvider>
              <AppRoutes />
              <Toaster richColors position="top-right" />
            </ConfirmDialogProvider>
          </ToastProvider>
        </VesselProvider>
      </I18nProvider>
    </BrowserRouter>
  );
}

export default App;