using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

/// <summary>
/// Yêu cầu tạo mới thông tin ốp lưng điện thoại trong kho.
/// </summary>
public class PhoneCaseCreateRequest
{
    /// <summary>
    /// ID của dòng điện thoại tương thích (từ API PhoneModels).
    /// </summary>
    /// <example>1</example>
    public int? ModelId { get; set; }

    /// <summary>
    /// Mã SKU quản lý hàng hóa của ốp lưng.
    /// </summary>
    /// <example>CASE-IP15P-CLR</example>
    [MaxLength(100, ErrorMessage = "Mã SKU tối đa 100 ký tự.")]
    public string? Sku { get; set; }

    /// <summary>
    /// Chất liệu sản phẩm (ví dụ: Silicone dẻo, Nhựa PC cứng, TPU chống sốc).
    /// </summary>
    /// <example>Silicone trong suốt chống ố</example>
    [MaxLength(100)]
    public string? Material { get; set; }

    /// <summary>
    /// Màu sắc của ốp.
    /// </summary>
    /// <example>Trong suốt</example>
    [MaxLength(50)]
    public string? Color { get; set; }

    /// <summary>
    /// Đơn giá bán của ốp (VNĐ).
    /// </summary>
    /// <example>80000</example>
    [Range(0, 99999999, ErrorMessage = "Đơn giá không hợp lệ.")]
    public decimal? Price { get; set; }

    /// <summary>
    /// Số lượng tồn kho ban đầu trong khay chứa của kiosk.
    /// </summary>
    /// <example>50</example>
    [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho không được âm.")]
    public int? StockQuantity { get; set; }
}
