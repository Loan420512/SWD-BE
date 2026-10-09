using Microsoft.EntityFrameworkCore;
using StickerRobot.Data;
using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;
using StickerRobot.Models;
using StickerRobot.Services.Interfaces;

namespace StickerRobot.Services;

public class CashFlowAuditService : ICashFlowAuditService
{
    private readonly StickerRobotDbContext _context;

    public CashFlowAuditService(StickerRobotDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CashFlowAuditResponse>> GetAllAsync()
    {
        return await _context.CashFlowAudits
            .AsNoTracking()
            .Select(a => new CashFlowAuditResponse
            {
                Id = a.Id,
                AdminId = a.AdminId,
                AuditDate = a.AuditDate,
                TotalRevenue = a.TotalRevenue,
                TotalRefund = a.TotalRefund,
                Status = a.Status
            })
            .ToListAsync();
    }

    public async Task<CashFlowAuditResponse?> GetByIdAsync(int id)
    {
        var a = await _context.CashFlowAudits.FindAsync(id);
        if (a == null) return null;

        return new CashFlowAuditResponse
        {
            Id = a.Id,
            AdminId = a.AdminId,
            AuditDate = a.AuditDate,
            TotalRevenue = a.TotalRevenue,
            TotalRefund = a.TotalRefund,
            Status = a.Status
        };
    }

    public async Task<(CashFlowAuditResponse? Data, string? ErrorMessage)> CreateAsync(CashFlowAuditCreateRequest request)
    {
        if (request.AdminId.HasValue && !await _context.Admins.AnyAsync(adm => adm.Id == request.AdminId))
        {
            return (null, $"Admin with ID {request.AdminId} does not exist.");
        }

        var audit = new CashFlowAudit
        {
            AdminId = request.AdminId,
            AuditDate = request.AuditDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            TotalRevenue = request.TotalRevenue,
            TotalRefund = request.TotalRefund,
            Status = request.Status
        };

        _context.CashFlowAudits.Add(audit);
        await _context.SaveChangesAsync();

        var response = new CashFlowAuditResponse
        {
            Id = audit.Id,
            AdminId = audit.AdminId,
            AuditDate = audit.AuditDate,
            TotalRevenue = audit.TotalRevenue,
            TotalRefund = audit.TotalRefund,
            Status = audit.Status
        };

        return (response, null);
    }

    public async Task<(CashFlowAuditResponse? Data, string? ErrorMessage)> UpdateAsync(int id, CashFlowAuditUpdateRequest request)
    {
        var audit = await _context.CashFlowAudits.FindAsync(id);
        if (audit == null) return (null, null);

        if (request.AdminId.HasValue && !await _context.Admins.AnyAsync(adm => adm.Id == request.AdminId))
        {
            return (null, $"Admin with ID {request.AdminId} does not exist.");
        }

        if (request.AdminId.HasValue) audit.AdminId = request.AdminId;
        if (request.AuditDate.HasValue) audit.AuditDate = request.AuditDate;
        if (request.TotalRevenue.HasValue) audit.TotalRevenue = request.TotalRevenue;
        if (request.TotalRefund.HasValue) audit.TotalRefund = request.TotalRefund;
        if (request.Status != null) audit.Status = request.Status;

        await _context.SaveChangesAsync();

        var response = new CashFlowAuditResponse
        {
            Id = audit.Id,
            AdminId = audit.AdminId,
            AuditDate = audit.AuditDate,
            TotalRevenue = audit.TotalRevenue,
            TotalRefund = audit.TotalRefund,
            Status = audit.Status
        };

        return (response, null);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var audit = await _context.CashFlowAudits.FindAsync(id);
        if (audit == null) return false;

        _context.CashFlowAudits.Remove(audit);
        await _context.SaveChangesAsync();
        return true;
    }
}
