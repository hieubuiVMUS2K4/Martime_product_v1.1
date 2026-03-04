using ProductApi.Data;
using ProductApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ProductApi.Services
{
    public class ShipRepository : IShipRepository
    {
        private readonly AppDbContext _db;
        public ShipRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Ship>> GetAllAsync()
        {
            return await _db.Ships.AsNoTracking().ToListAsync();
        }

        public async Task<Ship> AddAsync(Ship ship)
        {
            _db.Ships.Add(ship);
            await _db.SaveChangesAsync();
            return ship;
        }
    }
}
