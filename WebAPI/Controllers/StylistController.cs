using AutoMapper;
using BusinessObjects;
using DataAccessObjects.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StylistController : ControllerBase
    {
        private readonly IStylistRepository _repo;
        private readonly IMapper _mapper;
        private readonly HairSalonBookingDbContext _context;

        public StylistController(IStylistRepository repo, IMapper mapper, HairSalonBookingDbContext context)
        {
            _repo = repo;
            _mapper = mapper;
            _context = context;
        }


        // ==================== CRUD ====================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var stylists = await _repo.GetAllAsync(); 
            var dtos = _mapper.Map<IEnumerable<StylistDto>>(stylists);
            return Ok(dtos);
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var stylist = await _repo.GetByIdAsync(id);
            if (stylist == null) return NotFound();
            return Ok(_mapper.Map<StylistDto>(stylist));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StylistDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var stylist = _mapper.Map<Stylist>(dto);
            await _repo.AddAsync(stylist);
            return CreatedAtAction(nameof(GetById), new { id = stylist.Id }, _mapper.Map<StylistDto>(stylist));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] StylistDto dto)
        {
            // Controller
            var stylist = await _repo.GetByIdAsync(id);
            if (stylist == null) return NotFound();
            stylist.FullName = dto.FullName;
            stylist.Status = dto.Status;
            stylist.Bio = dto.Bio;
            stylist.UserId = dto.UserId;
            if (stylist.User != null) stylist.User.Email = dto.Email;
            await _repo.UpdateAsync(stylist);


            return NoContent();
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _repo.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("create-with-user")]
        public async Task<IActionResult> CreateWithUser([FromBody] CreateStylistDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (_context.Users.Any(u => u.Username.ToLower() == dto.Username.ToLower()))
                return Conflict(new { message = "Username đã tồn tại." });

            var hasher = new PasswordHasher<User>();
            var user = new User
            {
                Username = dto.Username.Trim(),
                Email = dto.Email?.Trim(),
                Phone = dto.Phone?.Trim(),
                Role = "Stylist",
                CreatedAt = DateTime.UtcNow
            };
            user.PasswordHash = hasher.HashPassword(user, dto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var stylist = new Stylist
            {
                UserId = user.Id,
                FullName = dto.FullName,
                Bio = dto.Bio
            };

            _context.Stylists.Add(stylist);
            await _context.SaveChangesAsync();

            var resultDto = new StylistDto
            {
                Id = stylist.Id,
                UserId = user.Id,
                FullName = stylist.FullName,
                Bio = stylist.Bio,
                Email = user.Email,
                Phone = user.Phone,
                Status = "Active"
            };

            return CreatedAtAction(nameof(GetById), new { id = stylist.Id }, resultDto);
        }

        [HttpGet("{stylistId}/working-hours")]
        public async Task<IActionResult> GetWorkingHours(int stylistId)
        {
            var hours = await _context.StylistWorkingHours
                .Where(h => h.StylistId == stylistId)
                .Select(h => new {
                    h.Id,
                    h.DayOfWeek,
                    h.StartTime,
                    h.EndTime
                })
                .ToListAsync();

            return Ok(hours);
        }
        [HttpPost("{stylistId}/working-hours")]
        public async Task<IActionResult> SaveWorkingHours(int stylistId, [FromBody] List<StylistWorkingHoursDto> hoursDto)
        {
            if (hoursDto == null || !hoursDto.Any())
                return BadRequest(new { message = "Danh sách giờ làm việc trống." });

            // Kiểm tra Stylist có tồn tại không
            var stylist = await _context.Stylists.FindAsync(stylistId);
            if (stylist == null)
                return NotFound(new { message = "Stylist không tồn tại." });

            // Xóa giờ làm việc cũ
            var oldHours = _context.StylistWorkingHours.Where(h => h.StylistId == stylistId);
            _context.StylistWorkingHours.RemoveRange(oldHours);

            // Thêm giờ làm việc mới từ DTO
            foreach (var dto in hoursDto)
            {
                var entity = new StylistWorkingHour
                {
                    StylistId = stylistId,
                    DayOfWeek = dto.DayOfWeek,
                    StartTime = TimeOnly.Parse(dto.StartTime),
                    EndTime = TimeOnly.Parse(dto.EndTime),
                    CreatedAt = DateTime.UtcNow
                };

                // KHÔNG gán Stylist = null!  → để EF tự map qua StylistId
                _context.StylistWorkingHours.Add(entity);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Lưu giờ làm việc thành công." });
        }










    }
}
