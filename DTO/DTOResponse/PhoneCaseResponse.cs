namespace StickerRobot.DTO.DTOResponse;

/// <summary>
/// Thông tin chi tiết của ốp lưng điện thoại trong hệ thống.
/// </summary>
public class PhoneCaseResponse
{
    /// <summary>
    /// Mã định danh duy nhất của ốp lưng.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// ID dòng điện thoại tương thích.
    /// </summary>
    /// <example>1</example>
    public int? ModelId { get; set; }

    /// <summary>
    /// Mã SKU sản phẩm.
    /// </summary>
    /// <example>CASE-IP15P-CLR</example>
    public string? Sku { get; set; }

    /// <summary>
    /// Chất liệu ốp lưng.
    /// </summary>
    /// <example>Silicone trong suốt chống ố</example>
    public string? Material { get; set; }

    /// <summary>
    /// Màu sắc ốp lưng.
    /// </summary>
    /// <example>Trong suốt</example>
    public string? Color { get; set; }

    /// <summary>
    /// Đơn giá bán (VNĐ).
    /// </summary>
    /// <example>80000</example>
    public decimal? Price { get; set; }

    /// <summary>
    /// Số lượng tồn kho hiện có tại kiosk.
    /// </summary>
    /// <example>50</example>
    public int? StockQuantity { get; set; }
}
