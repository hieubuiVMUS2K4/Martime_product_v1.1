using System.Security.Cryptography;
using System.Text;

namespace MaritimeEdge.Services.Core;

public interface ISyncRequestSigningService
{
    Task<HttpRequestMessage> CreateSignedRequestAsync(
        HttpMethod method,
        string url,
        string? jsonBody,
        CancellationToken cancellationToken);
}

public sealed class SyncRequestSigningService : ISyncRequestSigningService
{
    private const string NodeIdHeader = "X-Sync-Node-Id";
    private const string TimestampHeader = "X-Sync-Timestamp";
    private const string NonceHeader = "X-Sync-Nonce";
    private const string KeyVersionHeader = "X-Sync-Key-Version";
    private const string SignatureHeader = "X-Sync-Signature";
    private const string ContentHashHeader = "X-Sync-Content-SHA256";
    private const string ProtocolHeader = "X-Sync-Protocol";

    private readonly IConfiguration _configuration;

    public SyncRequestSigningService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<HttpRequestMessage> CreateSignedRequestAsync(
        HttpMethod method,
        string url,
        string? jsonBody,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var request = new HttpRequestMessage(method, url);
        if (!string.IsNullOrEmpty(jsonBody))
        {
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        }

        if (!_configuration.GetValue("SyncSecurity:Enabled", false))
            return Task.FromResult(request);

        var nodeId = _configuration["SyncSecurity:NodeId"] ?? _configuration["Vessel:IMO"] ?? "UNKNOWN";
        var signingKey = _configuration["SyncSecurity:SigningKey"];
        var protocolVersion = _configuration["SyncSecurity:ProtocolVersion"] ?? "2";
        var keyVersion = _configuration.GetValue("SyncSecurity:KeyVersion", 1);

        if (string.IsNullOrWhiteSpace(signingKey))
            throw new InvalidOperationException("SyncSecurity:SigningKey must be configured when signed sync is enabled.");
        if (keyVersion <= 0)
            throw new InvalidOperationException("SyncSecurity:KeyVersion must be a positive integer when signed sync is enabled.");

        var bodyBytes = jsonBody == null ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(jsonBody);
        var contentHash = ComputeSha256Hex(bodyBytes);
        var timestamp = DateTimeOffset.UtcNow.ToString("O");
        var nonce = Guid.NewGuid().ToString("N");
        var pathAndQuery = request.RequestUri?.PathAndQuery ?? "/";
        var canonical = string.Join('\n', new[]
        {
            method.Method.ToUpperInvariant(),
            pathAndQuery,
            nodeId,
            timestamp,
            nonce,
            keyVersion.ToString(),
            contentHash,
            protocolVersion
        });
        var signature = ComputeHmacBase64(signingKey, canonical);

        request.Headers.TryAddWithoutValidation(NodeIdHeader, nodeId);
        request.Headers.TryAddWithoutValidation(TimestampHeader, timestamp);
        request.Headers.TryAddWithoutValidation(NonceHeader, nonce);
        request.Headers.TryAddWithoutValidation(KeyVersionHeader, keyVersion.ToString());
        request.Headers.TryAddWithoutValidation(SignatureHeader, signature);
        request.Headers.TryAddWithoutValidation(ContentHashHeader, contentHash);
        request.Headers.TryAddWithoutValidation(ProtocolHeader, protocolVersion);

        return Task.FromResult(request);
    }

    private static string ComputeSha256Hex(byte[] content)
    {
        var hash = SHA256.HashData(content);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string ComputeHmacBase64(string sharedKey, string canonical)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(sharedKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(canonical));
        return Convert.ToBase64String(hash);
    }
}