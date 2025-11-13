using DataAccessObjects.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly HairSalonBookingDbContext _context;

        public NotificationRepository(HairSalonBookingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Notification>> GetAllAsync()
        {
            return await _context.Notifications.Include(n => n.Booking).ToListAsync();
        }


        public async Task<Notification> GetByIdAsync(int id)
        {
            return await _context.Notifications.FindAsync(id);
        }

        public async Task AddAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Notification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var n = await _context.Notifications.FindAsync(id);
            if (n != null)
            {
                _context.Notifications.Remove(n);
                await _context.SaveChangesAsync();
            }
        }
    }
}

