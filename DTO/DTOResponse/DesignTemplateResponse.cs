namespace StickerRobot.DTO.DTOResponse;

/// <summary>
/// Thông tin chi tiết mẫu thiết kế sticker.
/// </summary>
public class DesignTemplateResponse
{
    /// <summary>
    /// Mã định danh duy nhất của mẫu thiết kế.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// ID ốp lưng tương thích.
    /// </summary>
    /// <example>1</example>
    public int? CaseId { get; set; }

    /// <summary>
    /// ID nhân viên tạo mẫu.
    /// </summary>
    /// <example>1</example>
    public int? StaffId { get; set; }

    /// <summary>
    /// Tên mẫu thiết kế.
    /// </summary>
    /// <example>Mẫu Thiết Kế Mùa Hè Năng Động</example>
    public string? TemplateName { get; set; }

    /// <summary>
    /// Đường dẫn ảnh xem trước.
    /// </summary>
    /// <example>/uploads/sticker/seed-sun.png</example>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Trạng thái hoạt động.
    /// </summary>
    /// <example>true</example>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Thời điểm khởi tạo mẫu.
    /// </summary>
    /// <example>2026-10-20T10:00:00Z</example>
    public DateTime? CreatedAt { get; set; }
}
