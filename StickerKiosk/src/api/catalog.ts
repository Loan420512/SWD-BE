import {
  caseLabel,
  vnd,
  type CaseId,
  type CaseOffer,
  type Catalog,
  type ImportedModel,
  type ModelId,
  type PhoneModel,
  type StickerId,
  type StickerProduct,
} from '../kiosk/catalog';
import { decodeMask, isPose, DEFAULT_NEST, type AssetPose, type Axis, type PhoneNest } from '../kiosk/sheet';
import type { Http } from './http';

interface WirePose {
  readonly modelUrl?: string;
  readonly backAxis?: string;
  readonly upAxis?: string;
}

interface WireFit extends WirePose {
  readonly heightMm?: number;
  readonly backWidthMm?: number;
  readonly maskCols?: number;
  readonly maskRows?: number;
  readonly maskBase64?: string;
  readonly phoneInsetMm?: number;
  readonly phoneScale?: number;
}

interface WireCase {
  readonly id?: number;
  readonly sku?: string | null;
  readonly material?: string | null;
  readonly color?: string | null;
  readonly price?: number | null;
  readonly stockQuantity?: number | null;
  readonly fit?: WireFit | null;
}

interface WireModel {
  readonly id?: number;
  readonly brand?: string | null;
  readonly modelName?: string | null;
  readonly dimensions?: string | null;
  readonly phone?: WirePose | null;
  readonly cases?: readonly WireCase[];
}

export interface WireSticker {
  readonly id?: number;
  readonly stickerName?: string;
  readonly imageUrl?: string;
  readonly widthMm?: number;
  readonly heightMm?: number;
  readonly price?: number;
  readonly isActive?: boolean;
}

const AXIS = /^[+-][XYZ]$/;

export function parsePose(wire: WirePose | null | undefined): ImportedModel | null {
  if (!wire?.modelUrl || !wire.backAxis || !wire.upAxis) return null;
  if (!AXIS.test(wire.backAxis) || !AXIS.test(wire.upAxis)) return null;
  const pose: AssetPose = { back: wire.backAxis as Axis, up: wire.upAxis as Axis };
  return isPose(pose) ? { modelUrl: wire.modelUrl, pose } : null;
}

function parseCase(wire: WireCase, modelId: ModelId): CaseOffer | null {
  if (typeof wire.id !== 'number' || typeof wire.price !== 'number') return null;
  let fit: CaseOffer['fit'] = null;
  const pose = parsePose(wire.fit);
  const f = wire.fit;
  if (pose && f && typeof f.heightMm === 'number' && typeof f.backWidthMm === 'number' && f.maskBase64 && f.maskCols && f.maskRows) {
    try {
      const nest: PhoneNest = {
        insetMm: typeof f.phoneInsetMm === 'number' ? f.phoneInsetMm : DEFAULT_NEST.insetMm,
        scale: typeof f.phoneScale === 'number' && f.phoneScale > 0 ? f.phoneScale : DEFAULT_NEST.scale,
      };
      fit = { ...pose, nest, sheet: { widthMm: f.backWidthMm, heightMm: f.heightMm, mask: decodeMask(f.maskBase64, f.maskCols, f.maskRows) } };
    } catch {
      fit = null;
    }
  }
  const stock = typeof wire.stockQuantity === 'number' ? wire.stockQuantity : null;
  const material = wire.material ?? '';
  const color = wire.color ?? '';
  return {
    id: wire.id as CaseId,
    modelId,
    sku: wire.sku?.trim() || null,
    label: caseLabel(material, color),
    material,
    color,
    price: vnd(wire.price),
    stock,
    fit,
    sale: !fit ? { kind: 'unavailable', why: 'not-fitted' } : stock !== null && stock <= 0 ? { kind: 'unavailable', why: 'out-of-stock' } : { kind: 'orderable' },
  };
}

export function parseSticker(wire: WireSticker): StickerProduct | null {
  if (typeof wire.id !== 'number' || !wire.imageUrl || !wire.stickerName) return null;
  if (typeof wire.widthMm !== 'number' || typeof wire.heightMm !== 'number' || typeof wire.price !== 'number') return null;
  return {
    id: wire.id as StickerId,
    name: wire.stickerName,
    imageUrl: wire.imageUrl,
    widthMm: wire.widthMm,
    heightMm: wire.heightMm,
    price: vnd(wire.price),
  };
}

export function parseCatalog(data: unknown): Catalog {
  const record = (data ?? {}) as { models?: readonly WireModel[]; stickers?: readonly WireSticker[] };
  const models: PhoneModel[] = [];
  for (const wire of record.models ?? []) {
    if (typeof wire.id !== 'number') continue;
    const id = wire.id as ModelId;
    const brand = wire.brand?.trim() ?? '';
    const name = wire.modelName?.trim() ?? '';
    models.push({
      id,
      brand,
      name,
      label: [brand, name].filter(Boolean).join(' ') || `Mẫu ${id}`,
      dimensions: wire.dimensions ?? null,
      phone: parsePose(wire.phone),
      cases: (wire.cases ?? []).map((c) => parseCase(c, id)).filter((c): c is CaseOffer => c !== null),
    });
  }
  const stickers = (record.stickers ?? []).map(parseSticker).filter((s): s is StickerProduct => s !== null);
  return { models, stickers };
}

export async function fetchCatalog(http: Http): Promise<Catalog | 'unreachable'> {
  const result = await http.get('/Catalog');
  if (!result.ok) return 'unreachable';
  return parseCatalog(result.data);
}
