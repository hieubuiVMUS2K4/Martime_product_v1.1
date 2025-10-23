using ProductApi.DTOs;

namespace ProductApi.Services
{
    public interface IShipService
    {
        IEnumerable<ShipDto> GetAll();
        ShipDto Create(ShipDto dto);
    }
}
