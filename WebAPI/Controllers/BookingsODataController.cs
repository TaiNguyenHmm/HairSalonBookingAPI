using DataAccessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("odata/[controller]")]
    [Authorize]

    public class BookingsODataController : ControllerBase
    {
        private readonly HairSalonBookingDbContext _context;
        public BookingsODataController(HairSalonBookingDbContext context)
        {
            _context = context;
        }

        [EnableQuery]
        [HttpGet]
        public IQueryable<Booking> Get()
        {
            return _context.Bookings;
        }
    }
}
