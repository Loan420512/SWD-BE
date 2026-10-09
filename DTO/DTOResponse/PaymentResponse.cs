namespace StickerRobot.DTO.DTOResponse;

public class PaymentResponse
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int? AuditId { get; set; }
    public decimal? Amount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentStatus { get; set; }
    public string? TransactionId { get; set; }
    public DateTime? PaidAt { get; set; }
}
