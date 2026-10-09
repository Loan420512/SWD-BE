namespace StickerRobot.DTO.DTOResponse;

public class CashFlowAuditResponse
{
    public int Id { get; set; }
    public int? AdminId { get; set; }
    public DateOnly? AuditDate { get; set; }
    public decimal? TotalRevenue { get; set; }
    public decimal? TotalRefund { get; set; }
    public string? Status { get; set; }
}
