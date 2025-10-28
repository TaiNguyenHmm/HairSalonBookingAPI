using AutoMapper;
using BusinessObjects;
using DataAccessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using Services;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class BookingController : ControllerBase
    {
        private readonly IBookingRepository _repo;
        private readonly IMapper _mapper;
        private readonly BookingService _bookingService;

        public BookingController(IBookingRepository repo, IMapper mapper, BookingService bookingService)
        {
            _repo = repo;
            _mapper = mapper;
            _bookingService = bookingService;

        }

        // GET: api/Booking
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var bookings = await _repo.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<BookingDto>>(bookings);
            return Ok(dtos);
        }

        // GET: api/Booking/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var booking = await _repo.GetByIdAsync(id);
            if (booking == null) return NotFound();
            return Ok(_mapper.Map<BookingDto>(booking));
        }

        // POST: api/Booking
        [HttpPost]
        [Authorize] // Chỉ POST yêu cầu login

        public async Task<IActionResult> Create([FromBody] BookingDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 1️⃣ Lấy service để tính duration
            var service = await _bookingService.GetServiceById(dto.ServiceId);
            if (service == null) return BadRequest("Service không tồn tại");

            // 2️⃣ Tính EndTime dựa trên StartTime + DurationMinutes
            var endTime = dto.StartTime.AddMinutes(service.DurationMinutes);

            // 3️⃣ Lấy CustomerId từ token
            var customerIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (customerIdClaim == null)
                return Unauthorized("Không xác định được user");

            int customerId = int.Parse(customerIdClaim.Value); // ✅ sử dụng claim NameIdentifier

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




        // DELETE: api/Booking/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _repo.GetByIdAsync(id);
            if (booking == null) return NotFound();
            await _repo.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("{stylistId}/available-times")]
        public async Task<IActionResult> GetAvailableTimes(int stylistId, [FromQuery] int serviceId, [FromQuery] string date)
        {
            if (!DateTime.TryParse(date, out var bookingDate))
                return BadRequest("Ngày không hợp lệ");

            var slots = await _bookingService.GetAvailableTimes(stylistId, serviceId, bookingDate);
            return Ok(slots);
        }
    }
}
