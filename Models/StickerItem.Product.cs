namespace StickerRobot.Models;

public partial class StickerItem
{
    public int? StickerProductId { get; set; }

    public virtual StickerProduct? StickerProduct { get; set; }
}
