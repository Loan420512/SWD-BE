namespace StickerRobot.DTO.DTOResponse;

/// <summary>
/// Thông tin chi tiết của một dòng máy điện thoại.
/// </summary>
public class PhoneModelResponse
{
    /// <summary>
    /// Mã định danh duy nhất của dòng điện thoại.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// Tên thương hiệu sản xuất.
    /// </summary>
    /// <example>Apple</example>
    public string? Brand { get; set; }

    /// <summary>
    /// Tên thương mại của model điện thoại.
    /// </summary>
    /// <example>iPhone 15 Pro</example>
    public string? ModelName { get; set; }

    /// <summary>
    /// Kích thước vật lý cơ bản.
    /// </summary>
    /// <example>146.6 x 70.6 x 8.25 mm</example>
    public string? Dimensions { get; set; }
}
