namespace StickerRobot.DTO.DTOResponse;

/// <summary>
/// Thông tin phản hồi tài khoản quản trị viên.
/// </summary>
public class AdminResponse
{
    /// <summary>
    /// Mã định danh duy nhất của quản trị viên.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// Tên đăng nhập.
    /// </summary>
    /// <example>manager</example>
    public string? Username { get; set; }

    /// <summary>
    /// Email liên hệ.
    /// </summary>
    /// <example>manager@stickerrobot.com</example>
    public string? Email { get; set; }

    /// <summary>
    /// Vai trò hệ thống ('SuperAdmin', 'Manager', 'admin').
    /// </summary>
    /// <example>Manager</example>
    public string? Role { get; set; }
}
