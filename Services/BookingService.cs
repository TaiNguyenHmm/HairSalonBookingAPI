using DataAccessObjects.Models;
using Repositories;

namespace Services
{
    public class BookingService
    {
        private readonly IStylistRepository _stylistRepo;
        private readonly IServiceRepository _serviceRepo;

        public BookingService(IStylistRepository stylistRepo, IServiceRepository serviceRepo)
        {
            _stylistRepo = stylistRepo;
            _serviceRepo = serviceRepo;
        }
        public async Task<Service?> GetServiceById(int serviceId)
        {
            return await _serviceRepo.GetByIdAsync(serviceId);
        }


        public async Task<List<string>> GetAvailableTimes(int stylistId, int serviceId, DateTime date)
        {
            var stylist = await _stylistRepo.GetByIdAsync(stylistId);
            if (stylist == null) throw new Exception("Stylist không tồn tại");

            var service = await _serviceRepo.GetByIdAsync(serviceId);
            if (service == null) throw new Exception("Dịch vụ không tồn tại");

            var duration = TimeSpan.FromMinutes(service.DurationMinutes);

            var dayOfWeek = ((int)date.DayOfWeek == 0 ? 7 : (int)date.DayOfWeek);
            var workingHours = (await _stylistRepo.GetWorkingHoursAsync(stylistId, dayOfWeek)).ToList();
            var bookings = (await _stylistRepo.GetBookingsAsync(stylistId, date)).OrderBy(b => b.StartTime).ToList();

            var availableSlots = new List<string>();

            foreach (var wh in workingHours)
            {
                var slotStart = date.Date + wh.StartTime.ToTimeSpan();
                var slotEnd = date.Date + wh.EndTime.ToTimeSpan();

                while (slotStart + duration <= slotEnd)
                {
                    // kiểm tra xung đột với các booking hiện có
                    bool isOverlapping = bookings.Any(b => slotStart < b.EndTime && (slotStart + duration) > b.StartTime);

                    // kiểm tra thời gian còn lại >= 40 phút (ngưỡng)
                    bool enoughTimeLeft = (slotEnd - slotStart).TotalMinutes >= duration.TotalMinutes;

                    if (!isOverlapping && enoughTimeLeft)
                        availableSlots.Add(slotStart.ToString("HH:mm"));

                    slotStart = slotStart.AddMinutes(15); // step 15 phút
                }
            }

            return availableSlots.OrderBy(s => s).ToList();
        }
    }
}
