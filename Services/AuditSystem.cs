using DataAccessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services
{
    /// <summary>
    /// Interceptor tự động ghi AuditLog cho CRUD
    /// Không cần reference Microsoft.AspNetCore trong project này
    /// UserId được truyền qua callback từ WebAPI project
    /// </summary>
    public class AuditSystem : SaveChangesInterceptor
    {
        private readonly Func<int?> _getCurrentUserId;

        public AuditSystem(Func<int?> getCurrentUserId)
        {
            _getCurrentUserId = getCurrentUserId;
        }

        private int? GetCurrentUserId() => _getCurrentUserId();

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            AddAuditLogs(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            AddAuditLogs(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void AddAuditLogs(DbContext? context)
        {
            if (context == null) return;

            // Copy ra List để tránh lỗi "Collection was modified"
            var entries = context.ChangeTracker.Entries()
                .Where(e => e.Entity is not AuditLog &&
                            (e.State == EntityState.Added ||
                             e.State == EntityState.Modified ||
                             e.State == EntityState.Deleted))
                .ToList();

            foreach (var entry in entries)
            {
                var action = entry.State switch
                {
                    EntityState.Added => "CREATE",
                    EntityState.Modified => "UPDATE",
                    EntityState.Deleted => "DELETE",
                    _ => "UNKNOWN"
                };

                var entityName = entry.Entity.GetType().Name;
                var entityId = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue as int?;

                context.Add(new AuditLog
                {
                    UserId = GetCurrentUserId(),
                    Action = action,
                    EntityName = entityName,
                    EntityId = entityId,
                    Details = $"Thực hiện hành động {action} trên {entityName} (ID: {entityId ?? 0})",
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
