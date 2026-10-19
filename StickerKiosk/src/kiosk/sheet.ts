/**
 * The case back as a millimetre sheet. Origin is the top-left corner seen looking at the back,
 * x grows to the right, y grows down. A placement is the sticker center in tenths of a millimetre.
 * footprintOnSheet must agree bit for bit with SWD-BE/Utilities/BackSheetMath.cs.
 */

export type Axis = '+X' | '-X' | '+Y' | '-Y' | '+Z' | '-Z';

export const AXES: readonly Axis[] = ['+X', '-X', '+Y', '-Y', '+Z', '-Z'];

/** Which mesh axis faces the camera as the back (+Z) and which points up (+Y). */
export interface AssetPose {
  readonly back: Axis;
  readonly up: Axis;
}

export const DEFAULT_POSE: AssetPose = { back: '+Z', up: '+Y' };

declare const tenthsBrand: unique symbol;
export type Tenths = number & { readonly [tenthsBrand]: true };

export interface BackPoint {
  readonly x: Tenths;
  readonly y: Tenths;
}

export interface OccupancyMask {
  readonly cols: number;
  readonly rows: number;
  /** Row-major from the top-left. Bit 0 of byte 0 is cell (0, 0). */
  readonly bits: Uint8Array;
}

export interface BackSheet {
  readonly widthMm: number;
  readonly heightMm: number;
  readonly mask: OccupancyMask;
}

export interface StickerSize {
  readonly widthMm: number;
  readonly heightMm: number;
}

export function backPoint(xTenths: number, yTenths: number): BackPoint {
  return { x: Math.round(xTenths) as Tenths, y: Math.round(yTenths) as Tenths };
}

export function hundredths(mm: number): number {
  return Math.round(mm * 100);
}

export function cells(mm: number): number {
  return Math.trunc((hundredths(mm) + 99) / 100);
}

export function maskBytes(cols: number, rows: number): number {
  return Math.trunc((cols * rows + 7) / 8);
}

export function isCellSet(mask: OccupancyMask, i: number, j: number): boolean {
  const bit = j * mask.cols + i;
  return (mask.bits[bit >> 3]! & (1 << (bit & 7))) !== 0;
}

export function countSet(mask: OccupancyMask): number {
  let count = 0;
  const total = mask.cols * mask.rows;
  for (let bit = 0; bit < total; bit++) if (isCellSet(mask, bit % mask.cols, Math.trunc(bit / mask.cols))) count += 1;
  return count;
}

/** Clockwise degrees, looking at the case back. Whole degrees, 0 through 359. */
export const ROTATION_STEP = 1;

const COS_10000 = [10000, 9659, 8660, 7071, 5000, 2588, 0, -2588, -5000, -7071, -8660, -9659] as const;

export function normalizeRotation(deg: number): number {
  const rounded = Math.round(deg);
  return ((rounded % 360) + 360) % 360;
}

function cos10000(deg: number): number {
  const snapped = normalizeRotation(deg);
  if (snapped % 15 === 0) {
    const k = snapped / 15;
    return k < 12 ? COS_10000[k]! : -COS_10000[k - 12]!;
  }
  const value = Math.cos((snapped * Math.PI) / 180) * 10000;
  return value >= 0 ? Math.round(value) : -Math.round(-value);
}

function sin10000(deg: number): number {
  return cos10000(90 - deg);
}

function idiv(n: number, d: number): number {
  return Math.trunc(n / d);
}

/** How far the phone sits into this case, and the scale applied after matching heights. */
export interface PhoneNest {
  readonly insetMm: number;
  readonly scale: number;
}

export const DEFAULT_NEST: PhoneNest = { insetMm: 1.5, scale: 0.97 };

const tables = new WeakMap<OccupancyMask, Int32Array>();

