using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;

namespace StickerRobot.Services.Interfaces;

public interface IDesignTemplateService
{
    Task<IEnumerable<DesignTemplateResponse>> GetAllAsync();
    Task<DesignTemplateResponse?> GetByIdAsync(int id);
    Task<(DesignTemplateResponse? Data, string? ErrorMessage)> CreateAsync(DesignTemplateCreateRequest request);
    Task<(DesignTemplateResponse? Data, string? ErrorMessage)> UpdateAsync(int id, DesignTemplateUpdateRequest request);
    Task<bool> DeleteAsync(int id);
}
