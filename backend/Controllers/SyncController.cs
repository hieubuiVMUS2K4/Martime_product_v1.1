using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;
using System.Text.Json;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<SyncController> _logger;

    public SyncController(AppDbContext context, ILogger<SyncController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Sync([FromBody] List<SyncQueueItemDto> items)
    {
        if (items == null || items.Count == 0)
            return Ok(new { message = "No items to sync" });

        _logger.LogInformation($"Received {items.Count} items for sync.");

        var results = new List<SyncResultDto>();

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var item in items)
            {
                try
                {
                    await ProcessItemAsync(item);
                    results.Add(new SyncResultDto { Id = item.Id, Success = true });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to sync item {item.Id} ({item.TableName})");
                    results.Add(new SyncResultDto { Id = item.Id, Success = false, Error = ex.Message });
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Transaction failed during sync");
            return StatusCode(500, "Sync transaction failed");
        }

        return Ok(results);
    }

    private async Task ProcessItemAsync(SyncQueueItemDto item)
    {
        switch (item.TableName)
        {
            case "position_data":
                await SyncEntityAsync<PositionData>(item);
                break;
            case "engine_data":
                await SyncEntityAsync<EngineData>(item);
                break;
            case "maritime_report":
                await SyncEntityAsync<MaritimeReport>(item);
                break;
            case "noon_report":
                await SyncEntityAsync<NoonReport>(item);
                break;
            // Add other cases as needed
            default:
                _logger.LogWarning($"Unknown table name: {item.TableName}");
                break;
        }
    }

    private async Task SyncEntityAsync<T>(SyncQueueItemDto item) where T : class
    {
        var dbSet = _context.Set<T>();
        
        // Deserialize payload
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        
        if (item.ActionType == "CREATE")
        {
            var entity = JsonSerializer.Deserialize<T>(item.Payload, options);
            if (entity != null)
            {
                // Check if exists to avoid duplicates (idempotency)
                var idProperty = typeof(T).GetProperty("Id");
                if (idProperty != null)
                {
                    var idValue = idProperty.GetValue(entity);
                    var existing = await dbSet.FindAsync(idValue);
                    if (existing == null)
                    {
                        await dbSet.AddAsync(entity);
                    }
                    else
                    {
                        // Update existing if it's a create but already there (retry scenario)
                        _context.Entry(existing).CurrentValues.SetValues(entity);
                    }
                }
            }
        }
        else if (item.ActionType == "UPDATE")
        {
            // For Delta Sync, we need to fetch the existing entity and apply changes
            // This requires the RecordKey (ID)
            // Assuming RecordKey is the ID
            
            // This part is tricky with generic T and string RecordKey. 
            // For simplicity in this prototype, we assume ID is Guid.
            if (Guid.TryParse(item.RecordKey, out var guidId))
            {
                var existing = await dbSet.FindAsync(guidId);
                if (existing != null)
                {
                    // Deserialize partial update to a Dictionary or JsonElement
                    var patchData = JsonSerializer.Deserialize<Dictionary<string, object>>(item.Payload, options);
                    if (patchData != null)
                    {
                        var entry = _context.Entry(existing);
                        foreach (var kvp in patchData)
                        {
                            var property = entry.Metadata.FindProperty(kvp.Key);
                            if (property != null && !property.IsKey())
                            {
                                // Need to handle type conversion safely
                                // This is a simplified version. In production, use a robust patcher.
                                try 
                                {
                                    var targetType = property.ClrType;
                                    var value = Convert.ChangeType(kvp.Value.ToString(), targetType);
                                    property.PropertyInfo?.SetValue(existing, value);
                                }
                                catch
                                {
                                    // Ignore conversion errors for now
                                }
                            }
                        }
                    }
                }
            }
        }
        else if (item.ActionType == "DELETE")
        {
             if (Guid.TryParse(item.RecordKey, out var guidId))
             {
                 var existing = await dbSet.FindAsync(guidId);
                 if (existing != null)
                 {
                     dbSet.Remove(existing);
                 }
             }
        }
    }
}

public class SyncQueueItemDto
{
    public long Id { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string RecordKey { get; set; } = string.Empty;
    public string ActionType { get; set; } = "CREATE"; // CREATE, UPDATE, DELETE
    public string Payload { get; set; } = "{}";
}

public class SyncResultDto
{
    public long Id { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
}
