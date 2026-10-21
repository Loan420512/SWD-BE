namespace StickerRobot.DTO.DTOResponse;

/// <summary>
/// Thông tin chi tiết đơn hàng phản hồi từ hệ thống.
/// </summary>
public class OrderResponse
{
    /// <summary>
    /// Mã định danh duy nhất của đơn hàng.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// Mã đơn hàng công khai.
    /// </summary>
    /// <example>ORD-20261020-0001</example>
    public string? OrderCode { get; set; }

    /// <summary>
    /// ID khách hàng đặt đơn.
    /// </summary>
    /// <example>1</example>
    public int? CustomerId { get; set; }

    /// <summary>
    /// ID ốp lưng đã chọn.
    /// </summary>
    /// <example>1</example>
    public int? PhoneCaseId { get; set; }

    /// <summary>
    /// ID mẫu thiết kế đã chọn.
    /// </summary>
    /// <example>1</example>
    public int? DesignTemplateId { get; set; }

    /// <summary>
    /// Tổng tiền đơn hàng (VNĐ).
    /// </summary>
    /// <example>105000</example>
    public decimal? TotalAmount { get; set; }

    /// <summary>
    /// Trạng thái đơn hàng hiện tại ('Pending', 'Processing', 'Shipped', 'Completed', 'Cancelled').
    /// </summary>
    /// <example>Pending</example>
    public string? Status { get; set; }

    /// <summary>
    /// Thời điểm khởi tạo đơn hàng.
    /// </summary>
    /// <example>2026-10-20T14:30:00Z</example>
    public DateTime? CreatedAt { get; set; }
}
