using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

/// <summary>
/// Yêu cầu cập nhật bản ghi kiểm toán đối soát dòng tiền kiosk.
/// </summary>
public class CashFlowAuditUpdateRequest
{
    /// <summary>
    /// ID tài khoản quản trị viên thực hiện kiểm toán.
    /// </summary>
    /// <example>1</example>
    public int? AdminId { get; set; }

    /// <summary>
    /// Ngày đối soát (định dạng YYYY-MM-DD).
    /// </summary>
    /// <example>2026-10-20</example>
    public DateOnly? AuditDate { get; set; }

    /// <summary>
    /// Tổng doanh thu ghi nhận trong kỳ (VNĐ).
    /// </summary>
    /// <example>5000000</example>
    [Range(0, 99999999, ErrorMessage = "Tổng doanh thu không hợp lệ.")]
    public decimal? TotalRevenue { get; set; }

    /// <summary>
    /// Tổng số tiền hoàn trả trong kỳ (VNĐ).
    /// </summary>
    /// <example>120000</example>
    [Range(0, 99999999, ErrorMessage = "Tổng tiền hoàn không hợp lệ.")]
    public decimal? TotalRefund { get; set; }

    /// <summary>
    /// Trạng thái đối soát ('Matched': khớp dữ liệu, 'Discrepancy': có sai lệch).
    /// </summary>
    /// <example>Matched</example>
    [MaxLength(30)]
    public string? Status { get; set; }
}
