using Microsoft.EntityFrameworkCore;
using StickerRobot.Data;
using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;
using StickerRobot.Models;
using StickerRobot.Services.Interfaces;

namespace StickerRobot.Services;

public class PhoneCaseService : IPhoneCaseService
{
    private readonly StickerRobotDbContext _context;

    public PhoneCaseService(StickerRobotDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PhoneCaseResponse>> GetAllAsync()
    {
        return await _context.PhoneCases
            .AsNoTracking()
            .Select(pc => new PhoneCaseResponse
            {
                Id = pc.Id,
                ModelId = pc.ModelId,
                Sku = pc.Sku,
                Material = pc.Material,
                Color = pc.Color,
                Price = pc.Price,
                StockQuantity = pc.StockQuantity
            })
            .ToListAsync();
    }

    public async Task<PhoneCaseResponse?> GetByIdAsync(int id)
    {
        var pc = await _context.PhoneCases.FindAsync(id);
        if (pc == null) return null;

        return new PhoneCaseResponse
        {
            Id = pc.Id,
            ModelId = pc.ModelId,
            Sku = pc.Sku,
            Material = pc.Material,
            Color = pc.Color,
            Price = pc.Price,
            StockQuantity = pc.StockQuantity
        };
    }

    public async Task<(PhoneCaseResponse? Data, string? ErrorMessage)> CreateAsync(PhoneCaseCreateRequest request)
    {
        if (request.ModelId.HasValue && !await _context.PhoneModels.AnyAsync(m => m.Id == request.ModelId))
        {
            return (null, $"PhoneModel with ID {request.ModelId} does not exist.");
        }

        var phoneCase = new PhoneCase
        {
            ModelId = request.ModelId,
            Sku = request.Sku,
            Material = request.Material,
            Color = request.Color,
            Price = request.Price,
            StockQuantity = request.StockQuantity
        };

        _context.PhoneCases.Add(phoneCase);
        await _context.SaveChangesAsync();

        var response = new PhoneCaseResponse
        {
            Id = phoneCase.Id,
            ModelId = phoneCase.ModelId,
            Sku = phoneCase.Sku,
            Material = phoneCase.Material,
            Color = phoneCase.Color,
            Price = phoneCase.Price,
            StockQuantity = phoneCase.StockQuantity
        };

        return (response, null);
    }

    public async Task<(PhoneCaseResponse? Data, string? ErrorMessage)> UpdateAsync(int id, PhoneCaseUpdateRequest request)
    {
        var phoneCase = await _context.PhoneCases.FindAsync(id);
        if (phoneCase == null) return (null, null);

        if (request.ModelId.HasValue && !await _context.PhoneModels.AnyAsync(m => m.Id == request.ModelId))
        {
            return (null, $"PhoneModel with ID {request.ModelId} does not exist.");
        }

        if (request.ModelId.HasValue) phoneCase.ModelId = request.ModelId;
        if (request.Sku != null) phoneCase.Sku = request.Sku;
        if (request.Material != null) phoneCase.Material = request.Material;
        if (request.Color != null) phoneCase.Color = request.Color;
        if (request.Price.HasValue) phoneCase.Price = request.Price;
        if (request.StockQuantity.HasValue) phoneCase.StockQuantity = request.StockQuantity;

        await _context.SaveChangesAsync();

        var response = new PhoneCaseResponse
        {
            Id = phoneCase.Id,
            ModelId = phoneCase.ModelId,
            Sku = phoneCase.Sku,
            Material = phoneCase.Material,
            Color = phoneCase.Color,
            Price = phoneCase.Price,
            StockQuantity = phoneCase.StockQuantity
        };

        return (response, null);
    }

    public async Task<(bool Found, string? Blocked)> DeleteAsync(int id)
    {
        var phoneCase = await _context.PhoneCases.FindAsync(id);
        if (phoneCase == null) return (false, null);

        var inOrder = await _context.Orders.AnyAsync(order => order.PhoneCaseId == id);
        var inTemplate = await _context.DesignTemplates.AnyAsync(template => template.CaseId == id);
        if (inOrder || inTemplate)
        {
            return (true, "Ốp này đang nằm trong đơn hàng hoặc bản thiết kế, nên không xoá được.");
        }

        _context.PhoneCases.Remove(phoneCase);
        await _context.SaveChangesAsync();
        return (true, null);
    }
}
