using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

public class PhoneModelCreateRequest
{
    [Required]
    [MaxLength(50)]
    public string Brand { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string ModelName { get; set; } = null!;

    [MaxLength(100)]
    public string? Dimensions { get; set; }
}
