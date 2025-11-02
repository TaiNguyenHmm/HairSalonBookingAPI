using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using System.Security.Claims;

public class LoginModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public LoginModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public string Username { get; set; }
    [BindProperty]
    public string Password { get; set; }

    public string ErrorMessage { get; set; }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        var client = _httpClientFactory.CreateClient("api");

        var res = await client.PostAsJsonAsync("api/auth/login", new
        {
            username = Username,
            password = Password
        });

        if (!res.IsSuccessStatusCode)
        {
            ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng!";
            return Page();
        }

        var data = await res.Content.ReadFromJsonAsync<LoginResponseDto>();

        // Tạo Claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, data.Username),
            new Claim(ClaimTypes.Role, data.Role),
            new Claim("Token", data.Token)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true });

        if (string.IsNullOrEmpty(returnUrl))
        {
            returnUrl = data.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                ? "/AdminDashboard/Dashboard"
                : "/ServicesCustomer/Services";
        }

        return LocalRedirect(returnUrl);
    }

    public class LoginResponseDto
    {
        public string Token { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
    }
}
