using Microsoft.EntityFrameworkCore;
using StickerRobot.Data;
using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;
using StickerRobot.Models;
using StickerRobot.Services.Interfaces;

namespace StickerRobot.Services;

public class PhoneModelService : IPhoneModelService
{
    private readonly StickerRobotDbContext _context;

    public PhoneModelService(StickerRobotDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PhoneModelResponse>> GetAllAsync()
    {
        return await _context.PhoneModels
            .AsNoTracking()
            .Select(pm => new PhoneModelResponse
            {
                Id = pm.Id,
                Brand = pm.Brand,
                ModelName = pm.ModelName,
                Dimensions = pm.Dimensions
            })
            .ToListAsync();
    }

    public async Task<PhoneModelResponse?> GetByIdAsync(int id)
    {
        var pm = await _context.PhoneModels.FindAsync(id);
        if (pm == null) return null;

        return new PhoneModelResponse
        {
            Id = pm.Id,
            Brand = pm.Brand,
            ModelName = pm.ModelName,
            Dimensions = pm.Dimensions
        };
    }

    public async Task<PhoneModelResponse> CreateAsync(PhoneModelCreateRequest request)
    {
        var model = new PhoneModel
        {
            Brand = request.Brand,
            ModelName = request.ModelName,
            Dimensions = request.Dimensions
        };

        _context.PhoneModels.Add(model);
        await _context.SaveChangesAsync();

        return new PhoneModelResponse
        {
            Id = model.Id,
            Brand = model.Brand,
            ModelName = model.ModelName,
            Dimensions = model.Dimensions
        };
    }

    public async Task<PhoneModelResponse?> UpdateAsync(int id, PhoneModelUpdateRequest request)
    {
        var model = await _context.PhoneModels.FindAsync(id);
        if (model == null) return null;

        if (request.Brand != null) model.Brand = request.Brand;
        if (request.ModelName != null) model.ModelName = request.ModelName;
        if (request.Dimensions != null) model.Dimensions = request.Dimensions;

        await _context.SaveChangesAsync();

        return new PhoneModelResponse
        {
            Id = model.Id,
            Brand = model.Brand,
            ModelName = model.ModelName,
            Dimensions = model.Dimensions
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var model = await _context.PhoneModels.FindAsync(id);
        if (model == null) return false;

        _context.PhoneModels.Remove(model);
        await _context.SaveChangesAsync();
        return true;
    }
}
