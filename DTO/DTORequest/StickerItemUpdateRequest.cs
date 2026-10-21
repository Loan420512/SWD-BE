using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

/// <summary>
/// Yêu cầu cập nhật tọa độ hoặc thông số của sticker trong mẫu thiết kế.
/// </summary>
public class StickerItemUpdateRequest
{
    /// <summary>
    /// ID nhân viên chỉnh sửa.
    /// </summary>
    /// <example>1</example>
    public int? StaffId { get; set; }

    /// <summary>
    /// Tên sticker.
    /// </summary>
    /// <example>Sticker Mèo May Mắn</example>
    [MaxLength(150)]
    public string? StickerName { get; set; }

    /// <summary>
    /// Tọa độ tâm X mới (đơn vị: 1/10 mm).
    /// </summary>
    /// <example>360</example>
    public int? PositionX { get; set; }

    /// <summary>
    /// Tọa độ tâm Y mới (đơn vị: 1/10 mm).
    /// </summary>
    /// <example>720</example>
    public int? PositionY { get; set; }

    /// <summary>
    /// Tỷ lệ kích thước mới.
    /// </summary>
    /// <example>1.0</example>
    public double? Scale { get; set; }

    /// <summary>
    /// Góc xoay mới (độ).
    /// </summary>
    /// <example>90</example>
    [Range(0, 359, ErrorMessage = "Góc xoay phải từ 0 đến 359 độ.")]
    public int? RotationDeg { get; set; }
}
