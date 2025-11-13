using System;
using System.Collections.Generic;

namespace DataAccessObjects.Models;

public partial class Review
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public int CustomerId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsHidden { get; set; } = false;

    public virtual Booking Booking { get; set; } = null!;

    public virtual User Customer { get; set; } = null!;
}
