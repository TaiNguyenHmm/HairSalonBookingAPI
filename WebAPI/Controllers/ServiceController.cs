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
        private readonly IStylistRepository _repo;
        private readonly IServiceRepository _serviceRepo;
        private readonly IMapper _mapper;

        public ServiceController(IStylistRepository repo, IServiceRepository serviceRepo, IMapper mapper)
        {
            _repo = repo;
            _serviceRepo = serviceRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var services = await _serviceRepo.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<ServiceDto>>(services);
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var service = await _serviceRepo.GetByIdAsync(id);
            if (service == null) return NotFound();
            return Ok(_mapper.Map<ServiceDto>(service));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ServiceDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var service = _mapper.Map<Service>(dto);
            await _serviceRepo.AddAsync(service);
            return CreatedAtAction(nameof(GetById), new { id = service.Id }, _mapper.Map<ServiceDto>(service));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ServiceDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var service = await _serviceRepo.GetByIdAsync(id);
            if (service == null) return NotFound();
            _mapper.Map(dto, service);
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
