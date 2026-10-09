using System;
using System.Collections.Generic;

namespace StickerRobot.Models;

public partial class PhoneModel
{
    public int Id { get; set; }

    public string? Brand { get; set; }

    public string? ModelName { get; set; }

    public string? Dimensions { get; set; }

    public virtual ICollection<PhoneCase> PhoneCases { get; set; } = new List<PhoneCase>();
}
