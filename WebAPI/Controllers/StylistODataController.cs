using DataAccessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("odata/[controller]")]
    public class StylistsODataController : ControllerBase
    {
        private readonly HairSalonBookingDbContext _context;
        public StylistsODataController(HairSalonBookingDbContext context)
        {
            _context = context;
        }

        [EnableQuery]
        [HttpGet]
        public IQueryable<Stylist> Get()
        {
            return _context.Stylists;
        }
    }
}
