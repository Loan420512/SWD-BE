import type { CheckoutProblem, PaymentMethod } from '../kiosk/checkout';
import { vnd } from '../kiosk/catalog';
import type { Checkpoint, CheckoutReport, OrderDraft, PublicOrder, SubmitResult } from '../kiosk/port';
import { backPoint } from '../kiosk/sheet';
import { createdId, type Http, type HttpFailure, type HttpResult } from './http';
import { orderCodeOf, paymentDto, templateNameOf } from './wire';

type Rung =
  | { readonly at: 'start' }
  | { readonly at: 'customer'; readonly customerId: number }
  | { readonly at: 'template'; readonly customerId: number; readonly templateId: number }
  | { readonly at: 'sticker'; readonly customerId: number; readonly templateId: number }
  | { readonly at: 'order'; readonly orderId: number }
  | { readonly at: 'paid' };

const LADDER = ['start', 'customer', 'template', 'sticker', 'order', 'paid'] as const;
type LadderCovers = Exclude<Rung['at'], (typeof LADDER)[number]> extends never ? true : never;
export const ladderCovers: LadderCovers = true;

function seal(rung: Rung): Checkpoint {
  return rung as unknown as Checkpoint;
}

function unseal(checkpoint: Checkpoint): Rung {
  const rung = checkpoint as unknown as Rung;
  return LADDER.includes(rung.at) ? rung : { at: 'start' };
}

function progressOf(rung: Rung): number {
  return LADDER.indexOf(rung.at) / (LADDER.length - 1);
}

function problemOf(failure: HttpFailure): CheckoutProblem {
  if (failure.kind === 'network') return 'offline';
  if (failure.kind === 'server') return 'server';
  return 'rejected';
}

async function orderIdByCode(http: Http, code: string): Promise<number | null> {
  const result = await http.get(`/Orders/by-code/${encodeURIComponent(code)}`);
  return result.ok ? createdId(result.data) : null;
}

function step(result: HttpResult, next: (id: number) => Rung): { ok: true; rung: Rung } | { ok: false; failure: HttpFailure } {
  if (!result.ok) return { ok: false, failure: result.failure };
  const id = createdId(result.data);
  if (id == null) return { ok: false, failure: { kind: 'server', status: 200, detail: 'Response has no id' } };
  return { ok: true, rung: next(id) };
}

/**
 * One request. Each rung is safe to re-run with the same draft:
 *   sticker  one per template, so "already exists" means ours landed
 *   order    our orderCode is unique, so a conflict means ours landed; find it by code
 *   payment  one per order and we only send Success, so a conflict means done
 */
async function advance(http: Http, draft: OrderDraft, rung: Rung): Promise<{ ok: true; rung: Rung } | { ok: false; failure: HttpFailure }> {
  switch (rung.at) {
    case 'start': {
      const body: { fullName: string; phone?: string } = { fullName: draft.customer.fullName };
      if (draft.customer.phone) body.phone = draft.customer.phone;
      return step(await http.post('/Customers', body), (id) => ({ at: 'customer', customerId: id }));
    }
    case 'customer':
      return step(await http.post('/DesignTemplates', { templateName: templateNameOf(draft.attempt), caseId: draft.caseId }), (id) => ({
        at: 'template',
        customerId: rung.customerId,
        templateId: id,
      }));
    case 'template': {
      for (const sticker of draft.stickers) {
        const result = await http.post('/StickerItems', {
          templateId: rung.templateId,
          stickerProductId: sticker.productId,
          stickerName: sticker.name,
          positionX: sticker.at.x,
          positionY: sticker.at.y,
          rotationDeg: sticker.rotationDeg,
          scale: 1,
        });
        if (!result.ok && result.failure.kind !== 'conflict') return { ok: false, failure: result.failure };
      }
      return { ok: true, rung: { at: 'sticker', customerId: rung.customerId, templateId: rung.templateId } };
    }
    case 'sticker': {
      const code = orderCodeOf(draft.attempt);
      const result = await http.post('/Orders', {
        customerId: rung.customerId,
        phoneCaseId: draft.caseId,
        designTemplateId: rung.templateId,
        totalAmount: draft.total,
        orderCode: code,
      });
      if (result.ok) return step(result, (id) => ({ at: 'order', orderId: id }));
      if (result.failure.kind === 'conflict') {
        const id = await orderIdByCode(http, code);
        if (id) return { ok: true, rung: { at: 'order', orderId: id } };
      }
      return { ok: false, failure: result.failure };
    }
    case 'order': {
      const result = await http.post('/Payments', paymentDto(rung.orderId, draft));
      if (result.ok || result.failure.kind === 'conflict') return { ok: true, rung: { at: 'paid' } };
      return { ok: false, failure: result.failure };
    }
    case 'paid':
      return { ok: true, rung };
  }
}

