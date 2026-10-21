using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

/// <summary>
/// Yêu cầu cập nhật thông tin và tiến trình xử lý báo cáo sự cố.
/// </summary>
public class IncidentReportUpdateRequest
{
    /// <summary>
    /// ID nhân viên kỹ thuật tiếp nhận xử lý.
    /// </summary>
    /// <example>1</example>
    public int? StaffId { get; set; }

    /// <summary>
    /// Loại sự cố.
    /// </summary>
    /// <example>Kẹt khay in sticker</example>
    [MaxLength(50)]
    public string? IncidentType { get; set; }

    /// <summary>
    /// Mô tả chi tiết hoặc giải pháp kỹ thuật đã thực hiện.
    /// </summary>
    /// <example>Đã tháo khay 2, vệ sinh trục cuốn giấy, reset robot và in test thành công.</example>
    public string? Description { get; set; }

    /// <summary>
    /// Trạng thái xử lý sự cố ('Open', 'In Progress', 'Resolved').
    /// </summary>
    /// <example>Resolved</example>
    [MaxLength(30)]
    public string? Status { get; set; }
}
