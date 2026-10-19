using Microsoft.EntityFrameworkCore;
using StickerRobot.Data;
using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;
using StickerRobot.Models;
using StickerRobot.Services.Interfaces;
using StickerRobot.Utilities;

namespace StickerRobot.Services;

public class StickerItemService : IStickerItemService
{
    private readonly StickerRobotDbContext _context;

    public StickerItemService(StickerRobotDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StickerItemResponse>> GetAllAsync()
    {
        return await _context.StickerItems
            .AsNoTracking()
            .Select(s => new StickerItemResponse
            {
                Id = s.Id,
                TemplateId = s.TemplateId,
                StaffId = s.StaffId,
                StickerName = s.StickerName,
                PositionX = s.PositionX,
                PositionY = s.PositionY,
                Scale = s.Scale,
                RotationDeg = s.RotationDeg
            })
            .ToListAsync();
    }

    public async Task<StickerItemResponse?> GetByIdAsync(int id)
    {
        var s = await _context.StickerItems.FindAsync(id);
        if (s == null) return null;

        return new StickerItemResponse
        {
            Id = s.Id,
            TemplateId = s.TemplateId,
            StaffId = s.StaffId,
            StickerName = s.StickerName,
            PositionX = s.PositionX,
            PositionY = s.PositionY,
            Scale = s.Scale,
            RotationDeg = s.RotationDeg
        };
    }

    public async Task<(StickerItemResponse? Data, string? ErrorMessage)> CreateAsync(StickerItemCreateRequest request)
    {
        if (!await _context.DesignTemplates.AnyAsync(dt => dt.Id == request.TemplateId))
        {
            return (null, $"DesignTemplate with ID {request.TemplateId} does not exist.");
        }
        var duplicate = await _context.StickerItems.FirstOrDefaultAsync(si =>
            si.TemplateId == request.TemplateId
            && si.StickerProductId == request.StickerProductId
            && si.PositionX == request.PositionX
            && si.PositionY == request.PositionY
            && si.RotationDeg == request.RotationDeg);
        if (duplicate != null)
        {
            return (new StickerItemResponse
            {
                Id = duplicate.Id,
                TemplateId = duplicate.TemplateId,
                StaffId = duplicate.StaffId,
                StickerProductId = duplicate.StickerProductId,
                StickerName = duplicate.StickerName,
                PositionX = duplicate.PositionX,
                PositionY = duplicate.PositionY,
                Scale = duplicate.Scale,
                RotationDeg = duplicate.RotationDeg
            }, null);
        }
        if (request.StaffId.HasValue && !await _context.Staff.AnyAsync(st => st.Id == request.StaffId))
        {
            return (null, $"Staff with ID {request.StaffId} does not exist.");
        }

        var product = await _context.StickerProducts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == request.StickerProductId);
        if (product == null || !product.IsActive)
        {
            return (null, $"Sticker product {request.StickerProductId} is not on sale.");
        }

        var phoneCase = await _context.DesignTemplates.AsNoTracking()
            .Where(dt => dt.Id == request.TemplateId)
            .Select(dt => dt.Case)
            .FirstOrDefaultAsync();
        if (phoneCase?.BackMask == null || phoneCase.BackWidthMm == null || phoneCase.HeightMm == null)
        {
            return (null, "This case has no imported 3D model yet.");
        }

        if (!BackSheetMath.IsRotation(request.RotationDeg))
        {
            return (null, "Sticker rotation must be a multiple of 15 degrees.");
        }

        var fits = BackSheetMath.FootprintFits(
            request.PositionX!.Value, request.PositionY!.Value,
            product.WidthMm, product.HeightMm,
            phoneCase.BackWidthMm.Value, phoneCase.HeightMm.Value,
            phoneCase.BackMask, phoneCase.MaskCols!.Value, phoneCase.MaskRows!.Value,
            request.RotationDeg);
        if (!fits)
        {
            return (null, "Sticker footprint is off the case back.");
        }

        var item = new StickerItem
        {
            TemplateId = request.TemplateId,
            StaffId = request.StaffId,
            StickerProductId = product.Id,
            StickerName = product.StickerName,
            PositionX = request.PositionX,
            PositionY = request.PositionY,
            Scale = 1,
            RotationDeg = request.RotationDeg
        };

        _context.StickerItems.Add(item);
        await _context.SaveChangesAsync();

        var response = new StickerItemResponse
        {
            Id = item.Id,
            TemplateId = item.TemplateId,
            StaffId = item.StaffId,
            StickerProductId = item.StickerProductId,
            StickerName = item.StickerName,
            PositionX = item.PositionX,
            PositionY = item.PositionY,
            Scale = item.Scale,
            RotationDeg = item.RotationDeg
        };

        return (response, null);
    }

    public async Task<(StickerItemResponse? Data, string? ErrorMessage)> UpdateAsync(int id, StickerItemUpdateRequest request)
    {
        var item = await _context.StickerItems.FindAsync(id);
        if (item == null) return (null, null);

        if (request.StaffId.HasValue && !await _context.Staff.AnyAsync(st => st.Id == request.StaffId))
        {
            return (null, $"Staff with ID {request.StaffId} does not exist.");
        }

        if (request.StaffId.HasValue) item.StaffId = request.StaffId;
        if (request.StickerName != null) item.StickerName = request.StickerName;
        if (request.PositionX.HasValue) item.PositionX = request.PositionX;
        if (request.PositionY.HasValue) item.PositionY = request.PositionY;
        if (request.Scale.HasValue) item.Scale = request.Scale;

        await _context.SaveChangesAsync();

        var response = new StickerItemResponse
        {
            Id = item.Id,
            TemplateId = item.TemplateId,
            StaffId = item.StaffId,
            StickerProductId = item.StickerProductId,
            StickerName = item.StickerName,
            PositionX = item.PositionX,
            PositionY = item.PositionY,
            Scale = item.Scale,
            RotationDeg = item.RotationDeg
        };

        return (response, null);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _context.StickerItems.FindAsync(id);
        if (item == null) return false;

        _context.StickerItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }
}
