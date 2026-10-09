using System;
using System.Collections.Generic;

namespace StickerRobot.Models;

public partial class RefundTransaction
{
    public int Id { get; set; }

    public int? AuditId { get; set; }

    public int? AdminId { get; set; }

    public int PaymentId { get; set; }

    public decimal? RefundAmount { get; set; }

    public string? Reason { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Admin? Admin { get; set; }

    public virtual CashFlowAudit? Audit { get; set; }

    public virtual Payment Payment { get; set; } = null!;
}
