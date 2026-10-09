using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

public class AdminUpdateRequest
{
    [MaxLength(100)]
    public string? Username { get; set; }

    [MinLength(6)]
    public string? Password { get; set; }

    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? Role { get; set; }
}
