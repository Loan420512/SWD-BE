using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

public class DesignTemplateCreateRequest
{
    public int? CaseId { get; set; }
    public int? StaffId { get; set; }

    [Required]
    [MaxLength(150)]
    public string TemplateName { get; set; } = null!;

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public bool? IsActive { get; set; } = true;
}
