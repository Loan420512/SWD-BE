import {
  addVnd,
  vnd,
  findCase,
  findModel,
  findSticker,
  preferredCase,
  designableModels,
  modelsWithCases,
  type CaseId,
  type CaseOffer,
  type Catalog,
  type ModelId,
  type PhoneModel,
  type StickerId,
  type StickerProduct,
  type Vnd,
} from './catalog';
import type { CheckoutProblem, Customer, PaymentMethod } from './checkout';
import type { AttemptId, Checkpoint, CheckoutReport, OrderDraft, SubmitResult } from './port';
import { backPoint, footprintOnSheet, normalizeRotation, spot, type BackPoint } from './sheet';

export type CatalogState =
  | { readonly status: 'loading' }
  | { readonly status: 'unreachable' }
  | { readonly status: 'ready'; readonly catalog: Catalog };

/** One sticker on the case. Stickers may overlap; only the case mask can reject a spot. */
export interface Placement {
  readonly key: string;
  readonly stickerId: StickerId;
  readonly at: BackPoint;
  readonly rotationDeg: number;
}

/** What the customer picked. Every placement center is legal for that sticker and case. */
export interface Design {
  readonly modelId: ModelId | null;
  readonly caseId: CaseId | null;
  readonly placements: readonly Placement[];
  readonly activeKey: string | null;
}

export interface OrderSummary {
  readonly modelLabel: string;
  readonly caseLabel: string;
  readonly casePrice: Vnd;
  readonly stickerName: string;
  readonly stickerImageUrl: string;
  readonly stickerPrice: Vnd;
  readonly total: Vnd;
}

export interface PendingOrder {
  readonly draft: OrderDraft;
  readonly design: Design;
  readonly summary: OrderSummary;
  readonly checkpoint: Checkpoint | null;
  readonly progress: number;
  readonly orderCode: string | null;
}

export type Phase =
  | { readonly phase: 'design' }
  | { readonly phase: 'checkout' }
  | { readonly phase: 'paying'; readonly order: PendingOrder }
  | { readonly phase: 'failed'; readonly order: PendingOrder; readonly problem: CheckoutProblem }
  | { readonly phase: 'paid'; readonly order: PendingOrder; readonly orderCode: string };

export interface Session {
  readonly catalog: CatalogState;
  readonly design: Design;
  readonly now: Phase;
  readonly epoch: number;
}

export type SessionEvent =
  | { readonly type: 'catalog'; readonly catalog: CatalogState }
  | { readonly type: 'chooseModel'; readonly id: ModelId }
  | { readonly type: 'chooseCase'; readonly id: CaseId }
  | { readonly type: 'chooseSticker'; readonly id: StickerId; readonly at?: BackPoint }
  | { readonly type: 'select'; readonly key: string }
  | { readonly type: 'commit'; readonly key: string; readonly at: BackPoint }
  | { readonly type: 'rotateTo'; readonly key: string; readonly rotationDeg: number }
  | { readonly type: 'stack'; readonly direction: 'up' | 'down' }
  | { readonly type: 'nudge'; readonly dx: number; readonly dy: number }
  | { readonly type: 'recenter' }
  | { readonly type: 'removeSticker' }
  | { readonly type: 'openCheckout' }
  | { readonly type: 'back' }
  | { readonly type: 'pay'; readonly customer: Customer; readonly method: PaymentMethod; readonly attempt: AttemptId }
  | { readonly type: 'progress'; readonly report: CheckoutReport }
  | { readonly type: 'settled'; readonly result: SubmitResult }
  | { readonly type: 'retry' }
  | { readonly type: 'resume'; readonly order: PendingOrder }
  | { readonly type: 'newOrder' };

export const EMPTY_DESIGN: Design = { modelId: null, caseId: null, placements: [], activeKey: null };

let placementSerial = 0;
function placementKey(): string {
  placementSerial += 1;
  return `p${placementSerial.toString(36)}`;
}

function activePlacement(design: Design): Placement | null {
  return design.placements.find((item) => item.key === design.activeKey) ?? design.placements.at(-1) ?? null;
}

export function initialSession(design: Design = EMPTY_DESIGN): Session {
  return { catalog: { status: 'loading' }, design, now: { phase: 'design' }, epoch: 0 };
}

