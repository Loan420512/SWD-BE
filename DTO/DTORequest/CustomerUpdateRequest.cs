using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

/// <summary>
/// Yêu cầu cập nhật thông tin khách hàng.
/// </summary>
public class CustomerUpdateRequest
{
    /// <summary>
    /// Họ và tên mới của khách hàng.
    /// </summary>
    /// <example>Nguyễn Văn A</example>
    [MaxLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự.")]
    public string? FullName { get; set; }

    /// <summary>
    /// Số điện thoại liên hệ mới.
    /// </summary>
    /// <example>0912345678</example>
    [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
    [MaxLength(20)]
    public string? Phone { get; set; }

    /// <summary>
    /// Địa chỉ email mới.
    /// </summary>
    /// <example>nguyenvana@gmail.com</example>
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    [MaxLength(100)]
    public string? Email { get; set; }
}
