using System.ComponentModel.DataAnnotations;

namespace StickerRobot.DTO.DTORequest;

public class OrderCreateRequest
{
    [MaxLength(50)]
    public string? OrderCode { get; set; }

    public int? CustomerId { get; set; }
    public int? PhoneCaseId { get; set; }
    public int? DesignTemplateId { get; set; }

    [Range(0, 99999999)]
    public decimal? TotalAmount { get; set; }

    [MaxLength(30)]
    public string? Status { get; set; } = "Pending";
}
