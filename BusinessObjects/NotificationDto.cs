using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class NotificationDto
    {
        public int Id { get; set; }

        public int BookingId { get; set; }

        public string NotificationType { get; set; } = null!;

        public DateTime? SentAt { get; set; }

        public bool IsSent { get; set; }

        public DateTime CreatedAt { get; set; }
        public int DaysBeforeBooking { get; set; } = 1;


    }

}
