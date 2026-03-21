using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Services;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize(Policy = "InternalAccess")]
[Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("observability")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notifications;

    public NotificationsController(INotificationService notifications)
    {
        _notifications = notifications;
    }

    /// <summary>Lấy danh sách thông báo gần nhất (mặc định 50 bản ghi).</summary>
    [HttpGet]
    public async Task<IActionResult> GetRecent([FromQuery] int limit = 50)
    {
        if (limit < 1) limit = 1;
        if (limit > 200) limit = 200;
        var items = await _notifications.GetRecentAsync(limit);
        return Ok(items);
    }

    /// <summary>Đánh dấu tất cả thông báo là đã đọc.</summary>
    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllRead()
    {
        await _notifications.MarkAllReadAsync();
        return NoContent();
    }
}
