using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

/// <summary>
/// Yêu cầu cập nhật thông tin đơn hàng dành cho Quản trị viên.
/// </summary>
public class OrderUpdateRequest
{
    /// <summary>
    /// Mã đơn hàng.
    /// </summary>
    /// <example>ORD-20261020-0001</example>
    [MaxLength(50)]
    public string? OrderCode { get; set; }

    /// <summary>
    /// ID khách hàng liên kết.
    /// </summary>
    /// <example>1</example>
    public int? CustomerId { get; set; }

    /// <summary>
    /// ID ốp lưng liên kết.
    /// </summary>
    /// <example>1</example>
    public int? PhoneCaseId { get; set; }

    /// <summary>
    /// ID mẫu thiết kế liên kết.
    /// </summary>
    /// <example>1</example>
    public int? DesignTemplateId { get; set; }

    /// <summary>
    /// Tổng tiền đơn hàng (VNĐ).
    /// </summary>
    /// <example>105000</example>
    [Range(0, 99999999, ErrorMessage = "Tổng tiền không hợp lệ.")]
    public decimal? TotalAmount { get; set; }

    /// <summary>
    /// Trạng thái đơn hàng ('Pending', 'Processing', 'Shipped', 'Completed', 'Cancelled').
    /// </summary>
    /// <example>Completed</example>
    [MaxLength(30)]
    public string? Status { get; set; }
}
