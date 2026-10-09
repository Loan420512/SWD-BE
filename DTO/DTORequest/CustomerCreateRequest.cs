using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

public class CustomerCreateRequest
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = null!;

    [Phone]
    [MaxLength(20)]
    public string? Phone { get; set; }

    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }
}
