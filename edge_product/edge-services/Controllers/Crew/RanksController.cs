using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;

namespace MaritimeEdge.Controllers.Crew;

[Route("api/[controller]")]
[ApiController]
public class RanksController : ControllerBase
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<RanksController> _logger;

    public RanksController(EdgeDbContext context, ILogger<RanksController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all ranks
    /// GET /api/ranks
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Rank>>> GetRanks([FromQuery] bool includeInactive = false)
    {
        try
        {
            var query = _context.Ranks.AsQueryable();
            
            if (!includeInactive)
            {
                query = query.Where(r => r.IsActive);
            }
            
            return await query
                .OrderBy(r => r.RankName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ranks");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get rank by ID
    /// GET /api/ranks/{id}
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Rank>> GetRank(int id)
    {
        try
        {
            var rank = await _context.Ranks.FindAsync(id);

            if (rank == null)
            {
                return NotFound(new { error = "Rank not found" });
            }

            return rank;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rank {RankId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Create new rank
    /// POST /api/ranks
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Rank>> CreateRank(Rank rank)
    {
        try
        {
            // Check for duplicate rank code
            var existingRank = await _context.Ranks
                .FirstOrDefaultAsync(r => r.RankCode.ToLower() == rank.RankCode.ToLower());
            
            if (existingRank != null)
            {
                return BadRequest(new { error = "Rank code already exists" });
            }

            _context.Ranks.Add(rank);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRank), new { id = rank.Id }, rank);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating rank");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Update rank
    /// PUT /api/ranks/{id}
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRank(int id, Rank rank)
    {
        if (id != rank.Id)
        {
            return BadRequest(new { error = "ID mismatch" });
        }

        try
        {
            // Check for duplicate rank code (excluding current rank)
            var duplicateRank = await _context.Ranks
                .FirstOrDefaultAsync(r => r.Id != id && r.RankCode.ToLower() == rank.RankCode.ToLower());
            
            if (duplicateRank != null)
            {
                return BadRequest(new { error = "Rank code already exists" });
            }

            var existing = await _context.Ranks.FindAsync(id);
            if (existing == null)
            {
                return NotFound(new { error = "Rank not found" });
            }

            // Only update provided fields
            if (rank.RankCode != null) existing.RankCode = rank.RankCode;
            if (rank.RankName != null) existing.RankName = rank.RankName;
            existing.IsActive = rank.IsActive;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await RankExists(id))
            {
                return NotFound(new { error = "Rank not found" });
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating rank {RankId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete rank (soft delete by setting IsActive = false)
    /// DELETE /api/ranks/{id}
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRank(int id)
    {
        try
        {
            var rank = await _context.Ranks.FindAsync(id);
            if (rank == null)
            {
                return NotFound(new { error = "Rank not found" });
            }

            // Soft delete
            rank.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting rank {RankId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Permanently delete rank
    /// DELETE /api/ranks/{id}/permanent
    /// </summary>
    [HttpDelete("{id}/permanent")]
    public async Task<IActionResult> PermanentDeleteRank(int id)
    {
        try
        {
            var rank = await _context.Ranks.FindAsync(id);
            if (rank == null)
            {
                return NotFound(new { error = "Rank not found" });
            }

            _context.Ranks.Remove(rank);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error permanently deleting rank {RankId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    private async Task<bool> RankExists(int id)
    {
        return await _context.Ranks.AnyAsync(e => e.Id == id);
    }
}
