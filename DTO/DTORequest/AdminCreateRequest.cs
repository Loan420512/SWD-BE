using System.ComponentModel.DataAnnotations;
using StickerRobot.Constants;

namespace StickerRobot.DTO.DTORequest;

/// <summary>
/// Yêu cầu tạo mới tài khoản quản trị viên.
/// </summary>
public class AdminCreateRequest
{
    /// <summary>
    /// Tên đăng nhập duy nhất của quản trị viên.
    /// </summary>
    /// <example>manager_hn</example>
    [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
    [MaxLength(100, ErrorMessage = "Tên đăng nhập tối đa 100 ký tự.")]
    public string Username { get; set; } = null!;

    /// <summary>
    /// Mật khẩu khởi tạo (tối thiểu 6 ký tự).
    /// </summary>
    /// <example>Manager@2026</example>
    [Required(ErrorMessage = "Mật khẩu không được để trống.")]
    [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự.")]
    public string Password { get; set; } = null!;

    /// <summary>
    /// Email liên hệ của quản trị viên.
    /// </summary>
    /// <example>manager_hn@stickerrobot.com</example>
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
    [MaxLength(100)]
    public string? Email { get; set; }

    /// <summary>
    /// Vai trò của tài khoản trong hệ thống ('SuperAdmin', 'Manager', hoặc 'admin'). Mặc định là 'admin'.
    /// </summary>
    /// <example>Manager</example>
    [MaxLength(30)]
    public string? Role { get; set; } = UserRoles.Admin;
}
