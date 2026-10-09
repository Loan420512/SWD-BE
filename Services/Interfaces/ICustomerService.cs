using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;

namespace StickerRobot.Services.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<CustomerResponse>> GetAllAsync();
    Task<CustomerResponse?> GetByIdAsync(int id);
    Task<CustomerResponse> CreateAsync(CustomerCreateRequest request);
    Task<CustomerResponse?> UpdateAsync(int id, CustomerUpdateRequest request);
    Task<bool> DeleteAsync(int id);
}
