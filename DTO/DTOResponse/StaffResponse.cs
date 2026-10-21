namespace StickerRobot.DTO.DTOResponse;

/// <summary>
/// Thông tin chi tiết nhân viên kỹ thuật hoặc vận hành.
/// </summary>
public class StaffResponse
{
    /// <summary>
    /// Mã định danh duy nhất của nhân viên.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// ID tài khoản quản trị viên quản lý.
    /// </summary>
    /// <example>1</example>
    public int? AdminId { get; set; }

    /// <summary>
    /// Họ và tên nhân viên.
    /// </summary>
    /// <example>Trần Thị B</example>
    public string? FullName { get; set; }

    /// <summary>
    /// Mã nhân viên nội bộ.
    /// </summary>
    /// <example>STF-2026-001</example>
    public string? EmployeeCode { get; set; }

    /// <summary>
    /// Số điện thoại liên hệ.
    /// </summary>
    /// <example>0987654321</example>
    public string? Phone { get; set; }

    /// <summary>
    /// Vị trí làm việc ('Operator', 'Support', 'Designer').
    /// </summary>
    /// <example>Operator</example>
    public string? Position { get; set; }

    /// <summary>
    /// Trạng thái đang làm việc (true) hay đã nghỉ (false).
    /// </summary>
    /// <example>true</example>
    public bool? IsActive { get; set; }
}
