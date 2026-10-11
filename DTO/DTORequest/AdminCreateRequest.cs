using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

public class AdminCreateRequest
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = null!;

    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? Role { get; set; } = "admin";
}
