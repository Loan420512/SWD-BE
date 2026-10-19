namespace StickerRobot.Utilities;

/// <summary>
/// Footprint rule for a fixed-size sticker on a case back. Integer arithmetic in hundredths of a millimetre,
/// shared bit for bit with StickerKiosk/src/kiosk/sheet.ts.
/// Positions are tenths of a millimetre from the top-left of the case back, seen looking at the back.
/// </summary>
public static class BackSheetMath
{
    public const string AxisPattern = "^[+-][XYZ]$";

    public static int Hundredths(decimal mm) => (int)Math.Round(mm * 100m, MidpointRounding.AwayFromZero);

    public static int Cells(decimal mm) => (Hundredths(mm) + 99) / 100;

    public static int MaskBytes(int cols, int rows) => (cols * rows + 7) / 8;

    public static bool IsPose(string? back, string? up) =>
        back is { Length: 2 } && up is { Length: 2 }
        && (back[0] == '+' || back[0] == '-') && (up[0] == '+' || up[0] == '-')
        && "XYZ".Contains(back[1]) && "XYZ".Contains(up[1])
        && back[1] != up[1];

    /// <summary>cos(k * 15°) * 10000 for k = 0..11. The same table lives in StickerKiosk/src/kiosk/sheet.ts.</summary>
    private static readonly int[] Cos10000 =
    {
        10000, 9659, 8660, 7071, 5000, 2588, 0, -2588, -5000, -7071, -8660, -9659
    };

    public static bool IsRotation(int deg) => deg >= 0 && deg < 360;

    public static int NormalizeRotation(int deg)
    {
        var rounded = (int)Math.Round((double)deg, MidpointRounding.AwayFromZero);
        var mod = rounded % 360;
        if (mod < 0) mod += 360;
        return mod;
    }

    private static int Cos10000Of(int deg)
    {
        var snapped = NormalizeRotation(deg);
        if (snapped % 15 == 0)
        {
            var k = snapped / 15;
            return k < 12 ? Cos10000[k] : -Cos10000[k - 12];
        }
        var value = Math.Cos(snapped * Math.PI / 180.0) * 10000.0;
        return (int)Math.Round(value, MidpointRounding.AwayFromZero);
    }

    private static int Sin10000Of(int deg) => Cos10000Of(90 - deg);

    public static bool FootprintFits(
        int positionX, int positionY,
        decimal stickerWidthMm, decimal stickerHeightMm,
        decimal sheetWidthMm, decimal sheetHeightMm,
        byte[] mask, int cols, int rows,
        int rotationDeg = 0)
    {
        if (mask.Length < MaskBytes(cols, rows)) return false;
        var deg = NormalizeRotation(rotationDeg);
        if (deg % 180 == 0)
        {
            return AxisAligned(positionX, positionY, stickerWidthMm, stickerHeightMm, sheetWidthMm, sheetHeightMm, mask, cols, rows);
        }
        if (deg % 90 == 0)
        {
            return AxisAligned(positionX, positionY, stickerHeightMm, stickerWidthMm, sheetWidthMm, sheetHeightMm, mask, cols, rows);
        }
        return Rotated(positionX, positionY, stickerWidthMm, stickerHeightMm, sheetWidthMm, sheetHeightMm, mask, cols, rows, deg);
    }

    private static bool AxisAligned(
        int positionX, int positionY,
        decimal stickerWidthMm, decimal stickerHeightMm,
        decimal sheetWidthMm, decimal sheetHeightMm,
        byte[] mask, int cols, int rows)
    {
        long cx = positionX * 10L;
        long cy = positionY * 10L;
        long w = Hundredths(stickerWidthMm);
        long h = Hundredths(stickerHeightMm);
        long sheetW = Hundredths(sheetWidthMm);
        long sheetH = Hundredths(sheetHeightMm);

        if (2 * cx < w || 2 * cy < h || 2 * cx + w > 2 * sheetW || 2 * cy + h > 2 * sheetH) return false;

        long iMin = (2 * cx - w) / 200;
        long iMax = (2 * cx + w - 1) / 200;
        long jMin = (2 * cy - h) / 200;
        long jMax = (2 * cy + h - 1) / 200;
        if (iMax >= cols || jMax >= rows) return false;

        for (long j = jMin; j <= jMax; j++)
        {
            for (long i = iMin; i <= iMax; i++)
            {
                long bit = j * cols + i;
                if ((mask[bit >> 3] & (1 << (int)(bit & 7))) == 0) return false;
            }
        }
        return true;
    }

    private static bool Rotated(
        int positionX, int positionY,
        decimal stickerWidthMm, decimal stickerHeightMm,
        decimal sheetWidthMm, decimal sheetHeightMm,
        byte[] mask, int cols, int rows, int deg)
    {
        long cx = positionX * 10L;
        long cy = positionY * 10L;
        long w = Hundredths(stickerWidthMm);
        long h = Hundredths(stickerHeightMm);
        long sheetW = Hundredths(sheetWidthMm);
        long sheetH = Hundredths(sheetHeightMm);
        long c = Cos10000Of(deg);
        long s = Sin10000Of(deg);

        long minX = long.MaxValue, maxX = long.MinValue, minY = long.MaxValue, maxY = long.MinValue;
        for (var su = -1; su <= 1; su += 2)
        {
            for (var sv = -1; sv <= 1; sv += 2)
            {
                var sx = (su * w * c + sv * h * s) / 20000;
                var sy = (-su * w * s + sv * h * c) / 20000;
                var x = cx + sx;
                var y = cy - sy;
                if (x < 0 || y < 0 || x > sheetW || y > sheetH) return false;
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }
        }

        var iMin = Math.Max(0, minX / 100);
        var iMax = Math.Min(cols - 1, (maxX - 1) / 100);
        var jMin = Math.Max(0, minY / 100);
        var jMax = Math.Min(rows - 1, (maxY - 1) / 100);
        var covered = 0;
        for (var j = jMin; j <= jMax; j++)
        {
            for (var i = iMin; i <= iMax; i++)
            {
                var dx = i * 100 + 50 - cx;
                var dy = j * 100 + 50 - cy;
                var screenY = -dy;
                var uScaled = dx * c - screenY * s;
                var vScaled = dx * s + screenY * c;
                if (Math.Abs(uScaled) * 2 > w * 10000 || Math.Abs(vScaled) * 2 > h * 10000) continue;
                covered++;
                var bit = j * cols + i;
                if ((mask[bit >> 3] & (1 << (int)(bit & 7))) == 0) return false;
            }
        }
        return covered > 0;
    }
}
