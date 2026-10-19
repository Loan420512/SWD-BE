export type HttpFailure =
  | { readonly kind: 'network' }
  | { readonly kind: 'unauthorized' }
  | { readonly kind: 'not-found'; readonly message: string }
  | { readonly kind: 'invalid'; readonly message: string; readonly fields: Readonly<Record<string, readonly string[]>> }
  | { readonly kind: 'conflict'; readonly detail: string }
  | { readonly kind: 'rejected'; readonly status: number; readonly message: string }
  | { readonly kind: 'server'; readonly status: number; readonly detail: string };

export type HttpResult = { readonly ok: true; readonly data: unknown } | { readonly ok: false; readonly failure: HttpFailure };

export interface Http {
  get(path: string): Promise<HttpResult>;
  post(path: string, body: object): Promise<HttpResult>;
  put(path: string, body: object): Promise<HttpResult>;
  remove(path: string): Promise<HttpResult>;
  upload(path: string, form: FormData): Promise<HttpResult>;
}

export function createHttp(baseUrl: string, timeoutMs: number, token: () => string | null = () => null): Http {
  async function request(path: string, init: RequestInit = {}, json = true): Promise<HttpResult> {
    const headers: Record<string, string> = {};
    if (json) headers['Content-Type'] = 'application/json';
    const bearer = token();
    if (bearer) headers.Authorization = `Bearer ${bearer}`;
    try {
      const response = await fetch(`${baseUrl}${path}`, { ...init, headers, signal: AbortSignal.timeout(timeoutMs) });
      const body: unknown = await response.json().catch(() => null);
      if (response.ok) return { ok: true, data: unwrap(body) };
      return { ok: false, failure: classify(response.status, body) };
    } catch {
      return { ok: false, failure: { kind: 'network' } };
    }
  }
  return {
    get: (path) => request(path),
    post: (path, body) => request(path, { method: 'POST', body: JSON.stringify(body) }),
    put: (path, body) => request(path, { method: 'PUT', body: JSON.stringify(body) }),
    remove: (path) => request(path, { method: 'DELETE' }),
    upload: (path, form) => request(path, { method: 'POST', body: form }, false),
  };
}

function unwrap(body: unknown): unknown {
  if (body && typeof body === 'object' && 'success' in body && (body as { success: unknown }).success === true) {
    return (body as unknown as { data: unknown }).data;
  }
  return body;
}

export function classify(status: number, body: unknown): HttpFailure {
  if (status === 401 || status === 403) return { kind: 'unauthorized' };
  const record = body && typeof body === 'object' ? (body as Record<string, unknown>) : null;
  const message = typeof record?.message === 'string' ? record.message : '';
  const errors = record?.errors;
  let detail = '';
  if (errors && typeof errors === 'object' && !Array.isArray(errors) && 'detail' in errors) {
    const value = (errors as { detail?: unknown }).detail;
    if (typeof value === 'string') detail = value;
  }
  const text = `${message}\n${detail}`;
  if (/already exists/i.test(text) || /unique key/i.test(text) || /duplicate key/i.test(text)) {
    return { kind: 'conflict', detail: text.trim() };
  }
  if (status === 404) return { kind: 'not-found', message };
  if (record && typeof record.title === 'string' && record.errors && !('success' in record)) {
    return { kind: 'invalid', message: record.title, fields: fieldsOf(record.errors) };
  }
  if (status >= 500) return { kind: 'server', status, detail: text.trim() };
  return { kind: 'rejected', status, message: message || text.trim() };
}

function fieldsOf(errors: unknown): Readonly<Record<string, readonly string[]>> {
  if (!errors || typeof errors !== 'object') return {};
  const fields: Record<string, readonly string[]> = {};
  for (const [key, value] of Object.entries(errors)) {
    if (Array.isArray(value)) fields[key] = value.map(String);
  }
  return fields;
}

export function failureMessage(failure: HttpFailure): string {
  switch (failure.kind) {
    case 'network':
      return 'Không kết nối được máy chủ.';
    case 'unauthorized':
      return 'Phiên đăng nhập đã hết. Đăng nhập lại.';
    case 'not-found':
      return failure.message || 'Không tìm thấy.';
    case 'invalid':
      return Object.values(failure.fields).flat().join(' ') || failure.message;
    case 'conflict':
      return failure.detail;
    case 'rejected':
      return failure.message;
    case 'server':
      return 'Máy chủ lỗi. Thử lại sau.';
  }
}

export function createdId(data: unknown): number | null {
  if (!data || typeof data !== 'object' || !('id' in data)) return null;
  const id = (data as { id: unknown }).id;
  return typeof id === 'number' ? id : null;
}
