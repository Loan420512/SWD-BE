using Microsoft.EntityFrameworkCore;
using StickerRobot.Data;
using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;
using StickerRobot.Models;
using StickerRobot.Services.Interfaces;
using StickerRobot.Utilities;

namespace StickerRobot.Services;

public class CatalogService : ICatalogService
{
    private readonly StickerRobotDbContext _context;

    public CatalogService(StickerRobotDbContext context)
    {
        _context = context;
    }

    public async Task<CatalogResponse> GetCatalogAsync()
    {
        var models = await _context.PhoneModels.AsNoTracking().Include(m => m.PhoneCases).OrderBy(m => m.Id).ToListAsync();
        var stickers = await GetStickersAsync(includeInactive: false);

        return new CatalogResponse
        {
            Models = models.Select(m => new CatalogModelResponse
            {
                Id = m.Id,
                Brand = m.Brand,
                ModelName = m.ModelName,
                Dimensions = m.Dimensions,
                Phone = m.ModelUrl == null ? null : new PoseResponse { ModelUrl = m.ModelUrl, BackAxis = m.BackAxis!, UpAxis = m.UpAxis! },
                Cases = m.PhoneCases.OrderBy(c => c.Id).Select(ToCatalogCase).ToList()
            }).ToList(),
            Stickers = stickers.ToList()
        };
    }

