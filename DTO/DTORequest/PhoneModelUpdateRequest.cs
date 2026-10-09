using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

public class PhoneModelUpdateRequest
{
    [MaxLength(50)]
    public string? Brand { get; set; }

    [MaxLength(100)]
    public string? ModelName { get; set; }

    [MaxLength(100)]
    public string? Dimensions { get; set; }
}
