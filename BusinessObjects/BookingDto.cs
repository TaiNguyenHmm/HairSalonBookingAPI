using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class BookingDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public int StylistId { get; set; }
        public int ServiceId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string ServiceName { get; set; } 
        public string StylistName { get; set; } 

        public string Status { get; set; } = "";
        public string? Notes { get; set; }
     
    }


}
