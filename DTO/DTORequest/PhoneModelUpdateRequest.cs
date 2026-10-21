using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

/// <summary>
/// Yêu cầu cập nhật thông tin dòng điện thoại.
/// </summary>
public class PhoneModelUpdateRequest
{
    /// <summary>
    /// Tên thương hiệu điện thoại mới.
    /// </summary>
    /// <example>Apple</example>
    [MaxLength(50, ErrorMessage = "Tên thương hiệu tối đa 50 ký tự.")]
    public string? Brand { get; set; }

    /// <summary>
    /// Tên thương mại mới của dòng máy.
    /// </summary>
    /// <example>iPhone 15 Pro</example>
    [MaxLength(100, ErrorMessage = "Tên model tối đa 100 ký tự.")]
    public string? ModelName { get; set; }

    /// <summary>
    /// Kích thước vật lý mới.
    /// </summary>
    /// <example>146.6 x 70.6 x 8.25 mm</example>
    [MaxLength(100)]
    public string? Dimensions { get; set; }
}
