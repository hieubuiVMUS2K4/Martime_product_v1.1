using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;

namespace MaritimeEdge.Repositories;

public interface IShipDataRepository
{
    Task<ShipData?> GetAsync();
    Task<ShipData?> GetWithChildrenAsync();
    Task<ShipData> CreateAsync(ShipData shipData);
    Task<ShipData> UpdateAsync(ShipData shipData);

    // Batch save - gom tất cả thay đổi vào 1 round-trip duy nhất
    Task SaveAllChangesAsync();

    // Child collection operations (không gọi SaveChanges - dùng SaveAllChangesAsync ở cuối)
    Task ReplaceMainEnginesAsync(Guid shipDataId, List<ShipMainEngine> engines);
    Task ReplaceAuxiliaryEnginesAsync(Guid shipDataId, List<ShipAuxiliaryEngine> engines);
    Task ReplacePropellersAsync(Guid shipDataId, List<ShipPropeller> propellers);
    Task ReplaceBowthrusterAsync(Guid shipDataId, List<ShipBowthruster> items);
    Task ReplaceSternthrusterAsync(Guid shipDataId, List<ShipSternthruster> items);
    Task ReplaceRuddersAsync(Guid shipDataId, List<ShipRudder> items);
    Task ReplaceShaftGeneratorsAsync(Guid shipDataId, List<ShipShaftGenerator> items);
    Task ReplaceBoilersAsync(Guid shipDataId, List<ShipBoiler> items);
    Task ReplaceLoadLinesAsync(Guid shipDataId, List<ShipLoadLine> items);
    Task ReplacePilotCardDataAsync(Guid shipDataId, List<ShipPilotCardData> items);
}

public class ShipDataRepository : IShipDataRepository
{
    private readonly EdgeDbContext _context;

