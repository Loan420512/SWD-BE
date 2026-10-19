using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;

namespace StickerRobot.Services.Interfaces;

public interface IPhoneCaseService
{
    Task<IEnumerable<PhoneCaseResponse>> GetAllAsync();
    Task<PhoneCaseResponse?> GetByIdAsync(int id);
    Task<(PhoneCaseResponse? Data, string? ErrorMessage)> CreateAsync(PhoneCaseCreateRequest request);
    Task<(PhoneCaseResponse? Data, string? ErrorMessage)> UpdateAsync(int id, PhoneCaseUpdateRequest request);
    Task<(bool Found, string? Blocked)> DeleteAsync(int id);
}
