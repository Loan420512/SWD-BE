using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

/// <summary>
/// Yêu cầu gắn một sticker vào mẫu thiết kế với vị trí và góc xoay cụ thể.
/// </summary>
public class StickerItemCreateRequest
{
    /// <summary>
    /// ID mẫu thiết kế cần gắn sticker.
    /// </summary>
    /// <example>1</example>
    [Required(ErrorMessage = "TemplateId không được để trống.")]
    public int TemplateId { get; set; }

    /// <summary>
    /// ID nhân viên thiết kế (nếu có).
    /// </summary>
    /// <example>1</example>
    public int? StaffId { get; set; }

    /// <summary>
    /// ID sản phẩm sticker từ thư viện (StickerProducts).
    /// </summary>
    /// <example>1</example>
    [Required(ErrorMessage = "StickerProductId không được để trống.")]
    public int? StickerProductId { get; set; }

    /// <summary>
    /// Tên hoặc nhãn mô tả sticker (tùy chọn).
    /// </summary>
    /// <example>Sticker Mèo May Mắn</example>
    [MaxLength(150)]
    public string? StickerName { get; set; }

    /// <summary>
    /// Tọa độ tâm của sticker tính theo phương ngang X (đơn vị: 1/10 mm tính từ mép trái lưng ốp).
    /// </summary>
    /// <example>350</example>
    [Required(ErrorMessage = "Tọa độ PositionX không được để trống.")]
    public int? PositionX { get; set; }

    /// <summary>
    /// Tọa độ tâm của sticker tính theo phương dọc Y (đơn vị: 1/10 mm tính từ mép trên lưng ốp).
    /// </summary>
    /// <example>700</example>
    [Required(ErrorMessage = "Tọa độ PositionY không được để trống.")]
    public int? PositionY { get; set; }

    /// <summary>
    /// Tỷ lệ phóng to/thu nhỏ (mặc định 1.0).
    /// </summary>
    /// <example>1.0</example>
    public double? Scale { get; set; }

    /// <summary>
    /// Góc xoay sticker theo chiều kim đồng hồ nhìn từ mặt sau (0 - 359 độ, bội số của 15).
    /// </summary>
    /// <example>45</example>
    [Range(0, 359, ErrorMessage = "Góc xoay phải từ 0 đến 359 độ.")]
    public int RotationDeg { get; set; }
}
