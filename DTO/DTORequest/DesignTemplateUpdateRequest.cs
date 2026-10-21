using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

/// <summary>
/// Yêu cầu cập nhật thông tin mẫu thiết kế sticker.
/// </summary>
public class DesignTemplateUpdateRequest
{
    /// <summary>
    /// ID của ốp lưng áp dụng mẫu.
    /// </summary>
    /// <example>1</example>
    public int? CaseId { get; set; }

    /// <summary>
    /// ID nhân viên cập nhật/phụ trách.
    /// </summary>
    /// <example>1</example>
    public int? StaffId { get; set; }

    /// <summary>
    /// Tên mới của mẫu thiết kế.
    /// </summary>
    /// <example>Mẫu Thiết Kế Mùa Hè Rực Rỡ</example>
    [MaxLength(150, ErrorMessage = "Tên mẫu thiết kế tối đa 150 ký tự.")]
    public string? TemplateName { get; set; }

    /// <summary>
    /// Đường dẫn ảnh xem trước mới.
    /// </summary>
    /// <example>/uploads/sticker/seed-sun.png</example>
    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Trạng thái hoạt động của mẫu.
    /// </summary>
    /// <example>true</example>
    public bool? IsActive { get; set; }
}
