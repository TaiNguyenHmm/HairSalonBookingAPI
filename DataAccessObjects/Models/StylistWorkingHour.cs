using System;
using System.Collections.Generic;

namespace DataAccessObjects.Models;

public partial class StylistWorkingHour
{
    public int Id { get; set; }

    public int StylistId { get; set; }

    public int DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Stylist Stylist { get; set; } = null!;
}