export type Gate =
  | { readonly open: true }
  | { readonly open: false; readonly reason: 'loading' | 'unreachable' | 'pick-case' | 'not-fitted' | 'out-of-stock' | 'pick-sticker' | 'no-room' };

/** Everything a page shows about the current design, derived from ids. */
export interface Resolved {
  readonly model: PhoneModel | null;
  readonly offer: CaseOffer | null;
  readonly sticker: StickerProduct | null;
  readonly at: BackPoint | null;
  readonly rotationDeg: number;
  readonly placements: readonly (Placement & { readonly product: StickerProduct; readonly active: boolean })[];
  readonly total: Vnd | null;
  readonly gate: Gate;
}

export function resolve(session: Session, design: Design = session.design): Resolved {
  if (session.catalog.status !== 'ready') {
    const reason = session.catalog.status === 'loading' ? 'loading' : 'unreachable';
    return { model: null, offer: null, sticker: null, at: null, rotationDeg: 0, placements: [], total: null, gate: { open: false, reason } };
  }
  const catalog = session.catalog.catalog;
  const found = findCase(catalog, design.caseId);
  const model = found?.model ?? findModel(catalog, design.modelId);
  const offer = found?.offer ?? null;
  const placements = design.placements.flatMap((item) => {
    const product = findSticker(catalog, item.stickerId);
    return product ? [{ ...item, product, active: item.key === design.activeKey }] : [];
  });
  const current = placements.find((item) => item.active) ?? placements.at(-1) ?? null;
  const stickerTotal = placements.reduce((sum, item) => addVnd(sum, item.product.price), vnd(0));
  const total = offer && placements.length ? addVnd(offer.price, stickerTotal) : null;
  const gate: Gate = !offer
    ? { open: false, reason: 'pick-case' }
    : !offer.fit
      ? { open: false, reason: 'not-fitted' }
      : offer.sale.kind !== 'orderable'
        ? { open: false, reason: 'out-of-stock' }
        : placements.length === 0
          ? { open: false, reason: 'pick-sticker' }
          : { open: true };
  return {
    model,
    offer,
    sticker: current?.product ?? null,
    at: current?.at ?? null,
    rotationDeg: current?.rotationDeg ?? 0,
    placements,
    total,
    gate,
  };
}

/** Keeps every placement on the printable back. Overlaps between stickers are allowed. */
function settle(catalog: Catalog, design: Design): Design {
  const offer = findCase(catalog, design.caseId)?.offer ?? null;
  if (!offer?.fit) return design.placements.length === 0 ? design : { ...design, placements: [], activeKey: null };
  const sheet = offer.fit.sheet;
  const placements = design.placements.flatMap((item) => {
    const sticker = findSticker(catalog, item.stickerId);
    if (!sticker) return [];
    const rotationDeg = normalizeRotation(item.rotationDeg);
    if (footprintOnSheet(item.at, sticker, sheet, rotationDeg)) return [{ ...item, rotationDeg }];
    const at = spot(sticker, sheet, item.at, rotationDeg);
    return at ? [{ ...item, at, rotationDeg }] : [];
  });
  const activeKey = placements.some((item) => item.key === design.activeKey) ? design.activeKey : placements.at(-1)?.key ?? null;
  return { ...design, placements, activeKey };
}

/** Fills in a model and case when the catalog arrives, and drops ids that no longer exist. */
function adopt(catalog: Catalog, design: Design): Design {
  const found = findCase(catalog, design.caseId);
  const held = findModel(catalog, design.modelId);
  let modelId = found?.model.id ?? (held && held.cases.length > 0 ? held.id : null);
  let caseId = found?.offer.id ?? null;
  if (modelId === null) {
    const first = designableModels(catalog)[0] ?? modelsWithCases(catalog)[0];
    modelId = first?.id ?? null;
  }
  if (caseId === null && modelId !== null) {
    caseId = preferredCase(findModel(catalog, modelId)!)?.id ?? null;
  }
  return settle(catalog, { modelId, caseId, placements: design.placements, activeKey: design.activeKey });
}

function sameDesign(a: Design, b: Design): boolean {
  if (a.modelId !== b.modelId || a.caseId !== b.caseId || a.activeKey !== b.activeKey || a.placements.length !== b.placements.length) return false;
  return a.placements.every((item, index) => {
    const other = b.placements[index];
    return other && item.key === other.key && item.stickerId === other.stickerId && item.at.x === other.at.x && item.at.y === other.at.y && item.rotationDeg === other.rotationDeg;
  });
}

