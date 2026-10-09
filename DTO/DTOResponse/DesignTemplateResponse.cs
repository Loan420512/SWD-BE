namespace StickerRobot.DTO.DTOResponse;

public class DesignTemplateResponse
{
    public int Id { get; set; }
    public int? CaseId { get; set; }
    public int? StaffId { get; set; }
    public string? TemplateName { get; set; }
    public string? ImageUrl { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
}
