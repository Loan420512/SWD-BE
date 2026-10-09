using System;
using System.Collections.Generic;

namespace StickerRobot.Models;

public partial class CashFlowAudit
{
    public int Id { get; set; }

    public int? AdminId { get; set; }

    public DateOnly? AuditDate { get; set; }

    public decimal? TotalRevenue { get; set; }

    public decimal? TotalRefund { get; set; }

    public string? Status { get; set; }

    public virtual Admin? Admin { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<RefundTransaction> RefundTransactions { get; set; } = new List<RefundTransaction>();
}
