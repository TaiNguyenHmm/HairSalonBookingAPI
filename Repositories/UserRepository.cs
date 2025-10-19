
using Microsoft.EntityFrameworkCore;
using DataAccessObjects.Models; 


namespace Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly HairSalonBookingDbContext _context;
        public UserRepository(HairSalonBookingDbContext context) { _context = context; }
        public async Task<IEnumerable<User>> GetAllAsync() => await _context.Users.ToListAsync();
        public async Task<User> GetByIdAsync(int id) => await _context.Users.FindAsync(id);
        public async Task AddAsync(User user) { _context.Users.Add(user); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(User user) { _context.Users.Update(user); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id) { var user = await _context.Users.FindAsync(id); if (user != null) { _context.Users.Remove(user); await _context.SaveChangesAsync(); } }
    }
}
