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

        public IEnumerable<ShipDto> GetAll()
        {
            return _repo.GetAll().Select(s => new ShipDto { Id = s.Id, Name = s.Name, IMO = s.IMO, Capacity = s.Capacity });
        }

        public ShipDto Create(ShipDto dto)
        {
            var ship = new Ship { Id = Guid.NewGuid(), Name = dto.Name, IMO = dto.IMO, Capacity = dto.Capacity };
            var created = _repo.Add(ship);
            return new ShipDto { Id = created.Id, Name = created.Name, IMO = created.IMO, Capacity = created.Capacity };
        }
    }
}
