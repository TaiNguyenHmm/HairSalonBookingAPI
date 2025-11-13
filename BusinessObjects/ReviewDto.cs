using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class ReviewDto
    {
        public int BookingId { get; set; }
        public int Rating { get; set; } // 1-5
        public string Comment { get; set; }
        public bool IsHidden { get; set; } = false;
    }

}
