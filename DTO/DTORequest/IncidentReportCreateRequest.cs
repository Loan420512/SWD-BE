using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

public class IncidentReportCreateRequest
{
    public int? StaffId { get; set; }

    [MaxLength(50)]
    public string? IncidentType { get; set; }

    public string? Description { get; set; }

    [MaxLength(30)]
    public string? Status { get; set; } = "Open";
}
