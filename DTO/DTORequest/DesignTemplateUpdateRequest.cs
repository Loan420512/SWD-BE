using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

public class DesignTemplateUpdateRequest
{
    public int? CaseId { get; set; }
    public int? StaffId { get; set; }

    [MaxLength(150)]
    public string? TemplateName { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public bool? IsActive { get; set; }
}
