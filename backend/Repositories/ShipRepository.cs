using ProductApi.Data;
using ProductApi.Models;

namespace ProductApi.Services
{
    public class ShipRepository : IShipRepository
    {
        private readonly AppDbContext _db;
        public ShipRepository(AppDbContext db)
        {
            _db = db;
        }

        public IEnumerable<Ship> GetAll()
        {
            return _db.Ships.ToList();
        }

        public Ship Add(Ship ship)
        {
            _db.Ships.Add(ship);
            _db.SaveChanges();
            return ship;
        }
    }
}
