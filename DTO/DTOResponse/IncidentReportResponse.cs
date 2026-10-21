namespace StickerRobot.DTO.DTOResponse;

/// <summary>
/// Thông tin chi tiết báo cáo sự cố kỹ thuật.
/// </summary>
public class IncidentReportResponse
{
    /// <summary>
    /// Mã định danh duy nhất của báo cáo sự cố.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// ID nhân viên ghi nhận hoặc phụ trách.
    /// </summary>
    /// <example>1</example>
    public int? StaffId { get; set; }

    /// <summary>
    /// Loại sự cố.
    /// </summary>
    /// <example>Kẹt khay in sticker</example>
    public string? IncidentType { get; set; }

    /// <summary>
    /// Mô tả chi tiết sự cố.
    /// </summary>
    /// <example>Robot không thể nhả sticker ở khay số 2 sau lệnh in đơn hàng ORD-20261020-0001.</example>
    public string? Description { get; set; }

    /// <summary>
    /// Trạng thái sự cố ('Open', 'In Progress', 'Resolved').
    /// </summary>
    /// <example>Open</example>
    public string? Status { get; set; }

    /// <summary>
    /// Thời điểm phát sinh sự cố.
    /// </summary>
    /// <example>2026-10-20T11:15:00Z</example>
    public DateTime? ReportedAt { get; set; }
}