    public ShipDataRepository(EdgeDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get ship data (single record per vessel - edge only has one ship)
    /// </summary>
    public async Task<ShipData?> GetAsync()
    {
        return await _context.ShipData.FirstOrDefaultAsync();
    }

    /// <summary>
    /// Get ship data with all child collections eagerly loaded.
    /// AsNoTracking + AsSplitQuery: tránh change tracking overhead và cartesian explosion.
    /// </summary>
    public async Task<ShipData?> GetWithChildrenAsync()
    {
        return await _context.ShipData
            .AsNoTracking()
            .AsSplitQuery()
            .Include(s => s.MainEngines.OrderBy(e => e.SortOrder))
            .Include(s => s.AuxiliaryEngines.OrderBy(e => e.SortOrder))
            .Include(s => s.Propellers.OrderBy(e => e.SortOrder))
            .Include(s => s.Bowthrusters.OrderBy(e => e.SortOrder))
            .Include(s => s.Sternthrusters.OrderBy(e => e.SortOrder))
            .Include(s => s.Rudders.OrderBy(e => e.SortOrder))
            .Include(s => s.ShaftGenerators.OrderBy(e => e.SortOrder))
            .Include(s => s.Boilers.OrderBy(e => e.SortOrder))
            .Include(s => s.LoadLines.OrderBy(e => e.SortOrder))
            .Include(s => s.PilotCardData.OrderBy(e => e.SortOrder))
            .FirstOrDefaultAsync();
    }

    public Task<ShipData> CreateAsync(ShipData shipData)
    {
        shipData.CreatedAt = DateTime.UtcNow;
        shipData.UpdatedAt = DateTime.UtcNow;
        _context.ShipData.Add(shipData);
        // Không gọi SaveChangesAsync - sẽ batch save ở cuối transaction
        return Task.FromResult(shipData);
    }

    public Task<ShipData> UpdateAsync(ShipData shipData)
    {
        shipData.UpdatedAt = DateTime.UtcNow;
        // Don't call _context.ShipData.Update(shipData) - entity is already tracked
        // EF Core change tracker will only detect and update actually modified columns
        return Task.FromResult(shipData);
    }

    /// <summary>
    /// Gom tất cả pending changes vào 1 lần SaveChanges duy nhất (1 DB round-trip)
    /// </summary>
    public async Task SaveAllChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════
    // Child collection replacement (delete-all + re-insert pattern)
    // This is the simplest approach for "Save All" behavior
    // ═══════════════════════════════════════════

    public async Task ReplaceMainEnginesAsync(Guid shipDataId, List<ShipMainEngine> engines)
    {
        // Bulk delete không load vào memory (1 DELETE statement)
        await _context.ShipMainEngines
            .Where(e => e.ShipDataId == shipDataId).ExecuteDeleteAsync();

        foreach (var e in engines)
        {
            e.Id = Guid.NewGuid();
            e.ShipDataId = shipDataId;
        }
        _context.ShipMainEngines.AddRange(engines);
    }

    public async Task ReplaceAuxiliaryEnginesAsync(Guid shipDataId, List<ShipAuxiliaryEngine> engines)
    {
        await _context.ShipAuxiliaryEngines
            .Where(e => e.ShipDataId == shipDataId).ExecuteDeleteAsync();

        foreach (var e in engines)
        {
            e.Id = Guid.NewGuid();
            e.ShipDataId = shipDataId;
        }
        _context.ShipAuxiliaryEngines.AddRange(engines);
    }

    public async Task ReplacePropellersAsync(Guid shipDataId, List<ShipPropeller> propellers)
    {
        await _context.ShipPropellers
            .Where(e => e.ShipDataId == shipDataId).ExecuteDeleteAsync();

        foreach (var e in propellers)
        {
            e.Id = Guid.NewGuid();
            e.ShipDataId = shipDataId;
        }
        _context.ShipPropellers.AddRange(propellers);
    }

    public async Task ReplaceBowthrusterAsync(Guid shipDataId, List<ShipBowthruster> items)
    {
        await _context.ShipBowthrusters
            .Where(e => e.ShipDataId == shipDataId).ExecuteDeleteAsync();

        foreach (var e in items)
        {
            e.Id = Guid.NewGuid();
            e.ShipDataId = shipDataId;
        }
        _context.ShipBowthrusters.AddRange(items);
    }

    public async Task ReplaceSternthrusterAsync(Guid shipDataId, List<ShipSternthruster> items)
    {
        await _context.ShipSternthrusters
            .Where(e => e.ShipDataId == shipDataId).ExecuteDeleteAsync();

        foreach (var e in items)
        {
            e.Id = Guid.NewGuid();
            e.ShipDataId = shipDataId;
        }
        _context.ShipSternthrusters.AddRange(items);
    }

    public async Task ReplaceRuddersAsync(Guid shipDataId, List<ShipRudder> items)
    {
        await _context.ShipRudders
            .Where(e => e.ShipDataId == shipDataId).ExecuteDeleteAsync();

        foreach (var e in items)
        {
            e.Id = Guid.NewGuid();
            e.ShipDataId = shipDataId;
        }
        _context.ShipRudders.AddRange(items);
    }

    public async Task ReplaceShaftGeneratorsAsync(Guid shipDataId, List<ShipShaftGenerator> items)
    {
        await _context.ShipShaftGenerators
            .Where(e => e.ShipDataId == shipDataId).ExecuteDeleteAsync();

        foreach (var e in items)
        {
            e.Id = Guid.NewGuid();
            e.ShipDataId = shipDataId;
        }
        _context.ShipShaftGenerators.AddRange(items);
    }

    public async Task ReplaceBoilersAsync(Guid shipDataId, List<ShipBoiler> items)
    {
        await _context.ShipBoilers
            .Where(e => e.ShipDataId == shipDataId).ExecuteDeleteAsync();

        foreach (var e in items)
        {
            e.Id = Guid.NewGuid();
            e.ShipDataId = shipDataId;
        }
        _context.ShipBoilers.AddRange(items);
    }

    public async Task ReplaceLoadLinesAsync(Guid shipDataId, List<ShipLoadLine> items)
    {
        await _context.ShipLoadLines
            .Where(e => e.ShipDataId == shipDataId).ExecuteDeleteAsync();

        foreach (var e in items)
        {
            e.Id = Guid.NewGuid();
            e.ShipDataId = shipDataId;
        }
        _context.ShipLoadLines.AddRange(items);
    }

    public async Task ReplacePilotCardDataAsync(Guid shipDataId, List<ShipPilotCardData> items)
    {
        await _context.ShipPilotCardData
            .Where(e => e.ShipDataId == shipDataId).ExecuteDeleteAsync();

        foreach (var e in items)
        {
            e.Id = Guid.NewGuid();
            e.ShipDataId = shipDataId;
        }
        _context.ShipPilotCardData.AddRange(items);
    }
}
