using Microsoft.EntityFrameworkCore;
using StickerRobot.Data;
using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;
using StickerRobot.Models;
using StickerRobot.Services.Interfaces;

namespace StickerRobot.Services;

public class CustomerService : ICustomerService
{
    private readonly StickerRobotDbContext _context;

    public CustomerService(StickerRobotDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CustomerResponse>> GetAllAsync()
    {
        return await _context.Customers
            .AsNoTracking()
            .Select(c => new CustomerResponse
            {
                Id = c.Id,
                FullName = c.FullName,
                Phone = c.Phone,
                Email = c.Email,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<CustomerResponse?> GetByIdAsync(int id)
    {
        var c = await _context.Customers.FindAsync(id);
        if (c == null) return null;

        return new CustomerResponse
        {
            Id = c.Id,
            FullName = c.FullName,
            Phone = c.Phone,
            Email = c.Email,
            CreatedAt = c.CreatedAt
        };
    }

    public async Task<CustomerResponse> CreateAsync(CustomerCreateRequest request)
    {
        var customer = new Customer
        {
            FullName = request.FullName,
            Phone = request.Phone,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return new CustomerResponse
        {
            Id = customer.Id,
            FullName = customer.FullName,
            Phone = customer.Phone,
            Email = customer.Email,
            CreatedAt = customer.CreatedAt
        };
    }

    public async Task<CustomerResponse?> UpdateAsync(int id, CustomerUpdateRequest request)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return null;

        if (request.FullName != null) customer.FullName = request.FullName;
        if (request.Phone != null) customer.Phone = request.Phone;
        if (request.Email != null) customer.Email = request.Email;

        await _context.SaveChangesAsync();

        return new CustomerResponse
        {
            Id = customer.Id,
            FullName = customer.FullName,
            Phone = customer.Phone,
            Email = customer.Email,
            CreatedAt = customer.CreatedAt
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return false;

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        return true;
    }
}