export async function submitOrder(http: Http, draft: OrderDraft, resume: Checkpoint | null, report: (r: CheckoutReport) => void): Promise<SubmitResult> {
  let rung: Rung = resume ? unseal(resume) : { at: 'start' };
  let networkTries = 0;
  while (rung.at !== 'paid') {
    const stepped = await advance(http, draft, rung);
    if (!stepped.ok && stepped.failure.kind === 'network' && networkTries < 2) {
      networkTries += 1;
      await new Promise((resolve) => setTimeout(resolve, 300 * networkTries));
      continue;
    }
    networkTries = 0;
    if (!stepped.ok) return { ok: false, problem: problemOf(stepped.failure) };
    rung = stepped.rung;
    const code = rung.at === 'order' || rung.at === 'paid' ? orderCodeOf(draft.attempt) : null;
    report({ checkpoint: seal(rung), progress: progressOf(rung), orderCode: code });
  }
  return { ok: true, orderCode: orderCodeOf(draft.attempt) };
}

const STATUSES = ['Pending', 'Processing', 'Completed', 'Cancelled'] as const;
const METHODS = ['Cash', 'VNPay', 'Momo', 'ZaloPay'] as const;

export function parsePublicOrder(data: unknown): PublicOrder | null {
  const r = (data ?? {}) as Record<string, unknown>;
  if (typeof r.orderCode !== 'string') return null;
  const status = STATUSES.find((s) => s === r.status) ?? 'Pending';
  const method = METHODS.find((m) => m === r.paymentMethod) ?? null;
  return {
    orderCode: r.orderCode,
    status,
    total: typeof r.totalAmount === 'number' ? vnd(r.totalAmount) : null,
    modelLabel: typeof r.modelLabel === 'string' ? r.modelLabel : null,
    caseLabel: typeof r.caseLabel === 'string' ? r.caseLabel : null,
    stickerName: typeof r.stickerName === 'string' ? r.stickerName : null,
    stickerImageUrl: typeof r.stickerImageUrl === 'string' ? r.stickerImageUrl : null,
    at: typeof r.positionX === 'number' && typeof r.positionY === 'number' ? backPoint(r.positionX, r.positionY) : null,
    rotationDeg: typeof r.rotationDeg === 'number' ? r.rotationDeg : 0,
    placements: Array.isArray(r.placements)
      ? r.placements.flatMap((item) => {
          const row = item as Record<string, unknown>;
          if (typeof row.name !== 'string') return [];
          return [{
            name: row.name,
            imageUrl: typeof row.imageUrl === 'string' ? row.imageUrl : null,
            at: typeof row.positionX === 'number' && typeof row.positionY === 'number' ? backPoint(row.positionX, row.positionY) : null,
            rotationDeg: typeof row.rotationDeg === 'number' ? row.rotationDeg : 0,
          }];
        })
      : [],
    paymentMethod: method as PaymentMethod | null,
    createdAt: typeof r.createdAt === 'string' ? new Date(r.createdAt.endsWith('Z') ? r.createdAt : `${r.createdAt}Z`) : null,
  };
}

export async function lookupOrder(http: Http, code: string): Promise<PublicOrder | 'not-found' | 'unreachable'> {
  const result = await http.get(`/Orders/by-code/${encodeURIComponent(code.trim().toUpperCase())}`);
  if (result.ok) return parsePublicOrder(result.data) ?? 'not-found';
  return result.failure.kind === 'not-found' ? 'not-found' : 'unreachable';
}
