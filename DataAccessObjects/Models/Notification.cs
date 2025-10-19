using System;
using System.Collections.Generic;

namespace DataAccessObjects.Models;

public partial class Notification
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public string NotificationType { get; set; } = null!;

    public DateTime? SentAt { get; set; }

    public bool IsSent { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;
}
