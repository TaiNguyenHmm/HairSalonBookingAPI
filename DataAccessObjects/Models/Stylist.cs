using System;
using System.Collections.Generic;

namespace DataAccessObjects.Models;

public partial class Stylist
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? FullName { get; set; }

    public string? Bio { get; set; }
    public string Status { get; set; } = "Active"; 

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<StylistWorkingHour> StylistWorkingHours { get; set; } = new List<StylistWorkingHour>();

    public virtual User User { get; set; } = null!;
}
