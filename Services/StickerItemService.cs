using Microsoft.EntityFrameworkCore;
using StickerRobot.Data;
using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;
using StickerRobot.Models;
using StickerRobot.Services.Interfaces;

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
                Scale = s.Scale
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
            Scale = s.Scale
        };
    }

    public async Task<(StickerItemResponse? Data, string? ErrorMessage)> CreateAsync(StickerItemCreateRequest request)
    {
        if (!await _context.DesignTemplates.AnyAsync(dt => dt.Id == request.TemplateId))
        {
            return (null, $"DesignTemplate with ID {request.TemplateId} does not exist.");
        }
        if (await _context.StickerItems.AnyAsync(si => si.TemplateId == request.TemplateId))
        {
            return (null, $"StickerItem already exists for DesignTemplate ID {request.TemplateId}.");
        }
        if (request.StaffId.HasValue && !await _context.Staff.AnyAsync(st => st.Id == request.StaffId))
        {
            return (null, $"Staff with ID {request.StaffId} does not exist.");
        }

        var item = new StickerItem
        {
            TemplateId = request.TemplateId,
            StaffId = request.StaffId,
            StickerName = request.StickerName,
            PositionX = request.PositionX,
            PositionY = request.PositionY,
            Scale = request.Scale
        };

        _context.StickerItems.Add(item);
        await _context.SaveChangesAsync();

        var response = new StickerItemResponse
        {
            Id = item.Id,
            TemplateId = item.TemplateId,
            StaffId = item.StaffId,
            StickerName = item.StickerName,
            PositionX = item.PositionX,
            PositionY = item.PositionY,
            Scale = item.Scale
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
            StickerName = item.StickerName,
            PositionX = item.PositionX,
            PositionY = item.PositionY,
            Scale = item.Scale
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
