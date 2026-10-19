export const PAYMENT_METHODS = ['Cash', 'VNPay', 'Momo', 'ZaloPay'] as const;
export type PaymentMethod = (typeof PAYMENT_METHODS)[number];

export interface CheckoutForm {
  readonly fullName: string;
  readonly phone: string;
  readonly method: PaymentMethod;
}

export type FormErrors = {
  readonly fullName?: 'required' | 'too-long';
  readonly phone?: 'invalid';
};

export interface Customer {
  readonly fullName: string;
  readonly phone: string | null;
}

export type CheckoutProblem = 'offline' | 'rejected' | 'server';

export function parseCheckout(
  form: CheckoutForm,
): { readonly ok: true; readonly customer: Customer; readonly method: PaymentMethod } | { readonly ok: false; readonly errors: FormErrors } {
  const name = form.fullName.trim();
  const errors: { fullName?: 'required' | 'too-long'; phone?: 'invalid' } = {};
  if (!name) errors.fullName = 'required';
  else if (name.length > 100) errors.fullName = 'too-long';

  const digits = form.phone.replace(/[^\d]/g, '');
  let phone: string | null = null;
  if (form.phone.trim()) {
    if (!/^0\d{9}$/.test(digits)) errors.phone = 'invalid';
    else phone = digits;
  }

  if (errors.fullName || errors.phone) return { ok: false, errors };
  return { ok: true, customer: { fullName: name, phone }, method: form.method };
}
