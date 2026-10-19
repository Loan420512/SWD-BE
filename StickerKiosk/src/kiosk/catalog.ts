import type { AssetPose, BackSheet, PhoneNest } from './sheet';

declare const modelIdBrand: unique symbol;
export type ModelId = number & { readonly [modelIdBrand]: true };

declare const caseIdBrand: unique symbol;
export type CaseId = number & { readonly [caseIdBrand]: true };

declare const stickerIdBrand: unique symbol;
export type StickerId = number & { readonly [stickerIdBrand]: true };

declare const vndBrand: unique symbol;
/** Whole Vietnamese dong. */
export type Vnd = number & { readonly [vndBrand]: true };

export function vnd(amount: number): Vnd {
  if (!Number.isFinite(amount) || amount < 0) throw new Error('Price is not a whole dong amount');
  return Math.round(amount) as Vnd;
}

export function addVnd(a: Vnd, b: Vnd): Vnd {
  return (a + b) as Vnd;
}

export interface ImportedModel {
  readonly modelUrl: string;
  readonly pose: AssetPose;
}

export interface CaseFit extends ImportedModel {
  readonly sheet: BackSheet;
  readonly nest: PhoneNest;
}

export type Sale =
  | { readonly kind: 'orderable' }
  | { readonly kind: 'unavailable'; readonly why: 'out-of-stock' | 'not-fitted' };

export interface CaseOffer {
  readonly id: CaseId;
  readonly modelId: ModelId;
  readonly sku: string | null;
  readonly label: string;
  readonly material: string;
  readonly color: string;
  readonly price: Vnd;
  readonly stock: number | null;
  readonly fit: CaseFit | null;
  readonly sale: Sale;
}

export interface PhoneModel {
  readonly id: ModelId;
  readonly brand: string;
  readonly name: string;
  readonly label: string;
  readonly dimensions: string | null;
  readonly phone: ImportedModel | null;
  readonly cases: readonly CaseOffer[];
}

export interface StickerProduct {
  readonly id: StickerId;
  readonly name: string;
  readonly imageUrl: string;
  readonly widthMm: number;
  readonly heightMm: number;
  readonly price: Vnd;
}

export interface Catalog {
  readonly models: readonly PhoneModel[];
  readonly stickers: readonly StickerProduct[];
}

export function caseLabel(material: string, color: string): string {
  const parts = [material.trim(), color.trim()].filter(Boolean);
  return parts.length ? parts.join(' · ') : 'Ốp lưng';
}

export function findModel(catalog: Catalog, id: ModelId | null): PhoneModel | null {
  return catalog.models.find((model) => model.id === id) ?? null;
}

export function findCase(catalog: Catalog, id: CaseId | null): { model: PhoneModel; offer: CaseOffer } | null {
  for (const model of catalog.models) {
    const offer = model.cases.find((item) => item.id === id);
    if (offer) return { model, offer };
  }
  return null;
}

export function findSticker(catalog: Catalog, id: StickerId | null): StickerProduct | null {
  return catalog.stickers.find((sticker) => sticker.id === id) ?? null;
}

/** First case a customer can design on: orderable and fitted first, then any fitted case. */
export function preferredCase(model: PhoneModel): CaseOffer | null {
  return (
    model.cases.find((offer) => offer.sale.kind === 'orderable') ??
    model.cases.find((offer) => offer.fit !== null) ??
    model.cases[0] ??
    null
  );
}

export function modelsWithCases(catalog: Catalog): readonly PhoneModel[] {
  return catalog.models.filter((model) => model.cases.length > 0);
}

export function designableModels(catalog: Catalog): readonly PhoneModel[] {
  return catalog.models.filter((model) => model.cases.some((offer) => offer.fit !== null));
}
