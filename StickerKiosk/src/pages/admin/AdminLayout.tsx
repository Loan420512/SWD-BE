import { useCallback, useState } from 'react';
import { Link, Navigate, NavLink, Outlet, useNavigate } from 'react-router-dom';
import { DeviceMobile, Eye, Phone, Receipt, SignOut, Sticker } from '@phosphor-icons/react';
import { clearAdminSession, readAdminSession } from '../../api/admin';
import { PATHS } from '../../app/paths';
import { Glyph } from '../../app/glyph';

export function AdminLayout() {
  const [session] = useState(readAdminSession);
  const navigate = useNavigate();
  if (!session) return <Navigate to={PATHS.adminLogin} replace />;

  return (
    <div className="admin">
      <header className="admin-header">
        <Link to={PATHS.admin} className="brand">
          <span className="brand-mark" aria-hidden="true">
            <Glyph icon={Sticker} weight="fill" />
          </span>
          <span className="brand-word">StikerKiosk</span>
        </Link>
        <nav className="site-nav" aria-label="Quản trị">
          <NavLink to={PATHS.admin} end>
            <Glyph icon={Receipt} />
            Đơn hàng
          </NavLink>
          <NavLink to={PATHS.adminStickers}>
            <Glyph icon={Sticker} />
            Sticker
          </NavLink>
          <NavLink to={PATHS.adminCases}>
            <Glyph icon={DeviceMobile} />
            Ốp lưng
          </NavLink>
          <NavLink to={PATHS.adminPhones}>
            <Glyph icon={Phone} />
            Mẫu máy
          </NavLink>
        </nav>
        <div className="row">
          <span className="meta">
            {session.username} ({session.role})
          </span>
          <Link to={PATHS.home} className="link">
            <Glyph icon={Eye} />
            Xem trang khách
          </Link>
          <button
            type="button"
            className="button small ghost"
            onClick={() => {
              clearAdminSession();
              navigate(PATHS.adminLogin);
            }}
          >
            <Glyph icon={SignOut} />
            Đăng xuất
          </button>
        </div>
      </header>
      <main className="admin-main">
        <Outlet />
      </main>
    </div>
  );
}

/** Sends the admin to login when a call comes back unauthorized. */
export function useExpiry() {
  const navigate = useNavigate();
  return useCallback(
    (expired: boolean) => {
      if (!expired) return;
      clearAdminSession();
      navigate(PATHS.adminLogin, { replace: true });
    },
    [navigate],
  );
}
