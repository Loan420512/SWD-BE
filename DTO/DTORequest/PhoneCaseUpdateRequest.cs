using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

public class PhoneCaseUpdateRequest
{
    public int? ModelId { get; set; }

    [MaxLength(100)]
    public string? Sku { get; set; }

    [MaxLength(100)]
    public string? Material { get; set; }

    [MaxLength(50)]
    public string? Color { get; set; }

    [Range(0, 99999999)]
    public decimal? Price { get; set; }

    [Range(0, int.MaxValue)]
    public int? StockQuantity { get; set; }
}
