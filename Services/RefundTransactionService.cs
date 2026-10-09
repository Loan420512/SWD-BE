using Microsoft.EntityFrameworkCore;
using StickerRobot.Data;
using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;
using StickerRobot.Models;
using StickerRobot.Services.Interfaces;

namespace StickerRobot.Services;

public class RefundTransactionService : IRefundTransactionService
{
    private readonly StickerRobotDbContext _context;

    public RefundTransactionService(StickerRobotDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RefundTransactionResponse>> GetAllAsync()
    {
        return await _context.RefundTransactions
            .AsNoTracking()
            .Select(r => new RefundTransactionResponse
            {
                Id = r.Id,
                AuditId = r.AuditId,
                AdminId = r.AdminId,
                PaymentId = r.PaymentId,
                RefundAmount = r.RefundAmount,
                Reason = r.Reason,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<RefundTransactionResponse?> GetByIdAsync(int id)
    {
        var r = await _context.RefundTransactions.FindAsync(id);
        if (r == null) return null;

        return new RefundTransactionResponse
        {
            Id = r.Id,
            AuditId = r.AuditId,
            AdminId = r.AdminId,
            PaymentId = r.PaymentId,
            RefundAmount = r.RefundAmount,
            Reason = r.Reason,
            Status = r.Status,
            CreatedAt = r.CreatedAt
        };
    }

    public async Task<(RefundTransactionResponse? Data, string? ErrorMessage)> CreateAsync(RefundTransactionCreateRequest request)
    {
        if (!await _context.Payments.AnyAsync(p => p.Id == request.PaymentId))
        {
            return (null, $"Payment with ID {request.PaymentId} does not exist.");
        }
        if (await _context.RefundTransactions.AnyAsync(rt => rt.PaymentId == request.PaymentId))
        {
            return (null, $"RefundTransaction already exists for Payment ID {request.PaymentId}.");
        }
        if (request.AuditId.HasValue && !await _context.CashFlowAudits.AnyAsync(a => a.Id == request.AuditId))
        {
            return (null, $"CashFlowAudit with ID {request.AuditId} does not exist.");
        }
        if (request.AdminId.HasValue && !await _context.Admins.AnyAsync(a => a.Id == request.AdminId))
        {
            return (null, $"Admin with ID {request.AdminId} does not exist.");
        }

        var refund = new RefundTransaction
        {
            AuditId = request.AuditId,
            AdminId = request.AdminId,
            PaymentId = request.PaymentId,
            RefundAmount = request.RefundAmount,
            Reason = request.Reason,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Pending" : request.Status,
            CreatedAt = DateTime.UtcNow
        };

        _context.RefundTransactions.Add(refund);
        await _context.SaveChangesAsync();

        var response = new RefundTransactionResponse
        {
            Id = refund.Id,
            AuditId = refund.AuditId,
            AdminId = refund.AdminId,
            PaymentId = refund.PaymentId,
            RefundAmount = refund.RefundAmount,
            Reason = refund.Reason,
            Status = refund.Status,
            CreatedAt = refund.CreatedAt
        };

        return (response, null);
    }

    public async Task<(RefundTransactionResponse? Data, string? ErrorMessage)> UpdateAsync(int id, RefundTransactionUpdateRequest request)
    {
        var refund = await _context.RefundTransactions.FindAsync(id);
        if (refund == null) return (null, null);

        if (request.AuditId.HasValue && !await _context.CashFlowAudits.AnyAsync(a => a.Id == request.AuditId))
        {
            return (null, $"CashFlowAudit with ID {request.AuditId} does not exist.");
        }
        if (request.AdminId.HasValue && !await _context.Admins.AnyAsync(a => a.Id == request.AdminId))
        {
            return (null, $"Admin with ID {request.AdminId} does not exist.");
        }

        if (request.AuditId.HasValue) refund.AuditId = request.AuditId;
        if (request.AdminId.HasValue) refund.AdminId = request.AdminId;
        if (request.RefundAmount.HasValue) refund.RefundAmount = request.RefundAmount;
        if (request.Reason != null) refund.Reason = request.Reason;
        if (request.Status != null) refund.Status = request.Status;

        await _context.SaveChangesAsync();

        var response = new RefundTransactionResponse
        {
            Id = refund.Id,
            AuditId = refund.AuditId,
            AdminId = refund.AdminId,
            PaymentId = refund.PaymentId,
            RefundAmount = refund.RefundAmount,
            Reason = refund.Reason,
            Status = refund.Status,
            CreatedAt = refund.CreatedAt
        };

        return (response, null);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var refund = await _context.RefundTransactions.FindAsync(id);
        if (refund == null) return false;

        _context.RefundTransactions.Remove(refund);
        await _context.SaveChangesAsync();
        return true;
    }
}