/** Summed-area table, (cols + 1) x (rows + 1). sat[y * (cols + 1) + x] counts set cells with i < x and j < y. */
function summedArea(mask: OccupancyMask): Int32Array {
  const cached = tables.get(mask);
  if (cached) return cached;
  const stride = mask.cols + 1;
  const sat = new Int32Array(stride * (mask.rows + 1));
  for (let j = 0; j < mask.rows; j++) {
    let row = 0;
    for (let i = 0; i < mask.cols; i++) {
      row += isCellSet(mask, i, j) ? 1 : 0;
      sat[(j + 1) * stride + (i + 1)] = sat[j * stride + (i + 1)]! + row;
    }
  }
  tables.set(mask, sat);
  return sat;
}

/** Cell ranges a sticker centered at `center` covers, or null when it leaves the sheet rectangle. */
function footprintCells(center: BackPoint, size: StickerSize, sheet: BackSheet) {
  const cx = center.x * 10;
  const cy = center.y * 10;
  const w = hundredths(size.widthMm);
  const h = hundredths(size.heightMm);
  const sheetW = hundredths(sheet.widthMm);
  const sheetH = hundredths(sheet.heightMm);
  if (2 * cx < w || 2 * cy < h || 2 * cx + w > 2 * sheetW || 2 * cy + h > 2 * sheetH) return null;
  const iMin = Math.trunc((2 * cx - w) / 200);
  const iMax = Math.trunc((2 * cx + w - 1) / 200);
  const jMin = Math.trunc((2 * cy - h) / 200);
  const jMax = Math.trunc((2 * cy + h - 1) / 200);
  if (iMax >= sheet.mask.cols || jMax >= sheet.mask.rows) return null;
  return { iMin, iMax, jMin, jMax };
}

function axisAligned(center: BackPoint, size: StickerSize, sheet: BackSheet): boolean {
  const range = footprintCells(center, size, sheet);
  if (!range) return false;
  const sat = summedArea(sheet.mask);
  const stride = sheet.mask.cols + 1;
  const x0 = range.iMin;
  const x1 = range.iMax + 1;
  const y0 = range.jMin;
  const y1 = range.jMax + 1;
  const sum = sat[y1 * stride + x1]! - sat[y0 * stride + x1]! - sat[y1 * stride + x0]! + sat[y0 * stride + x0]!;
  return sum === (x1 - x0) * (y1 - y0);
}

/** Cell-center test for a sticker rotated clockwise by `deg` (a multiple of 15, not a multiple of 90). */
function footprintRotated(center: BackPoint, size: StickerSize, sheet: BackSheet, deg: number): boolean {
  const cx = center.x * 10;
  const cy = center.y * 10;
  const w = hundredths(size.widthMm);
  const h = hundredths(size.heightMm);
  const sheetW = hundredths(sheet.widthMm);
  const sheetH = hundredths(sheet.heightMm);
  const c = cos10000(deg);
  const s = sin10000(deg);
  let minX = Infinity;
  let maxX = -Infinity;
  let minY = Infinity;
  let maxY = -Infinity;
  for (const su of [-1, 1]) {
    for (const sv of [-1, 1]) {
      const sx = idiv(su * w * c + sv * h * s, 20000);
      const sy = idiv(-su * w * s + sv * h * c, 20000);
      const x = cx + sx;
      const y = cy - sy;
      if (x < 0 || y < 0 || x > sheetW || y > sheetH) return false;
      minX = Math.min(minX, x);
      maxX = Math.max(maxX, x);
      minY = Math.min(minY, y);
      maxY = Math.max(maxY, y);
    }
  }
  const iMin = Math.max(0, idiv(minX, 100));
  const iMax = Math.min(sheet.mask.cols - 1, idiv(maxX - 1, 100));
  const jMin = Math.max(0, idiv(minY, 100));
  const jMax = Math.min(sheet.mask.rows - 1, idiv(maxY - 1, 100));
  let covered = 0;
  for (let j = jMin; j <= jMax; j++) {
    for (let i = iMin; i <= iMax; i++) {
      const dx = i * 100 + 50 - cx;
      const dy = j * 100 + 50 - cy;
      const screenY = -dy;
      const uScaled = dx * c - screenY * s;
      const vScaled = dx * s + screenY * c;
      if (Math.abs(uScaled) * 2 > w * 10000 || Math.abs(vScaled) * 2 > h * 10000) continue;
      covered += 1;
      if (!isCellSet(sheet.mask, i, j)) return false;
    }
  }
  return covered > 0;
}

