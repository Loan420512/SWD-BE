using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;

namespace StickerRobot.Services.Interfaces;

public interface ICatalogService
{
    Task<CatalogResponse> GetCatalogAsync();
    Task<IEnumerable<StickerProductResponse>> GetStickersAsync(bool includeInactive);
    Task<(StickerProductResponse? Data, string? ErrorMessage)> CreateStickerAsync(StickerProductRequest request);
    Task<(StickerProductResponse? Data, string? ErrorMessage)> UpdateStickerAsync(int id, StickerProductRequest request);
    Task<(bool Found, string? ErrorMessage)> SetModelFitAsync(int modelId, PhoneModelFitRequest request);
    Task<(bool Found, string? ErrorMessage)> SetCaseFitAsync(int caseId, PhoneCaseFitRequest request);
}
