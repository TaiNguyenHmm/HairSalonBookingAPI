using DataAccessObjects.Models;
using Microsoft.EntityFrameworkCore;
namespace Repositories
{
    public class StylistRepository : IStylistRepository
    {
        private readonly HairSalonBookingDbContext _context;

        public StylistRepository(HairSalonBookingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Stylist>> GetAllAsync()
        {
            return await _context.Stylists.ToListAsync();
        }

        public async Task<Stylist?> GetByIdAsync(int id)
        {
            return await _context.Stylists.FindAsync(id);
        }

        public async Task AddAsync(Stylist stylist)
        {
            _context.Stylists.Add(stylist);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Stylist stylist)
        {
            _context.Stylists.Update(stylist);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var stylist = await _context.Stylists.FindAsync(id);
            if (stylist != null)
            {
                _context.Stylists.Remove(stylist);
                await _context.SaveChangesAsync();
            }
        }
    }
}
