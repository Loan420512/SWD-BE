using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

/// <summary>
/// Yêu cầu tạo mới bản ghi thanh toán cho đơn hàng tại kiosk.
/// </summary>
public class PaymentCreateRequest
{
    /// <summary>
    /// ID đơn hàng cần thanh toán.
    /// </summary>
    /// <example>1</example>
    [Required(ErrorMessage = "OrderId không được để trống.")]
    public int OrderId { get; set; }

    /// <summary>
    /// ID phiên kiểm toán dòng tiền liên kết (nếu có).
    /// </summary>
    /// <example>1</example>
    public int? AuditId { get; set; }

    /// <summary>
    /// Số tiền thanh toán thực tế (VNĐ).
    /// </summary>
    /// <example>105000</example>
    [Range(0, 99999999, ErrorMessage = "Số tiền không hợp lệ.")]
    public decimal? Amount { get; set; }

    /// <summary>
    /// Phương thức thanh toán được áp dụng ('Cash', 'VNPay', 'Momo', 'ZaloPay').
    /// </summary>
    /// <example>VNPay</example>
    [MaxLength(30)]
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Trạng thái thanh toán ban đầu ('Success', 'Pending', 'Paid', 'Failed', 'Refunded').
    /// </summary>
    /// <example>Pending</example>
    [MaxLength(30)]
    public string? PaymentStatus { get; set; }

    /// <summary>
    /// Mã giao dịch tham chiếu từ cổng thanh toán đối tác hoặc mã hóa đơn tiền mặt.
    /// </summary>
    /// <example>VNPAY143526789</example>
    [MaxLength(100)]
    public string? TransactionId { get; set; }

    /// <summary>
    /// Thời điểm thanh toán hoàn tất.
    /// </summary>
    /// <example>2026-10-20T14:35:00Z</example>
    public DateTime? PaidAt { get; set; }
}
