using DataAccessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("odata/[controller]")]
    public class ServicesODataController : ControllerBase
    {
        private readonly HairSalonBookingDbContext _context;
        public ServicesODataController(HairSalonBookingDbContext context)
        {
            _context = context;
        }

        [EnableQuery]
        [HttpGet]
        public IQueryable<Service> Get()
        {
            return _context.Services;
        }
    }
}
