using BusinessObjects;
using DataAccessObjects.Models;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Repositories;

namespace WebAPI
{
    public class Program
    {
        // Đặt static method GetEdmModel ở đây, ngoài Main
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<User>("Users"); // đúng namespace entity User
            return builder.GetEdmModel();
        }

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




            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
