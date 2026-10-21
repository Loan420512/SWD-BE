namespace StickerRobot.DTO.DTOResponse;

/// <summary>
/// Thông tin chi tiết một phần tử sticker được gán trên mẫu thiết kế.
/// </summary>
public class StickerItemResponse
{
    /// <summary>
    /// Mã định danh duy nhất của phần tử sticker.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// ID mẫu thiết kế sở hữu.
    /// </summary>
    /// <example>1</example>
    public int TemplateId { get; set; }

    /// <summary>
    /// ID nhân viên tạo/chỉnh sửa.
    /// </summary>
    /// <example>1</example>
    public int? StaffId { get; set; }

    /// <summary>
    /// ID sản phẩm sticker gốc trong thư viện.
    /// </summary>
    /// <example>1</example>
    public int? StickerProductId { get; set; }

    /// <summary>
    /// Tên sticker.
    /// </summary>
    /// <example>Sticker Mèo May Mắn</example>
    public string? StickerName { get; set; }

    /// <summary>
    /// Tọa độ tâm X (đơn vị: 1/10 mm).
    /// </summary>
    /// <example>350</example>
    public int? PositionX { get; set; }

    /// <summary>
    /// Tọa độ tâm Y (đơn vị: 1/10 mm).
    /// </summary>
    /// <example>700</example>
    public int? PositionY { get; set; }

    /// <summary>
    /// Tỷ lệ kích thước áp dụng.
    /// </summary>
    /// <example>1.0</example>
    public double? Scale { get; set; }

    /// <summary>
    /// Góc xoay (độ).
    /// </summary>
    /// <example>45</example>
    public int RotationDeg { get; set; }
}
