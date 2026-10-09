using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;

namespace StickerRobot.Services.Interfaces;

public interface IStickerItemService
{
    Task<IEnumerable<StickerItemResponse>> GetAllAsync();
    Task<StickerItemResponse?> GetByIdAsync(int id);
    Task<(StickerItemResponse? Data, string? ErrorMessage)> CreateAsync(StickerItemCreateRequest request);
    Task<(StickerItemResponse? Data, string? ErrorMessage)> UpdateAsync(int id, StickerItemUpdateRequest request);
    Task<bool> DeleteAsync(int id);
}
