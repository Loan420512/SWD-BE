using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace StickerRobot.DTO.DTORequest;

/// <summary>
/// Yêu cầu tải lên tệp tin mô hình 3D hoặc hình ảnh sticker.
/// </summary>
public class FileUploadRequest
{
    /// <summary>
    /// Tệp nhị phân cần tải lên (.glb cho 3D model tối đa 25MB, .png/.webp cho sticker tối đa 8MB).
    /// </summary>
    [Required]
    public IFormFile File { get; set; } = null!;

    /// <summary>
    /// Loại tài nguyên tải lên: 'phone' (mô hình 3D điện thoại), 'case' (mô hình 3D ốp lưng), hoặc 'sticker' (hình ảnh sticker).
    /// </summary>
    /// <example>sticker</example>
    [Required]
    public string Kind { get; set; } = null!;
}
