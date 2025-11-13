using DataAccessObjects.Models;
using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public partial class AuditLog
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string Action { get; set; } = null!;
        public string EntityName { get; set; } = null!;
        public int? EntityId { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Details { get; set; }

        public virtual User? User { get; set; }
    }

}
