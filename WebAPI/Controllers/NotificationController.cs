using DataAccessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using Services;
using System;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _notificationService;
        private readonly INotificationRepository _notificationRepo;

        public NotificationController(NotificationService notificationService,
                                      INotificationRepository notificationRepo)
        {
            _notificationService = notificationService;
            _notificationRepo = notificationRepo;
        }

        // GET: api/notifications
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var notifications = await _notificationRepo.GetAllAsync();

                var dtos = notifications.Select(n => new
                {
                    n.Id,
                    n.BookingId,
                    BookingStartTime = n.Booking?.StartTime,
                    CustomerName = n.Booking?.Customer?.Username ?? "-",
                    ServiceName = n.Booking?.Service?.Name ?? n.NotificationType,
                    n.NotificationType,
                    n.IsSent,
                    n.SentAt,
                    n.CreatedAt,
                    n.DaysBeforeBooking
                });

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, stack = ex.StackTrace });
            }
        }

        // PUT: api/notifications/{id}/days-before
        [HttpPut("{id}/days-before")]
        public async Task<IActionResult> UpdateDaysBeforeBooking(int id, [FromBody] int days)
        {
            var notification = await _notificationRepo.GetByIdAsync(id);
            if (notification == null)
                return NotFound("Notification not found.");

            notification.DaysBeforeBooking = days;
            await _notificationRepo.UpdateAsync(notification);

            return Ok(new { message = "Cập nhật DaysBeforeBooking thành công." });
        }

        // POST: api/notifications/send-manual
        [HttpPost("send-manual")]
        public async Task<IActionResult> SendManual()
        {
            await _notificationService.SendPendingNotificationsAsync(manual: true);
            return Ok(new { message = "Đã gửi các notification chưa gửi." });
        }
    }
}
