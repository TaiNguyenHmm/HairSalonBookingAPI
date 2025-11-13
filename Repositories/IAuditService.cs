using System.Threading.Tasks;

namespace Repositories
{
    public interface IAuditService
    {
        Task LogAsync(int? userId, string action, string entityName, int? entityId, string details);
    }
}
