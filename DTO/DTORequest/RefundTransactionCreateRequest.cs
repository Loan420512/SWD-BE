using System.ComponentModel.DataAnnotations;
using StickerRobot.Constants;

namespace StickerRobot.DTO.DTORequest;

/// <summary>
/// Yêu cầu tạo mới giao dịch hoàn tiền cho khách hàng.
/// </summary>
public class RefundTransactionCreateRequest
{
    /// <summary>
    /// ID phiên đối soát kiểm toán liên quan (nếu có).
    /// </summary>
    /// <example>1</example>
    public int? AuditId { get; set; }

    /// <summary>
    /// ID quản trị viên phê duyệt hoàn tiền.
    /// </summary>
    /// <example>1</example>
    public int? AdminId { get; set; }

    /// <summary>
    /// ID giao dịch thanh toán gốc cần hoàn tiền.
    /// </summary>
    /// <example>1</example>
    [Required(ErrorMessage = "PaymentId không được để trống.")]
    public int PaymentId { get; set; }

    /// <summary>
    /// Số tiền hoàn trả cho khách (VNĐ).
    /// </summary>
    /// <example>105000</example>
    [Range(0, 99999999, ErrorMessage = "Số tiền hoàn không hợp lệ.")]
    public decimal? RefundAmount { get; set; }

    /// <summary>
    /// Lý do hoàn tiền (ví dụ: 'Robot kẹt khay in hỏng ốp', 'Khách hủy đơn trước khi in').
    /// </summary>
    /// <example>Robot gặp sự cố kẹt tem dán hỏng ốp, hoàn tiền theo yêu cầu khách.</example>
    [MaxLength(500)]
    public string? Reason { get; set; }

    /// <summary>
    /// Trạng thái yêu cầu hoàn tiền ('Pending', 'Approved', 'Rejected'). Mặc định là 'Pending'.
    /// </summary>
    /// <example>Pending</example>
    [MaxLength(30)]
    public string? Status { get; set; } = RefundStatus.Pending;
}
