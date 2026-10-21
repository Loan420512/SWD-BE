namespace StickerRobot.DTO.DTOResponse;

/// <summary>
/// Thông tin chi tiết giao dịch thanh toán.
/// </summary>
public class PaymentResponse
{
    /// <summary>
    /// Mã định danh duy nhất của giao dịch thanh toán.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// ID đơn hàng liên kết.
    /// </summary>
    /// <example>1</example>
    public int OrderId { get; set; }

    /// <summary>
    /// ID phiên đối soát kiểm toán (nếu có).
    /// </summary>
    /// <example>1</example>
    public int? AuditId { get; set; }

    /// <summary>
    /// Số tiền thanh toán (VNĐ).
    /// </summary>
    /// <example>105000</example>
    public decimal? Amount { get; set; }

    /// <summary>
    /// Phương thức thanh toán ('Cash', 'VNPay', 'Momo', 'ZaloPay').
    /// </summary>
    /// <example>VNPay</example>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Trạng thái thanh toán ('Success', 'Pending', 'Paid', 'Failed', 'Refunded').
    /// </summary>
    /// <example>Success</example>
    public string? PaymentStatus { get; set; }

    /// <summary>
    /// Mã giao dịch từ đối tác cổng thanh toán.
    /// </summary>
    /// <example>VNPAY143526789</example>
    public string? TransactionId { get; set; }

    /// <summary>
    /// Thời điểm thanh toán thành công.
    /// </summary>
    /// <example>2026-10-20T14:35:00Z</example>
    public DateTime? PaidAt { get; set; }
}
