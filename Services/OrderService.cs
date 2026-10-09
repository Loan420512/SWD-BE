using Microsoft.EntityFrameworkCore;
using StickerRobot.Constants;
using StickerRobot.Data;
using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;
using StickerRobot.Models;
using StickerRobot.Services.Interfaces;

namespace StickerRobot.Services;

public class OrderService : IOrderService
{
    private readonly StickerRobotDbContext _context;

    public OrderService(StickerRobotDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderResponse>> GetAllAsync()
    {
        return await _context.Orders
            .AsNoTracking()
            .Select(o => new OrderResponse
            {
                Id = o.Id,
                OrderCode = o.OrderCode,
                CustomerId = o.CustomerId,
                PhoneCaseId = o.PhoneCaseId,
                DesignTemplateId = o.DesignTemplateId,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<OrderResponse?> GetByIdAsync(int id)
    {
        var o = await _context.Orders.FindAsync(id);
        if (o == null) return null;

        return new OrderResponse
        {
            Id = o.Id,
            OrderCode = o.OrderCode,
            CustomerId = o.CustomerId,
            PhoneCaseId = o.PhoneCaseId,
            DesignTemplateId = o.DesignTemplateId,
            TotalAmount = o.TotalAmount,
            Status = o.Status,
            CreatedAt = o.CreatedAt
        };
    }

    public async Task<(OrderResponse? Data, string? ErrorMessage)> CreateAsync(OrderCreateRequest request)
    {
        if (request.CustomerId.HasValue && !await _context.Customers.AnyAsync(c => c.Id == request.CustomerId))
        {
            return (null, $"Customer with ID {request.CustomerId} does not exist.");
        }
        if (request.PhoneCaseId.HasValue && !await _context.PhoneCases.AnyAsync(pc => pc.Id == request.PhoneCaseId))
        {
            return (null, $"PhoneCase with ID {request.PhoneCaseId} does not exist.");
        }
        if (request.DesignTemplateId.HasValue && !await _context.DesignTemplates.AnyAsync(dt => dt.Id == request.DesignTemplateId))
        {
            return (null, $"DesignTemplate with ID {request.DesignTemplateId} does not exist.");
        }

        var order = new Order
        {
            OrderCode = string.IsNullOrWhiteSpace(request.OrderCode) ? $"ORD-{Guid.NewGuid().ToString("N")[..8].ToUpper()}" : request.OrderCode,
            CustomerId = request.CustomerId,
            PhoneCaseId = request.PhoneCaseId,
            DesignTemplateId = request.DesignTemplateId,
            TotalAmount = request.TotalAmount,
            Status = string.IsNullOrWhiteSpace(request.Status) ? OrderStatus.Pending : request.Status,
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var response = new OrderResponse
        {
            Id = order.Id,
            OrderCode = order.OrderCode,
            CustomerId = order.CustomerId,
            PhoneCaseId = order.PhoneCaseId,
            DesignTemplateId = order.DesignTemplateId,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            CreatedAt = order.CreatedAt
        };

        return (response, null);
    }

    public async Task<(OrderResponse? Data, string? ErrorMessage)> UpdateAsync(int id, OrderUpdateRequest request)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return (null, null);

        if (request.CustomerId.HasValue && !await _context.Customers.AnyAsync(c => c.Id == request.CustomerId))
        {
            return (null, $"Customer with ID {request.CustomerId} does not exist.");
        }
        if (request.PhoneCaseId.HasValue && !await _context.PhoneCases.AnyAsync(pc => pc.Id == request.PhoneCaseId))
        {
            return (null, $"PhoneCase with ID {request.PhoneCaseId} does not exist.");
        }
        if (request.DesignTemplateId.HasValue && !await _context.DesignTemplates.AnyAsync(dt => dt.Id == request.DesignTemplateId))
        {
            return (null, $"DesignTemplate with ID {request.DesignTemplateId} does not exist.");
        }

        if (request.OrderCode != null) order.OrderCode = request.OrderCode;
        if (request.CustomerId.HasValue) order.CustomerId = request.CustomerId;
        if (request.PhoneCaseId.HasValue) order.PhoneCaseId = request.PhoneCaseId;
        if (request.DesignTemplateId.HasValue) order.DesignTemplateId = request.DesignTemplateId;
        if (request.TotalAmount.HasValue) order.TotalAmount = request.TotalAmount;
        if (request.Status != null) order.Status = request.Status;

        await _context.SaveChangesAsync();

        var response = new OrderResponse
        {
            Id = order.Id,
            OrderCode = order.OrderCode,
            CustomerId = order.CustomerId,
            PhoneCaseId = order.PhoneCaseId,
            DesignTemplateId = order.DesignTemplateId,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            CreatedAt = order.CreatedAt
        };

        return (response, null);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return false;

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return true;
    }
}
