import type { CheckoutProblem, Gate, PaymentMethod, PublicOrder } from '../kiosk';
import { PAYMENT_METHODS } from '../kiosk';

type GateReason = Extract<Gate, { open: false }>['reason'];

export const COPY = {
  gate: {
    loading: 'Đang tải danh mục.',
    unreachable: 'Quầy chưa kết nối được máy chủ, nên chưa đặt hàng được.',
    'pick-case': 'Chọn một ốp lưng.',
    'not-fitted': 'Ốp này chưa có model 3D. Chọn ốp khác.',
    'out-of-stock': 'Ốp này đã hết hàng. Chọn ốp khác.',
    'pick-sticker': 'Chọn một sticker trong khay.',
    'no-room': 'Sticker này không vừa lưng ốp đã chọn.',
  } satisfies Record<GateReason, string>,
  method: {
    Cash: 'Tiền mặt tại quầy',
    VNPay: 'VNPay',
    Momo: 'MoMo',
    ZaloPay: 'ZaloPay',
  } satisfies Record<PaymentMethod, string>,
  problem: {
    offline: 'Mất kết nối giữa chừng. Bấm thử lại, quầy sẽ tiếp tục từ bước còn thiếu và không tạo đơn trùng.',
    rejected: 'Máy chủ không nhận đơn. Đưa mã đơn (nếu có) cho nhân viên.',
    server: 'Máy chủ đang lỗi. Thử lại sau ít phút.',
  } satisfies Record<CheckoutProblem, string>,
  status: {
    Pending: 'Đã nhận đơn',
    Processing: 'Đang dán',
    Completed: 'Sẵn sàng nhận',
    Cancelled: 'Đã huỷ',
  } satisfies Record<PublicOrder['status'], string>,
  methods: PAYMENT_METHODS,
};

export function price(value: number): string {
  return `${new Intl.NumberFormat('vi-VN').format(value)}₫`;
}

export function mm(tenths: number): string {
  return `${new Intl.NumberFormat('vi-VN', { minimumFractionDigits: 1, maximumFractionDigits: 1 }).format(tenths / 10)} mm`;
}

export function size(widthMm: number, heightMm: number): string {
  const f = new Intl.NumberFormat('vi-VN', { maximumFractionDigits: 1 });
  return `${f.format(widthMm)} × ${f.format(heightMm)} mm`;
}
