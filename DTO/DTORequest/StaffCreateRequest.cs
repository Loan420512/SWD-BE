using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

public class StaffCreateRequest
{
    public int? AdminId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = null!;

    [MaxLength(50)]
    public string? EmployeeCode { get; set; }

    [Phone]
    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(30)]
    public string? Position { get; set; }

    public bool? IsActive { get; set; } = true;
}
