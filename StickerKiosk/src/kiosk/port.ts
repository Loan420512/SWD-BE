/**
 * What a backend adapter provides. src/api implements it; pages never import it.
 */

import type { CaseId, Catalog, StickerId, Vnd } from './catalog';
import type { CheckoutProblem, Customer, PaymentMethod } from './checkout';
import type { BackPoint } from './sheet';

declare const attemptBrand: unique symbol;
/** One press of Pay. Retries keep it; every server-side key is derived from it. */
export type AttemptId = string & { readonly [attemptBrand]: true };

declare const checkpointBrand: unique symbol;
/** Opaque resume point. The session stores it; only the adapter reads it. */
export type Checkpoint = { readonly [checkpointBrand]: true };

export function attemptIdOf(raw: string): AttemptId {
  return raw as AttemptId;
}

export interface OrderDraft {
  readonly attempt: AttemptId;
  readonly customer: Customer;
  readonly caseId: CaseId;
  readonly stickers: readonly { readonly productId: StickerId; readonly name: string; readonly at: BackPoint; readonly rotationDeg: number }[];
  readonly total: Vnd;
  readonly method: PaymentMethod;
}

export interface CheckoutReport {
  readonly checkpoint: Checkpoint;
  readonly progress: number;
  readonly orderCode: string | null;
}

export type SubmitResult =
  | { readonly ok: true; readonly orderCode: string }
  | { readonly ok: false; readonly problem: CheckoutProblem };

export interface PublicOrder {
  readonly orderCode: string;
  readonly status: 'Pending' | 'Processing' | 'Completed' | 'Cancelled';
  readonly total: Vnd | null;
  readonly modelLabel: string | null;
  readonly caseLabel: string | null;
  readonly stickerName: string | null;
  readonly stickerImageUrl: string | null;
  readonly at: BackPoint | null;
  readonly rotationDeg: number;
  readonly placements: readonly { readonly name: string; readonly imageUrl: string | null; readonly at: BackPoint | null; readonly rotationDeg: number }[];
  readonly paymentMethod: PaymentMethod | null;
  readonly createdAt: Date | null;
}

export interface KioskBackend {
  fetchCatalog(): Promise<Catalog | 'unreachable'>;
  submitOrder(draft: OrderDraft, resume: Checkpoint | null, report: (report: CheckoutReport) => void): Promise<SubmitResult>;
  lookupOrder(code: string): Promise<PublicOrder | 'not-found' | 'unreachable'>;
}
