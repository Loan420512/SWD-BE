using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

/// <summary>
/// Yêu cầu cập nhật thông tin ốp lưng điện thoại.
/// </summary>
public class PhoneCaseUpdateRequest
{
    /// <summary>
    /// ID của dòng điện thoại tương thích.
    /// </summary>
    /// <example>1</example>
    public int? ModelId { get; set; }

    /// <summary>
    /// Mã SKU mới.
    /// </summary>
    /// <example>CASE-IP15P-CLR</example>
    [MaxLength(100, ErrorMessage = "Mã SKU tối đa 100 ký tự.")]
    public string? Sku { get; set; }

    /// <summary>
    /// Chất liệu mới.
    /// </summary>
    /// <example>Silicone trong suốt chống ố</example>
    [MaxLength(100)]
    public string? Material { get; set; }

    /// <summary>
    /// Màu sắc mới.
    /// </summary>
    /// <example>Trong suốt</example>
    [MaxLength(50)]
    public string? Color { get; set; }

    /// <summary>
    /// Đơn giá bán mới (VNĐ).
    /// </summary>
    /// <example>80000</example>
    [Range(0, 99999999, ErrorMessage = "Đơn giá không hợp lệ.")]
    public decimal? Price { get; set; }

    /// <summary>
    /// Số lượng tồn kho cập nhật.
    /// </summary>
    /// <example>45</example>
    [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho không được âm.")]
    public int? StockQuantity { get; set; }
}
