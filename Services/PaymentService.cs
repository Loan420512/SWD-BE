using Microsoft.EntityFrameworkCore;
using StickerRobot.Constants;
using StickerRobot.Data;
using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;
using StickerRobot.Models;
using StickerRobot.Services.Interfaces;

namespace StickerRobot.Services;

public class PaymentService : IPaymentService
{
    private readonly StickerRobotDbContext _context;

    public PaymentService(StickerRobotDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PaymentResponse>> GetAllAsync()
    {
        return await _context.Payments
            .AsNoTracking()
            .Select(p => new PaymentResponse
            {
                Id = p.Id,
                OrderId = p.OrderId,
                AuditId = p.AuditId,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                PaymentStatus = p.PaymentStatus,
                TransactionId = p.TransactionId,
                PaidAt = p.PaidAt
            })
            .ToListAsync();
    }

    public async Task<PaymentResponse?> GetByIdAsync(int id)
    {
        var p = await _context.Payments.FindAsync(id);
        if (p == null) return null;

        return new PaymentResponse
        {
            Id = p.Id,
            OrderId = p.OrderId,
            AuditId = p.AuditId,
            Amount = p.Amount,
            PaymentMethod = p.PaymentMethod,
            PaymentStatus = p.PaymentStatus,
            TransactionId = p.TransactionId,
            PaidAt = p.PaidAt
        };
    }

    public async Task<(PaymentResponse? Data, string? ErrorMessage)> CreateAsync(PaymentCreateRequest request)
    {
        if (!await _context.Orders.AnyAsync(o => o.Id == request.OrderId))
        {
            return (null, $"Order with ID {request.OrderId} does not exist.");
        }
        if (await _context.Payments.AnyAsync(p => p.OrderId == request.OrderId))
        {
            return (null, $"Payment already exists for Order ID {request.OrderId}.");
        }
        if (request.AuditId.HasValue && !await _context.CashFlowAudits.AnyAsync(a => a.Id == request.AuditId))
        {
            return (null, $"CashFlowAudit with ID {request.AuditId} does not exist.");
        }

        var status = request.PaymentStatus?.Trim();
        if (string.IsNullOrWhiteSpace(status))
        {
            status = PaymentStatus.Success;
        }
        else if (status.Equals("Paid", StringComparison.OrdinalIgnoreCase))
        {
            status = PaymentStatus.Success;
        }

        var payment = new Payment
        {
            OrderId = request.OrderId,
            AuditId = request.AuditId,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            PaymentStatus = status,
            TransactionId = request.TransactionId,
            PaidAt = request.PaidAt ?? DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        var response = new PaymentResponse
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            AuditId = payment.AuditId,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            PaymentStatus = payment.PaymentStatus,
            TransactionId = payment.TransactionId,
            PaidAt = payment.PaidAt
        };

        return (response, null);
    }

    public async Task<(PaymentResponse? Data, string? ErrorMessage)> UpdateAsync(int id, PaymentUpdateRequest request)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null) return (null, null);

        if (request.AuditId.HasValue && !await _context.CashFlowAudits.AnyAsync(a => a.Id == request.AuditId))
        {
            return (null, $"CashFlowAudit with ID {request.AuditId} does not exist.");
        }

        if (request.AuditId.HasValue) payment.AuditId = request.AuditId;
        if (request.Amount.HasValue) payment.Amount = request.Amount;
        if (request.PaymentMethod != null) payment.PaymentMethod = request.PaymentMethod;
        if (request.PaymentStatus != null)
        {
            var status = request.PaymentStatus.Trim();
            if (status.Equals("Paid", StringComparison.OrdinalIgnoreCase))
            {
                status = PaymentStatus.Success;
            }
            payment.PaymentStatus = status;
        }
        if (request.TransactionId != null) payment.TransactionId = request.TransactionId;
        if (request.PaidAt.HasValue) payment.PaidAt = request.PaidAt;

        await _context.SaveChangesAsync();

        var response = new PaymentResponse
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            AuditId = payment.AuditId,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            PaymentStatus = payment.PaymentStatus,
            TransactionId = payment.TransactionId,
            PaidAt = payment.PaidAt
        };

        return (response, null);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null) return false;

        _context.Payments.Remove(payment);
        await _context.SaveChangesAsync();
        return true;
    }
}
