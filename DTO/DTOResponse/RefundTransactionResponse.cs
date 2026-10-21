namespace StickerRobot.DTO.DTOResponse;

/// <summary>
/// Thông tin chi tiết giao dịch hoàn tiền.
/// </summary>
public class RefundTransactionResponse
{
    /// <summary>
    /// Mã định danh duy nhất của giao dịch hoàn tiền.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// ID phiên đối soát kiểm toán liên kết.
    /// </summary>
    /// <example>1</example>
    public int? AuditId { get; set; }

    /// <summary>
    /// ID quản trị viên duyệt hoàn tiền.
    /// </summary>
    /// <example>1</example>
    public int? AdminId { get; set; }

    /// <summary>
    /// ID giao dịch thanh toán gốc được hoàn.
    /// </summary>
    /// <example>1</example>
    public int PaymentId { get; set; }

    /// <summary>
    /// Số tiền hoàn trả (VNĐ).
    /// </summary>
    /// <example>105000</example>
    public decimal? RefundAmount { get; set; }

    /// <summary>
    /// Lý do hoàn tiền.
    /// </summary>
    /// <example>Robot kẹt khay in hỏng ốp, hoàn tiền theo yêu cầu khách.</example>
    public string? Reason { get; set; }

    /// <summary>
    /// Trạng thái giao dịch hoàn tiền ('Pending', 'Approved', 'Rejected').
    /// </summary>
    /// <example>Approved</example>
    public string? Status { get; set; }

    /// <summary>
    /// Thời điểm khởi tạo giao dịch hoàn tiền.
    /// </summary>
    /// <example>2026-10-20T15:00:00Z</example>
    public DateTime? CreatedAt { get; set; }
}
