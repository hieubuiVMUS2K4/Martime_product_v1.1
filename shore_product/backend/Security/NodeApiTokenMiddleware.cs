using ProductApi.Services;

namespace ProductApi.Security;

/// <summary>
/// Vessel Provisioning v3 — Component 4.
/// Resolves the per-node API token (header X-Node-Api-Token) to a verified NodeId and stores it
/// in HttpContext.Items for downstream controllers (handshake, sync).
///
/// This middleware is intentionally non-blocking by default (Security:NodeToken:RequireNodeToken=false):
/// it only *identifies* the node; it does not by itself enforce authentication. Combined with
/// SyncRequestVerificationMiddleware (HMAC signing), the two mechanisms are independent — token
/// identifies "which ship", HMAC verifies "request wasn't tampered with".
/// </summary>
public sealed class NodeApiTokenMiddleware : IMiddleware
{
    public const string NodeApiTokenHeader = "X-Node-Api-Token";
    public const string VerifiedNodeIdItemKey = "VerifiedNodeApiTokenNodeId";

    private readonly IVesselProvisioningService _provisioningService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NodeApiTokenMiddleware> _logger;

    public NodeApiTokenMiddleware(
        IVesselProvisioningService provisioningService,
        IConfiguration configuration,
        ILogger<NodeApiTokenMiddleware> logger)
    {
        _provisioningService = provisioningService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var requireNodeToken = _configuration.GetValue("Security:NodeToken:RequireNodeToken", false);
        var rawToken = context.Request.Headers[NodeApiTokenHeader].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(rawToken))
        {
            var node = await _provisioningService.ValidateNodeTokenAsync(rawToken);
            if (node != null)
            {
                context.Items[VerifiedNodeIdItemKey] = node.NodeId;
            }
            else
            {
                _logger.LogWarning(
                    "NodeApiTokenMiddleware: token presented on {Path} did not match any active node (hash lookup miss or revoked).",
                    context.Request.Path);
            }
        }

        if (requireNodeToken && context.Items[VerifiedNodeIdItemKey] is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Valid X-Node-Api-Token header is required." });
            return;
        }

        await next(context);
    }
}
