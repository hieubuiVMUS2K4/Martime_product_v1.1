using System.Security.Cryptography;
using System.Text;

namespace ProductApi.Security;

public interface IDataEncryptionService
{
    bool IsConfigured { get; }
    string? Encrypt(string? plaintext);
    string? Decrypt(string? protectedValue);
    bool IsEncrypted(string? value);
    byte[] EncryptBytes(byte[] plaintext);
    byte[] DecryptBytes(byte[] protectedPayload);
    bool IsEncryptedPayload(byte[]? payload);
}

public sealed class DataEncryptionService : IDataEncryptionService
{
    private const string LegacyEnvelopePrefix = "enc:v1:";
    private const string VersionedEnvelopePrefix = "enc:v2:";
    private static readonly byte[] BinaryEnvelopePrefix = Encoding.ASCII.GetBytes("encf:v2:");
    private const int NonceSizeBytes = 12;
    private const int TagSizeBytes = 16;
    private static readonly byte[] Aad = Encoding.UTF8.GetBytes("ProductApi.DataEncryption.v1");

    private readonly Dictionary<string, byte[]> _versionedKeys;
    private readonly byte[]? _legacyKeyMaterial;
    private readonly string _currentKeyVersion;
    private readonly byte[]? _currentKeyMaterial;

    public DataEncryptionService(IConfiguration configuration)
    {
        _legacyKeyMaterial = ResolveKeyMaterial(configuration["DataProtection:EncryptionKey"]);
        _currentKeyVersion = configuration["DataProtection:CurrentKeyVersion"]?.Trim() ?? "v1";
        _versionedKeys = configuration
            .GetSection("DataProtection:EncryptionKeys")
            .GetChildren()
            .Where(section => !string.IsNullOrWhiteSpace(section.Value))
            .ToDictionary(
                section => section.Key,
                section => ResolveKeyMaterial(section.Value)!,
                StringComparer.Ordinal);

        if (_versionedKeys.Count == 0 && _legacyKeyMaterial is { Length: > 0 })
            _versionedKeys[_currentKeyVersion] = _legacyKeyMaterial;

        _currentKeyMaterial = _versionedKeys.GetValueOrDefault(_currentKeyVersion);
    }

    public bool IsConfigured => _currentKeyMaterial is { Length: > 0 } || _legacyKeyMaterial is { Length: > 0 };

    public string? Encrypt(string? plaintext)
    {
        if (string.IsNullOrWhiteSpace(plaintext))
            return plaintext;

        if (IsEncrypted(plaintext))
            return plaintext;

        var keyMaterial = GetCurrentKeyMaterial();

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

        return $"{VersionedEnvelopePrefix}{_currentKeyVersion}:{Convert.ToBase64String(payload)}";
    }

    public string? Decrypt(string? protectedValue)
    {
        if (string.IsNullOrWhiteSpace(protectedValue))
            return protectedValue;

        if (!IsEncrypted(protectedValue))
            return protectedValue;

        var (keyMaterial, payload) = ParseStringEnvelope(protectedValue);
        return Encoding.UTF8.GetString(DecryptPayload(payload, keyMaterial));
    }

    public bool IsEncrypted(string? value)
        => !string.IsNullOrWhiteSpace(value)
            && (value.StartsWith(LegacyEnvelopePrefix, StringComparison.Ordinal)
                || value.StartsWith(VersionedEnvelopePrefix, StringComparison.Ordinal));

    public byte[] EncryptBytes(byte[] plaintext)
    {
        ArgumentNullException.ThrowIfNull(plaintext);

        if (IsEncryptedPayload(plaintext))
            return plaintext;

        var keyMaterial = GetCurrentKeyMaterial();
        var nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagSizeBytes];

        using var aes = new AesGcm(keyMaterial, TagSizeBytes);
        aes.Encrypt(nonce, plaintext, ciphertext, tag, Aad);

