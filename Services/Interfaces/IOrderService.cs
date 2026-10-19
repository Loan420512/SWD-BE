using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;

namespace StickerRobot.Services.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderResponse>> GetAllAsync();
    Task<OrderResponse?> GetByIdAsync(int id);
    Task<(OrderResponse? Data, string? ErrorMessage)> CreateAsync(OrderCreateRequest request);
    Task<(OrderResponse? Data, string? ErrorMessage)> UpdateAsync(int id, OrderUpdateRequest request);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<AdminOrderResponse>> GetAdminListAsync();
    Task<PublicOrderResponse?> GetByCodeAsync(string orderCode);
    Task<(bool Found, string? ErrorMessage)> SetStatusAsync(int id, string status);
}
