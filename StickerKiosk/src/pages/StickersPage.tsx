import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Sticker } from '@phosphor-icons/react';
import { resolve, spot, useKiosk, useSession, type StickerProduct } from '../kiosk';
import { PATHS } from '../app/paths';
import { price, size } from '../app/copy';
import { Glyph } from '../app/glyph';

type Sort = 'new' | 'price-asc' | 'price-desc' | 'small';

const SORTS: Record<Sort, { label: string; compare: (a: StickerProduct, b: StickerProduct) => number }> = {
  new: { label: 'Mới nhất', compare: () => 0 },
  'price-asc': { label: 'Giá thấp trước', compare: (a, b) => a.price - b.price },
  'price-desc': { label: 'Giá cao trước', compare: (a, b) => b.price - a.price },
  small: { label: 'Nhỏ trước', compare: (a, b) => a.widthMm * a.heightMm - b.widthMm * b.heightMm },
};

export function StickersPage() {
  const session = useSession();
  const kiosk = useKiosk();
  const navigate = useNavigate();
  const [query, setQuery] = useState('');
  const [sort, setSort] = useState<Sort>('new');
  const { offer } = resolve(session);
  const catalog = session.catalog.status === 'ready' ? session.catalog.catalog : null;

  const stickers = useMemo(() => {
    const all = catalog?.stickers ?? [];
    const needle = query.trim().toLowerCase();
    const filtered = needle ? all.filter((s) => s.name.toLowerCase().includes(needle)) : [...all];
    return filtered.sort(SORTS[sort].compare);
  }, [catalog, query, sort]);

  function choose(sticker: StickerProduct) {
    kiosk.dispatch({ type: 'chooseSticker', id: sticker.id });
    navigate(PATHS.studio);
  }

  return (
    <section className="page">
      <header className="page-head">
        <h1>Thư viện sticker</h1>
        {offer?.fit ? <p className="meta">Đang so với {offer.label}.</p> : null}
      </header>

      <div className="toolbar">
        <label>
          Tìm theo tên
          <input value={query} onChange={(e) => setQuery(e.target.value)} placeholder="Ví dụ: sóng" />
        </label>
        <label>
          Sắp xếp
          <select value={sort} onChange={(e) => setSort(e.target.value as Sort)}>
            {(Object.keys(SORTS) as Sort[]).map((key) => (
              <option key={key} value={key}>
                {SORTS[key].label}
              </option>
            ))}
          </select>
        </label>
        <p className="meta">{stickers.length} sticker</p>
      </div>

      {session.catalog.status === 'loading' ? <p className="empty">Đang tải thư viện…</p> : null}
      {session.catalog.status === 'unreachable' ? (
        <p className="empty">
          Không tải được thư viện.{' '}
          <button type="button" className="link" onClick={kiosk.reloadCatalog}>
            Thử lại
          </button>
        </p>
      ) : null}
      {catalog && stickers.length === 0 ? <p className="empty">{query ? 'Không có sticker nào khớp tên này.' : 'Thư viện chưa có sticker.'}</p> : null}

      <div className="sticker-grid">
        {stickers.map((sticker) => {
          const fits = offer?.fit ? spot(sticker, offer.fit.sheet) !== null : true;
          return (
            <article key={sticker.id} className="sticker-card">
              <div className="sticker-art">
                <img src={sticker.imageUrl} alt={sticker.name} loading="lazy" />
                <span className="ruler">{size(sticker.widthMm, sticker.heightMm)}</span>
              </div>
              <h3>{sticker.name}</h3>
              <p className="meta">{price(sticker.price)}</p>
              {fits ? null : <p className="warn">Không vừa {offer?.label}</p>}
              <button type="button" className="button small" onClick={() => choose(sticker)}>
                <Glyph icon={Sticker} />
                Dán thử lên ốp
              </button>
            </article>
          );
        })}
      </div>
    </section>
  );
}
