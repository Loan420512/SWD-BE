namespace StickerRobot.DTO.DTOResponse;

public class RefundTransactionResponse
{
    public int Id { get; set; }
    public int? AuditId { get; set; }
    public int? AdminId { get; set; }
    public int PaymentId { get; set; }
    public decimal? RefundAmount { get; set; }
    public string? Reason { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
}
