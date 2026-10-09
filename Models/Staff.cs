using System;
using System.Collections.Generic;

namespace StickerRobot.Models;

public partial class Staff
{
    public int Id { get; set; }

    public int? AdminId { get; set; }

    public string? FullName { get; set; }

    public string? EmployeeCode { get; set; }

    public string? Phone { get; set; }

    public string? Position { get; set; }

    public bool? IsActive { get; set; }

    public virtual Admin? Admin { get; set; }

    public virtual ICollection<DesignTemplate> DesignTemplates { get; set; } = new List<DesignTemplate>();

    public virtual ICollection<IncidentReport> IncidentReports { get; set; } = new List<IncidentReport>();

    public virtual ICollection<StickerItem> StickerItems { get; set; } = new List<StickerItem>();
}
