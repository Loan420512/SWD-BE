using System;
using System.Collections.Generic;

namespace StickerRobot.Models;

public partial class StickerItem
{
    public int Id { get; set; }

    public int TemplateId { get; set; }

    public int? StaffId { get; set; }

    public string? StickerName { get; set; }

    public int? PositionX { get; set; }

    public int? PositionY { get; set; }

    public double? Scale { get; set; }

    public virtual Staff? Staff { get; set; }

    public virtual DesignTemplate Template { get; set; } = null!;
}
