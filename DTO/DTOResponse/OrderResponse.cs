namespace StickerRobot.DTO.DTOResponse;

public class OrderResponse
{
    public int Id { get; set; }
    public string? OrderCode { get; set; }
    public int? CustomerId { get; set; }
    public int? PhoneCaseId { get; set; }
    public int? DesignTemplateId { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
}
