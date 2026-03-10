import { BrowserRouter } from 'react-router-dom';
import { AppRoutes } from './routes/AppRoutes';
import { ToastProvider } from './components/common/Toast';
import { ConfirmDialogProvider } from './components/common/ConfirmDialog';
import './styles/variables.css';
import './styles/common.css';
import './styles/global.css';
import './App.css';

function App() {
  return (
    <BrowserRouter>
      <ToastProvider>
        <ConfirmDialogProvider>
          <AppRoutes />
        </ConfirmDialogProvider>
      </ToastProvider>
    </BrowserRouter>
  );
}

export default App;