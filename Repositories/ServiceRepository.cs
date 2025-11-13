using DataAccessObjects.Models;

using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly HairSalonBookingDbContext _context;

        public ServiceRepository(HairSalonBookingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Service>> GetAllAsync()
        {
            return await _context.Services.ToListAsync();
        }

        public async Task<Service?> GetByIdAsync(int id)
        {
            return await _context.Services.FindAsync(id);
        }

        public async Task AddAsync(Service service)
        {
            _context.Services.Add(service);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Service service)
        {
            // Tìm entity hiện có trong DB
            var existing = await _context.Services.AsTracking().FirstOrDefaultAsync(s => s.Id == service.Id);
            if (existing == null)
                throw new Exception($"Service with ID {service.Id} not found.");

            // Cập nhật các trường cần thiết
            existing.Name = service.Name;
            existing.Description = service.Description;
            existing.Price = service.Price;
            existing.DurationMinutes = service.DurationMinutes;

            // Cập nhật thời gian sửa đổi
            existing.UpdatedAt = DateTime.Now;

            // Giữ nguyên CreatedAt (không cho EF overwrite)
            _context.Entry(existing).Property(x => x.CreatedAt).IsModified = false;

            // Ghi lại thay đổi
            await _context.SaveChangesAsync();
        }



        public async Task DeleteAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }
        }
    }
}
