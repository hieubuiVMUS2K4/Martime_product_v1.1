using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProductApi.Security;

public sealed class InternalAccessHandler : AuthorizationHandler<InternalAccessRequirement>
{
    private const string InternalApiKeyHeader = "X-Internal-Api-Key";

    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<InternalAccessHandler> _logger;

    public InternalAccessHandler(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ILogger<InternalAccessHandler> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
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

        if (context.User.Identity?.IsAuthenticated == true)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var httpContext = context.Resource switch
        {
            HttpContext directHttpContext => directHttpContext,
            AuthorizationFilterContext mvcContext => mvcContext.HttpContext,
            _ => null
        };

        if (httpContext == null)
            return Task.CompletedTask;

        var remoteIp = httpContext.Connection.RemoteIpAddress;
        if (_environment.IsDevelopment() && remoteIp != null && IPAddress.IsLoopback(remoteIp))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var configuredApiKey = _configuration["InternalAccess:ApiKey"];
        if (string.IsNullOrWhiteSpace(configuredApiKey))
        {
            _logger.LogWarning("Internal access policy denied request because InternalAccess:ApiKey is not configured.");
            return Task.CompletedTask;
        }

        if (httpContext.Request.Headers.TryGetValue(InternalApiKeyHeader, out var providedApiKey)
            && string.Equals(providedApiKey, configuredApiKey, StringComparison.Ordinal))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}