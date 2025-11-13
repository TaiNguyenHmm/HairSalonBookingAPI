using DataAccessObjects.Models; // namespace của DbContext
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [Authorize(Roles = "Admin")]

    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly HairSalonBookingDbContext _context;

        public DashboardController(HairSalonBookingDbContext context)
        {
            _context = context;
        }
        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview()
        {
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // Tổng số booking hôm nay
            var bookingsToday = await _context.Bookings
                .CountAsync(b => b.CreatedAt.Date == today);

            // Doanh thu hôm nay (theo service)
            var revenueToday = await _context.Bookings
                .Include(b => b.Service)
                .Where(b => b.CreatedAt.Date == today && b.Service != null)
                .SumAsync(b => (decimal?)b.Service.Price ?? 0);

            // Rating trung bình
            var avgRating = await _context.Reviews.AnyAsync()
                ? await _context.Reviews.AverageAsync(r => (double)r.Rating)
                : 0;

            // Tổng booking pending
            var pendingBookings = await _context.Bookings
                .CountAsync(b => b.Status == "Pending");
            var reviewsToday = await _context.Reviews.CountAsync(r => r.CreatedAt.Date == today);
            // Doanh thu theo dịch vụ trong tháng (chỉ tính booking Completed)
            var serviceRevenue = await _context.Bookings
                .Include(b => b.Service)
                .Where(b =>
                    b.Status == "Completed" &&
                    b.CreatedAt.Date >= firstDayOfMonth &&
                    b.CreatedAt.Date <= lastDayOfMonth &&
                    b.Service != null)
                .GroupBy(b => new { b.ServiceId, b.Service.Name })
                .Select(g => new
                {
                    name = g.Key.Name,
                    total = g.Sum(x => (decimal?)x.Service.Price ?? 0)
                })
                .OrderByDescending(x => x.total)
                .ToListAsync();

            return Ok(new
            {
                bookingsToday,
                revenueToday,
                avgRating = Math.Round(avgRating, 1),
                pendingBookings,
                reviewsToday,
                serviceRevenue
            });
        }


    }
}

