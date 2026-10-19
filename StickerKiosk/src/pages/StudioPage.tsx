import { useEffect, useRef, useState, type DragEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { ArrowCounterClockwise, ArrowDown, ArrowUp, Crosshair, Trash, Wallet } from '@phosphor-icons/react';
import { findSticker, modelsWithCases, resolve, spot, useKiosk, useSession, type StickerId, type StickerProduct } from '../kiosk';
import { PATHS } from '../app/paths';
import { useStageView } from '../app/stageView';
import { COPY, mm, price, size } from '../app/copy';
import { Glyph } from '../app/glyph';
import { CaseStage, type CaseStageHandle, type StagePlacement } from '../stage';

const DRAG_TYPE = 'application/x-sticker-id';

export function StudioPage() {
  const session = useSession();
  const kiosk = useKiosk();
  const navigate = useNavigate();
  const stage = useRef<CaseStageHandle>(null);
  const [notice, setNotice] = useState<string | null>(null);
  const [dragging, setDragging] = useState<StickerProduct | null>(null);
  const resolved = resolve(session);
  const { model, offer, sticker, at, rotationDeg, placements, total, gate } = resolved;
  const layerIndex = placements.findIndex((item) => item.active);
  const layerCount = placements.length;
  const catalog = session.catalog.status === 'ready' ? session.catalog.catalog : null;
  const stagePlacements: readonly StagePlacement[] = placements.map((item) => ({
    key: item.key,
    imageUrl: item.product.imageUrl,
    widthMm: item.product.widthMm,
    heightMm: item.product.heightMm,
    at: item.at,
    rotationDeg: item.rotationDeg,
    active: item.active,
  }));
  const view = useStageView(model, offer, stagePlacements, true);

  useEffect(() => {
    if (session.now.phase === 'checkout' || session.now.phase === 'paying' || session.now.phase === 'failed') navigate(PATHS.checkout);
  }, [session.now.phase, navigate]);

  useEffect(() => {
    if (!catalog || model?.cases.length) return;
    const next = modelsWithCases(catalog)[0];
    if (next) kiosk.dispatch({ type: 'chooseModel', id: next.id });
  }, [catalog, model, kiosk]);

  useEffect(() => {
    if (!notice) return undefined;
    const timer = window.setTimeout(() => setNotice(null), 2600);
    return () => window.clearTimeout(timer);
  }, [notice]);

  function onDragOver(event: DragEvent) {
    if (!dragging) return;
    event.preventDefault();
    stage.current?.preview(event.clientX, event.clientY, dragging);
  }

  function onDrop(event: DragEvent) {
    event.preventDefault();
    const id = Number(event.dataTransfer.getData(DRAG_TYPE)) as StickerId;
    stage.current?.preview(0, 0, null);
    setDragging(null);
    if (!catalog || !findSticker(catalog, id)) return;
    const point = stage.current?.pointToBack(event.clientX, event.clientY);
    kiosk.dispatch({ type: 'chooseSticker', id, ...(point ? { at: point } : {}) });
  }

  if (session.now.phase === 'paid') {
    return (
      <section className="page">
        <p className="empty">
          Đơn vừa rồi đã ghi xong.{' '}
          <button type="button" className="link" onClick={() => kiosk.dispatch({ type: 'newOrder' })}>
            Thiết kế đơn mới
          </button>
        </p>
      </section>
    );
  }

  return (
    <section className="studio">
      <aside className="studio-panel">
        <h2>Mẫu máy</h2>
        {catalog && modelsWithCases(catalog).length ? (
          <div className="choice-list">
            {modelsWithCases(catalog).map((m) => (
              <button
                key={m.id}
                type="button"
                className={m.id === model?.id ? 'choice on' : 'choice'}
                onClick={() => kiosk.dispatch({ type: 'chooseModel', id: m.id })}
              >
                {m.label}
                {m.dimensions ? <small>{m.dimensions}</small> : null}
              </button>
            ))}
          </div>
        ) : (
          <p className="meta">{session.catalog.status === 'loading' ? 'Đang tải…' : 'Chưa có mẫu máy.'}</p>
        )}

        <h2>Ốp lưng</h2>
        {model?.cases.length ? (
          <div className="choice-list">
            {model.cases.map((c) => (
              <button
                key={c.id}
                type="button"
                className={c.id === offer?.id ? 'choice on' : 'choice'}
                onClick={() => kiosk.dispatch({ type: 'chooseCase', id: c.id })}
              >
                {c.label}
                <small>
                  {price(c.price)}
                  {c.sale.kind === 'unavailable' ? (c.sale.why === 'out-of-stock' ? ', hết hàng' : ', chưa có model 3D') : ''}
                </small>
              </button>
            ))}
          </div>
        ) : (
          <p className="meta">Mẫu này chưa có ốp.</p>
        )}
      </aside>

      <div className="studio-stage" onDragOver={onDragOver} onDrop={onDrop} onDragLeave={() => stage.current?.preview(0, 0, null)}>
        <CaseStage
          ref={stage}
          view={view}
          onCommit={(key, next) => kiosk.dispatch({ type: 'commit', key, at: next })}
          onNudge={(dx, dy) => kiosk.dispatch({ type: 'nudge', dx, dy })}
          onRotateTo={(key, rotation) => kiosk.dispatch({ type: 'rotateTo', key, rotationDeg: rotation })}
          onSelect={(key) => kiosk.dispatch({ type: 'select', key })}
          onReject={() => setNotice('Chỗ này không dán được: sticker chạm lỗ camera, gờ hoặc mép ốp.')}
          className="stage-studio"
        />
        <div className="stage-tools">
          {sticker && at ? (
            <p className="readout">
              Lớp {layerIndex + 1}/{layerCount}. Tâm cách mép trái {mm(at.x)}, cách mép trên {mm(at.y)}, xoay {rotationDeg}°.
            </p>
          ) : (
            <p className="readout">{placements.length ? COPY.gate['no-room'] : 'Chọn một sticker trong khay.'}</p>
          )}
          <div className="row">
            <button type="button" className="button small ghost" onClick={() => stage.current?.resetView()}>
              <Glyph icon={ArrowCounterClockwise} />
              Góc nhìn ban đầu
            </button>
            <button type="button" className="button small ghost" disabled={layerIndex < 0 || layerIndex === layerCount - 1} onClick={() => kiosk.dispatch({ type: 'stack', direction: 'up' })}>
              <Glyph icon={ArrowUp} />
              Lên trên
            </button>
            <button type="button" className="button small ghost" disabled={layerIndex <= 0} onClick={() => kiosk.dispatch({ type: 'stack', direction: 'down' })}>
              <Glyph icon={ArrowDown} />
              Xuống dưới
            </button>
            <button type="button" className="button small ghost" disabled={!sticker} onClick={() => kiosk.dispatch({ type: 'recenter' })}>
              <Glyph icon={Crosshair} />
              Đặt vào giữa
            </button>
            <button type="button" className="button small ghost" disabled={!sticker} onClick={() => kiosk.dispatch({ type: 'removeSticker' })}>
              <Glyph icon={Trash} />
              Gỡ sticker
            </button>
          </div>
        </div>
        {notice ? (
          <p className="toast" role="status">
            {notice}
          </p>
        ) : null}
      </div>

      <aside className="studio-panel tray">
        <div className="tray-head">
          <h2>Sticker</h2>
          <Link to={PATHS.stickers} className="link">
            Mở thư viện
          </Link>
        </div>
        {catalog?.stickers.length ? (
          <div className="tray-grid">
            {catalog.stickers.map((s) => {
              const fits = offer?.fit ? spot(s, offer.fit.sheet) !== null : false;
              return (
                <button
                  key={s.id}
                  type="button"
                  draggable={fits}
                  className={s.id === sticker?.id ? 'tray-item on' : 'tray-item'}
                  disabled={!fits}
                  onClick={() => kiosk.dispatch({ type: 'chooseSticker', id: s.id })}
                  onDragStart={(event) => {
                    event.dataTransfer.setData(DRAG_TYPE, String(s.id));
                    event.dataTransfer.effectAllowed = 'copy';
                    setDragging(s);
                  }}
                  onDragEnd={() => setDragging(null)}
                >
                  <img src={s.imageUrl} alt="" />
                  <span>{s.name}</span>
                  <small>
                    {size(s.widthMm, s.heightMm)}
                    <br />
                    {fits ? price(s.price) : 'Không vừa ốp này'}
                  </small>
                </button>
              );
            })}
          </div>
        ) : (
          <p className="meta">{session.catalog.status === 'loading' ? 'Đang tải…' : 'Chưa có sticker.'}</p>
        )}
      </aside>

      <footer className="studio-bar">
        <dl className="sum">
          <div>
            <dt>Ốp</dt>
            <dd>{offer ? price(offer.price) : 'Chưa chọn'}</dd>
          </div>
          <div>
            <dt>Sticker</dt>
            <dd>{sticker ? price(sticker.price) : 'Chưa chọn'}</dd>
          </div>
          <div className="total">
            <dt>Tổng</dt>
            <dd>{total !== null ? price(total) : 'Chưa đủ'}</dd>
          </div>
        </dl>
        {gate.open ? (
          <button
            type="button"
            className="button"
            onClick={() => {
              kiosk.dispatch({ type: 'openCheckout' });
              navigate(PATHS.checkout);
            }}
          >
            <Glyph icon={Wallet} />
            Thanh toán
          </button>
        ) : (
          <p className="gate-note">{COPY.gate[gate.reason]}</p>
        )}
      </footer>
    </section>
  );
}
