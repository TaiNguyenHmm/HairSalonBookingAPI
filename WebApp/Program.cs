using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
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

            if (builder.Environment.IsDevelopment())
            {
                builder.Logging.ClearProviders();
                builder.Logging.AddConsole();
                builder.Logging.AddDebug();
                builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
            }

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

            var issuer = builder.Configuration["Jwt:Issuer"];
            var audience = builder.Configuration["Jwt:Audience"];
            var keyStr = builder.Configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(issuer)) throw new InvalidOperationException("Missing Jwt:Issuer in WebApp appsettings.");
            if (string.IsNullOrWhiteSpace(audience)) throw new InvalidOperationException("Missing Jwt:Audience in WebApp appsettings.");
            if (string.IsNullOrWhiteSpace(keyStr)) throw new InvalidOperationException("Missing Jwt:Key in WebApp appsettings.");

            var keyBytes = Encoding.UTF8.GetBytes(keyStr);
            if (keyBytes.Length < 32)
                throw new InvalidOperationException($"Jwt:Key must be >= 32 bytes. Current: {keyBytes.Length}.");
            var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
            var signingKey = new SymmetricSecurityKey(keyBytes);
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(opt =>
               {
                   opt.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuer = true,
                       ValidateAudience = true,
                       ValidateIssuerSigningKey = true,
                       ValidIssuer = builder.Configuration["Jwt:Issuer"],
                       ValidAudience = builder.Configuration["Jwt:Audience"],
                       IssuerSigningKey = new SymmetricSecurityKey(key),
                       NameClaimType = ClaimTypes.NameIdentifier,
                       RoleClaimType = ClaimTypes.Role,
                       ClockSkew = TimeSpan.FromMinutes(5)
                   };

                   // 👇 Thêm phần này để log lỗi chi tiết
                   opt.Events = new JwtBearerEvents
                   {
                       OnAuthenticationFailed = ctx =>
                       {
                           Console.WriteLine("❌ JWT lỗi: " + ctx.Exception.Message);
                           if (ctx.Exception.InnerException != null)
                               Console.WriteLine("   ↳ Inner: " + ctx.Exception.InnerException.Message);
                           return Task.CompletedTask;
                       },
                       OnTokenValidated = ctx =>
                       {
                           Console.WriteLine("✅ JWT hợp lệ cho user: " + ctx.Principal.Identity?.Name);
                           return Task.CompletedTask;
                       },
                       OnMessageReceived = ctx =>
                       {
                           Console.WriteLine("🔹 Token nhận được: " + ctx.Token);
                           return Task.CompletedTask;
                       }
                   };
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

            app.Logger.LogInformation("WebApp JWT loaded: Issuer={Issuer}, Audience={Audience}, KeyLen={Len}",
                issuer, audience, keyBytes.Length);

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGet("/whoami", (HttpContext http) =>
            {
                var user = http.User;
                var claims = user.Claims.Select(c => new { c.Type, c.Value });
                return Results.Json(new
                {
                    Authenticated = user.Identity?.IsAuthenticated,
                    Name = user.Identity?.Name,
                    Claims = claims
                });
            });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();

            app.MapGet("/", context =>
            {
                context.Response.Redirect("/Home");
                return Task.CompletedTask;
            });

            app.Run();
        }
    }
}
