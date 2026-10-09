namespace StickerRobot.DTO.DTOResponse;

public class PhoneCaseResponse
{
    public int Id { get; set; }
    public int? ModelId { get; set; }
    public string? Sku { get; set; }
    public string? Material { get; set; }
    public string? Color { get; set; }
    public decimal? Price { get; set; }
    public int? StockQuantity { get; set; }
}
