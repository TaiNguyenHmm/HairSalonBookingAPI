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
        private readonly IServiceRepository _repo;
        private readonly IMapper _mapper;

        public ServiceController(IServiceRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var services = await _repo.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<ServiceDto>>(services);
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var service = await _repo.GetByIdAsync(id);
            if (service == null) return NotFound();
            return Ok(_mapper.Map<ServiceDto>(service));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ServiceDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var service = _mapper.Map<Service>(dto);
            await _repo.AddAsync(service);
            return CreatedAtAction(nameof(GetById), new { id = service.Id }, _mapper.Map<ServiceDto>(service));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ServiceDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var service = await _repo.GetByIdAsync(id);
            if (service == null) return NotFound();
            _mapper.Map(dto, service);
            await _repo.UpdateAsync(service);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _repo.GetByIdAsync(id);
            if (service == null) return NotFound();
            await _repo.DeleteAsync(id);
            return NoContent();
        }
    }
}
