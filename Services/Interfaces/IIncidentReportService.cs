using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;

namespace StickerRobot.Services.Interfaces;

public interface IIncidentReportService
{
    Task<IEnumerable<IncidentReportResponse>> GetAllAsync();
    Task<IncidentReportResponse?> GetByIdAsync(int id);
    Task<(IncidentReportResponse? Data, string? ErrorMessage)> CreateAsync(IncidentReportCreateRequest request);
    Task<(IncidentReportResponse? Data, string? ErrorMessage)> UpdateAsync(int id, IncidentReportUpdateRequest request);
    Task<bool> DeleteAsync(int id);
}
