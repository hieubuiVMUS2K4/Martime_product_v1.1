using Microsoft.AspNetCore.Authorization;
using MaritimeEdge.Services.Core;

namespace MaritimeEdge.Security;

public sealed class InternalAccessHandler : AuthorizationHandler<InternalAccessRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;

    public InternalAccessHandler(
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _environment = environment;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        InternalAccessRequirement requirement)
    {
        if (!_configuration.GetValue("Security:RequireInternalAccess", false))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return Task.CompletedTask;

        if (httpContext.IsAuthenticated())
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var remoteIp = httpContext.Connection.RemoteIpAddress;
        if (_environment.IsDevelopment() && remoteIp != null && System.Net.IPAddress.IsLoopback(remoteIp))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var configuredApiKey = _configuration["InternalAccess:ApiKey"];
        var providedApiKey = httpContext.Request.Headers["X-Internal-Api-Key"].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(configuredApiKey) &&
            string.Equals(configuredApiKey, providedApiKey, StringComparison.Ordinal))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}