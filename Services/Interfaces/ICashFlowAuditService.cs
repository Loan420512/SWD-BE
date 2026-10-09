using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;

namespace StickerRobot.Services.Interfaces;

public interface ICashFlowAuditService
{
    Task<IEnumerable<CashFlowAuditResponse>> GetAllAsync();
    Task<CashFlowAuditResponse?> GetByIdAsync(int id);
    Task<(CashFlowAuditResponse? Data, string? ErrorMessage)> CreateAsync(CashFlowAuditCreateRequest request);
    Task<(CashFlowAuditResponse? Data, string? ErrorMessage)> UpdateAsync(int id, CashFlowAuditUpdateRequest request);
    Task<bool> DeleteAsync(int id);
}
