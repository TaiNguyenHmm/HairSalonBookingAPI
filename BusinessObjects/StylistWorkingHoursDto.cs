using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class StylistWorkingHoursDto
    {
        public int Id { get; set; }
        public int StylistId { get; set; }
        public int DayOfWeek { get; set; }
        public string StartTime { get; set; } // HH:mm
        public string EndTime { get; set; }   // HH:mm
    }
}
