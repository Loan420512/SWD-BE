using System;
using System.Collections.Generic;

namespace StickerRobot.Models;

public partial class PhoneCase
{
    public int Id { get; set; }

    public int? ModelId { get; set; }

    public string? Sku { get; set; }

    public string? Material { get; set; }

    public string? Color { get; set; }

    public decimal? Price { get; set; }

    public int? StockQuantity { get; set; }

    public virtual ICollection<DesignTemplate> DesignTemplates { get; set; } = new List<DesignTemplate>();

    public virtual PhoneModel? Model { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
