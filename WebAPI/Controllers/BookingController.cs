using AutoMapper;
using BusinessObjects;
using DataAccessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingRepository _repo;
        private readonly IMapper _mapper;
        private readonly BookingService _bookingService;
        private readonly HairSalonBookingDbContext _context;

        public BookingController(IBookingRepository repo, IMapper mapper, BookingService bookingService, HairSalonBookingDbContext context)
        {
            _repo = repo;
            _mapper = mapper;
            _bookingService = bookingService;
            _context = context;
        }

        // ===========================================
        // GET: api/Booking  → tất cả booking (admin)
        // ===========================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Stylist)
                .ThenInclude(s => s.User)
                .Include(b => b.Service)
                .ToListAsync();

            var dtos = bookings.Select(b => new BookingDto
            {
                Id = b.Id,
                CustomerName = b.Customer.Username,
                StylistName = b.Stylist.FullName,
                ServiceName = b.Service.Name,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                Status = b.Status
            });

            return Ok(dtos);
        }
        // ===========================================
        // GET: api/Booking/{id}
        // ===========================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var booking = await _repo.GetByIdAsync(id);
            if (booking == null) return NotFound();
            return Ok(_mapper.Map<BookingDto>(booking));
        }

        // ===========================================
        // ✅ GET: api/Booking/my  → Lịch hẹn của tôi
        // ===========================================
        [HttpGet("MyBookings")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized("Không xác định được người dùng từ token.");

            int userId = int.Parse(userIdStr);

            // Lấy tất cả booking của user
            var bookings = await _context.Bookings
                .Include(b => b.Service)
                .Include(b => b.Stylist)
                .ThenInclude(s => s.User)
                .Where(b => b.CustomerId == userId)
                .OrderByDescending(b => b.StartTime)
                .ToListAsync(); // <-- load vào memory

            // Lấy danh sách bookingId mà user đã review
            var reviewedBookingIds = await _context.Reviews
                .Where(r => r.CustomerId == userId)
                .Select(r => r.BookingId)
                .ToListAsync();

            var result = bookings.Select(b => new
            {
                b.Id,
                ServiceName = b.Service.Name,
                StylistName = b.Stylist.FullName,
                b.StartTime,
                b.EndTime,
                b.Status,
                b.Notes,
                IsReviewed = reviewedBookingIds.Contains(b.Id)
            });

            return Ok(result);
        }




        // ===========================================
        // POST: api/Booking → Tạo booking
        // ===========================================
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create([FromBody] BookingDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 1️⃣ Lấy service để tính duration
            var service = await _bookingService.GetServiceById(dto.ServiceId);
            if (service == null) return BadRequest("Service không tồn tại");

            // 2️⃣ Tính EndTime dựa trên StartTime + DurationMinutes
            var endTime = dto.StartTime.AddMinutes(service.DurationMinutes);

            // 3️⃣ Lấy CustomerId từ token
            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (customerIdClaim == null)
                return Unauthorized("Không xác định được user");

            int customerId = int.Parse(customerIdClaim.Value);

            // 4️⃣ Tạo entity Booking
            var booking = new Booking
            {
                StylistId = dto.StylistId,
                ServiceId = dto.ServiceId,
                StartTime = dto.StartTime,
                EndTime = endTime,
                CustomerId = customerId,
                Status = "Pending"
            };

            await _repo.AddAsync(booking);
            var resultDto = _mapper.Map<BookingDto>(booking);

            return CreatedAtAction(nameof(GetById), new { id = booking.Id }, resultDto);
        }

        // ===========================================
        // DELETE: api/Booking/{id}
        // ===========================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _repo.GetByIdAsync(id);
            if (booking == null) return NotFound();

            await _repo.DeleteAsync(id);
            return NoContent();
        }

        // ===========================================
        // GET: api/Booking/{stylistId}/available-times
        // ===========================================
        [HttpGet("{stylistId}/available-times")]
        public async Task<IActionResult> GetAvailableTimes(int stylistId, [FromQuery] int serviceId, [FromQuery] string date)
        {
            if (!DateTime.TryParse(date, out var bookingDate))
                return BadRequest("Ngày không hợp lệ");

            var slots = await _bookingService.GetAvailableTimes(stylistId, serviceId, bookingDate);
            return Ok(slots);
        }

        [HttpPost("Review")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateReview([FromBody] ReviewDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized();

            int userId = int.Parse(userIdStr);

            // Kiểm tra booking có tồn tại và thuộc user
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == dto.BookingId && b.CustomerId == userId);
            if (booking == null) return BadRequest("Booking không hợp lệ");

            // Kiểm tra đã review chưa
            if (await _context.Reviews.AnyAsync(r => r.BookingId == dto.BookingId && r.CustomerId == userId))
                return BadRequest("Bạn đã đánh giá lịch hẹn này rồi");

            var review = new Review
            {
                BookingId = dto.BookingId,
                CustomerId = userId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Trả về DTO thay vì entity để tránh vòng lặp
            var resultDto = new ReviewDto
            {
                BookingId = review.BookingId,
                Rating = review.Rating,
                Comment = review.Comment
            };

            return Ok(resultDto);
        }

        [HttpPut("{id}/cancel")]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var booking = await _repo.GetByIdAsync(id);
            if (booking == null) return NotFound();

            // Chỉ cho phép customer hủy booking của họ
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdStr))
            {
                int userId = int.Parse(userIdStr);
                if (booking.CustomerId != userId && !User.IsInRole("Admin"))
                    return Forbid();
            }

            booking.Status = "Cancelled";
            await _repo.UpdateAsync(booking);

            return Ok(new { message = "Đã hủy booking" });
        }
        [HttpPut("{id}/confirm")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmBooking(int id)
        {
            var booking = await _repo.GetByIdAsync(id);
            if (booking == null) return NotFound();

            booking.Status = "Confirmed";
            await _repo.UpdateAsync(booking);

            return Ok(new { message = "Booking đã được confirm" });
        }




    }
}
