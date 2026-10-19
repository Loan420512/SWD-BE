using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

public class StickerItemCreateRequest
{
    [Required]
    public int TemplateId { get; set; }

    public int? StaffId { get; set; }

    /// <summary>Printed sticker being applied. Its size is fixed; scale is ignored.</summary>
    [Required]
    public int? StickerProductId { get; set; }

    [MaxLength(150)]
    public string? StickerName { get; set; }

    /// <summary>Sticker center, tenths of a millimetre from the left edge of the case back.</summary>
    [Required]
    public int? PositionX { get; set; }

    /// <summary>Sticker center, tenths of a millimetre from the top edge of the case back.</summary>
    [Required]
    public int? PositionY { get; set; }

    public double? Scale { get; set; }

    /// <summary>Clockwise degrees looking at the case back. Multiples of 15 only.</summary>
    [Range(0, 359)]
    public int RotationDeg { get; set; }
}
