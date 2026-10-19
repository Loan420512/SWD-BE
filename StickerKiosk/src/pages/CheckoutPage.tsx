import { useEffect, useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { ArrowClockwise, ArrowLeft, Check, Trash } from '@phosphor-icons/react';
import { resolve, useKiosk, useSession, type FormErrors, type PaymentMethod } from '../kiosk';
import { PATHS } from '../app/paths';
import { useStageView } from '../app/stageView';
import { COPY, mm, price, size } from '../app/copy';
import { Glyph } from '../app/glyph';
import { CaseStage } from '../stage';

export function CheckoutPage() {
  const session = useSession();
  const kiosk = useKiosk();
  const navigate = useNavigate();
  const [fullName, setFullName] = useState('');
  const [phone, setPhone] = useState('');
  const [method, setMethod] = useState<PaymentMethod>('Cash');
  const [errors, setErrors] = useState<FormErrors | null>(null);

  const now = session.now;
  const order = now.phase === 'paying' || now.phase === 'failed' || now.phase === 'paid' ? now.order : null;
  const design = order?.design ?? session.design;
  const resolved = resolve(session, design);
  const view = useStageView(
    resolved.model,
    resolved.offer,
    resolved.placements.map((item) => ({
      key: item.key,
      imageUrl: item.product.imageUrl,
      widthMm: item.product.widthMm,
      heightMm: item.product.heightMm,
      at: item.at,
      rotationDeg: item.rotationDeg,
      active: item.active,
    })),
    false,
  );

  useEffect(() => {
    if (now.phase === 'paid') navigate(PATHS.order(now.orderCode), { replace: true });
  }, [now, navigate]);

  if (now.phase === 'design') {
    return (
      <section className="page">
        <h1>Thanh toán</h1>
        <p className="empty">
          Chưa có thiết kế nào sẵn sàng thanh toán. <Link to={PATHS.studio}>Quay lại bàn thiết kế</Link>
        </p>
      </section>
    );
  }

  function submit(event: FormEvent) {
    event.preventDefault();
    setErrors(kiosk.pay({ fullName, phone, method }));
  }

  const summary = order?.summary;
  const casePrice = summary?.casePrice ?? resolved.offer?.price ?? 0;
  const stickerPrice = summary?.stickerPrice ?? resolved.sticker?.price ?? 0;
  const total = summary?.total ?? resolved.total ?? 0;

  return (
    <section className="checkout">
      <div className="checkout-stage">
        <CaseStage view={view} className="stage-checkout" />
        <dl className="spec">
          <div>
            <dt>Máy</dt>
            <dd>{resolved.model?.label}</dd>
          </div>
          <div>
            <dt>Ốp</dt>
            <dd>{resolved.offer?.label}</dd>
          </div>
          <div>
            <dt>Sticker</dt>
            <dd>
              {resolved.placements.map((item) => item.product.name).join(', ') || 'Chưa chọn'}
            </dd>
          </div>
          {resolved.placements.map((item) => (
            <div key={item.key}>
              <dt>{item.product.name}</dt>
              <dd>
                {mm(item.at.x)} từ mép trái, {mm(item.at.y)} từ mép trên, xoay {item.rotationDeg}°, {size(item.product.widthMm, item.product.heightMm)}
              </dd>
            </div>
          ))}
        </dl>
      </div>

      <div className="checkout-panel">
        {now.phase === 'checkout' ? (
          <form onSubmit={submit} className="form" noValidate>
            <h1>Thanh toán</h1>
            <label>
              Họ tên
              <input value={fullName} onChange={(e) => setFullName(e.target.value)} autoComplete="name" aria-invalid={Boolean(errors?.fullName)} />
            </label>
            {errors?.fullName === 'required' ? <p className="error">Cần họ tên để in phiếu nhận ốp.</p> : null}
            {errors?.fullName === 'too-long' ? <p className="error">Họ tên dài quá 100 ký tự.</p> : null}
            <label>
              Số điện thoại (không bắt buộc)
              <input value={phone} onChange={(e) => setPhone(e.target.value)} inputMode="tel" autoComplete="tel" aria-invalid={Boolean(errors?.phone)} />
            </label>
            {errors?.phone ? <p className="error">Số điện thoại gồm 10 chữ số, bắt đầu bằng 0.</p> : null}
            <fieldset>
              <legend>Cách trả tiền</legend>
              <div className="row">
                {COPY.methods.map((m) => (
                  <button key={m} type="button" className={m === method ? 'choice on' : 'choice'} aria-pressed={m === method} onClick={() => setMethod(m)}>
                    {COPY.method[m]}
                  </button>
                ))}
              </div>
            </fieldset>
            <Totals casePrice={casePrice} stickerPrice={stickerPrice} total={total} />
            <div className="row">
              <button
                type="button"
                className="button ghost"
                onClick={() => {
                  kiosk.dispatch({ type: 'back' });
                  navigate(PATHS.studio);
                }}
              >
                <Glyph icon={ArrowLeft} />
                Sửa thiết kế
              </button>
              <button type="submit" className="button">
                <Glyph icon={Check} />
                Xác nhận và ghi đơn
              </button>
            </div>
          </form>
        ) : null}

        {now.phase === 'paying' ? (
          <div className="form">
            <h1>Đang ghi đơn</h1>
            <progress value={now.order.progress} max={1} />
            <Totals casePrice={casePrice} stickerPrice={stickerPrice} total={total} />
          </div>
        ) : null}

        {now.phase === 'failed' ? (
          <div className="form">
            <h1>Đơn chưa ghi xong</h1>
            <p>{COPY.problem[now.problem]}</p>
            {now.order.orderCode ? <p className="code">{now.order.orderCode}</p> : null}
            <div className="row">
              <button type="button" className="button ghost" onClick={() => kiosk.dispatch({ type: 'newOrder' })}>
                <Glyph icon={Trash} />
                Bỏ đơn này
              </button>
              <button type="button" className="button" onClick={() => kiosk.dispatch({ type: 'retry' })}>
                <Glyph icon={ArrowClockwise} />
                Thử lại
              </button>
            </div>
          </div>
        ) : null}
      </div>
    </section>
  );
}

function Totals(props: { readonly casePrice: number; readonly stickerPrice: number; readonly total: number }) {
  return (
    <dl className="sum stacked">
      <div>
        <dt>Ốp lưng</dt>
        <dd>{price(props.casePrice)}</dd>
      </div>
      <div>
        <dt>Sticker</dt>
        <dd>{price(props.stickerPrice)}</dd>
      </div>
      <div className="total">
        <dt>Tổng</dt>
        <dd>{price(props.total)}</dd>
      </div>
    </dl>
  );
}
