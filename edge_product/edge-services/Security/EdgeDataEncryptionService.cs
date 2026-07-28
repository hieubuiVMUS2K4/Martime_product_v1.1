using System.Security.Cryptography;
using System.Text;

namespace MaritimeEdge.Security;

/// <summary>
/// Encrypts/decrypts sensitive fields (NodeApiToken, SigningKey) stored in
/// <see cref="MaritimeEdge.Models.EdgeProvisioningProfile"/>. Uses AES-GCM with a key derived from
/// the <c>EDGE_DATA_PROTECTION_KEY</c> environment variable / <c>DataProtection:EncryptionKey</c>
/// config value. Mirrors the Shore-side <c>IDataEncryptionService</c> pattern (same envelope idea,
/// simplified — Edge only ever needs a single active key, no key-version rotation history).
/// </summary>
public interface IEdgeDataEncryptionService
{
    bool IsConfigured { get; }
    string? Encrypt(string? plaintext);
    string? Decrypt(string? protectedValue);
    bool IsEncrypted(string? value);
}

public sealed class EdgeDataEncryptionService : IEdgeDataEncryptionService
{
    private const string EnvelopePrefix = "enc:edge:v1:";
    private const int NonceSizeBytes = 12;
    private const int TagSizeBytes = 16;
    private static readonly byte[] Aad = Encoding.UTF8.GetBytes("MaritimeEdge.DataEncryption.v1");

    private readonly byte[]? _keyMaterial;

    public EdgeDataEncryptionService(IConfiguration configuration)
    {
        var rawKey = configuration["DataProtection:EncryptionKey"]
                     ?? Environment.GetEnvironmentVariable("EDGE_DATA_PROTECTION_KEY");
        _keyMaterial = ResolveKeyMaterial(rawKey);
    }

    public bool IsConfigured => _keyMaterial is { Length: > 0 };

    public string? Encrypt(string? plaintext)
    {
        if (string.IsNullOrWhiteSpace(plaintext))
            return plaintext;

        if (IsEncrypted(plaintext))
            return plaintext;

        var keyMaterial = GetKeyMaterialOrThrow();

        var nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes);
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[TagSizeBytes];

        using var aes = new AesGcm(keyMaterial, TagSizeBytes);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag, Aad);

        var payload = new byte[NonceSizeBytes + TagSizeBytes + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, payload, 0, NonceSizeBytes);
        Buffer.BlockCopy(tag, 0, payload, NonceSizeBytes, TagSizeBytes);
        Buffer.BlockCopy(ciphertext, 0, payload, NonceSizeBytes + TagSizeBytes, ciphertext.Length);

        return $"{EnvelopePrefix}{Convert.ToBase64String(payload)}";
    }

    public string? Decrypt(string? protectedValue)
    {
        if (string.IsNullOrWhiteSpace(protectedValue))
            return protectedValue;

        if (!IsEncrypted(protectedValue))
            return protectedValue;

        var keyMaterial = GetKeyMaterialOrThrow();

        var base64 = protectedValue[EnvelopePrefix.Length..];
        var payload = Convert.FromBase64String(base64);

        if (payload.Length < NonceSizeBytes + TagSizeBytes)
            throw new CryptographicException("Invalid encrypted payload for EdgeProvisioningProfile field.");

        var nonce = payload[..NonceSizeBytes];
        var tag = payload[NonceSizeBytes..(NonceSizeBytes + TagSizeBytes)];
        var ciphertext = payload[(NonceSizeBytes + TagSizeBytes)..];
        var plaintextBytes = new byte[ciphertext.Length];

        using var aes = new AesGcm(keyMaterial, TagSizeBytes);
        aes.Decrypt(nonce, ciphertext, tag, plaintextBytes, Aad);

        return Encoding.UTF8.GetString(plaintextBytes);
    }

    public bool IsEncrypted(string? value) => value is not null && value.StartsWith(EnvelopePrefix, StringComparison.Ordinal);

    private byte[] GetKeyMaterialOrThrow()
    {
        return _keyMaterial ?? throw new InvalidOperationException(
            "EDGE_DATA_PROTECTION_KEY / DataProtection:EncryptionKey is not configured. " +
            "Cannot encrypt/decrypt Vessel Provisioning credentials.");
    }

    private static byte[]? ResolveKeyMaterial(string? rawKey)
    {
        if (string.IsNullOrWhiteSpace(rawKey))
            return null;

        // Accept base64 (preferred) or raw UTF-8 passphrase (hashed to 32 bytes via SHA-256).
        try
        {
            var decoded = Convert.FromBase64String(rawKey);
            if (decoded.Length == 32)
                return decoded;
        }
        catch (FormatException)
        {
            // not base64 — fall through to passphrase hashing
        }

        return SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
    }
}
