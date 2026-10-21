using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

/// <summary>
/// Yêu cầu cập nhật thông tin nhân viên.
/// </summary>
public class StaffUpdateRequest
{
    /// <summary>
    /// ID tài khoản quản trị viên quản lý trực tiếp mới.
    /// </summary>
    /// <example>1</example>
    public int? AdminId { get; set; }

    /// <summary>
    /// Họ và tên nhân viên mới.
    /// </summary>
    /// <example>Trần Thị B</example>
    [MaxLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự.")]
    public string? FullName { get; set; }

    /// <summary>
    /// Mã số nhân viên mới.
    /// </summary>
    /// <example>STF-2026-001</example>
    [MaxLength(50)]
    public string? EmployeeCode { get; set; }

    /// <summary>
    /// Số điện thoại liên hệ mới.
    /// </summary>
    /// <example>0987654321</example>
    [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
    [MaxLength(20)]
    public string? Phone { get; set; }

    /// <summary>
    /// Vị trí làm việc mới ('Operator', 'Support', 'Designer').
    /// </summary>
    /// <example>Support</example>
    [MaxLength(30)]
    public string? Position { get; set; }

    /// <summary>
    /// Trạng thái hoạt động.
    /// </summary>
    /// <example>true</example>
    public bool? IsActive { get; set; }
}
