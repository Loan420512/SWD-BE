using System;
using System.Collections.Generic;

namespace StickerRobot.Models;

public partial class Order
{
    public int Id { get; set; }

    public string? OrderCode { get; set; }

    public int? CustomerId { get; set; }

    public int? PhoneCaseId { get; set; }

    public int? DesignTemplateId { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual DesignTemplate? DesignTemplate { get; set; }

    public virtual Payment? Payment { get; set; }

    public virtual PhoneCase? PhoneCase { get; set; }
}
