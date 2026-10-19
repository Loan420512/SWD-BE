import type { KioskBackend } from './port';
import { attemptIdOf } from './port';
import { parseCheckout, type CheckoutForm, type FormErrors } from './checkout';
import { EMPTY_DESIGN, initialSession, reduce, type Design, type PendingOrder, type Session, type SessionEvent } from './session';

const DESIGN_KEY = 'stickerkiosk.design';
const ATTEMPT_KEY = 'stickerkiosk.attempt';

/** One customer session. `reduce` is the only writer; this shell runs effects on phase entry. */
export interface Kiosk {
  getState(): Session;
  subscribe(onChange: () => void): () => void;
  dispatch(event: SessionEvent): void;
  /** Validates the form; on success starts the five-request order. */
  pay(form: CheckoutForm): FormErrors | null;
  reloadCatalog(): void;
  readonly backend: KioskBackend;
}

export function createKiosk(backend: KioskBackend): Kiosk {
  let state = initialSession(readDesign());
  const listeners = new Set<() => void>();
  let resume = readAttempt();

  function dispatch(event: SessionEvent) {
    const next = reduce(state, event);
    if (next === state) return;
    const entered = next.now.phase !== state.now.phase || next.epoch !== state.epoch;
    state = next;
    persist();
    for (const listener of listeners) listener();
    if (entered && state.now.phase === 'paying') runOrder(state.epoch, state.now.order);
  }

  function persist() {
    try {
      sessionStorage.setItem(DESIGN_KEY, JSON.stringify(state.design));
      if (state.now.phase === 'paying' || state.now.phase === 'failed') {
        sessionStorage.setItem(ATTEMPT_KEY, JSON.stringify(state.now.order));
      } else {
        sessionStorage.removeItem(ATTEMPT_KEY);
      }
    } catch {
      // Private mode or full storage: the session still works in memory.
    }
  }

  function runOrder(epoch: number, order: PendingOrder) {
    void backend
      .submitOrder(order.draft, order.checkpoint, (report) => {
        if (state.epoch === epoch) dispatch({ type: 'progress', report });
      })
      .then((result) => {
        if (state.epoch === epoch) dispatch({ type: 'settled', result });
      });
  }

  function reloadCatalog() {
    dispatch({ type: 'catalog', catalog: { status: 'loading' } });
    void backend.fetchCatalog().then((catalog) => {
      dispatch({ type: 'catalog', catalog: catalog === 'unreachable' ? { status: 'unreachable' } : { status: 'ready', catalog } });
      if (resume && state.catalog.status === 'ready') {
        const order = resume;
        resume = null;
        dispatch({ type: 'resume', order });
      }
    });
  }

  reloadCatalog();

  return {
    getState: () => state,
    subscribe(onChange) {
      listeners.add(onChange);
      return () => listeners.delete(onChange);
    },
    dispatch,
    pay(form) {
      const parsed = parseCheckout(form);
      if (!parsed.ok) return parsed.errors;
      dispatch({ type: 'pay', customer: parsed.customer, method: parsed.method, attempt: attemptIdOf(crypto.randomUUID()) });
      return null;
    },
    reloadCatalog,
    backend,
  };
}

function readDesign(): Design {
  try {
    const raw = sessionStorage.getItem(DESIGN_KEY);
    if (!raw) return EMPTY_DESIGN;
    const parsed = JSON.parse(raw) as Partial<Design>;
    return {
      modelId: (parsed.modelId ?? null) as Design['modelId'],
      caseId: (parsed.caseId ?? null) as Design['caseId'],
      placements: Array.isArray(parsed.placements) ? parsed.placements : [],
      activeKey: parsed.activeKey ?? null,
    };
  } catch {
    return EMPTY_DESIGN;
  }
}

function readAttempt(): PendingOrder | null {
  try {
    const raw = sessionStorage.getItem(ATTEMPT_KEY);
    if (!raw) return null;
    const parsed = JSON.parse(raw) as PendingOrder;
    return parsed?.draft?.attempt ? parsed : null;
  } catch {
    return null;
  }
}
