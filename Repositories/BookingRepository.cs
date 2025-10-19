using DataAccessObjects.Models;

using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly HairSalonBookingDbContext _context;

        public BookingRepository(HairSalonBookingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Booking>> GetAllAsync()
        {
            return await _context.Bookings
                .Include(b => b.Service)
                .Include(b => b.Stylist)
                .Include(b => b.Customer)
                .ToListAsync();
        }

        public async Task<Booking?> GetByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.Service)
                .Include(b => b.Stylist)
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task AddAsync(Booking booking)
        {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Booking booking)
        {
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Booking>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Bookings
                .Where(b => b.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetByStylistIdAsync(int stylistId)
        {
            return await _context.Bookings
                .Where(b => b.StylistId == stylistId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetByStatusAsync(string status)
        {
            return await _context.Bookings
                .Where(b => b.Status == status)
                .ToListAsync();
        }
    }
}
