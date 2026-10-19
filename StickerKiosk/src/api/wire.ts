import type { PaymentMethod } from '../kiosk/checkout';
import type { AttemptId, OrderDraft } from '../kiosk/port';

type WirePaymentMethod = 'Cash' | 'VNPay' | 'Momo' | 'ZaloPay';

const WIRE_METHOD: Record<PaymentMethod, WirePaymentMethod> = {
  Cash: 'Cash',
  VNPay: 'VNPay',
  Momo: 'Momo',
  ZaloPay: 'ZaloPay',
};

/** paymentStatus is the literal 'Success': the SQL check rejects Pending and Paid. */
export interface CreatePaymentDto {
  readonly orderId: number;
  readonly amount: number;
  readonly paymentMethod: WirePaymentMethod;
  readonly paymentStatus: 'Success';
  readonly transactionId: string;
}

function token(attempt: AttemptId): string {
  return attempt.replace(/-/g, '').slice(0, 8).toUpperCase();
}

export function orderCodeOf(attempt: AttemptId): string {
  return `SK-${token(attempt)}`;
}

export function transactionIdOf(attempt: AttemptId, method: PaymentMethod): string {
  return `${orderCodeOf(attempt)}-${method}`;
}

export function templateNameOf(attempt: AttemptId): string {
  return `Kiosk ${orderCodeOf(attempt)}`;
}

export function paymentDto(orderId: number, draft: OrderDraft): CreatePaymentDto {
  return {
    orderId,
    amount: draft.total,
    paymentMethod: WIRE_METHOD[draft.method],
    paymentStatus: 'Success',
    transactionId: transactionIdOf(draft.attempt, draft.method),
  };
}
