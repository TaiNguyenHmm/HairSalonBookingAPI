using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeFolder("/AdminDashboard", "AdminOnly");
            });

            builder.Services.AddHttpClient("api", c =>
            {
                c.BaseAddress = new Uri("https://localhost:7144");
                c.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            });

            builder.Services.AddHttpContextAccessor();

            var keyStr = builder.Configuration["Jwt:Key"];
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Authentication/Login";
                    options.AccessDeniedPath = "/Authentication/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(9);
                    options.SlidingExpiration = true;
                });

            builder.Services.AddAuthorization(o =>
            {
                o.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            // Debug middleware
            app.Use(async (context, next) =>
            {
                var user = context.User;
                if (user?.Identity?.IsAuthenticated == true)
                {
                    Console.WriteLine("User authenticated:");
                    foreach (var c in user.Claims)
                        Console.WriteLine($" - {c.Type}: {c.Value}");
                }
                else
                {
                    Console.WriteLine("User NOT authenticated");
                }

                await next();
            });

            // Endpoint nhận JWT từ WebAPI và tạo cookie
            app.MapPost("/Authentication/SignInWithToken", async (HttpContext http, [FromBody] TokenDto dto) =>
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(dto.Token);

                var claims = new List<Claim>();
                foreach (var c in token.Claims)
                {
                    if (c.Type == "role")
                        claims.Add(new Claim(ClaimTypes.Role, c.Value));
                    else if (c.Type == "name" || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
                        claims.Add(new Claim(ClaimTypes.Name, c.Value));
                    else
                        claims.Add(new Claim(c.Type, c.Value));
                }

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                    new AuthenticationProperties { IsPersistent = true });

                return Results.Ok();
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGet("/", () => Results.Redirect("/Home"));
            app.MapRazorPages();
            app.Run();
        }
    }

    // TokenDto khai báo bên ngoài Main
    public record TokenDto(string Token);
}
