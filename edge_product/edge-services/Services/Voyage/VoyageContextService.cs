using MaritimeEdge.Data;
using MaritimeEdge.Models;
using Microsoft.EntityFrameworkCore;

namespace MaritimeEdge.Services.Voyage
{
    public interface IVoyageContextService
    {
        Task<(Guid? VoyageId, Guid? VoyagePlanLegId)> ResolveActiveVoyageAsync(DateTime? recordDateTime = null);
    }

    public class VoyageContextService : IVoyageContextService
    {
        private readonly EdgeDbContext _context;

        public VoyageContextService(EdgeDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Find the active voyage (UNDERWAY or ARRIVED) and optionally the matching leg.
        /// </summary>
        public async Task<(Guid? VoyageId, Guid? VoyagePlanLegId)> ResolveActiveVoyageAsync(DateTime? recordDateTime = null)
        {
            var now = recordDateTime ?? DateTime.UtcNow;

            var voyage = await _context.VoyageRecords
                .AsNoTracking()
                .Where(v => Constants.VoyageStatus.CurrentVoyageStatuses.Contains(v.VoyageStatus))
                .OrderByDescending(v => v.DepartureTime)
                .FirstOrDefaultAsync();

            if (voyage == null)
                return (null, null);

            var leg = await _context.Set<VoyagePlanLeg>()
                .AsNoTracking()
                .Where(l => l.VoyageId == voyage.Id
                    && l.PlannedDepartureTime <= now
                    && l.PlannedArrivalTime >= now)
                .OrderBy(l => l.Sequence)
                .FirstOrDefaultAsync();

            return (voyage.Id, leg?.Id);
        }
    }
}
