using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DataAccessObjects.Models;
using Repositories;
using BusinessObjects;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Customer,Stylist,Admin")] 
    public class ProfileController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _hasher;

        public ProfileController(IUserRepository userRepository, IPasswordHasher<User> hasher)
        {
            _userRepository = userRepository;
            _hasher = hasher;
        }

        // =========================
        // LẤY THÔNG TIN NGƯỜI DÙNG
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { message = "Không xác định được người dùng." });

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "Người dùng không tồn tại." });

            return Ok(new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email,
                phone = user.Phone,
                role = user.Role
            });
        }

        // =========================
        // CẬP NHẬT THÔNG TIN
        // =========================
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ." });

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { message = "Không xác định được người dùng." });

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "Người dùng không tồn tại." });

            // Cập nhật thông tin
            user.Username = string.IsNullOrWhiteSpace(dto.Username) ? user.Username : dto.Username;
            user.Email = string.IsNullOrWhiteSpace(dto.Email) ? user.Email : dto.Email;
            user.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? user.Phone : dto.Phone;

            await _userRepository.UpdateAsync(user);
            return Ok(new { message = "Cập nhật thông tin thành công." });
        }

        // =========================
        // ĐỔI MẬT KHẨU
        // =========================
        [HttpPut("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ." });

            if (dto.NewPassword != dto.ConfirmNewPassword)
                return BadRequest(new { message = "Mật khẩu mới và xác nhận mật khẩu không khớp." });

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { message = "Không xác định được người dùng." });

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "Người dùng không tồn tại." });

            // Kiểm tra mật khẩu hiện tại
            var check = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.CurrentPassword);
            if (check == PasswordVerificationResult.Failed)
                return BadRequest(new { message = "Mật khẩu hiện tại không chính xác." });

            // Mã hoá mật khẩu mới bằng IPasswordHasher<User>
            user.PasswordHash = _hasher.HashPassword(user, dto.NewPassword);
            await _userRepository.UpdateAsync(user);

            return Ok(new { message = "Đổi mật khẩu thành công." });
        }
    }
}
