using ProductApi.DTOs;
using ProductApi.Models;

namespace ProductApi.Services
{
    public class ShipService : IShipService
    {
        private readonly IShipRepository _repo;
        public ShipService(IShipRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<ShipDto>> GetAllAsync()
        {
            var ships = await _repo.GetAllAsync();
            return ships.Select(s => new ShipDto { Id = s.Id, Name = s.Name, IMO = s.IMO, Capacity = s.Capacity });
        }

        public async Task<ShipDto> CreateAsync(ShipDto dto)
        {
            var ship = new Ship { Id = Guid.NewGuid(), Name = dto.Name, IMO = dto.IMO, Capacity = dto.Capacity };
            var created = await _repo.AddAsync(ship);
            return new ShipDto { Id = created.Id, Name = created.Name, IMO = created.IMO, Capacity = created.Capacity };
        }
    }
}