function withDesign(session: Session, design: Design): Session {
  return sameDesign(session.design, design) ? session : { ...session, design };
}

function bumped(session: Session, now: Phase, design = session.design): Session {
  return { ...session, design, now, epoch: session.epoch + 1 };
}

export function summarize(catalog: Catalog, design: Design): OrderSummary | null {
  const found = findCase(catalog, design.caseId);
  const products = design.placements.flatMap((item) => {
    const sticker = findSticker(catalog, item.stickerId);
    return sticker ? [sticker] : [];
  });
  const sticker = products[0];
  if (!found || !sticker) return null;
  const stickerPrice = products.reduce((sum, item) => addVnd(sum, item.price), vnd(0));
  return {
    modelLabel: found.model.label,
    caseLabel: found.offer.label,
    casePrice: found.offer.price,
    stickerName: products.map((item) => item.name).join(', '),
    stickerImageUrl: sticker.imageUrl,
    stickerPrice,
    total: addVnd(found.offer.price, stickerPrice),
  };
}

export function reduce(session: Session, event: SessionEvent): Session {
  switch (event.type) {
    case 'catalog': {
      const next = { ...session, catalog: event.catalog };
      if (event.catalog.status !== 'ready') return next;
      return { ...next, design: adopt(event.catalog.catalog, session.design) };
    }
    case 'resume': {
      if (session.now.phase !== 'design') return session;
      return bumped(session, { phase: 'failed', order: event.order, problem: 'offline' }, event.order.design);
    }
    case 'newOrder': {
      if (session.now.phase === 'paying') return session;
      const design = session.catalog.status === 'ready' ? adopt(session.catalog.catalog, EMPTY_DESIGN) : EMPTY_DESIGN;
      return bumped(session, { phase: 'design' }, design);
    }
    case 'retry': {
      if (session.now.phase !== 'failed') return session;
      return bumped(session, { phase: 'paying', order: session.now.order });
    }
    case 'progress': {
      if (session.now.phase !== 'paying') return session;
      const order = session.now.order;
      return {
        ...session,
        now: {
          phase: 'paying',
          order: {
            ...order,
            checkpoint: event.report.checkpoint,
            progress: Math.max(order.progress, event.report.progress),
            orderCode: event.report.orderCode ?? order.orderCode,
          },
        },
      };
    }
    case 'settled': {
      if (session.now.phase !== 'paying') return session;
      const order = session.now.order;
      return event.result.ok
        ? bumped(session, { phase: 'paid', order, orderCode: event.result.orderCode })
        : bumped(session, { phase: 'failed', order, problem: event.result.problem });
    }
    default:
      break;
  }

  if (session.catalog.status !== 'ready') return session;
  const catalog = session.catalog.catalog;

  if (event.type === 'back') {
    return session.now.phase === 'checkout' ? bumped(session, { phase: 'design' }) : session;
  }

  if (event.type === 'openCheckout') {
    if (session.now.phase !== 'design' || !resolve(session).gate.open) return session;
    return bumped(session, { phase: 'checkout' });
  }

  if (event.type === 'pay') {
    if (session.now.phase !== 'checkout') return session;
    const resolved = resolve(session);
    const summary = summarize(catalog, session.design);
    if (!resolved.gate.open || !resolved.offer || resolved.placements.length === 0 || !summary) return session;
    const draft: OrderDraft = {
      attempt: event.attempt,
      customer: event.customer,
      caseId: resolved.offer.id,
      stickers: resolved.placements.map((item) => ({
        productId: item.product.id,
        name: item.product.name,
        at: item.at,
        rotationDeg: item.rotationDeg,
      })),
      total: summary.total,
      method: event.method,
    };
    return bumped(session, {
      phase: 'paying',
      order: { draft, design: session.design, summary, checkpoint: null, progress: 0, orderCode: null },
    });
  }

  if (session.now.phase !== 'design') return session;
  const design = session.design;

  switch (event.type) {
    case 'chooseModel': {
      const model = findModel(catalog, event.id);
      if (!model || model.cases.length === 0 || model.id === design.modelId) return session;
      return withDesign(session, settle(catalog, { ...design, modelId: model.id, caseId: preferredCase(model)?.id ?? null }));
    }
    case 'chooseCase': {
      const found = findCase(catalog, event.id);
      if (!found || found.offer.id === design.caseId) return session;
      return withDesign(session, settle(catalog, { ...design, modelId: found.model.id, caseId: found.offer.id }));
    }
    case 'chooseSticker': {
      const sticker = findSticker(catalog, event.id);
      const offer = findCase(catalog, design.caseId)?.offer;
      if (!sticker || !offer?.fit) return session;
      const sheet = offer.fit.sheet;
      const dropped = event.at && footprintOnSheet(event.at, sticker, sheet, 0) ? event.at : null;
      const near = event.at ?? activePlacement(design)?.at;
      const at = dropped ?? spot(sticker, sheet, near ? backPoint(near.x + 40, near.y + 40) : undefined);
      if (!at) return session;
      const key = placementKey();
      return withDesign(session, {
        ...design,
        activeKey: key,
        placements: [...design.placements, { key, stickerId: sticker.id, at, rotationDeg: 0 }],
      });
    }
    case 'select': {
      if (!design.placements.some((item) => item.key === event.key) || event.key === design.activeKey) return session;
      return withDesign(session, { ...design, activeKey: event.key });
    }
    case 'commit': {
      const offer = findCase(catalog, design.caseId)?.offer;
      const item = design.placements.find((placement) => placement.key === event.key);
      const sticker = item ? findSticker(catalog, item.stickerId) : null;
      if (!offer?.fit || !item || !sticker || !footprintOnSheet(event.at, sticker, offer.fit.sheet, item.rotationDeg)) return session;
      return withDesign(session, {
        ...design,
        activeKey: item.key,
        placements: design.placements.map((placement) => (placement.key === item.key ? { ...placement, at: event.at } : placement)),
      });
    }
    case 'rotateTo': {
      const offer = findCase(catalog, design.caseId)?.offer;
      const item = design.placements.find((placement) => placement.key === event.key);
      const sticker = item ? findSticker(catalog, item.stickerId) : null;
      if (!offer?.fit || !item || !sticker) return session;
      const rotationDeg = normalizeRotation(event.rotationDeg);
      if (rotationDeg === item.rotationDeg) return session;
      const sheet = offer.fit.sheet;
      if (footprintOnSheet(item.at, sticker, sheet, rotationDeg)) {
        return withDesign(session, {
          ...design,
          activeKey: item.key,
          placements: design.placements.map((placement) => (placement.key === item.key ? { ...placement, rotationDeg } : placement)),
        });
      }
      const at = spot(sticker, sheet, item.at, rotationDeg);
      if (!at) return session;
      return withDesign(session, {
        ...design,
        activeKey: item.key,
        placements: design.placements.map((placement) => (placement.key === item.key ? { ...placement, rotationDeg, at } : placement)),
      });
    }
    case 'stack': {
      const item = activePlacement(design);
      if (!item) return session;
      const index = design.placements.findIndex((placement) => placement.key === item.key);
      const next = event.direction === 'up' ? index + 1 : index - 1;
      if (index < 0 || next < 0 || next >= design.placements.length) return session;
      const placements = design.placements.slice();
      const [moved] = placements.splice(index, 1);
      placements.splice(next, 0, moved!);
      return withDesign(session, { ...design, placements });
    }
    case 'nudge': {
      const item = activePlacement(design);
      if (!item) return session;
      return reduce(session, { type: 'commit', key: item.key, at: backPoint(item.at.x + event.dx, item.at.y + event.dy) });
    }
    case 'recenter': {
      const offer = findCase(catalog, design.caseId)?.offer;
      const item = activePlacement(design);
      const sticker = item ? findSticker(catalog, item.stickerId) : null;
      if (!offer?.fit || !item || !sticker) return session;
      const at = spot(sticker, offer.fit.sheet, undefined, item.rotationDeg);
      if (!at) return session;
      return withDesign(session, {
        ...design,
        placements: design.placements.map((placement) => (placement.key === item.key ? { ...placement, at } : placement)),
      });
    }
    case 'removeSticker': {
      const item = activePlacement(design);
      if (!item) return session;
      const placements = design.placements.filter((placement) => placement.key !== item.key);
      return withDesign(session, { ...design, placements, activeKey: placements.at(-1)?.key ?? null });
    }
    default:
      return session;
  }
}
