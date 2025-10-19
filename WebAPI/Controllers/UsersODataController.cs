using DataAccessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("odata/[controller]")]
    public class UsersODataController : ControllerBase
    {
        private readonly HairSalonBookingDbContext _context;
        public UsersODataController(HairSalonBookingDbContext context)
        {
            _context = context;
        }

        [EnableQuery]
        [HttpGet]
        public IQueryable<User> GetOData()
        {
            return _context.Users;
        }
    }
}
