using DataAccessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogsController : ControllerBase
    {
        private readonly HairSalonBookingDbContext _context;

        public AuditLogsController(HairSalonBookingDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách AuditLogs với phân trang
        /// </summary>
        /// <param name="page">Trang hiện tại (mặc định 1)</param>
        /// <param name="pageSize">Số bản ghi mỗi trang (mặc định 10)</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAuditLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _context.AuditLogs
                .Include(a => a.User) 
                .OrderByDescending(a => a.Timestamp)
                .AsQueryable();

            var total = await query.CountAsync();

            var logs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new
                {
                    l.Id,
                    l.Timestamp,
                    l.Action,
                    l.EntityName,
                    l.EntityId,
                    l.Details,
                    UserId = l.UserId,
                    User = l.User == null ? null : new
                    {
                        l.User.Id,
                        l.User.Username
                    }
                })
                .ToListAsync();

            var result = new
            {
                page,
                pageSize,
                total,
                logs
            };

            return Ok(result);
        }
    }
}
