using ProductApi.Models;

namespace ProductApi.Services
{
    public interface IShipRepository
    {
        Task<IEnumerable<Ship>> GetAllAsync();
        Task<Ship> AddAsync(Ship ship);
    }
}
