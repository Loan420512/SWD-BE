namespace StickerRobot.DTO.DTOResponse;

/// <summary>Bearer token for admin calls.</summary>
public class LoginResponse
{
    public string Token { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Role { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}

/// <summary>Site path of a stored upload, always under /uploads/{kind}/.</summary>
public class UploadResponse
{
    public string Url { get; set; } = null!;
}

public class StickerProductResponse
{
    public int Id { get; set; }
    public string StickerName { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public decimal WidthMm { get; set; }
    public decimal HeightMm { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PoseResponse
{
    public string ModelUrl { get; set; } = null!;
    public string BackAxis { get; set; } = null!;
    public string UpAxis { get; set; } = null!;
}

public class CaseFitResponse : PoseResponse
{
    public decimal HeightMm { get; set; }
    public decimal BackWidthMm { get; set; }
    public int MaskCols { get; set; }
    public int MaskRows { get; set; }
    public string MaskBase64 { get; set; } = null!;
    public decimal PhoneInsetMm { get; set; }
    public decimal PhoneScale { get; set; }
}

public class CatalogCaseResponse
{
    public int Id { get; set; }
    public string? Sku { get; set; }
    public string? Material { get; set; }
    public string? Color { get; set; }
    public decimal? Price { get; set; }
    public int? StockQuantity { get; set; }
    public CaseFitResponse? Fit { get; set; }
}

public class CatalogModelResponse
{
    public int Id { get; set; }
    public string? Brand { get; set; }
    public string? ModelName { get; set; }
    public string? Dimensions { get; set; }
    public PoseResponse? Phone { get; set; }
    public List<CatalogCaseResponse> Cases { get; set; } = new();
}

/// <summary>Everything the kiosk needs to design: models with cases, and active stickers.</summary>
public class CatalogResponse
{
    public List<CatalogModelResponse> Models { get; set; } = new();
    public List<StickerProductResponse> Stickers { get; set; } = new();
}

public class OrderPlacementResponse
{
    public string? Name { get; set; }
    public string? ImageUrl { get; set; }
    public int? PositionX { get; set; }
    public int? PositionY { get; set; }
    public int RotationDeg { get; set; }
}

/// <summary>Public order lookup by code. Carries no customer contact data.</summary>
public class PublicOrderResponse
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = null!;
    public string? Status { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? ModelLabel { get; set; }
    public string? CaseLabel { get; set; }
    public string? CaseSku { get; set; }
    public string? StickerName { get; set; }
    public string? StickerImageUrl { get; set; }
    public int? PositionX { get; set; }
    public int? PositionY { get; set; }
    public int RotationDeg { get; set; }
    public List<OrderPlacementResponse> Placements { get; set; } = new();
    public string? PaymentMethod { get; set; }
    public DateTime? CreatedAt { get; set; }
}

/// <summary>Admin order row with what the robot needs.</summary>
public class AdminOrderResponse : PublicOrderResponse
{
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? PaymentStatus { get; set; }
}
