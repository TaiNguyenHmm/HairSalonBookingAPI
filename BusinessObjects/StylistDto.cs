using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class StylistDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string? Bio { get; set; }
        public string Status { get; set; } = "Active";
        public string? Email { get; set; }
        public string Phone { get; set; }
        public string Username { get; set; } = null!;



    }

}