        var versionBytes = Encoding.UTF8.GetBytes(_currentKeyVersion);
        var versionLength = checked((byte)versionBytes.Length);
        var payload = new byte[BinaryEnvelopePrefix.Length + 1 + versionBytes.Length + NonceSizeBytes + TagSizeBytes + ciphertext.Length];
        Buffer.BlockCopy(BinaryEnvelopePrefix, 0, payload, 0, BinaryEnvelopePrefix.Length);
        payload[BinaryEnvelopePrefix.Length] = versionLength;
        Buffer.BlockCopy(versionBytes, 0, payload, BinaryEnvelopePrefix.Length + 1, versionBytes.Length);
        Buffer.BlockCopy(nonce, 0, payload, BinaryEnvelopePrefix.Length + 1 + versionBytes.Length, NonceSizeBytes);
        Buffer.BlockCopy(tag, 0, payload, BinaryEnvelopePrefix.Length + 1 + versionBytes.Length + NonceSizeBytes, TagSizeBytes);
        Buffer.BlockCopy(ciphertext, 0, payload, BinaryEnvelopePrefix.Length + 1 + versionBytes.Length + NonceSizeBytes + TagSizeBytes, ciphertext.Length);
        return payload;
    }

    public byte[] DecryptBytes(byte[] protectedPayload)
    {
        ArgumentNullException.ThrowIfNull(protectedPayload);

        if (!IsEncryptedPayload(protectedPayload))
            return protectedPayload;

        if (protectedPayload.Length < BinaryEnvelopePrefix.Length + 1 + NonceSizeBytes + TagSizeBytes)
            throw new InvalidOperationException("Protected file payload is too short.");

        var versionLength = protectedPayload[BinaryEnvelopePrefix.Length];
        var versionStart = BinaryEnvelopePrefix.Length + 1;
        var payloadStart = versionStart + versionLength;
        if (protectedPayload.Length < payloadStart + NonceSizeBytes + TagSizeBytes)
            throw new InvalidOperationException("Protected file payload has an invalid version header.");

        var version = Encoding.UTF8.GetString(protectedPayload, versionStart, versionLength);
        var keyMaterial = ResolveKeyMaterialForVersion(version);
        return DecryptPayload(protectedPayload[payloadStart..], keyMaterial);
    }

    public bool IsEncryptedPayload(byte[]? payload)
    {
        if (payload == null || payload.Length < BinaryEnvelopePrefix.Length)
            return false;

        return payload.AsSpan(0, BinaryEnvelopePrefix.Length).SequenceEqual(BinaryEnvelopePrefix);
    }

    private void EnsureConfiguredForEncryption()
    {
        if (_currentKeyMaterial is not { Length: > 0 })
        {
            throw new InvalidOperationException(
                "DataProtection must define a current encryption key before writing protected values. Configure DataProtection:CurrentKeyVersion with DataProtection:EncryptionKeys or provide DataProtection:EncryptionKey for legacy mode.");
        }
    }

    private byte[] GetCurrentKeyMaterial()
    {
        EnsureConfiguredForEncryption();
        return _currentKeyMaterial!;
    }

    private (byte[] KeyMaterial, byte[] Payload) ParseStringEnvelope(string protectedValue)
    {
        if (protectedValue.StartsWith(LegacyEnvelopePrefix, StringComparison.Ordinal))
        {
            var legacyKey = _legacyKeyMaterial ?? _currentKeyMaterial
                ?? throw new InvalidOperationException("No compatible data-protection key is configured to decrypt legacy protected values.");
            return (legacyKey, ParseBase64Payload(protectedValue[LegacyEnvelopePrefix.Length..]));
        }

        if (!protectedValue.StartsWith(VersionedEnvelopePrefix, StringComparison.Ordinal))
            throw new InvalidOperationException("Protected value has an unsupported encryption envelope.");

        var remainder = protectedValue[VersionedEnvelopePrefix.Length..];
        var separatorIndex = remainder.IndexOf(':');
        if (separatorIndex <= 0 || separatorIndex == remainder.Length - 1)
            throw new InvalidOperationException("Protected value has an invalid versioned encryption envelope.");

        var version = remainder[..separatorIndex];
        return (ResolveKeyMaterialForVersion(version), ParseBase64Payload(remainder[(separatorIndex + 1)..]));
    }

    private byte[] ResolveKeyMaterialForVersion(string version)
    {
        if (_versionedKeys.TryGetValue(version, out var versionedKey))
            return versionedKey;

        if (string.Equals(version, _currentKeyVersion, StringComparison.Ordinal) && _currentKeyMaterial is { Length: > 0 })
            return _currentKeyMaterial;

        throw new InvalidOperationException($"No data-protection key is configured for version '{version}'.");
    }

    private static byte[] ParseBase64Payload(string encodedPayload)
    {
        try
        {
            return Convert.FromBase64String(encodedPayload);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException("Protected value has an invalid encryption envelope.", ex);
        }
    }

    private static byte[] DecryptPayload(byte[] payload, byte[] keyMaterial)
    {
        if (payload.Length < NonceSizeBytes + TagSizeBytes)
            throw new InvalidOperationException("Protected value payload is too short.");

        var nonce = payload.AsSpan(0, NonceSizeBytes);
        var tag = payload.AsSpan(NonceSizeBytes, TagSizeBytes);
        var ciphertext = payload.AsSpan(NonceSizeBytes + TagSizeBytes);
        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(keyMaterial, TagSizeBytes);
        aes.Decrypt(nonce, ciphertext, tag, plaintext, Aad);
        return plaintext;
    }

    private static byte[]? ResolveKeyMaterial(string? configuredKey)
    {
        if (string.IsNullOrWhiteSpace(configuredKey))
            return null;

        var trimmed = configuredKey.Trim();
        try
        {
            var decoded = Convert.FromBase64String(trimmed);
            if (decoded.Length is 16 or 24 or 32)
                return decoded;
        }
        catch (FormatException)
        {
        }

        var utf8 = Encoding.UTF8.GetBytes(trimmed);
        if (utf8.Length < 32)
        {
            throw new InvalidOperationException(
                "DataProtection:EncryptionKey must be a valid base64 AES key (16/24/32 bytes) or a raw secret with at least 32 UTF-8 bytes.");
        }

        using var sha = SHA256.Create();
        return sha.ComputeHash(utf8);
    }
}