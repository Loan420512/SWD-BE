using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;

namespace StickerRobot.Services.Interfaces;

public interface IPaymentService
{
    Task<IEnumerable<PaymentResponse>> GetAllAsync();
    Task<PaymentResponse?> GetByIdAsync(int id);
    Task<(PaymentResponse? Data, string? ErrorMessage)> CreateAsync(PaymentCreateRequest request);
    Task<(PaymentResponse? Data, string? ErrorMessage)> UpdateAsync(int id, PaymentUpdateRequest request);
    Task<bool> DeleteAsync(int id);
}
