using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

/// <summary>
/// Yêu cầu tạo mới thông tin khách hàng tại kiosk.
/// </summary>
public class CustomerCreateRequest
{
    /// <summary>
    /// Họ và tên khách hàng.
    /// </summary>
    /// <example>Nguyễn Văn A</example>
    [Required(ErrorMessage = "Họ tên không được để trống.")]
    [MaxLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự.")]
    public string FullName { get; set; } = null!;

    /// <summary>
    /// Số điện thoại liên hệ (dùng để nhận mã thông báo hoàn thành in).
    /// </summary>
    /// <example>0912345678</example>
    [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
    [MaxLength(20)]
    public string? Phone { get; set; }

    /// <summary>
    /// Địa chỉ email nhận hóa đơn điện tử hoặc thông báo.
    /// </summary>
    /// <example>nguyenvana@gmail.com</example>
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    [MaxLength(100)]
    public string? Email { get; set; }
}
