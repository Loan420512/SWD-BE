using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

/// <summary>
/// Yêu cầu thêm mới nhân viên vận hành hoặc hỗ trợ kỹ thuật kiosk.
/// </summary>
public class StaffCreateRequest
{
    /// <summary>
    /// ID tài khoản quản trị viên quản lý trực tiếp.
    /// </summary>
    /// <example>1</example>
    public int? AdminId { get; set; }

    /// <summary>
    /// Họ và tên nhân viên.
    /// </summary>
    /// <example>Trần Thị B</example>
    [Required(ErrorMessage = "Họ tên không được để trống.")]
    [MaxLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự.")]
    public string FullName { get; set; } = null!;

    /// <summary>
    /// Mã số nhân viên nội bộ (nếu có).
    /// </summary>
    /// <example>STF-2026-001</example>
    [MaxLength(50)]
    public string? EmployeeCode { get; set; }

    /// <summary>
    /// Số điện thoại liên hệ của nhân viên.
    /// </summary>
    /// <example>0987654321</example>
    [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
    [MaxLength(20)]
    public string? Phone { get; set; }

    /// <summary>
    /// Vị trí làm việc ('Operator': vận hành kiosk, 'Support': hỗ trợ khách hàng, 'Designer': thiết kế mẫu).
    /// </summary>
    /// <example>Operator</example>
    [MaxLength(30)]
    public string? Position { get; set; }

    /// <summary>
    /// Trạng thái đang làm việc (true) hoặc đã nghỉ việc (false). Mặc định là true.
    /// </summary>
    /// <example>true</example>
    public bool? IsActive { get; set; } = true;
}
