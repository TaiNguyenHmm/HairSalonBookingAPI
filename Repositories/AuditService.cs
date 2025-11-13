using DataAccessObjects.Models;
using System;
using System.Threading.Tasks;

namespace Repositories
{
    public class AuditService : IAuditService
    {
        private readonly HairSalonBookingDbContext _context;

        public AuditService(HairSalonBookingDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(int? userId, string action, string entityName, int? entityId, string details)
        {
            var log = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
