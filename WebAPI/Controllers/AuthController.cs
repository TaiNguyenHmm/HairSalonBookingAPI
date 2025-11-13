using BusinessObjects.Authentication;
using DataAccessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Helpers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly HairSalonBookingDbContext _context;
    private readonly IAuditService _auditService;

    public AuthController(HairSalonBookingDbContext context, IConfiguration configuration, IAuditService auditService)
    {
        _context = context;
        _configuration = configuration;
        _auditService = auditService;
    }


    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        var user = _context.Users.SingleOrDefault(u =>
            u.Username.ToLower().Trim() == model.Username.ToLower().Trim());

        if (user == null)
            return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu!" });

        var hasher = new PasswordHasher<User>();
        if (hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password)
            == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu!" });

        // Sinh token
        var token = JwtHelper.GenerateToken(user, _configuration, TimeSpan.FromHours(9));

        // Ghi Audit log login
        await _auditService.LogAsync(user.Id, "LOGIN", "User", user.Id, $"User {user.Username} đã đăng nhập");

        return Ok(new
        {
            token,
            username = user.Username,
            role = user.Role
        });
    }


    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Thông tin đăng ký không hợp lệ." });

        if (_context.Users.Any(u => u.Username.ToLower().Trim() == model.Username.ToLower().Trim()))
            return Conflict(new { message = "Tên đăng nhập đã tồn tại." });

        if (!string.IsNullOrWhiteSpace(model.Email) &&
            _context.Users.Any(u => u.Email.ToLower().Trim() == model.Email.ToLower().Trim()))
            return Conflict(new { message = "Email đã tồn tại." });

        var hasher = new PasswordHasher<User>();
        var user = new User
        {
            Username = model.Username.Trim(),
            Role = "Customer",
            Email = model.Email?.Trim(),
            Phone = model.Phone?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = hasher.HashPassword(user, model.Password);

        _context.Users.Add(user);
        _context.SaveChanges();

        return StatusCode(201, new { message = "Đăng ký thành công!" });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Dữ liệu không hợp lệ." });

        var user = _context.Users.SingleOrDefault(u =>
            u.Username.ToLower().Trim() == model.Username.ToLower().Trim() &&
            u.Email.ToLower().Trim() == model.Email.ToLower().Trim());

        if (user == null)
            return BadRequest(new { message = "Tên đăng nhập hoặc email không đúng." });

        // Tạo mật khẩu mới ngẫu nhiên
        var newPassword = Guid.NewGuid().ToString("N")[..8];
        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, newPassword);
        _context.SaveChanges();

        await EmailHelper.SendEmailAsync(user.Email,
            "Mật khẩu mới HairSalon",
            $"Mật khẩu mới của bạn là: {newPassword}");

        return Ok(new { message = "Mật khẩu mới đã được gửi đến email của bạn." });
    }



}
