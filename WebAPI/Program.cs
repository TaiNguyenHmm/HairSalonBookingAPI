using BusinessObjects;
using DataAccessObjects.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Repositories;
using Services;
using System.Security.Claims;
using System.Text;
using WebAPI;

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


// ==========================
// Authorization Policy
// ==========================
builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

// ==========================
// Swagger + Bearer
// ==========================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "HairSalonBooking API", Version = "v1" });
    var scheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập: Bearer {token}"
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer"}
            },
            Array.Empty<string>()
        }
    });
});

// ==========================
// Services
// ==========================
builder.Services.AddScoped<BookingService>();

// ==========================
// CORS
// ==========================
builder.Services.AddCors(o =>
{
    o.AddPolicy("AllowWebApp", p => p
        .WithOrigins("https://localhost:7285")
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
app.UseCors("AllowWebApp");

// Authentication phải trước Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// ==========================
// OData EDM model
// ==========================
static IEdmModel GetEdmModel()
{
    var mb = new ODataConventionModelBuilder();
    mb.EntitySet<User>("Users");
    mb.EntitySet<Service>("Services");
    mb.EntitySet<Stylist>("Stylists");
    mb.EntitySet<StylistWorkingHour>("StylistWorkingHours");
    mb.EntitySet<Booking>("Bookings");
    mb.EntitySet<Review>("Reviews");
    mb.EntitySet<Notification>("Notifications");
    return mb.GetEdmModel();
}
