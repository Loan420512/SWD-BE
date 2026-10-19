using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StickerRobot.Configurations;
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
    private readonly OrderSettings _orderSettings;

    public OrderService(StickerRobotDbContext context, IOptions<OrderSettings> orderSettings)
    {
        _context = context;
        _orderSettings = orderSettings.Value;
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

        var placed = request.DesignTemplateId.HasValue
            ? await _context.StickerItems.AsNoTracking()
                .Where(si => si.TemplateId == request.DesignTemplateId && si.StickerProductId != null)
                .Select(si => new { si.StickerProduct!.Price, CaseId = si.Template.CaseId, CasePrice = si.Template.Case!.Price })
                .ToListAsync()
            : null;
        if (placed is { Count: > 0 })
        {
            if (placed[0].CaseId != request.PhoneCaseId)
            {
                return (null, "Order case does not match the design case.");
            }
            var expected = (placed[0].CasePrice ?? 0) + placed.Sum(item => item.Price);
            if (expected != request.TotalAmount)
            {
                return (null, "Total does not match case plus stickers.");
            }
        }

        var order = new Order
        {
            OrderCode = string.IsNullOrWhiteSpace(request.OrderCode) ? $"{_orderSettings.OrderCodePrefix}{Guid.NewGuid().ToString("N")[..8].ToUpper()}" : request.OrderCode,
            CustomerId = request.CustomerId,
            PhoneCaseId = request.PhoneCaseId,
            DesignTemplateId = request.DesignTemplateId,
            TotalAmount = request.TotalAmount,
            Status = OrderStatus.Pending,
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

    public async Task<IEnumerable<AdminOrderResponse>> GetAdminListAsync()
    {
        return await _context.Orders.AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new AdminOrderResponse
            {
                Id = o.Id,
                OrderCode = o.OrderCode!,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                ModelLabel = o.PhoneCase!.Model!.Brand + " " + o.PhoneCase.Model.ModelName,
                CaseLabel = o.PhoneCase.Material + " · " + o.PhoneCase.Color,
                CaseSku = o.PhoneCase.Sku,
                StickerName = o.DesignTemplate!.StickerItems.OrderBy(s => s.Id).Select(s => s.StickerName).FirstOrDefault(),
                StickerImageUrl = o.DesignTemplate.StickerItems.OrderBy(s => s.Id).Select(s => s.StickerProduct!.ImageUrl).FirstOrDefault(),
                PositionX = o.DesignTemplate.StickerItems.OrderBy(s => s.Id).Select(s => s.PositionX).FirstOrDefault(),
                PositionY = o.DesignTemplate.StickerItems.OrderBy(s => s.Id).Select(s => s.PositionY).FirstOrDefault(),
                RotationDeg = o.DesignTemplate.StickerItems.OrderBy(s => s.Id).Select(s => s.RotationDeg).FirstOrDefault(),
                Placements = o.DesignTemplate.StickerItems.OrderBy(s => s.Id).Select(s => new OrderPlacementResponse
                {
                    Name = s.StickerName,
                    ImageUrl = s.StickerProduct!.ImageUrl,
                    PositionX = s.PositionX,
                    PositionY = s.PositionY,
                    RotationDeg = s.RotationDeg
                }).ToList(),
                PaymentMethod = o.Payment!.PaymentMethod,
                PaymentStatus = o.Payment.PaymentStatus,
                CustomerName = o.Customer!.FullName,
                CustomerPhone = o.Customer.Phone,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<PublicOrderResponse?> GetByCodeAsync(string orderCode)
    {
        return await _context.Orders.AsNoTracking()
            .Where(o => o.OrderCode == orderCode)
            .Select(o => new PublicOrderResponse
            {
                Id = o.Id,
                OrderCode = o.OrderCode!,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                ModelLabel = o.PhoneCase!.Model!.Brand + " " + o.PhoneCase.Model.ModelName,
                CaseLabel = o.PhoneCase.Material + " · " + o.PhoneCase.Color,
                CaseSku = o.PhoneCase.Sku,
                StickerName = o.DesignTemplate!.StickerItems.OrderBy(s => s.Id).Select(s => s.StickerName).FirstOrDefault(),
                StickerImageUrl = o.DesignTemplate.StickerItems.OrderBy(s => s.Id).Select(s => s.StickerProduct!.ImageUrl).FirstOrDefault(),
                PositionX = o.DesignTemplate.StickerItems.OrderBy(s => s.Id).Select(s => s.PositionX).FirstOrDefault(),
                PositionY = o.DesignTemplate.StickerItems.OrderBy(s => s.Id).Select(s => s.PositionY).FirstOrDefault(),
                RotationDeg = o.DesignTemplate.StickerItems.OrderBy(s => s.Id).Select(s => s.RotationDeg).FirstOrDefault(),
                Placements = o.DesignTemplate.StickerItems.OrderBy(s => s.Id).Select(s => new OrderPlacementResponse
                {
                    Name = s.StickerName,
                    ImageUrl = s.StickerProduct!.ImageUrl,
                    PositionX = s.PositionX,
                    PositionY = s.PositionY,
                    RotationDeg = s.RotationDeg
                }).ToList(),
                PaymentMethod = o.Payment!.PaymentMethod,
                CreatedAt = o.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    private static readonly Dictionary<string, string[]> NextStatus = new()
    {
        [OrderStatus.Pending] = new[] { OrderStatus.Processing, OrderStatus.Cancelled },
        [OrderStatus.Processing] = new[] { OrderStatus.Completed, OrderStatus.Cancelled },
    };

    public async Task<(bool Found, string? ErrorMessage)> SetStatusAsync(int id, string status)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return (false, null);
        var current = order.Status ?? OrderStatus.Pending;
        if (!NextStatus.TryGetValue(current, out var allowed) || !allowed.Contains(status))
        {
            return (true, $"Cannot move an order from {current} to {status}.");
        }
        order.Status = status;
        await _context.SaveChangesAsync();
        return (true, null);
    }
}
