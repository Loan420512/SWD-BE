import { describe, expect, it } from 'vitest';
import {
  backPoint,
  backToWorld,
  cells,
  footprintOnSheet,
  maskBytes,
  normalizeRotation,
  paintMask,
  spot,
  turnPose,
  worldToBack,
  type AssetPose,
  type BackSheet,
  type OccupancyMask,
} from './sheet';

function filledMask(widthMm: number, heightMm: number, clear: (i: number, j: number) => boolean = () => false): OccupancyMask {
  const cols = cells(widthMm);
  const rows = cells(heightMm);
  const bits = new Uint8Array(maskBytes(cols, rows));
  for (let j = 0; j < rows; j++) {
    for (let i = 0; i < cols; i++) {
      const whole = (i + 1) * 100 <= Math.round(widthMm * 100) && (j + 1) * 100 <= Math.round(heightMm * 100);
      if (!whole || clear(i, j)) continue;
      const bit = j * cols + i;
      bits[bit >> 3]! |= 1 << (bit & 7);
    }
  }
  return { cols, rows, bits };
}

function sheet(widthMm: number, heightMm: number, clear?: (i: number, j: number) => boolean): BackSheet {
  return { widthMm, heightMm, mask: filledMask(widthMm, heightMm, clear) };
}

describe('footprintOnSheet golden vectors (shared with BackSheetMath.cs)', () => {
  it('accepts a 55 mm sticker inside a 78.40 x 160 back', () => {
    expect(footprintOnSheet(backPoint(494, 850), { widthMm: 55, heightMm: 55 }, sheet(78.4, 160))).toBe(true);
  });

  it('rejects a center too close to the left edge', () => {
    expect(footprintOnSheet(backPoint(100, 850), { widthMm: 55, heightMm: 55 }, sheet(78.4, 160))).toBe(false);
  });

  it('rejects a footprint over a cleared cell', () => {
    const holed = sheet(80, 160, (i, j) => i === 40 && j === 20);
    expect(footprintOnSheet(backPoint(400, 200), { widthMm: 10, heightMm: 10 }, holed)).toBe(false);
    expect(footprintOnSheet(backPoint(400, 800), { widthMm: 10, heightMm: 10 }, holed)).toBe(true);
  });

  it('treats the partial last column as not printable', () => {
    const s = sheet(78.4, 160);
    expect(footprintOnSheet(backPoint(784 - 50, 800), { widthMm: 10, heightMm: 10 }, s)).toBe(false);
    expect(footprintOnSheet(backPoint(780 - 50, 800), { widthMm: 10, heightMm: 10 }, s)).toBe(true);
  });
});

describe('rotated footprints', () => {
  it('keeps 0° on the axis-aligned rule and accepts 90° when the swapped box fits', () => {
    const wide = { widthMm: 70, heightMm: 20 };
    const s = sheet(80, 160);
    expect(footprintOnSheet(backPoint(100, 800), wide, s, 0)).toBe(false);
    expect(footprintOnSheet(backPoint(100, 800), wide, s, 90)).toBe(true);
  });

  it('rejects a 15° sticker whose center sits on a cleared cell', () => {
    const holed = sheet(80, 160, (i, j) => i === 40 && j === 80);
    expect(footprintOnSheet(backPoint(400, 800), { widthMm: 20, heightMm: 20 }, holed, 15)).toBe(false);
    expect(footprintOnSheet(backPoint(400, 200), { widthMm: 20, heightMm: 20 }, holed, 15)).toBe(true);
  });

  it('normalizes negative degrees onto the 15° circle', () => {
    expect(normalizeRotation(-15)).toBe(345);
    expect(normalizeRotation(360)).toBe(0);
  });
});

describe('paintMask', () => {
  it('opens a cleared cell and blocks a printed one', () => {
    const s = sheet(40, 40, (i, j) => i === 10 && j === 10);
    const opened = paintMask(s.mask, backPoint(105, 105), 1.2, true);
    const closed = paintMask(opened, backPoint(200, 200), 1.2, false);
    expect(footprintOnSheet(backPoint(105, 105), { widthMm: 4, heightMm: 4 }, { ...s, mask: opened })).toBe(true);
    expect(footprintOnSheet(backPoint(200, 200), { widthMm: 4, heightMm: 4 }, { ...s, mask: closed })).toBe(false);
  });
});

describe('spot', () => {
  it('returns the middle when it fits', () => {
    expect(spot({ widthMm: 40, heightMm: 40 }, sheet(80, 160))).toEqual(backPoint(400, 800));
  });

  it('moves off a camera hole to the nearest legal center', () => {
    const camera = sheet(80, 160, (i, j) => i < 45 && j < 45);
    const at = spot({ widthMm: 30, heightMm: 30 }, camera, backPoint(200, 200));
    expect(at).not.toBeNull();
    expect(footprintOnSheet(at!, { widthMm: 30, heightMm: 30 }, camera)).toBe(true);
  });

  it('returns null when the sticker is larger than the back', () => {
    expect(spot({ widthMm: 90, heightMm: 20 }, sheet(80, 160))).toBeNull();
  });
});

describe('world and back coordinates', () => {
  it('round-trips the worked example', () => {
    const s = { widthMm: 78.4, heightMm: 160 };
    const at = worldToBack(10.2, -5, s);
    expect(at).toEqual(backPoint(494, 850));
    const back = backToWorld(at, s);
    expect(back.x).toBeCloseTo(10.2);
    expect(back.y).toBeCloseTo(-5);
  });
});

describe('turnPose', () => {
  it('cycles back to the start after four quarter turns', () => {
    const start = { back: '-X', up: '+Y' } as const;
    for (const about of ['x', 'y', 'z'] as const) {
      let current: AssetPose = start;
      for (let k = 0; k < 4; k++) current = turnPose(current, about);
      expect(current).toEqual(start);
    }
  });

  it('turning about Y brings the side to the back', () => {
    expect(turnPose({ back: '+Z', up: '+Y' }, 'y')).toEqual({ back: '-X', up: '+Y' });
    expect(turnPose({ back: '+Z', up: '+Y' }, 'x')).toEqual({ back: '+Y', up: '-Z' });
  });
});
