using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

public class StaffUpdateRequest
{
    public int? AdminId { get; set; }

    [MaxLength(100)]
    public string? FullName { get; set; }

    [MaxLength(50)]
    public string? EmployeeCode { get; set; }

    [Phone]
    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(30)]
    public string? Position { get; set; }

    public bool? IsActive { get; set; }
}
