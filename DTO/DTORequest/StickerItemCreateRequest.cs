using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

public class StickerItemCreateRequest
{
    [Required]
    public int TemplateId { get; set; }

    public int? StaffId { get; set; }

    [MaxLength(150)]
    public string? StickerName { get; set; }

    public int? PositionX { get; set; }
    public int? PositionY { get; set; }
    public double? Scale { get; set; }
}
