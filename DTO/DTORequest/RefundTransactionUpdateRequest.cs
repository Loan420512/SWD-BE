using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

/// <summary>
/// Yêu cầu cập nhật thông tin và trạng thái phê duyệt hoàn tiền.
/// </summary>
public class RefundTransactionUpdateRequest
{
    /// <summary>
    /// ID phiên kiểm toán đối soát.
    /// </summary>
    /// <example>1</example>
    public int? AuditId { get; set; }

    /// <summary>
    /// ID quản trị viên xử lý phê duyệt.
    /// </summary>
    /// <example>1</example>
    public int? AdminId { get; set; }

    /// <summary>
    /// Số tiền hoàn trả (VNĐ).
    /// </summary>
    /// <example>105000</example>
    [Range(0, 99999999, ErrorMessage = "Số tiền hoàn không hợp lệ.")]
    public decimal? RefundAmount { get; set; }

    /// <summary>
    /// Ghi chú hoặc lý do phê duyệt/từ chối hoàn tiền.
    /// </summary>
    /// <example>Đã xác nhận sự cố kỹ thuật và hoàn tiền qua tài khoản VNPay của khách.</example>
    [MaxLength(500)]
    public string? Reason { get; set; }

    /// <summary>
    /// Trạng thái phê duyệt mới ('Pending', 'Approved', 'Rejected').
    /// </summary>
    /// <example>Approved</example>
    [MaxLength(30)]
    public string? Status { get; set; }
}