    public async Task<IEnumerable<StickerProductResponse>> GetStickersAsync(bool includeInactive)
    {
        return await _context.StickerProducts
            .AsNoTracking()
            .Where(s => includeInactive || s.IsActive)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new StickerProductResponse
            {
                Id = s.Id,
                StickerName = s.StickerName,
                ImageUrl = s.ImageUrl,
                WidthMm = s.WidthMm,
                HeightMm = s.HeightMm,
                Price = s.Price,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<(StickerProductResponse? Data, string? ErrorMessage)> CreateStickerAsync(StickerProductRequest request)
    {
        if (!request.ImageUrl.StartsWith("/uploads/sticker/", StringComparison.Ordinal))
        {
            return (null, "Sticker image must be uploaded to /uploads/sticker/.");
        }

        var sticker = new StickerProduct
        {
            StickerName = request.StickerName.Trim(),
            ImageUrl = request.ImageUrl,
            WidthMm = request.WidthMm,
            HeightMm = request.HeightMm,
            Price = request.Price,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        _context.StickerProducts.Add(sticker);
        await _context.SaveChangesAsync();
        return (ToResponse(sticker), null);
    }

    public async Task<(StickerProductResponse? Data, string? ErrorMessage)> UpdateStickerAsync(int id, StickerProductRequest request)
    {
        var sticker = await _context.StickerProducts.FindAsync(id);
        if (sticker == null) return (null, null);
        if (!request.ImageUrl.StartsWith("/uploads/sticker/", StringComparison.Ordinal))
        {
            return (null, "Sticker image must be uploaded to /uploads/sticker/.");
        }

        var used = await _context.StickerItems.AnyAsync(i => i.StickerProductId == id);
        if (used && (sticker.WidthMm != request.WidthMm || sticker.HeightMm != request.HeightMm))
        {
            return (null, "This sticker is on existing orders. Create a new sticker for a different size.");
        }

        sticker.StickerName = request.StickerName.Trim();
        sticker.ImageUrl = request.ImageUrl;
        sticker.WidthMm = request.WidthMm;
        sticker.HeightMm = request.HeightMm;
        sticker.Price = request.Price;
        sticker.IsActive = request.IsActive;
        await _context.SaveChangesAsync();
        return (ToResponse(sticker), null);
    }

    public async Task<(bool Found, string? ErrorMessage)> SetModelFitAsync(int modelId, PhoneModelFitRequest request)
    {
        var model = await _context.PhoneModels.FindAsync(modelId);
        if (model == null) return (false, null);
        if (!request.ModelUrl.StartsWith("/uploads/phone/", StringComparison.Ordinal)) return (true, "Phone model must be uploaded to /uploads/phone/.");
        if (!BackSheetMath.IsPose(request.BackAxis, request.UpAxis)) return (true, "Back and up must be two different axes.");

        model.ModelUrl = request.ModelUrl;
        model.BackAxis = request.BackAxis;
        model.UpAxis = request.UpAxis;
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Found, string? ErrorMessage)> SetCaseFitAsync(int caseId, PhoneCaseFitRequest request)
    {
        var phoneCase = await _context.PhoneCases.FindAsync(caseId);
        if (phoneCase == null) return (false, null);
        if (!request.ModelUrl.StartsWith("/uploads/case/", StringComparison.Ordinal)) return (true, "Case model must be uploaded to /uploads/case/.");
        if (!BackSheetMath.IsPose(request.BackAxis, request.UpAxis)) return (true, "Back and up must be two different axes.");

        var width = Math.Round(request.BackWidthMm, 2, MidpointRounding.AwayFromZero);
        var height = Math.Round(request.HeightMm, 2, MidpointRounding.AwayFromZero);
        if (request.MaskCols != BackSheetMath.Cells(width) || request.MaskRows != BackSheetMath.Cells(height))
        {
            return (true, "Mask size does not match the case back size.");
        }

        byte[] mask;
        try
        {
            mask = Convert.FromBase64String(request.MaskBase64);
        }
        catch (FormatException)
        {
            return (true, "Mask is not valid base64.");
        }
        if (mask.Length != BackSheetMath.MaskBytes(request.MaskCols, request.MaskRows))
        {
            return (true, "Mask length does not match its columns and rows.");
        }
        if (request.PhoneInsetMm < -8m || request.PhoneInsetMm > 20m)
        {
            return (true, "Phone inset must be between -8 and 20 mm.");
        }
        if (request.PhoneScale < 0.8m || request.PhoneScale > 1.1m)
        {
            return (true, "Phone scale must be between 0.80 and 1.10.");
        }

        phoneCase.ModelUrl = request.ModelUrl;
        phoneCase.BackAxis = request.BackAxis;
        phoneCase.UpAxis = request.UpAxis;
        phoneCase.HeightMm = height;
        phoneCase.BackWidthMm = width;
        phoneCase.MaskCols = request.MaskCols;
        phoneCase.MaskRows = request.MaskRows;
        phoneCase.BackMask = mask;
        phoneCase.PhoneInsetMm = Math.Round(request.PhoneInsetMm, 2, MidpointRounding.AwayFromZero);
        phoneCase.PhoneScale = Math.Round(request.PhoneScale, 3, MidpointRounding.AwayFromZero);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    private static CatalogCaseResponse ToCatalogCase(PhoneCase c) => new()
    {
        Id = c.Id,
        Sku = c.Sku,
        Material = c.Material,
        Color = c.Color,
        Price = c.Price,
        StockQuantity = c.StockQuantity,
        Fit = c.ModelUrl == null || c.BackMask == null ? null : new CaseFitResponse
        {
            ModelUrl = c.ModelUrl,
            BackAxis = c.BackAxis!,
            UpAxis = c.UpAxis!,
            HeightMm = c.HeightMm!.Value,
            BackWidthMm = c.BackWidthMm!.Value,
            MaskCols = c.MaskCols!.Value,
            MaskRows = c.MaskRows!.Value,
            MaskBase64 = Convert.ToBase64String(c.BackMask),
            PhoneInsetMm = c.PhoneInsetMm,
            PhoneScale = c.PhoneScale
        }
    };

    private static StickerProductResponse ToResponse(StickerProduct s) => new()
    {
        Id = s.Id,
        StickerName = s.StickerName,
        ImageUrl = s.ImageUrl,
        WidthMm = s.WidthMm,
        HeightMm = s.HeightMm,
        Price = s.Price,
        IsActive = s.IsActive,
        CreatedAt = s.CreatedAt
    };
}
