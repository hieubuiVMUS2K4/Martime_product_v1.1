using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using ProductApi.Services.Sync;

namespace ProductApi.Controllers;

[ApiController]
[Route("uploads")]
[Route("api/uploads")]
[Authorize(Policy = "InternalAccess")]
public class ProtectedUploadsController : ControllerBase
{
    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

    private readonly ISyncFileStorageService _syncFileStorageService;
    private readonly ILogger<ProtectedUploadsController> _logger;

    public ProtectedUploadsController(ISyncFileStorageService syncFileStorageService, ILogger<ProtectedUploadsController> logger)
    {
        _syncFileStorageService = syncFileStorageService;
        _logger = logger;
    }

    [HttpGet("{**relativePath}")]
    public async Task<IActionResult> Download(string relativePath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return NotFound();

        var normalizedPath = "/uploads/" + relativePath.TrimStart('/');
        if (!_syncFileStorageService.Exists(normalizedPath))
            return NotFound();

        try
        {
            var fileBytes = await _syncFileStorageService.ReadAllBytesAsync(normalizedPath, cancellationToken);
            if (!ContentTypeProvider.TryGetContentType(normalizedPath, out var contentType))
                contentType = "application/octet-stream";

            return File(fileBytes, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to serve protected upload {Path}", normalizedPath);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to read protected upload" });
        }
    }
}