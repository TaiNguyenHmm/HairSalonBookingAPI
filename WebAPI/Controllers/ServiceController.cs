using AutoMapper;
using BusinessObjects;
using DataAccessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using Repositories;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceController : ControllerBase
    {
        private readonly IServiceRepository _serviceRepo;
        private readonly IMapper _mapper;

        public ServiceController(IServiceRepository serviceRepo, IMapper mapper)
        {
            _serviceRepo = serviceRepo;
            _mapper = mapper;
        }

        // Lấy toàn bộ dịch vụ
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var services = await _serviceRepo.GetAllAsync();
            var dtos = services.Select(s => new
            {
                s.Id,
                s.Name,
                s.Description,
                s.Price,
                s.DurationMinutes,
                CreatedAt = s.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                UpdatedAt = s.UpdatedAt.HasValue ? s.UpdatedAt.Value.ToString("dd/MM/yyyy HH:mm") : "Chưa cập nhật"
            });
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var service = await _serviceRepo.GetByIdAsync(id);
            if (service == null) return NotFound();

            return Ok(new
            {
                service.Id,
                service.Name,
                service.Description,
                service.Price,
                service.DurationMinutes,
                CreatedAt = service.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                UpdatedAt = service.UpdatedAt.HasValue ? service.UpdatedAt.Value.ToString("dd/MM/yyyy HH:mm") : "Chưa cập nhật"
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ServiceDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var service = _mapper.Map<Service>(dto);
            service.CreatedAt = DateTime.Now;
            await _serviceRepo.AddAsync(service);

            return CreatedAtAction(nameof(GetById), new { id = service.Id }, _mapper.Map<ServiceDto>(service));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ServiceDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var service = _mapper.Map<Service>(dto);
            service.Id = id;

            await _serviceRepo.UpdateAsync(service);

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _serviceRepo.GetByIdAsync(id);
            if (service == null) return NotFound();
            await _serviceRepo.DeleteAsync(id);
            return NoContent();
        }
    }
}

