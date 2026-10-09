using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

public class RefundTransactionUpdateRequest
{
    public int? AuditId { get; set; }
    public int? AdminId { get; set; }

    [Range(0, 99999999)]
    public decimal? RefundAmount { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    [MaxLength(30)]
    public string? Status { get; set; }
}
