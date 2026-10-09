using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

public class CashFlowAuditCreateRequest
{
    public int? AdminId { get; set; }
    public DateOnly? AuditDate { get; set; }

    [Range(0, 99999999)]
    public decimal? TotalRevenue { get; set; }

    [Range(0, 99999999)]
    public decimal? TotalRefund { get; set; }

    [MaxLength(30)]
    public string? Status { get; set; }
}
