using DataAccessObjects.Models;


namespace Repositories
{
    public interface IStylistRepository
    {
        Task<IEnumerable<Stylist>> GetAllAsync();
        Task<Stylist?> GetByIdAsync(int id);
        Task AddAsync(Stylist stylist);
        Task UpdateAsync(Stylist stylist);
        Task DeleteAsync(int id);
    }
}
