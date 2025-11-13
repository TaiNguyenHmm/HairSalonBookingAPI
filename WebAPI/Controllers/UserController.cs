using AutoMapper;
using BusinessObjects;
using DataAccessObjects.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Repositories;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _repo;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher<User> _hasher;


        public UserController(IUserRepository repo, IMapper mapper, IPasswordHasher<User> hasher)
        {
            _repo = repo;
            _mapper = mapper;
            _hasher = hasher;

        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var users = await _repo.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<UserDto>>(users);
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(_mapper.Map<UserDto>(user));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = _mapper.Map<User>(dto);
            user.CreatedAt = DateTime.Now;

            // Sử dụng Identity PasswordHasher
            if (!string.IsNullOrEmpty(dto.Password))
            {
                user.PasswordHash = _hasher.HashPassword(user, dto.Password);
            }

            await _repo.AddAsync(user);

            // Trả về UserDto, không trả password
            var result = _mapper.Map<UserDto>(user);
            result.Password = null;
            return CreatedAtAction(nameof(Get), new { id = user.Id }, result);
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] AdminUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _repo.GetByIdAsync(id);
            if (user == null)
                return NotFound();
            user.Username = dto.Username;
            user.Email = dto.Email;
            user.Phone = dto.Phone;
            user.Role = dto.Role;
            user.UpdatedAt = DateTime.Now;

            await _repo.UpdateAsync(user);
            return NoContent();
        }
    }
}
