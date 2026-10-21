using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

/// <summary>
/// Yêu cầu thêm mới dòng máy điện thoại được hệ thống hỗ trợ.
/// </summary>
public class PhoneModelCreateRequest
{
    /// <summary>
    /// Tên thương hiệu điện thoại (Apple, Samsung, Xiaomi,...).
    /// </summary>
    /// <example>Apple</example>
    [Required(ErrorMessage = "Thương hiệu không được để trống.")]
    [MaxLength(50, ErrorMessage = "Tên thương hiệu tối đa 50 ký tự.")]
    public string Brand { get; set; } = null!;

    /// <summary>
    /// Tên thương mại đầy đủ của model máy.
    /// </summary>
    /// <example>iPhone 15 Pro</example>
    [Required(ErrorMessage = "Tên model không được để trống.")]
    [MaxLength(100, ErrorMessage = "Tên model tối đa 100 ký tự.")]
    public string ModelName { get; set; } = null!;

    /// <summary>
    /// Kích thước vật lý cơ bản (Dài x Rộng x Dày tính bằng mm).
    /// </summary>
    /// <example>146.6 x 70.6 x 8.25 mm</example>
    [MaxLength(100)]
    public string? Dimensions { get; set; }
}
