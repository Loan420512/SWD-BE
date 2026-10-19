namespace StickerRobot.Models;

public partial class PhoneCase
{
    public string? ModelUrl { get; set; }

    public string? BackAxis { get; set; }

    public string? UpAxis { get; set; }

    public decimal? HeightMm { get; set; }

    public decimal? BackWidthMm { get; set; }

    public int? MaskCols { get; set; }

    public int? MaskRows { get; set; }

    public byte[]? BackMask { get; set; }

    public decimal PhoneInsetMm { get; set; }

    public decimal PhoneScale { get; set; }
}
