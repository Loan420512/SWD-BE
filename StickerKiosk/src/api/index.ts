import type { KioskBackend } from '../kiosk/port';
import { fetchCatalog } from './catalog';
import { createHttp } from './http';
import { lookupOrder, submitOrder } from './order';

export interface HttpBackendOptions {
  readonly baseUrl: string;
  readonly timeoutMs: number;
}

export function createHttpBackend(opts: HttpBackendOptions): KioskBackend {
  const http = createHttp(opts.baseUrl, opts.timeoutMs);
  return {
    fetchCatalog: () => fetchCatalog(http),
    submitOrder: (draft, resume, report) => submitOrder(http, draft, resume, report),
    lookupOrder: (code) => lookupOrder(http, code),
  };
}
