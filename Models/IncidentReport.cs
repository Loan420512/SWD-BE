using System;
using System.Collections.Generic;

namespace StickerRobot.Models;

public partial class IncidentReport
{
    public int Id { get; set; }

    public int? StaffId { get; set; }

    public string? IncidentType { get; set; }

    public string? Description { get; set; }

    public string? Status { get; set; }

    public DateTime? ReportedAt { get; set; }

    public virtual Staff? Staff { get; set; }
}