/**
 * True when every 1 mm cell under the fixed-size sticker is printable case back.
 * `rotationDeg` is clockwise when looking at the back. 0, 90, 180, and 270 keep the
 * axis-aligned test. Other multiples of 15 test cell centers inside the rotated rectangle.
 */
export function footprintOnSheet(center: BackPoint, size: StickerSize, sheet: BackSheet, rotationDeg = 0): boolean {
  const deg = normalizeRotation(rotationDeg);
  if (deg % 180 === 0) return axisAligned(center, size, sheet);
  if (deg % 90 === 0) return axisAligned(center, { widthMm: size.heightMm, heightMm: size.widthMm }, sheet);
  return footprintRotated(center, size, sheet, deg);
}

/** True when a world point on the back lies inside the sticker rectangle. */
export function stickerCovers(
  worldX: number,
  worldY: number,
  center: BackPoint,
  size: StickerSize,
  sheet: Pick<BackSheet, 'widthMm' | 'heightMm'>,
  rotationDeg = 0,
): boolean {
  const at = worldToBack(worldX, worldY, sheet);
  const dx = (at.x - center.x) * 10;
  const screenY = -((at.y - center.y) * 10);
  const deg = normalizeRotation(rotationDeg);
  const c = cos10000(deg);
  const s = sin10000(deg);
  const uScaled = dx * c - screenY * s;
  const vScaled = dx * s + screenY * c;
  return Math.abs(uScaled) * 2 <= hundredths(size.widthMm) * 10000 && Math.abs(vScaled) * 2 <= hundredths(size.heightMm) * 10000;
}

/** Opens or blocks every 1 mm cell whose center lies inside the brush. Returns a new mask. */
export function paintMask(mask: OccupancyMask, center: BackPoint, radiusMm: number, open: boolean): OccupancyMask {
  const bits = new Uint8Array(mask.bits);
  const radius = Math.max(50, hundredths(radiusMm));
  const cx = center.x * 10;
  const cy = center.y * 10;
  const i0 = Math.max(0, Math.floor((cx - radius) / 100));
  const i1 = Math.min(mask.cols - 1, Math.floor((cx + radius) / 100));
  const j0 = Math.max(0, Math.floor((cy - radius) / 100));
  const j1 = Math.min(mask.rows - 1, Math.floor((cy + radius) / 100));
  const r2 = radius * radius;
  for (let j = j0; j <= j1; j++) {
    for (let i = i0; i <= i1; i++) {
      const dx = i * 100 + 50 - cx;
      const dy = j * 100 + 50 - cy;
      if (dx * dx + dy * dy > r2) continue;
      const bit = j * mask.cols + i;
      const flag = 1 << (bit & 7);
      const index = bit >> 3;
      bits[index] = open ? bits[index]! | flag : bits[index]! & ~flag;
    }
  }
  return { cols: mask.cols, rows: mask.rows, bits };
}

/** Nearest legal center to `near` (default: middle of the back). Null when it never fits. */
export function spot(size: StickerSize, sheet: BackSheet, near?: BackPoint, rotationDeg = 0): BackPoint | null {
  const rotation = normalizeRotation(rotationDeg);
  const target = near ?? backPoint(sheet.widthMm * 5, sheet.heightMm * 5);
  if (footprintOnSheet(target, size, sheet, rotation)) return target;
  const xMax = Math.floor(sheet.widthMm * 10);
  const yMax = Math.floor(sheet.heightMm * 10);
  const step = rotation % 90 === 0 ? 5 : 10;
  let best: BackPoint | null = null;
  let bestDistance = Infinity;
  for (let y = 0; y <= yMax; y += step) {
    for (let x = 0; x <= xMax; x += step) {
      const dx = x - target.x;
      const dy = y - target.y;
      const distance = dx * dx + dy * dy;
      if (distance >= bestDistance) continue;
      const candidate = backPoint(x, y);
      if (!footprintOnSheet(candidate, size, sheet, rotation)) continue;
      best = candidate;
      bestDistance = distance;
    }
  }
  return best;
}

