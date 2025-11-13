using Microsoft.EntityFrameworkCore;
using DataAccessObjects.Models;

namespace Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly HairSalonBookingDbContext _context;
        public UserRepository(HairSalonBookingDbContext context) { _context = context; }

        public async Task<IEnumerable<User>> GetAllAsync()
            => await _context.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();

        public async Task<User> GetByIdAsync(int id)
            => await _context.Users.FindAsync(id);

        public async Task AddAsync(User user)
        {
            user.CreatedAt = DateTime.Now;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            user.UpdatedAt = DateTime.Now;
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        // ❌ Bỏ Delete vì không xoá user thực sự
        public async Task DisableUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                // Ví dụ bạn có thể dùng IsActive để ẩn user nếu muốn
                // user.IsActive = false;
                user.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}
