import { useCallback, useEffect, useState } from 'react';
import { ArrowClockwise, Check, PaintBrush, Prohibit } from '@phosphor-icons/react';
import { listOrders, setOrderStatus, type AdminOrder } from '../../api/admin';
import { COPY, mm, price } from '../../app/copy';
import { Glyph } from '../../app/glyph';
import { useExpiry } from './AdminLayout';

const NEXT: Record<AdminOrder['status'], readonly ('Processing' | 'Completed' | 'Cancelled')[]> = {
  Pending: ['Processing', 'Cancelled'],
  Processing: ['Completed', 'Cancelled'],
  Completed: [],
  Cancelled: [],
};

const ACTION: Record<'Processing' | 'Completed' | 'Cancelled', string> = {
  Processing: 'Bắt đầu dán',
  Completed: 'Đã dán xong',
  Cancelled: 'Huỷ',
};

export function AdminOrders() {
  const [orders, setOrders] = useState<AdminOrder[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [filter, setFilter] = useState<AdminOrder['status'] | 'all'>('all');
  const expire = useExpiry();

  const load = useCallback(async () => {
    const result = await listOrders();
    if (result.ok) {
      setOrders(result.data);
      setError(null);
    } else {
      expire(result.expired);
      setError(result.message);
    }
  }, [expire]);

  useEffect(() => {
    void load();
  }, [load]);

  async function move(order: AdminOrder, status: 'Processing' | 'Completed' | 'Cancelled') {
    if (status === 'Cancelled' && !window.confirm(`Huỷ đơn ${order.orderCode}?`)) return;
    const result = await setOrderStatus(order.id, status);
    if (!result.ok) {
      expire(result.expired);
      setError(result.message);
      return;
    }
    await load();
  }

  const shown = (orders ?? []).filter((o) => filter === 'all' || o.status === filter);

  return (
    <section>
      <header className="page-head row spread">
        <div>
          <h1>Đơn hàng</h1>
        </div>
        <div className="row">
          <select className="compact" value={filter} onChange={(e) => setFilter(e.target.value as typeof filter)} aria-label="Lọc trạng thái">
            <option value="all">Tất cả</option>
            {(Object.keys(COPY.status) as AdminOrder['status'][]).map((s) => (
              <option key={s} value={s}>
                {COPY.status[s]}
              </option>
            ))}
          </select>
          <button type="button" className="button small ghost" onClick={() => void load()}>
            <Glyph icon={ArrowClockwise} />
            Tải lại
          </button>
        </div>
      </header>
      {error ? <p className="error">{error}</p> : null}
      {orders === null ? <p className="empty">Đang tải đơn…</p> : null}
      {orders && shown.length === 0 ? <p className="empty">Không có đơn nào.</p> : null}
      {shown.length ? (
        <div className="table-wrap">
          <table className="table">
            <thead>
              <tr>
                <th>Mã đơn</th>
                <th>Khách</th>
                <th>Ốp</th>
                <th>Sticker</th>
                <th>Vị trí tâm</th>
                <th>Tổng</th>
                <th>Trạng thái</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {shown.map((order) => (
                <tr key={order.id}>
                  <td>
                    <strong>{order.orderCode}</strong>
                    <br />
                    <small>{order.createdAt?.toLocaleString('vi-VN')}</small>
                  </td>
                  <td>
                    {order.customerName}
                    <br />
                    <small>{order.customerPhone}</small>
                  </td>
                  <td>
                    {order.modelLabel}
                    <br />
                    <small>{order.caseLabel}</small>
                  </td>
                  <td>
                    <div className="cell-art">
                      {order.stickerImageUrl ? <img src={order.stickerImageUrl} alt="" /> : null}
                      {order.stickerName}
                    </div>
                  </td>
                  <td>
                    {order.at ? (
                      <>
                        x {mm(order.at.x)}
                        <br />y {mm(order.at.y)}
                        <br />xoay {order.rotationDeg}°
                      </>
                    ) : (
                      <small>Không có</small>
                    )}
                  </td>
                  <td>
                    {order.total !== null ? price(order.total) : ''}
                    <br />
                    <small>
                      {order.paymentMethod ? COPY.method[order.paymentMethod] : 'Chưa trả'}
                      {order.paymentStatus ? `, ${order.paymentStatus}` : ''}
                    </small>
                  </td>
                  <td>
                    <span className={`status status-${order.status.toLowerCase()}`}>{COPY.status[order.status]}</span>
                  </td>
                  <td>
                    <div className="row">
                      {NEXT[order.status].map((status) => (
                        <button key={status} type="button" className={status === 'Cancelled' ? 'button small ghost' : 'button small'} onClick={() => void move(order, status)}>
                          <Glyph icon={status === 'Cancelled' ? Prohibit : status === 'Completed' ? Check : PaintBrush} />
                          {ACTION[status]}
                        </button>
                      ))}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : null}
    </section>
  );
}
