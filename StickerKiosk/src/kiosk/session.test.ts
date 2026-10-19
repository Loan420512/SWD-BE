import { describe, expect, it } from 'vitest';
import { vnd, type CaseId, type Catalog, type ModelId, type StickerId } from './catalog';
import { attemptIdOf } from './port';
import { reduce, resolve, initialSession, type Session } from './session';
import { backPoint, cells, maskBytes, type BackSheet } from './sheet';

function sheet(widthMm: number, heightMm: number, cameraUntilMm = 0): BackSheet {
  const cols = cells(widthMm);
  const rows = cells(heightMm);
  const bits = new Uint8Array(maskBytes(cols, rows));
  for (let j = 0; j < rows; j++) {
    for (let i = 0; i < cols; i++) {
      if (i < cameraUntilMm && j < cameraUntilMm) continue;
      const bit = j * cols + i;
      bits[bit >> 3]! |= 1 << (bit & 7);
    }
  }
  return { widthMm, heightMm, mask: { cols, rows, bits } };
}

const pose = { back: '+Z', up: '+Y' } as const;

const catalog: Catalog = {
  models: [
    {
      id: 1 as ModelId,
      brand: 'Apple',
      name: 'iPhone 15',
      label: 'Apple iPhone 15',
      dimensions: null,
      phone: null,
      cases: [
        { id: 10 as CaseId, modelId: 1 as ModelId, sku: 'A', label: 'Clear', material: '', color: '', price: vnd(189000), stock: 5, fit: { modelUrl: '/a.glb', pose, sheet: sheet(75, 150, 40), nest: { insetMm: 1.5, scale: 0.97 } }, sale: { kind: 'orderable' } },
        { id: 11 as CaseId, modelId: 1 as ModelId, sku: 'B', label: 'Out', material: '', color: '', price: vnd(179000), stock: 0, fit: { modelUrl: '/a.glb', pose, sheet: sheet(75, 150, 40), nest: { insetMm: 1.5, scale: 0.97 } }, sale: { kind: 'unavailable', why: 'out-of-stock' } },
      ],
    },
  ],
  stickers: [
    { id: 1 as StickerId, name: 'Sun', imageUrl: '/s.png', widthMm: 40, heightMm: 40, price: vnd(35000) },
    { id: 2 as StickerId, name: 'Huge', imageUrl: '/h.png', widthMm: 90, heightMm: 20, price: vnd(10000) },
  ],
};

function ready(): Session {
  return reduce(initialSession(), { type: 'catalog', catalog: { status: 'ready', catalog } });
}

describe('session', () => {
  it('picks the first orderable case when the catalog arrives', () => {
    expect(ready().design.caseId).toBe(10);
  });

  it('auto-spots a chosen sticker on a legal center and prices case plus sticker', () => {
    const s = reduce(ready(), { type: 'chooseSticker', id: 1 as StickerId });
    expect(s.design.placements[0]?.at).toEqual(backPoint(375, 750));
    const r = resolve(s);
    expect(r.total).toBe(224000);
    expect(r.gate.open).toBe(true);
  });

  it('moves the selected sticker one layer toward the front', () => {
    let s = reduce(ready(), { type: 'chooseSticker', id: 1 as StickerId });
    s = reduce(s, { type: 'chooseSticker', id: 1 as StickerId });
    const bottom = s.design.placements[0]!.key;
    s = reduce(s, { type: 'select', key: bottom });
    s = reduce(s, { type: 'stack', direction: 'up' });
    expect(s.design.placements[1]?.key).toBe(bottom);
    expect(reduce(s, { type: 'stack', direction: 'up' })).toBe(s);
  });

  it('ignores a commit over the camera hole and keeps the sticker where it was', () => {
    const placed = reduce(ready(), { type: 'chooseSticker', id: 1 as StickerId });
    const key = placed.design.placements[0]?.key;
    const after = key ? reduce(placed, { type: 'commit', key, at: backPoint(200, 200) }) : placed;
    expect(after).toBe(placed);
  });

  it('cannot place a sticker wider than the case back', () => {
    const s = reduce(ready(), { type: 'chooseSticker', id: 2 as StickerId });
    expect(s.design.placements).toHaveLength(0);
    expect(resolve(s).gate).toEqual({ open: false, reason: 'pick-sticker' });
  });

  it('blocks checkout on an out-of-stock case', () => {
    let s = reduce(ready(), { type: 'chooseSticker', id: 1 as StickerId });
    s = reduce(s, { type: 'chooseCase', id: 11 as CaseId });
    expect(resolve(s).gate).toEqual({ open: false, reason: 'out-of-stock' });
    expect(reduce(s, { type: 'openCheckout' })).toBe(s);
  });

  it('freezes the design once paying starts', () => {
    let s = reduce(ready(), { type: 'chooseSticker', id: 1 as StickerId });
    s = reduce(s, { type: 'openCheckout' });
    s = reduce(s, { type: 'pay', customer: { fullName: 'Lan', phone: null }, method: 'Cash', attempt: attemptIdOf('a-b-c') });
    expect(s.now.phase).toBe('paying');
    if (s.now.phase !== 'paying') return;
    expect(s.now.order.draft.total).toBe(224000);
    expect(reduce(s, { type: 'nudge', dx: 10, dy: 0 })).toBe(s);
  });
});
