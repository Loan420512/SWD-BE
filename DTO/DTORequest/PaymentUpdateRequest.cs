using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

public class PaymentUpdateRequest
{
    public int? AuditId { get; set; }

    [Range(0, 99999999)]
    public decimal? Amount { get; set; }

    [MaxLength(30)]
    public string? PaymentMethod { get; set; }

    [MaxLength(30)]
    public string? PaymentStatus { get; set; }

    [MaxLength(100)]
    public string? TransactionId { get; set; }

    public DateTime? PaidAt { get; set; }
}
