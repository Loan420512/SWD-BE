using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;

namespace StickerRobot.Services.Interfaces;

public interface IPhoneModelService
{
    Task<IEnumerable<PhoneModelResponse>> GetAllAsync();
    Task<PhoneModelResponse?> GetByIdAsync(int id);
    Task<PhoneModelResponse> CreateAsync(PhoneModelCreateRequest request);
    Task<PhoneModelResponse?> UpdateAsync(int id, PhoneModelUpdateRequest request);
    Task<(bool Found, string? Blocked)> DeleteAsync(int id);
}
