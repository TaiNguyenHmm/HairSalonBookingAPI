using BusinessObjects;
using DataAccessObjects.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Repositories;

var builder = WebApplication.CreateBuilder(args);
Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

Console.WriteLine($"🔑 JWT Key = {builder.Configuration["Jwt:Key"]}");

// ==========================
// DbContext
// ==========================
var cs = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<HairSalonBookingDbContext>(o => o.UseSqlServer(cs));

// ==========================
// Repositories + AutoMapper
// ==========================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IStylistRepository, StylistRepository>();
builder.Services.AddAutoMapper(typeof(UserProfile).Assembly);
builder.Services.AddAutoMapper(typeof(ServiceProfile).Assembly);
builder.Services.AddAutoMapper(typeof(StylistProfile).Assembly);
builder.Services.AddAutoMapper(typeof(BookingProfile).Assembly);
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();


// ==========================
// Controllers + OData + XML
// ==========================
builder.Services
    .AddControllers(opt => opt.RespectBrowserAcceptHeader = true)
    .AddXmlSerializerFormatters()
    .AddOData(odata =>
        odata.Select().Filter().OrderBy().Expand().Count().SetMaxTop(100)
             .AddRouteComponents("odata", GetEdmModel()));

// ==========================
// JWT Authentication
// ==========================
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
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
           ClockSkew = TimeSpan.FromMinutes(5) // cho phép lệch giờ ±5 phút
       };

       // 👇 Thêm đoạn này ngay sau phần TokenValidationParameters
       opt.Events = new JwtBearerEvents
       {
           OnMessageReceived = ctx =>
           {
               Console.WriteLine("🔹 Token nhận được: " + (ctx.Token ?? "(null)"));
               return Task.CompletedTask;
           },
           OnAuthenticationFailed = ctx =>
           {
               Console.WriteLine("❌ JWT lỗi: " + ctx.Exception);
               return Task.CompletedTask;
           },
           OnTokenValidated = ctx =>
           {
               Console.WriteLine("✅ JWT hợp lệ! Claims:");
               foreach (var c in ctx.Principal.Claims)
                   Console.WriteLine($"   {c.Type}: {c.Value}");
               return Task.CompletedTask;
           }
       };
   });

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Đăng ký services
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<HairSalonBookingDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddAutoMapper(typeof(BusinessObjects.UserProfile).Assembly);
            builder.Services.AddAutoMapper(typeof(BusinessObjects.BookingProfile).Assembly);

            builder.Services.AddControllers()
                                   .AddOData(opt =>
         opt.AddRouteComponents("odata", GetEdmModel())
             .Select().Filter().OrderBy().Expand().Count().SetMaxTop(100));
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<IBookingRepository, BookingRepository>();
            builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
            builder.Services.AddAutoMapper(typeof(ServiceProfile).Assembly);
            builder.Services.AddScoped<IStylistRepository, StylistRepository>();
            builder.Services.AddAutoMapper(typeof(StylistProfile).Assembly);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowWebApp",
                    policy => policy.WithOrigins("https://localhost:7285") // Đúng port của WebApp
                                    .AllowAnyHeader()
                                    .AllowAnyMethod());
            });


            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.UseCors("AllowWebApp");

            app.MapControllers();
            app.Run();
        }
    }
}
