namespace StickerRobot.DTO.DTOResponse;

/// <summary>
/// Thông tin bản ghi kiểm toán đối soát dòng tiền.
/// </summary>
public class CashFlowAuditResponse
{
    /// <summary>
    /// Mã định danh duy nhất của bản ghi kiểm toán.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// ID quản trị viên chịu trách nhiệm đối soát.
    /// </summary>
    /// <example>1</example>
    public int? AdminId { get; set; }

    /// <summary>
    /// Ngày ghi nhận đối soát.
    /// </summary>
    /// <example>2026-10-20</example>
    public DateOnly? AuditDate { get; set; }

    /// <summary>
    /// Tổng doanh thu ghi nhận (VNĐ).
    /// </summary>
    /// <example>5000000</example>
    public decimal? TotalRevenue { get; set; }

    /// <summary>
    /// Tổng tiền đã hoàn trả khách (VNĐ).
    /// </summary>
    /// <example>120000</example>
    public decimal? TotalRefund { get; set; }

    /// <summary>
    /// Trạng thái đối soát ('Matched', 'Discrepancy').
    /// </summary>
    /// <example>Matched</example>
    public string? Status { get; set; }
}
