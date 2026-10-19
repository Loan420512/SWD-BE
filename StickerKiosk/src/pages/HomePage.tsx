import { useMemo, useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { MagnifyingGlass, PenNib } from '@phosphor-icons/react';
import { designableModels, spot, useKiosk, useSession, type CaseOffer, type PhoneModel } from '../kiosk';
import { PATHS } from '../app/paths';
import { useStageView } from '../app/stageView';
import { price, size } from '../app/copy';
import { Glyph } from '../app/glyph';
import { CaseStage } from '../stage';

export function HomePage() {
  const session = useSession();
  const kiosk = useKiosk();
  const navigate = useNavigate();
  const [code, setCode] = useState('');
  const catalog = session.catalog.status === 'ready' ? session.catalog.catalog : null;

  const showcase = useMemo(() => {
    if (!catalog) return null;
    for (const model of designableModels(catalog)) {
      const offer = model.cases.find((c) => c.fit && c.sale.kind === 'orderable') ?? model.cases.find((c) => c.fit);
      if (!offer?.fit) continue;
      for (const sticker of catalog.stickers) {
        const at = spot(sticker, offer.fit.sheet);
        if (at) return { model, offer, sticker, at };
      }
      return { model, offer, sticker: null, at: null };
    }
    return null;
  }, [catalog]);

  const homePlacements = useMemo(() => {
    if (!showcase?.sticker || !showcase.at) return [];
    return [{
      key: 'home',
      imageUrl: showcase.sticker.imageUrl,
      widthMm: showcase.sticker.widthMm,
      heightMm: showcase.sticker.heightMm,
      at: showcase.at,
      rotationDeg: 0,
      active: false,
    }];
  }, [showcase]);

  const view = useStageView(
    showcase?.model ?? null,
    (showcase?.offer as CaseOffer | undefined) ?? null,
    homePlacements,
    false,
  );

  function lookup(event: FormEvent) {
    event.preventDefault();
    if (code.trim()) navigate(PATHS.order(code.trim().toUpperCase()));
  }

  const fresh = catalog?.stickers ?? [];
  const models: readonly PhoneModel[] = catalog ? designableModels(catalog) : [];

  return (
    <>
      <section className="bench">
        <div className="bench-copy">
          <h1>Dán sticker lên ốp.</h1>
          <p className="bench-spec">
            {showcase?.sticker
              ? `${size(showcase.sticker.widthMm, showcase.sticker.heightMm)} / ${showcase.offer.label}`
              : 'Đúng từng milimét.'}
          </p>
          <div className="row">
            <Link to={PATHS.studio} className="button">
              <Glyph icon={PenNib} />
              Bắt đầu thiết kế
            </Link>
          </div>
          {models.length ? <p className="hint">Ốp cho {models.map((m) => m.label).join(', ')}.</p> : null}
          {session.catalog.status === 'unreachable' ? (
            <p className="notice">
              Quầy chưa kết nối được máy chủ.{' '}
              <button type="button" className="link" onClick={kiosk.reloadCatalog}>
                Thử lại
              </button>
            </p>
          ) : null}
        </div>
        <div className="bench-stage">
          <CaseStage view={view} className="stage-hero" />
        </div>
        <ol className="bench-index">
          <li>
            <span>01</span>
            Chọn máy và ốp
          </li>
          <li>
            <span>02</span>
            Đặt sticker
          </li>
          <li>
            <span>03</span>
            Thanh toán và nhận ốp
          </li>
        </ol>
      </section>

      {fresh.length ? (
        <section className="rail" aria-label="Sticker mới về">
          <div className="rail-head">
            <h2>Sticker trên bàn</h2>
            <Link to={PATHS.stickers}>Tất cả {fresh.length}</Link>
          </div>
          <div className="rail-track">
            {fresh.map((sticker) => (
              <button
                key={sticker.id}
                type="button"
                className="rail-item"
                onClick={() => {
                  kiosk.dispatch({ type: 'chooseSticker', id: sticker.id });
                  navigate(PATHS.studio);
                }}
              >
                <span className="sticker-art">
                  <img src={sticker.imageUrl} alt="" />
                </span>
                <span className="rail-name">{sticker.name}</span>
                <span className="rail-meta">
                  {size(sticker.widthMm, sticker.heightMm)} / {price(sticker.price)}
                </span>
              </button>
            ))}
          </div>
        </section>
      ) : null}

      <section className="ticket">
        <h2>Đã đặt hàng?</h2>
        <form className="ticket-form" onSubmit={lookup}>
          <label htmlFor="home-code">
            Mã đơn
            <input id="home-code" value={code} onChange={(e) => setCode(e.target.value)} placeholder="SK-7F3K9Q2M" />
          </label>
          <button type="submit" className="button">
            <Glyph icon={MagnifyingGlass} />
            Tra cứu
          </button>
        </form>
      </section>
    </>
  );
}
