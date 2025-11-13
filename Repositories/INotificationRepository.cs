using DataAccessObjects.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetAllAsync();
        Task<Notification> GetByIdAsync(int id);
        Task UpdateAsync(Notification notification);
        Task AddAsync(Notification notification);
        Task DeleteAsync(int id);
    }
}

