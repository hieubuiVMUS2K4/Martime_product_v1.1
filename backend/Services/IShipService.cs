using ProductApi.DTOs;

namespace ProductApi.Services
{
    public interface IShipService
    {
        Task<IEnumerable<ShipDto>> GetAllAsync();
        Task<ShipDto> CreateAsync(ShipDto dto);
    }
}
