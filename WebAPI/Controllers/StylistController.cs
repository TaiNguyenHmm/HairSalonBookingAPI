using AutoMapper;
using BusinessObjects;
using DataAccessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using Repositories;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StylistController : ControllerBase
    {
        private readonly IStylistRepository _repo;
        private readonly IMapper _mapper;

        public StylistController(IStylistRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

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
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var stylist = await _repo.GetByIdAsync(id);
            if (stylist == null) return NotFound();
            _mapper.Map(dto, stylist);
            await _repo.UpdateAsync(stylist);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var stylist = await _repo.GetByIdAsync(id);
            if (stylist == null) return NotFound();
            await _repo.DeleteAsync(id);
            return NoContent();
        }
    }
}
