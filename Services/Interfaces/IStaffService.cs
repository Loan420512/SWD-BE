using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;

namespace StickerRobot.Services.Interfaces;

public interface IStaffService
{
    Task<IEnumerable<StaffResponse>> GetAllAsync();
    Task<StaffResponse?> GetByIdAsync(int id);
    Task<(StaffResponse? Data, string? ErrorMessage)> CreateAsync(StaffCreateRequest request);
    Task<(StaffResponse? Data, string? ErrorMessage)> UpdateAsync(int id, StaffUpdateRequest request);
    Task<bool> DeleteAsync(int id);
}
