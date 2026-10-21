using System.ComponentModel.DataAnnotations;
using StickerRobot.Constants;

namespace StickerRobot.DTO.DTORequest;

/// <summary>
/// Yêu cầu tạo mới đơn hàng dán sticker tại kiosk robot.
/// </summary>
public class OrderCreateRequest
{
    /// <summary>
    /// Mã đơn hàng tùy chỉnh (nếu bỏ trống, hệ thống sẽ tự sinh theo cấu hình tiền tố OrderSettings).
    /// </summary>
    /// <example>ORD-20261020-0001</example>
    [MaxLength(50)]
    public string? OrderCode { get; set; }

    /// <summary>
    /// ID khách hàng đặt đơn (từ API Customers).
    /// </summary>
    /// <example>1</example>
    public int? CustomerId { get; set; }

    /// <summary>
    /// ID ốp lưng điện thoại được chọn dán.
    /// </summary>
    /// <example>1</example>
    public int? PhoneCaseId { get; set; }

    /// <summary>
    /// ID mẫu thiết kế sticker được chọn (nếu dùng mẫu thiết kế sẵn).
    /// </summary>
    /// <example>1</example>
    public int? DesignTemplateId { get; set; }

    /// <summary>
    /// Tổng tiền đơn hàng (VNĐ) bao gồm giá ốp lưng và các sticker đã chọn.
    /// </summary>
    /// <example>105000</example>
    [Range(0, 99999999, ErrorMessage = "Tổng tiền không hợp lệ.")]
    public decimal? TotalAmount { get; set; }

    /// <summary>
    /// Trạng thái ban đầu của đơn hàng ('Pending', 'Processing', 'Shipped', 'Completed', 'Cancelled'). Mặc định là 'Pending'.
    /// </summary>
    /// <example>Pending</example>
    [MaxLength(30)]
    public string? Status { get; set; } = OrderStatus.Pending;
}
