using System;
using System.Collections.Generic;

namespace StickerRobot.Models;

public partial class DesignTemplate
{
    public int Id { get; set; }

    public int? CaseId { get; set; }

    public int? StaffId { get; set; }

    public string? TemplateName { get; set; }

    public string? ImageUrl { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual PhoneCase? Case { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Staff? Staff { get; set; }

    public virtual ICollection<StickerItem> StickerItems { get; set; } = new List<StickerItem>();
}
