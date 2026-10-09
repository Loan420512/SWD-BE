using Microsoft.EntityFrameworkCore;
using StickerRobot.Data;
using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;
using StickerRobot.Models;
using StickerRobot.Services.Interfaces;

namespace StickerRobot.Services;

public class DesignTemplateService : IDesignTemplateService
{
    private readonly StickerRobotDbContext _context;

    public DesignTemplateService(StickerRobotDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DesignTemplateResponse>> GetAllAsync()
    {
        return await _context.DesignTemplates
            .AsNoTracking()
            .Select(dt => new DesignTemplateResponse
            {
                Id = dt.Id,
                CaseId = dt.CaseId,
                StaffId = dt.StaffId,
                TemplateName = dt.TemplateName,
                ImageUrl = dt.ImageUrl,
                IsActive = dt.IsActive,
                CreatedAt = dt.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<DesignTemplateResponse?> GetByIdAsync(int id)
    {
        var dt = await _context.DesignTemplates.FindAsync(id);
        if (dt == null) return null;

        return new DesignTemplateResponse
        {
            Id = dt.Id,
            CaseId = dt.CaseId,
            StaffId = dt.StaffId,
            TemplateName = dt.TemplateName,
            ImageUrl = dt.ImageUrl,
            IsActive = dt.IsActive,
            CreatedAt = dt.CreatedAt
        };
    }

    public async Task<(DesignTemplateResponse? Data, string? ErrorMessage)> CreateAsync(DesignTemplateCreateRequest request)
    {
        if (request.CaseId.HasValue && !await _context.PhoneCases.AnyAsync(pc => pc.Id == request.CaseId))
        {
            return (null, $"PhoneCase with ID {request.CaseId} does not exist.");
        }
        if (request.StaffId.HasValue && !await _context.Staff.AnyAsync(s => s.Id == request.StaffId))
        {
            return (null, $"Staff with ID {request.StaffId} does not exist.");
        }

        var template = new DesignTemplate
        {
            CaseId = request.CaseId,
            StaffId = request.StaffId,
            TemplateName = request.TemplateName,
            ImageUrl = request.ImageUrl,
            IsActive = request.IsActive ?? true,
            CreatedAt = DateTime.UtcNow
        };

        _context.DesignTemplates.Add(template);
        await _context.SaveChangesAsync();

        var response = new DesignTemplateResponse
        {
            Id = template.Id,
            CaseId = template.CaseId,
            StaffId = template.StaffId,
            TemplateName = template.TemplateName,
            ImageUrl = template.ImageUrl,
            IsActive = template.IsActive,
            CreatedAt = template.CreatedAt
        };

        return (response, null);
    }

    public async Task<(DesignTemplateResponse? Data, string? ErrorMessage)> UpdateAsync(int id, DesignTemplateUpdateRequest request)
    {
        var template = await _context.DesignTemplates.FindAsync(id);
        if (template == null) return (null, null);

        if (request.CaseId.HasValue && !await _context.PhoneCases.AnyAsync(pc => pc.Id == request.CaseId))
        {
            return (null, $"PhoneCase with ID {request.CaseId} does not exist.");
        }
        if (request.StaffId.HasValue && !await _context.Staff.AnyAsync(s => s.Id == request.StaffId))
        {
            return (null, $"Staff with ID {request.StaffId} does not exist.");
        }

        if (request.CaseId.HasValue) template.CaseId = request.CaseId;
        if (request.StaffId.HasValue) template.StaffId = request.StaffId;
        if (request.TemplateName != null) template.TemplateName = request.TemplateName;
        if (request.ImageUrl != null) template.ImageUrl = request.ImageUrl;
        if (request.IsActive.HasValue) template.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        var response = new DesignTemplateResponse
        {
            Id = template.Id,
            CaseId = template.CaseId,
            StaffId = template.StaffId,
            TemplateName = template.TemplateName,
            ImageUrl = template.ImageUrl,
            IsActive = template.IsActive,
            CreatedAt = template.CreatedAt
        };

        return (response, null);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var template = await _context.DesignTemplates.FindAsync(id);
        if (template == null) return false;

        _context.DesignTemplates.Remove(template);
        await _context.SaveChangesAsync();
        return true;
    }
}
