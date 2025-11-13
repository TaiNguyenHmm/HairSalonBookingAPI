using Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using Helpers;

namespace Services
{
    public class NotificationService
    {
        private readonly INotificationRepository _notificationRepo;
        private readonly IBookingRepository _bookingRepo;
        private readonly IUserRepository _userRepo;
        private readonly IServiceRepository _serviceRepo;

        public NotificationService(
            INotificationRepository notificationRepo,
            IBookingRepository bookingRepo,
            IUserRepository userRepo,
            IServiceRepository serviceRepo)
        {
            _notificationRepo = notificationRepo;
            _bookingRepo = bookingRepo;
            _userRepo = userRepo;
            _serviceRepo = serviceRepo;
        }

        /// <summary>
        /// Gửi các notification chưa gửi
        /// </summary>
        /// <param name="manual">Nếu true là gửi thủ công, bỏ qua DaysBeforeBooking</param>
        public async Task SendPendingNotificationsAsync(bool manual = false)
        {
            var pendingNotifications = await _notificationRepo.GetAllAsync();

            foreach (var notification in pendingNotifications.Where(n => !n.IsSent))
            {
                var booking = await _bookingRepo.GetByIdAsync(notification.BookingId);
                if (booking == null || booking.Status != "Confirmed")
                    continue;

                // Nếu không gửi thủ công, chỉ gửi khi đủ ngày trước booking
                if (!manual && booking.StartTime.Date.AddDays(-notification.DaysBeforeBooking) > DateTime.UtcNow.Date)
                    continue;

                var user = await _userRepo.GetByIdAsync(booking.CustomerId);
                var service = await _serviceRepo.GetByIdAsync(booking.ServiceId);

                if (user == null || string.IsNullOrEmpty(user.Email))
                    continue;

                string subject = $"Thông báo dịch vụ: {service.Name}";
                string body = $"Chào {user.Username}, bạn có booking dịch vụ '{service.Name}' vào lúc {booking.StartTime}.";

                try
                {
                    await EmailHelper.SendEmailAsync(user.Email, subject, body);

                    notification.IsSent = true;
                    notification.SentAt = DateTime.UtcNow;
                    await _notificationRepo.UpdateAsync(notification);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Gửi email thất bại: {ex.Message}");
                }
            }
        }
    }
}

