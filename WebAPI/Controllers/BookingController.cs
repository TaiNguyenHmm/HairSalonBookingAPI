using AutoMapper;
using BusinessObjects;
using DataAccessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using Repositories;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingRepository _repo;
        private readonly IMapper _mapper;

        public BookingController(IBookingRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
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
        public async Task<IActionResult> Create([FromBody] BookingDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var booking = _mapper.Map<Booking>(dto);
            // TODO: Kiểm tra trùng lịch, ca làm việc, v.v. ở đây nếu cần
            await _repo.AddAsync(booking);
            return CreatedAtAction(nameof(GetById), new { id = booking.Id }, _mapper.Map<BookingDto>(booking));
        }

        // PUT: api/Booking/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BookingDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var booking = await _repo.GetByIdAsync(id);
            if (booking == null) return NotFound();
            _mapper.Map(dto, booking);
            await _repo.UpdateAsync(booking);
            return NoContent();
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
    }
}
