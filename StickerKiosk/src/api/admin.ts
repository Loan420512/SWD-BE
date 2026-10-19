import type { CaseId, ModelId, StickerId, StickerProduct } from '../kiosk/catalog';
import { encodeMask, type AssetPose, type OccupancyMask, type PhoneNest } from '../kiosk/sheet';
import type { PublicOrder } from '../kiosk/port';
import { parseSticker, type WireSticker } from './catalog';
import { createHttp, failureMessage, type HttpResult } from './http';
import { parsePublicOrder } from './order';

const TOKEN_KEY = 'stickerkiosk.admin';

export interface AdminSession {
  readonly token: string;
  readonly username: string;
  readonly role: string;
  readonly expiresAt: string;
}

export type AdminResult<T> = { readonly ok: true; readonly data: T } | { readonly ok: false; readonly message: string; readonly expired: boolean };

export interface AdminSticker extends StickerProduct {
  readonly active: boolean;
}

export interface AdminOrder extends PublicOrder {
  readonly id: number;
  readonly customerName: string | null;
  readonly customerPhone: string | null;
  readonly paymentStatus: string | null;
}

export interface CaseFitInput {
  readonly modelUrl: string;
  readonly pose: AssetPose;
  readonly heightMm: number;
  readonly widthMm: number;
  readonly mask: OccupancyMask;
  readonly nest: PhoneNest;
}

export interface CaseInput {
  readonly modelId: ModelId;
  readonly sku: string;
  readonly material: string;
  readonly color: string;
  readonly price: number;
  readonly stockQuantity: number;
}

export interface StickerInput {
  readonly name: string;
  readonly imageUrl: string;
  readonly widthMm: number;
  readonly heightMm: number;
  readonly price: number;
  readonly active: boolean;
}

export function readAdminSession(): AdminSession | null {
  try {
    const raw = sessionStorage.getItem(TOKEN_KEY);
    if (!raw) return null;
    const parsed = JSON.parse(raw) as AdminSession;
    return new Date(parsed.expiresAt).getTime() > Date.now() ? parsed : null;
  } catch {
    return null;
  }
}

export function clearAdminSession() {
  sessionStorage.removeItem(TOKEN_KEY);
}

function result<T>(r: HttpResult, map: (data: unknown) => T): AdminResult<T> {
  if (r.ok) return { ok: true, data: map(r.data) };
  return { ok: false, message: failureMessage(r.failure), expired: r.failure.kind === 'unauthorized' };
}

const http = createHttp('/api', 30_000, () => readAdminSession()?.token ?? null);

export async function login(username: string, password: string): Promise<AdminResult<AdminSession>> {
  const r = await http.post('/Auth/login', { username, password });
  const mapped = result(r, (data) => data as AdminSession);
  if (mapped.ok) sessionStorage.setItem(TOKEN_KEY, JSON.stringify(mapped.data));
  return mapped;
}

export async function uploadFile(kind: 'phone' | 'case' | 'sticker', file: File): Promise<AdminResult<string>> {
  const form = new FormData();
  form.append('kind', kind);
  form.append('file', file);
  return result(await http.upload('/Uploads', form), (data) => (data as { url: string }).url);
}

export async function createModel(input: { brand: string; modelName: string; dimensions: string }): Promise<AdminResult<number>> {
  return result(await http.post('/PhoneModels', input), (data) => (data as { id: number }).id);
}

export async function updateModel(
  id: ModelId,
  input: { brand: string; modelName: string; dimensions: string },
): Promise<AdminResult<null>> {
  return result(await http.put(`/PhoneModels/${id}`, input), () => null);
}

export async function deleteModel(id: ModelId): Promise<AdminResult<null>> {
  return result(await http.remove(`/PhoneModels/${id}`), () => null);
}

export async function saveModelFit(id: ModelId, modelUrl: string, pose: AssetPose): Promise<AdminResult<null>> {
  return result(await http.put(`/PhoneModels/${id}/fit`, { modelUrl, backAxis: pose.back, upAxis: pose.up }), () => null);
}

export async function createCase(input: CaseInput): Promise<AdminResult<number>> {
  return result(await http.post('/PhoneCases', input), (data) => (data as { id: number }).id);
}

export async function updateCase(id: CaseId, input: CaseInput): Promise<AdminResult<null>> {
  return result(await http.put(`/PhoneCases/${id}`, input), () => null);
}

export async function deleteCase(id: CaseId): Promise<AdminResult<null>> {
  return result(await http.remove(`/PhoneCases/${id}`), () => null);
}

export async function saveCaseFit(id: CaseId, fit: CaseFitInput): Promise<AdminResult<null>> {
  return result(
    await http.put(`/PhoneCases/${id}/fit`, {
      modelUrl: fit.modelUrl,
      backAxis: fit.pose.back,
      upAxis: fit.pose.up,
      heightMm: fit.heightMm,
      backWidthMm: fit.widthMm,
      maskCols: fit.mask.cols,
      maskRows: fit.mask.rows,
      maskBase64: encodeMask(fit.mask),
      phoneInsetMm: fit.nest.insetMm,
      phoneScale: fit.nest.scale,
    }),
    () => null,
  );
}

export async function listStickers(): Promise<AdminResult<AdminSticker[]>> {
  return result(await http.get('/StickerProducts'), (data) =>
    (Array.isArray(data) ? (data as WireSticker[]) : []).flatMap((wire) => {
      const sticker = parseSticker(wire);
      return sticker ? [{ ...sticker, active: wire.isActive !== false }] : [];
    }),
  );
}

function stickerBody(input: StickerInput) {
  return {
    stickerName: input.name,
    imageUrl: input.imageUrl,
    widthMm: input.widthMm,
    heightMm: input.heightMm,
    price: input.price,
    isActive: input.active,
  };
}

export async function createSticker(input: StickerInput): Promise<AdminResult<null>> {
  return result(await http.post('/StickerProducts', stickerBody(input)), () => null);
}

export async function updateSticker(id: StickerId, input: StickerInput): Promise<AdminResult<null>> {
  return result(await http.put(`/StickerProducts/${id}`, stickerBody(input)), () => null);
}

export async function listOrders(): Promise<AdminResult<AdminOrder[]>> {
  return result(await http.get('/Orders'), (data) =>
    (Array.isArray(data) ? data : []).flatMap((row) => {
      const order = parsePublicOrder(row);
      if (!order) return [];
      const r = row as Record<string, unknown>;
      return [
        {
          ...order,
          id: Number(r.id),
          customerName: typeof r.customerName === 'string' ? r.customerName : null,
          customerPhone: typeof r.customerPhone === 'string' ? r.customerPhone : null,
          paymentStatus: typeof r.paymentStatus === 'string' ? r.paymentStatus : null,
        },
      ];
    }),
  );
}

export async function setOrderStatus(id: number, status: 'Processing' | 'Completed' | 'Cancelled'): Promise<AdminResult<null>> {
  return result(await http.put(`/Orders/${id}/status`, { status }), () => null);
}
