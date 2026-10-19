import { createBrowserRouter } from 'react-router-dom';
import { AdminCases } from '../pages/admin/AdminCases';
import { AdminLayout } from '../pages/admin/AdminLayout';
import { AdminLogin } from '../pages/admin/AdminLogin';
import { AdminOrders } from '../pages/admin/AdminOrders';
import { AdminPhones } from '../pages/admin/AdminPhones';
import { AdminStickers } from '../pages/admin/AdminStickers';
import { CheckoutPage } from '../pages/CheckoutPage';
import { HomePage } from '../pages/HomePage';
import { NotFoundPage } from '../pages/NotFoundPage';
import { OrderPage } from '../pages/OrderPage';
import { StickersPage } from '../pages/StickersPage';
import { StudioPage } from '../pages/StudioPage';
import { PATHS } from './paths';
import { SiteLayout } from './SiteLayout';


export const router = createBrowserRouter([
  {
    element: <SiteLayout />,
    children: [
      { path: PATHS.home, element: <HomePage /> },
      { path: PATHS.stickers, element: <StickersPage /> },
      { path: PATHS.studio, element: <StudioPage /> },
      { path: PATHS.checkout, element: <CheckoutPage /> },
      { path: PATHS.orders, element: <OrderPage /> },
      { path: `${PATHS.orders}/:code`, element: <OrderPage /> },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
  { path: PATHS.adminLogin, element: <AdminLogin /> },
  {
    path: PATHS.admin,
    element: <AdminLayout />,
    children: [
      { index: true, element: <AdminOrders /> },
      { path: 'sticker', element: <AdminStickers /> },
      { path: 'op-lung', element: <AdminCases /> },
      { path: 'mau-may', element: <AdminPhones /> },
    ],
  },
]);
