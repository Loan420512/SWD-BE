namespace StickerRobot.Models;

public partial class StickerProduct
{
    public int Id { get; set; }

    public string StickerName { get; set; } = null!;

    public string ImageUrl { get; set; } = null!;

    public decimal WidthMm { get; set; }

    public decimal HeightMm { get; set; }

    public decimal Price { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<StickerItem> StickerItems { get; set; } = new List<StickerItem>();
}