/** Fitted world millimetres (origin at the back center, +Y up) to a stored center. */
export function worldToBack(worldX: number, worldY: number, sheet: Pick<BackSheet, 'widthMm' | 'heightMm'>): BackPoint {
  return backPoint((worldX + sheet.widthMm / 2) * 10, (sheet.heightMm / 2 - worldY) * 10);
}

export function backToWorld(at: BackPoint, sheet: Pick<BackSheet, 'widthMm' | 'heightMm'>): { x: number; y: number } {
  return { x: at.x / 10 - sheet.widthMm / 2, y: sheet.heightMm / 2 - at.y / 10 };
}

export function decodeMask(base64: string, cols: number, rows: number): OccupancyMask {
  const raw = atob(base64);
  const bits = new Uint8Array(raw.length);
  for (let k = 0; k < raw.length; k++) bits[k] = raw.charCodeAt(k);
  if (bits.length !== maskBytes(cols, rows)) throw new Error('Mask length does not match columns and rows');
  return { cols, rows, bits };
}

export function encodeMask(mask: OccupancyMask): string {
  let raw = '';
  for (const byte of mask.bits) raw += String.fromCharCode(byte);
  return btoa(raw);
}

// ── Pose ────────────────────────────────────────────────────────────────────

type Vec = readonly [number, number, number];

function axisVector(axis: Axis): Vec {
  const sign = axis[0] === '+' ? 1 : -1;
  if (axis[1] === 'X') return [sign, 0, 0];
  if (axis[1] === 'Y') return [0, sign, 0];
  return [0, 0, sign];
}

function vectorAxis(v: Vec): Axis {
  const index = v.findIndex((value) => value !== 0);
  const letter = (['X', 'Y', 'Z'] as const)[index]!;
  return `${v[index]! > 0 ? '+' : '-'}${letter}` as Axis;
}

function cross(a: Vec, b: Vec): Vec {
  return [a[1] * b[2] - a[2] * b[1], a[2] * b[0] - a[0] * b[2], a[0] * b[1] - a[1] * b[0]];
}

export function isPose(pose: AssetPose): boolean {
  return pose.back[1] !== pose.up[1];
}

/**
 * Rows of the rotation that takes mesh coordinates to the fitted frame:
 * world = R * mesh, with R rows = [right, up, back] as mesh vectors.
 */
export function poseRows(pose: AssetPose): readonly [Vec, Vec, Vec] {
  const back = axisVector(pose.back);
  const up = axisVector(pose.up);
  return [cross(up, back), up, back];
}

const QUARTER: Record<'x' | 'y' | 'z', readonly [Vec, Vec, Vec]> = {
  x: [[1, 0, 0], [0, 0, -1], [0, 1, 0]],
  y: [[0, 0, 1], [0, 1, 0], [-1, 0, 0]],
  z: [[0, -1, 0], [1, 0, 0], [0, 0, 1]],
};

/** Turns the fitted model +90 degrees about a world axis, as seen in the preview. */
export function turnPose(pose: AssetPose, about: 'x' | 'y' | 'z'): AssetPose {
  const r = poseRows(pose);
  const q = QUARTER[about];
  const row = (k: number): Vec => [
    q[k]![0] * r[0][0] + q[k]![1] * r[1][0] + q[k]![2] * r[2][0],
    q[k]![0] * r[0][1] + q[k]![1] * r[1][1] + q[k]![2] * r[2][1],
    q[k]![0] * r[0][2] + q[k]![1] * r[1][2] + q[k]![2] * r[2][2],
  ];
  return { back: vectorAxis(row(2)), up: vectorAxis(row(1)) };
}
