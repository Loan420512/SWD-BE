import { useEffect, useState, type FormEvent } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { MagnifyingGlass, PenNib } from '@phosphor-icons/react';
import { useKiosk, useSession, type PublicOrder } from '../kiosk';
import { PATHS } from '../app/paths';
import { COPY, mm, price } from '../app/copy';
import { Glyph } from '../app/glyph';

const FLOW: readonly PublicOrder['status'][] = ['Pending', 'Processing', 'Completed'];

type Lookup = { state: 'idle' } | { state: 'loading' } | { state: 'found'; order: PublicOrder } | { state: 'not-found' } | { state: 'unreachable' };

export function OrderPage() {
  const { code } = useParams();
  const kiosk = useKiosk();
  const session = useSession();
  const navigate = useNavigate();
  const [input, setInput] = useState(code ?? '');
  const [lookup, setLookup] = useState<Lookup>({ state: 'idle' });
  const justPaid = session.now.phase === 'paid' && session.now.orderCode === code;

  useEffect(() => {
    if (!code) {
      setLookup({ state: 'idle' });
      return;
    }
    let live = true;
    setInput(code);
    setLookup({ state: 'loading' });
    void kiosk.backend.lookupOrder(code).then((result) => {
      if (!live) return;
      setLookup(typeof result === 'string' ? { state: result } : { state: 'found', order: result });
    });
    return () => {
      live = false;
    };
  }, [code, kiosk]);

  function submit(event: FormEvent) {
    event.preventDefault();
    if (input.trim()) navigate(PATHS.order(input.trim().toUpperCase()));
  }

  return (
    <section className="page narrow">
      {justPaid ? (
        <div className="banner">
          <h1>Cảm ơn bạn, đơn đã ghi xong.</h1>
          <button type="button" className="button small" onClick={() => kiosk.dispatch({ type: 'newOrder' })}>
            <Glyph icon={PenNib} />
            Thiết kế đơn mới
          </button>
        </div>
      ) : (
        <h1>Tra cứu đơn</h1>
      )}

      <form className="row" onSubmit={submit}>
        <label className="grow">
          Mã đơn
          <input value={input} onChange={(e) => setInput(e.target.value)} placeholder="SK-7F3K9Q2M" />
        </label>
        <button type="submit" className="button">
          <Glyph icon={MagnifyingGlass} />
          Tra cứu
        </button>
      </form>

      {lookup.state === 'loading' ? <p className="empty">Đang tìm đơn…</p> : null}
      {lookup.state === 'not-found' ? <p className="empty">Không có đơn nào mang mã {code}. Kiểm tra lại mã trên phiếu.</p> : null}
      {lookup.state === 'unreachable' ? <p className="empty">Không kết nối được máy chủ. Thử lại sau.</p> : null}

      {lookup.state === 'found' ? <OrderCard order={lookup.order} /> : null}

      {lookup.state !== 'found' ? (
        <p className="hint">
          Chưa có đơn? <Link to={PATHS.studio}>Bắt đầu thiết kế</Link>
        </p>
      ) : null}
    </section>
  );
}

function OrderCard({ order }: { readonly order: PublicOrder }) {
  const reached = FLOW.indexOf(order.status);
  return (
    <article className="order-card">
      <header>
        <p className="code">{order.orderCode}</p>
        <p className={`status status-${order.status.toLowerCase()}`}>{COPY.status[order.status]}</p>
      </header>
      {order.status === 'Cancelled' ? (
        <p>Đơn này đã huỷ. Liên hệ nhân viên quầy nếu cần hoàn tiền.</p>
      ) : (
        <ol className="timeline">
          {FLOW.map((status, index) => (
            <li key={status} className={index <= reached ? 'done' : ''}>
              {COPY.status[status]}
            </li>
          ))}
        </ol>
      )}
      <div className="order-body">
        {order.stickerImageUrl ? <img src={order.stickerImageUrl} alt={order.stickerName ?? ''} className="order-art" /> : null}
        <dl className="spec">
          <div>
            <dt>Máy</dt>
            <dd>{order.modelLabel}</dd>
          </div>
          <div>
            <dt>Ốp</dt>
            <dd>{order.caseLabel}</dd>
          </div>
          <div>
            <dt>Sticker</dt>
            <dd>{order.stickerName}</dd>
          </div>
          {order.at ? (
            <div>
              <dt>Vị trí tâm</dt>
              <dd>
                {mm(order.at.x)} từ mép trái, {mm(order.at.y)} từ mép trên, xoay {order.rotationDeg}°
              </dd>
            </div>
          ) : null}
          {order.total !== null ? (
            <div>
              <dt>Tổng</dt>
              <dd>
                {price(order.total)}
                {order.paymentMethod ? `, ${COPY.method[order.paymentMethod]}` : ''}
              </dd>
            </div>
          ) : null}
          {order.createdAt ? (
            <div>
              <dt>Đặt lúc</dt>
              <dd>{order.createdAt.toLocaleString('vi-VN')}</dd>
            </div>
          ) : null}
        </dl>
      </div>
    </article>
  );
}
