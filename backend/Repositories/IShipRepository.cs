using ProductApi.Models;

namespace ProductApi.Services
{
    public interface IShipRepository
    {
        IEnumerable<Ship> GetAll();
        Ship Add(Ship ship);
    }
}
