namespace StickerRobot.DTO.DTOResponse;

/// <summary>
/// Thông tin chi tiết khách hàng.
/// </summary>
public class CustomerResponse
{
    /// <summary>
    /// Mã định danh duy nhất của khách hàng.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// Họ và tên khách hàng.
    /// </summary>
    /// <example>Nguyễn Văn A</example>
    public string? FullName { get; set; }

    /// <summary>
    /// Số điện thoại liên hệ.
    /// </summary>
    /// <example>0912345678</example>
    public string? Phone { get; set; }

    /// <summary>
    /// Địa chỉ email khách hàng.
    /// </summary>
    /// <example>nguyenvana@gmail.com</example>
    public string? Email { get; set; }

    /// <summary>
    /// Thời điểm khởi tạo hồ sơ khách hàng.
    /// </summary>
    /// <example>2026-10-20T10:00:00Z</example>
    public DateTime? CreatedAt { get; set; }
}
