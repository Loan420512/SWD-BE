using System.ComponentModel.DataAnnotations;
using StickerRobot.Constants;

namespace StickerRobot.DTO.DTORequest;

/// <summary>
/// Yêu cầu tạo mới báo cáo sự cố kỹ thuật của kiosk robot.
/// </summary>
public class IncidentReportCreateRequest
{
    /// <summary>
    /// ID nhân viên phát hiện hoặc ghi nhận sự cố.
    /// </summary>
    /// <example>1</example>
    public int? StaffId { get; set; }

    /// <summary>
    /// Phân loại sự cố (ví dụ: 'Kẹt khay giấy/tem', 'Hỏng đầu in', 'Lỗi cảm biến căn chỉnh', 'Lỗi thanh toán').
    /// </summary>
    /// <example>Kẹt khay in sticker</example>
    [MaxLength(50)]
    public string? IncidentType { get; set; }

    /// <summary>
    /// Mô tả chi tiết hiện tượng sự cố và mã đơn hàng liên quan nếu có.
    /// </summary>
    /// <example>Robot không thể nhả sticker ở khay số 2 sau lệnh in đơn hàng ORD-20261020-0001.</example>
    public string? Description { get; set; }

    /// <summary>
    /// Trạng thái ban đầu của sự cố ('Open', 'In Progress', 'Resolved'). Mặc định là 'Open'.
    /// </summary>
    /// <example>Open</example>
    [MaxLength(30)]
    public string? Status { get; set; } = IncidentStatus.Open;
}
