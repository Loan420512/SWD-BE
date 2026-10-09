using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;

namespace StickerRobot.Services.Interfaces;

public interface IAdminService
{
    Task<IEnumerable<AdminResponse>> GetAllAsync();
    Task<AdminResponse?> GetByIdAsync(int id);
    Task<(AdminResponse? Data, string? ErrorMessage)> CreateAsync(AdminCreateRequest request);
    Task<(AdminResponse? Data, string? ErrorMessage)> UpdateAsync(int id, AdminUpdateRequest request);
    Task<bool> DeleteAsync(int id);
}
