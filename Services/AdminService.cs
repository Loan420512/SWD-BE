using Microsoft.EntityFrameworkCore;
using StickerRobot.Data;
using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;
using StickerRobot.Models;
using StickerRobot.Services.Interfaces;
using BCrypt.Net;

namespace StickerRobot.Services;

public class AdminService : IAdminService
{
    private readonly StickerRobotDbContext _context;

    public AdminService(StickerRobotDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AdminResponse>> GetAllAsync()
    {
        return await _context.Admins
            .AsNoTracking()
            .Select(a => new AdminResponse
            {
                Id = a.Id,
                Username = a.Username,
                Email = a.Email,
                Role = a.Role
            })
            .ToListAsync();
    }

    public async Task<AdminResponse?> GetByIdAsync(int id)
    {
        var admin = await _context.Admins.FindAsync(id);
        if (admin == null) return null;

        return new AdminResponse
        {
            Id = admin.Id,
            Username = admin.Username,
            Email = admin.Email,
            Role = admin.Role
        };
    }

    public async Task<(AdminResponse? Data, string? ErrorMessage)> CreateAsync(AdminCreateRequest request)
    {
        if (await _context.Admins.AnyAsync(a => a.Username == request.Username))
        {
            return (null, $"Username '{request.Username}' is already taken.");
        }

        var admin = new Admin
        {
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Email = request.Email,
            Role = string.IsNullOrWhiteSpace(request.Role) ? "Admin" : request.Role
        };

        _context.Admins.Add(admin);
        await _context.SaveChangesAsync();

        var response = new AdminResponse
        {
            Id = admin.Id,
            Username = admin.Username,
            Email = admin.Email,
            Role = admin.Role
        };

        return (response, null);
    }

    public async Task<(AdminResponse? Data, string? ErrorMessage)> UpdateAsync(int id, AdminUpdateRequest request)
    {
        var admin = await _context.Admins.FindAsync(id);
        if (admin == null) return (null, null);

        if (!string.IsNullOrWhiteSpace(request.Username) && request.Username != admin.Username)
        {
            if (await _context.Admins.AnyAsync(a => a.Username == request.Username))
            {
                return (null, $"Username '{request.Username}' is already taken.");
            }
            admin.Username = request.Username;
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        if (request.Email != null) admin.Email = request.Email;
        if (request.Role != null) admin.Role = request.Role;

        await _context.SaveChangesAsync();

        var response = new AdminResponse
        {
            Id = admin.Id,
            Username = admin.Username,
            Email = admin.Email,
            Role = admin.Role
        };

        return (response, null);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var admin = await _context.Admins.FindAsync(id);
        if (admin == null) return false;

        _context.Admins.Remove(admin);
        await _context.SaveChangesAsync();
        return true;
    }
}
