using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;

namespace StickerRobot.Services.Interfaces;

public interface IRefundTransactionService
{
    Task<IEnumerable<RefundTransactionResponse>> GetAllAsync();
    Task<RefundTransactionResponse?> GetByIdAsync(int id);
    Task<(RefundTransactionResponse? Data, string? ErrorMessage)> CreateAsync(RefundTransactionCreateRequest request);
    Task<(RefundTransactionResponse? Data, string? ErrorMessage)> UpdateAsync(int id, RefundTransactionUpdateRequest request);
    Task<bool> DeleteAsync(int id);
}
