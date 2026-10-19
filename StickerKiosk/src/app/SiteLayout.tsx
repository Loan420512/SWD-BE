import { Link, NavLink, Outlet, useLocation } from 'react-router-dom';
import { useEffect } from 'react';
import { Gear, MagnifyingGlass, PenNib, Sticker } from '@phosphor-icons/react';
import { resolve, useSession } from '../kiosk';
import { PATHS } from './paths';
import { Glyph } from './glyph';

export function SiteLayout() {
  const session = useSession();
  const { offer, sticker, placements } = resolve(session);
  const location = useLocation();

  useEffect(() => {
    window.scrollTo(0, 0);
  }, [location.pathname]);

  return (
    <div className="site">
      <header className="site-header">
        <Link to={PATHS.home} className="brand" aria-label="StikerKiosk, trang chủ">
          <span className="brand-mark" aria-hidden="true">
            <Glyph icon={Sticker} weight="fill" />
          </span>
          <span className="brand-word">StikerKiosk</span>
        </Link>
        <nav className="site-nav" aria-label="Chính">
          <NavLink to={PATHS.stickers}>
            <Glyph icon={Sticker} />
            Thư viện sticker
          </NavLink>
          <NavLink to={PATHS.studio}>
            <Glyph icon={PenNib} />
            Thiết kế
          </NavLink>
          <NavLink to={PATHS.orders}>
            <Glyph icon={MagnifyingGlass} />
            Tra cứu đơn
          </NavLink>
        </nav>
        {sticker && offer ? (
          <Link to={PATHS.studio} className="draft-chip">
            <img src={sticker.imageUrl} alt="" />
            <span>
              {placements.length > 1 ? `${placements.length} sticker` : sticker.name}
              <small>{offer.label}</small>
            </span>
          </Link>
        ) : location.pathname === PATHS.home ? null : (
          <Link to={PATHS.studio} className="button small">
            <Glyph icon={PenNib} />
            Bắt đầu thiết kế
          </Link>
        )}
      </header>
      <main className="site-main">
        <Outlet />
      </main>
      <footer className="site-footer">
        <Link to={PATHS.admin}>
          <Glyph icon={Gear} />
          Khu quản trị
        </Link>
      </footer>
    </div>
  );
}
