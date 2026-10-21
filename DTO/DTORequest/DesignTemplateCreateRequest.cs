using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

/// <summary>
/// Yêu cầu tạo mới mẫu thiết kế sticker phối sẵn.
/// </summary>
public class DesignTemplateCreateRequest
{
    /// <summary>
    /// ID của ốp lưng tương thích với mẫu thiết kế này.
    /// </summary>
    /// <example>1</example>
    public int? CaseId { get; set; }

    /// <summary>
    /// ID nhân viên thiết kế tạo mẫu.
    /// </summary>
    /// <example>1</example>
    public int? StaffId { get; set; }

    /// <summary>
    /// Tên gọi của mẫu thiết kế.
    /// </summary>
    /// <example>Mẫu Thiết Kế Mùa Hè Năng Động</example>
    [Required(ErrorMessage = "Tên mẫu thiết kế không được để trống.")]
    [MaxLength(150, ErrorMessage = "Tên mẫu thiết kế tối đa 150 ký tự.")]
    public string TemplateName { get; set; } = null!;

    /// <summary>
    /// Đường dẫn ảnh xem trước của toàn bộ mẫu thiết kế.
    /// </summary>
    /// <example>/uploads/sticker/seed-sun.png</example>
    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Trạng thái kích hoạt cho phép khách hàng chọn mẫu này tại kiosk.
    /// </summary>
    /// <example>true</example>
    public bool? IsActive { get; set; } = true;
}
