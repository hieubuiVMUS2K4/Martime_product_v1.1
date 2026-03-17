using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Services.Sync;
using Maritime.Shared.Models.Sync;

namespace ProductApi.Controllers;

/// <summary>
/// Manages certificate assignments per vessel.
/// Shore assigns which certificate types each vessel requires.
/// Synced to edge so each vessel knows its required certificates.
/// </summary>
[ApiController]
[Route("api/vessels/{vesselId:guid}/certificates")]
[AllowAnonymous]
public class VesselCertificateAssignmentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ISyncOutboxService _syncOutbox;
    private readonly ILogger<VesselCertificateAssignmentsController> _logger;

    public VesselCertificateAssignmentsController(
        AppDbContext context,
        ISyncOutboxService syncOutbox,
        ILogger<VesselCertificateAssignmentsController> logger)
    {
        _context = context;
        _syncOutbox = syncOutbox;
        _logger = logger;
    }

    /// <summary>GET — Get all certificate types assigned to this vessel.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAssignments(Guid vesselId)
    {
        var vessel = await _context.Vessels.FindAsync(vesselId);
        if (vessel == null) return NotFound(new { error = "Vessel not found" });

        var assignments = await _context.VesselCertificateAssignments
            .Where(a => a.VesselId == vesselId)
            .Include(a => a.Certificate)
            .OrderBy(a => a.Certificate!.Category)
            .ThenBy(a => a.Certificate!.CertificateName)
            .Select(a => new
            {
                a.Id,
                a.CertificateId,
                a.VesselId,
                a.AssignedAt,
                CertificateCode = a.Certificate!.CertificateCode,
                CertificateName = a.Certificate.CertificateName,
                Category = a.Certificate.Category,
                IsMandatory = a.Certificate.IsMandatory,
            })
            .ToListAsync();

        return Ok(assignments);
    }

    /// <summary>GET — Get all certificate types (for picking).</summary>
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable(Guid vesselId)
    {
        var assignedIds = await _context.VesselCertificateAssignments
            .Where(a => a.VesselId == vesselId)
            .Select(a => a.CertificateId)
            .ToListAsync();

        var available = await _context.CrewCertificateTypes
            .Where(c => c.IsActive && !assignedIds.Contains(c.Id))
            .OrderBy(c => c.Category)
            .ThenBy(c => c.CertificateName)
            .Select(c => new
            {
                c.Id,
                c.CertificateCode,
                c.CertificateName,
                c.Category,
                c.IsMandatory,
            })
            .ToListAsync();

        return Ok(available);
    }

    /// <summary>POST — Assign certificate types to vessel (batch).</summary>
    [HttpPost]
    public async Task<IActionResult> Assign(Guid vesselId, [FromBody] AssignCertificatesRequest request)
    {
        var vessel = await _context.Vessels.FindAsync(vesselId);
        if (vessel == null) return NotFound(new { error = "Vessel not found" });

        var existing = await _context.VesselCertificateAssignments
            .Where(a => a.VesselId == vesselId)
            .Select(a => a.CertificateId)
            .ToListAsync();

        var toAdd = request.CertificateIds.Except(existing).ToList();
        if (toAdd.Count == 0) return Ok(new { added = 0, message = "Tất cả đã được gán" });

        var validCertIds = await _context.CrewCertificateTypes
            .Where(c => toAdd.Contains(c.Id) && c.IsActive)
            .Select(c => c.Id)
            .ToListAsync();

        var assignments = validCertIds.Select(certId => new VesselCertificateAssignment
        {
            CertificateId = certId,
            VesselId = vesselId,
            AssignedAt = DateTime.UtcNow,
        }).ToList();

        _context.VesselCertificateAssignments.AddRange(assignments);
        await _context.SaveChangesAsync();

        // Sync to edge — enqueue for this vessel's IMO
        foreach (var a in assignments)
        {
            await _syncOutbox.EnqueueAsync(
                vessel.IMO,
                "vessel_certificate_assignment",
                a.Id.ToString(),
                SyncActionType.CREATE,
                a);
        }

        _logger.LogInformation("Assigned {Count} certificate types to vessel {VesselId}", assignments.Count, vesselId);
        return Ok(new { added = assignments.Count });
    }

    /// <summary>DELETE — Remove a certificate assignment from vessel.</summary>
    [HttpDelete("{assignmentId:int}")]
    public async Task<IActionResult> Unassign(Guid vesselId, int assignmentId)
    {
        var assignment = await _context.VesselCertificateAssignments
            .FirstOrDefaultAsync(a => a.Id == assignmentId && a.VesselId == vesselId);

        if (assignment == null) return NotFound(new { error = "Assignment not found" });

        var vessel = await _context.Vessels.FindAsync(vesselId);

        _context.VesselCertificateAssignments.Remove(assignment);
        await _context.SaveChangesAsync();

        // Sync deletion to edge
        if (vessel != null)
        {
            await _syncOutbox.EnqueueAsync(
                vessel.IMO,
                "vessel_certificate_assignment",
                assignmentId.ToString(),
                SyncActionType.DELETE,
                new { Id = assignmentId });
        }

        return NoContent();
    }

    /// <summary>PUT — Replace all assignments for a vessel (set exact list).</summary>
    [HttpPut]
    public async Task<IActionResult> SetAssignments(Guid vesselId, [FromBody] AssignCertificatesRequest request)
    {
        var vessel = await _context.Vessels.FindAsync(vesselId);
        if (vessel == null) return NotFound(new { error = "Vessel not found" });

        // Remove existing
        var existing = await _context.VesselCertificateAssignments
            .Where(a => a.VesselId == vesselId)
            .ToListAsync();

        _context.VesselCertificateAssignments.RemoveRange(existing);

        // Add new
        var validCertIds = await _context.CrewCertificateTypes
            .Where(c => request.CertificateIds.Contains(c.Id) && c.IsActive)
            .Select(c => c.Id)
            .ToListAsync();

        var newAssignments = validCertIds.Select(certId => new VesselCertificateAssignment
        {
            CertificateId = certId,
            VesselId = vesselId,
            AssignedAt = DateTime.UtcNow,
        }).ToList();

        _context.VesselCertificateAssignments.AddRange(newAssignments);
        await _context.SaveChangesAsync();

        // Sync: broadcast full snapshot for this vessel
        foreach (var a in newAssignments)
        {
            await _syncOutbox.EnqueueAsync(
                vessel.IMO,
                "vessel_certificate_assignment",
                a.Id.ToString(),
                SyncActionType.CREATE,
                a);
        }
        // Sync removed
        foreach (var old in existing.Where(o => !validCertIds.Contains(o.CertificateId)))
        {
            await _syncOutbox.EnqueueAsync(
                vessel.IMO,
                "vessel_certificate_assignment",
                old.Id.ToString(),
                SyncActionType.DELETE,
                new { old.Id });
        }

        _logger.LogInformation("Set {Count} certificate assignments for vessel {VesselId}", newAssignments.Count, vesselId);
        return Ok(new { total = newAssignments.Count });
    }
}

public class AssignCertificatesRequest
{
    public List<int> CertificateIds { get; set; } = new();
}
