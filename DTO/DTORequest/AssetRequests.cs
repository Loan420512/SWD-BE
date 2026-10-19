using System.ComponentModel.DataAnnotations;
using StickerRobot.Utilities;

namespace StickerRobot.DTO.DTORequest;

/// <summary>Admin login against ADMIN.username and the BCrypt password_hash.</summary>
public class LoginRequest
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}

/// <summary>Orientation of an imported phone GLB. The phone is display only.</summary>
public class PhoneModelFitRequest
{
    [Required]
    [MaxLength(500)]
    public string ModelUrl { get; set; } = null!;

    [Required]
    [RegularExpression(BackSheetMath.AxisPattern)]
    public string BackAxis { get; set; } = null!;

    [Required]
    [RegularExpression(BackSheetMath.AxisPattern)]
    public string UpAxis { get; set; } = null!;
}

/// <summary>Orientation, real size, and baked 1 mm occupancy mask of an imported case GLB.</summary>
public class PhoneCaseFitRequest
{
    [Required]
    [MaxLength(500)]
    public string ModelUrl { get; set; } = null!;

    [Required]
    [RegularExpression(BackSheetMath.AxisPattern)]
    public string BackAxis { get; set; } = null!;

    [Required]
    [RegularExpression(BackSheetMath.AxisPattern)]
    public string UpAxis { get; set; } = null!;

    [Range(20, 400)]
    public decimal HeightMm { get; set; }

    [Range(10, 300)]
    public decimal BackWidthMm { get; set; }

    [Range(1, 400)]
    public int MaskCols { get; set; }

    [Range(1, 400)]
    public int MaskRows { get; set; }

    [Required]
    public string MaskBase64 { get; set; } = null!;

    /// <summary>Millimetres the phone is pushed into the case, along the back normal. 0 sits the phone back on the case's outer AABB.</summary>
    public decimal PhoneInsetMm { get; set; } = 1.5m;

    /// <summary>Scale applied after the phone height is matched to the case height.</summary>
    public decimal PhoneScale { get; set; } = 0.97m;
}

/// <summary>A printed sticker sold at the kiosk. Size is physical and fixed.</summary>
public class StickerProductRequest
{
    [Required]
    [MaxLength(150)]
    public string StickerName { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = null!;

    [Range(5, 200)]
    public decimal WidthMm { get; set; }

    [Range(5, 200)]
    public decimal HeightMm { get; set; }

    [Range(0, 99999999)]
    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>Admin order status change.</summary>
public class OrderStatusRequest
{
    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = null!;
}
