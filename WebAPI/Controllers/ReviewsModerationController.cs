using DataAccessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // <--- quan trọng
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsModerationController : ControllerBase
    {
        private readonly HairSalonBookingDbContext _context;

        public ReviewsModerationController(HairSalonBookingDbContext context)
        {
            _context = context;
        }

        // Lấy danh sách review (cả ẩn và hiển thị)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reviews = await _context.Reviews
                .Include(r => r.Customer)
                .Include(r => r.Booking)
                .ThenInclude(b => b.Stylist)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.Id,
                    r.Rating,
                    r.Comment,
                    CustomerName = r.Customer.Username,
                    BookingInfo = $"{r.Booking.Service.Name} ({r.Booking.Stylist.FullName})",
                    r.IsHidden,
                    r.UpdatedAt
                })
                .ToListAsync();

            return Ok(reviews);
        }

        // Ẩn / hiện review
        [HttpPut("{id}/toggle-hide")]
        public async Task<IActionResult> ToggleHide(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            review.IsHidden = !review.IsHidden;
            review.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                review.Id,
                review.IsHidden,
                review.UpdatedAt
            });
        }
    }
}
