using BusinessObjects.Authentication;
using DataAccessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using WebAPI.Helpers; 

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly HairSalonBookingDbContext _context;
    public AuthController(HairSalonBookingDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest("Dữ liệu không hợp lệ!");
        var user = _context.Users.SingleOrDefault(u => u.Username.ToLower().Trim() == model.Username.ToLower().Trim());
        if (user == null)
            return Unauthorized("Sai tài khoản hoặc mật khẩu!");
        var hasher = new PasswordHasher<User>();
        var checkResult = hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
        if (checkResult == PasswordVerificationResult.Failed)
            return Unauthorized("Sai tài khoản hoặc mật khẩu!");
        // Quyết định thời hạn token theo RememberMe
        TimeSpan expiry = model.RememberMe ? TimeSpan.FromDays(7) : TimeSpan.FromHours(1);
        var token = JwtHelper.GenerateToken(user, _configuration, expiry);
        return Ok(new { token });
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Thông tin đăng ký không hợp lệ." });
        if (_context.Users.Any(u => u.Username.ToLower().Trim() == model.Username.ToLower().Trim()))
            return Conflict(new { message = "Tên đăng nhập đã tồn tại." });
        if (_context.Users.Any(u => u.Email.ToLower().Trim() == model.Email.ToLower().Trim()))
            return Conflict(new { message = "Email đã tồn tại." });
        var hasher = new PasswordHasher<User>();
        var user = new User
        {
            Username = model.Username.Trim(),
            Role = "Customer",
            Email = model.Email.Trim(),
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
        // Tìm user theo username và email (case-insensitive)
        var user = _context.Users.SingleOrDefault(u =>
            u.Username.ToLower().Trim() == model.Username.ToLower().Trim() &&
            u.Email.ToLower().Trim() == model.Email.ToLower().Trim());
        if (user == null)
            return BadRequest(new { message = "Tên đăng nhập hoặc email không đúng." });

        // Tạo mật khẩu mới ngẫu nhiên
        var newPassword = Guid.NewGuid().ToString().Substring(0, 8); // Ví dụ: 8 ký tự
        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, newPassword);
        _context.SaveChanges();

        // Gửi email (dùng SMTP hoặc dịch vụ email)
        await EmailHelper.SendEmailAsync(user.Email, "Mật khẩu mới HairSalon", $"Mật khẩu mới của bạn là: {newPassword}");

        return Ok(new { message = "Mật khẩu mới đã được gửi đến email của bạn." });
    }

}
