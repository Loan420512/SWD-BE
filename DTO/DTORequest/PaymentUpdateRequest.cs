using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

/// <summary>
/// Yêu cầu cập nhật thông tin giao dịch thanh toán.
/// </summary>
public class PaymentUpdateRequest
{
    /// <summary>
    /// ID phiên kiểm toán đối soát.
    /// </summary>
    /// <example>1</example>
    public int? AuditId { get; set; }

    /// <summary>
    /// Số tiền thanh toán (VNĐ).
    /// </summary>
    /// <example>105000</example>
    [Range(0, 99999999, ErrorMessage = "Số tiền không hợp lệ.")]
    public decimal? Amount { get; set; }

    /// <summary>
    /// Phương thức thanh toán ('Cash', 'VNPay', 'Momo', 'ZaloPay').
    /// </summary>
    /// <example>VNPay</example>
    [MaxLength(30)]
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Trạng thái thanh toán ('Success', 'Pending', 'Paid', 'Failed', 'Refunded').
    /// </summary>
    /// <example>Success</example>
    [MaxLength(30)]
    public string? PaymentStatus { get; set; }

    /// <summary>
    /// Mã giao dịch tham chiếu đối tác.
    /// </summary>
    /// <example>VNPAY143526789</example>
    [MaxLength(100)]
    public string? TransactionId { get; set; }

    /// <summary>
    /// Thời điểm thanh toán thành công.
    /// </summary>
    /// <example>2026-10-20T14:35:00Z</example>
    public DateTime? PaidAt { get; set; }
}
