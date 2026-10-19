import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { RouterProvider } from 'react-router-dom';
import { createHttpBackend } from './api';
import './app/theme.css';
import { router } from './app/routes';
import { createKiosk, KioskProvider } from './kiosk';

const kiosk = createKiosk(createHttpBackend({ baseUrl: '/api', timeoutMs: 8_000 }));

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <KioskProvider kiosk={kiosk}>
      <RouterProvider router={router} />
    </KioskProvider>
  </StrictMode>,
);
