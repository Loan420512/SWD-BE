using Microsoft.EntityFrameworkCore;
using StickerRobot.Data;
using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;
using StickerRobot.Models;
using StickerRobot.Services.Interfaces;

namespace StickerRobot.Services;

public class StaffService : IStaffService
{
    private readonly StickerRobotDbContext _context;

    public StaffService(StickerRobotDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StaffResponse>> GetAllAsync()
    {
        return await _context.Staff
            .AsNoTracking()
            .Select(s => new StaffResponse
            {
                Id = s.Id,
                AdminId = s.AdminId,
                FullName = s.FullName,
                EmployeeCode = s.EmployeeCode,
                Phone = s.Phone,
                Position = s.Position,
                IsActive = s.IsActive
            })
            .ToListAsync();
    }

    public async Task<StaffResponse?> GetByIdAsync(int id)
    {
        var s = await _context.Staff.FindAsync(id);
        if (s == null) return null;

        return new StaffResponse
        {
            Id = s.Id,
            AdminId = s.AdminId,
            FullName = s.FullName,
            EmployeeCode = s.EmployeeCode,
            Phone = s.Phone,
            Position = s.Position,
            IsActive = s.IsActive
        };
    }

    public async Task<(StaffResponse? Data, string? ErrorMessage)> CreateAsync(StaffCreateRequest request)
    {
        if (request.AdminId.HasValue && !await _context.Admins.AnyAsync(a => a.Id == request.AdminId))
        {
            return (null, $"Admin with ID {request.AdminId} does not exist.");
        }

        var staff = new Staff
        {
            AdminId = request.AdminId,
            FullName = request.FullName,
            EmployeeCode = request.EmployeeCode,
            Phone = request.Phone,
            Position = request.Position,
            IsActive = request.IsActive ?? true
        };

        _context.Staff.Add(staff);
        await _context.SaveChangesAsync();

        var response = new StaffResponse
        {
            Id = staff.Id,
            AdminId = staff.AdminId,
            FullName = staff.FullName,
            EmployeeCode = staff.EmployeeCode,
            Phone = staff.Phone,
            Position = staff.Position,
            IsActive = staff.IsActive
        };

        return (response, null);
    }

    public async Task<(StaffResponse? Data, string? ErrorMessage)> UpdateAsync(int id, StaffUpdateRequest request)
    {
        var staff = await _context.Staff.FindAsync(id);
        if (staff == null) return (null, null);

        if (request.AdminId.HasValue && !await _context.Admins.AnyAsync(a => a.Id == request.AdminId))
        {
            return (null, $"Admin with ID {request.AdminId} does not exist.");
        }

        if (request.AdminId.HasValue) staff.AdminId = request.AdminId;
        if (request.FullName != null) staff.FullName = request.FullName;
        if (request.EmployeeCode != null) staff.EmployeeCode = request.EmployeeCode;
        if (request.Phone != null) staff.Phone = request.Phone;
        if (request.Position != null) staff.Position = request.Position;
        if (request.IsActive.HasValue) staff.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        var response = new StaffResponse
        {
            Id = staff.Id,
            AdminId = staff.AdminId,
            FullName = staff.FullName,
            EmployeeCode = staff.EmployeeCode,
            Phone = staff.Phone,
            Position = staff.Position,
            IsActive = staff.IsActive
        };

        return (response, null);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var staff = await _context.Staff.FindAsync(id);
        if (staff == null) return false;

        _context.Staff.Remove(staff);
        await _context.SaveChangesAsync();
        return true;
    }
}
