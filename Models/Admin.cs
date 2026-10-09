using System;
using System.Collections.Generic;

namespace StickerRobot.Models;

public partial class Admin
{
    public int Id { get; set; }

    public string? Username { get; set; }

    public string? PasswordHash { get; set; }

    public string? Email { get; set; }

    public string? Role { get; set; }

    public virtual ICollection<CashFlowAudit> CashFlowAudits { get; set; } = new List<CashFlowAudit>();

    public virtual ICollection<RefundTransaction> RefundTransactions { get; set; } = new List<RefundTransaction>();

    public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();
}
