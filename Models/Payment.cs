using System;
using System.Collections.Generic;

namespace StickerRobot.Models;

public partial class Payment
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int? AuditId { get; set; }

    public decimal? Amount { get; set; }

    public string? PaymentMethod { get; set; }

    public string? PaymentStatus { get; set; }

    public string? TransactionId { get; set; }

    public DateTime? PaidAt { get; set; }

    public virtual CashFlowAudit? Audit { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual RefundTransaction? RefundTransaction { get; set; }
}
