using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

/// <summary>
/// Yêu cầu cập nhật thông tin tài khoản quản trị viên.
/// </summary>
public class AdminUpdateRequest
{
    /// <summary>
    /// Tên đăng nhập mới của quản trị viên (nếu muốn thay đổi).
    /// </summary>
    /// <example>manager_hn</example>
    [MaxLength(100, ErrorMessage = "Tên đăng nhập tối đa 100 ký tự.")]
    public string? Username { get; set; }

    /// <summary>
    /// Mật khẩu mới (nếu muốn đổi mật khẩu, tối thiểu 6 ký tự).
    /// </summary>
    /// <example>NewManagerPass@2026</example>
    [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự.")]
    public string? Password { get; set; }

    /// <summary>
    /// Email liên hệ mới.
    /// </summary>
    /// <example>manager_hn@stickerrobot.com</example>
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
    [MaxLength(100)]
    public string? Email { get; set; }

    /// <summary>
    /// Vai trò hệ thống mới ('SuperAdmin', 'Manager', 'admin').
    /// </summary>
    /// <example>Manager</example>
    [MaxLength(30)]
    public string? Role { get; set; }
}
