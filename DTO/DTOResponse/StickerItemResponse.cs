namespace StickerRobot.DTO.DTOResponse;

public class StickerItemResponse
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public int? StaffId { get; set; }
    public int? StickerProductId { get; set; }
    public string? StickerName { get; set; }
    public int? PositionX { get; set; }
    public int? PositionY { get; set; }
    public double? Scale { get; set; }
    public int RotationDeg { get; set; }
}
