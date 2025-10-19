using DataAccessObjects.Models;


namespace Repositories
{
    public interface IBookingRepository
    {
        Task<IEnumerable<Booking>> GetAllAsync();
        Task<Booking?> GetByIdAsync(int id);
        Task AddAsync(Booking booking);
        Task UpdateAsync(Booking booking);
        Task DeleteAsync(int id);

        // Có thể mở rộng thêm các hàm filter:
        Task<IEnumerable<Booking>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Booking>> GetByStylistIdAsync(int stylistId);
        Task<IEnumerable<Booking>> GetByStatusAsync(string status);
    }
}
