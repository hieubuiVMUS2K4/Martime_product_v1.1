using MaritimeEdge.Data;
using MaritimeEdge.Models;
using MaritimeEdge.Security;
using MaritimeEdge.Services.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace MaritimeEdge.Tests.Services.Core;

/// <summary>
/// Vessel Provisioning v3 runtime config precedence and fail-closed tests.
/// DB active profile wins unconditionally; missing/corrupted managed config fails closed;
/// legacy fallback is available only when Sync:ConfigMode/EDGE_SYNC_CONFIG_MODE is explicitly Legacy.
/// </summary>
public class EdgeRuntimeConfigServiceTests
{
    private static EdgeDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<EdgeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new EdgeDbContext(options);
    }

    private static IConfiguration CreateConfiguration(Dictionary<string, string?>? values = null)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values ?? new Dictionary<string, string?>())
            .Build();
    }

    private static Mock<IEdgeDataEncryptionService> CreatePassthroughEncryptionMock()
    {
        var mock = new Mock<IEdgeDataEncryptionService>();
        mock.Setup(e => e.Decrypt(It.IsAny<string?>()))
            .Returns((string? v) => v); // passthrough: treat stored value as already-plaintext for tests
        mock.Setup(e => e.IsConfigured).Returns(true);
        return mock;
    }

    [Fact]
    public async Task GetSyncConfigAsync_ActiveDbProfile_WinsOverLegacyConfig_SourceIsDb()
    {
        // Test case 1 — DB active profile thắng tất cả (Managed Mode)
        using var db = CreateInMemoryContext();
        db.EdgeProvisioningProfiles.Add(new EdgeProvisioningProfile
        {
            IsActive = true,
            NodeId = "edge-9292929-main",
            ShoreBaseUrl = "https://shcdvmu.site",
            NodeApiToken = "plaintext-token",
            SigningKey = "plaintext-signing-key",
            VesselId = Guid.Parse("c42dab69-0000-0000-0000-000000000001"),
            VesselImo = "9292929",
            VesselName = "MV Test Vessel",
            KeyVersion = 3,
            ProtocolVersion = "2",
            SecurityEnabled = true
        });
        await db.SaveChangesAsync();

        // Legacy config present but MUST be ignored since an active profile exists.
        var config = CreateConfiguration(new Dictionary<string, string?>
        {
            ["SyncSecurity:NodeId"] = "old-node-aaa",
            ["ShoreAPI:BaseUrl"] = "https://old-shore.example",
            ["ShoreAPI:VesselId"] = "11111111-1111-1111-1111-111111111111"
        });

        var service = new EdgeRuntimeConfigService(
            db, CreatePassthroughEncryptionMock().Object, config,
            NullLogger<EdgeRuntimeConfigService>.Instance);

        var result = await service.GetSyncConfigAsync();

        Assert.Equal("edge-9292929-main", result.NodeId);
        Assert.Equal("https://shcdvmu.site", result.ShoreBaseUrl);
        Assert.Equal(Guid.Parse("c42dab69-0000-0000-0000-000000000001"), result.ShoreVesselId);
        Assert.Equal("db", result.Source);
    }

    [Fact]
    public async Task GetSyncConfigAsync_NoProfile_NoExplicitMode_ThrowsProvisioningRequired()
    {
        // Test case 2 - fresh-start Edge must not sync using legacy identity before provisioning.
        using var db = CreateInMemoryContext();
        // No profile rows at all.

        var config = CreateConfiguration(new Dictionary<string, string?>
        {
            ["SyncSecurity:NodeId"] = "legacy-node-999",
            ["ShoreAPI:BaseUrl"] = "https://shore.example",
            ["NodeApiToken"] = "legacy-token"
        });

        var service = new EdgeRuntimeConfigService(
            db, CreatePassthroughEncryptionMock().Object, config,
            NullLogger<EdgeRuntimeConfigService>.Instance);

        await Assert.ThrowsAsync<ProvisioningRequiredException>(() => service.GetSyncConfigAsync());
    }
    [Fact]
    public async Task GetSyncConfigAsync_NoProfile_ExplicitLegacyMode_UsesLegacyConfig()
    {
        // Test case 3 - Legacy fallback is still available, but only as an explicit opt-in.
        using var db = CreateInMemoryContext();

        var config = CreateConfiguration(new Dictionary<string, string?>
        {
            ["Sync:ConfigMode"] = "Legacy",
            ["SyncSecurity:NodeId"] = "legacy-node-999",
            ["ShoreAPI:BaseUrl"] = "https://shore.example",
            ["NodeApiToken"] = "legacy-token"
        });

        var service = new EdgeRuntimeConfigService(
            db, CreatePassthroughEncryptionMock().Object, config,
            NullLogger<EdgeRuntimeConfigService>.Instance);

        var result = await service.GetSyncConfigAsync();

        Assert.Equal("legacy-node-999", result.NodeId);
        Assert.Equal("https://shore.example", result.ShoreBaseUrl);
        Assert.Equal("legacy_config", result.Source);
    }
    [Fact]
    public async Task GetSyncConfigAsync_ActiveProfileMissingRequiredField_ThrowsConfigInvalid()
    {
        // Test case 4 — Fail-Closed khi DB profile active nhưng thiếu trường bắt buộc (NodeApiToken).
        using var db = CreateInMemoryContext();
        db.EdgeProvisioningProfiles.Add(new EdgeProvisioningProfile
        {
            IsActive = true,
            NodeId = "edge-9292929-main",
            ShoreBaseUrl = "https://shcdvmu.site",
            NodeApiToken = null, // missing required field
            SigningKey = "plaintext-signing-key"
        });
        await db.SaveChangesAsync();

        var config = CreateConfiguration();
        var service = new EdgeRuntimeConfigService(
            db, CreatePassthroughEncryptionMock().Object, config,
            NullLogger<EdgeRuntimeConfigService>.Instance);

        await Assert.ThrowsAsync<ConfigInvalidException>(() => service.GetSyncConfigAsync());
    }

    [Fact]
    public async Task GetSyncConfigAsync_ActiveProfileMissingVesselBinding_ThrowsConfigInvalid()
    {
        using var db = CreateInMemoryContext();
        db.EdgeProvisioningProfiles.Add(new EdgeProvisioningProfile
        {
            IsActive = true,
            NodeId = "edge-9292929-main",
            ShoreBaseUrl = "https://shore.example",
            NodeApiToken = "plaintext-token",
            SigningKey = "plaintext-signing-key",
            VesselImo = null,
            VesselId = null
        });
        await db.SaveChangesAsync();

        var service = new EdgeRuntimeConfigService(
            db, CreatePassthroughEncryptionMock().Object, CreateConfiguration(),
            NullLogger<EdgeRuntimeConfigService>.Instance);

        await Assert.ThrowsAsync<ConfigInvalidException>(() => service.GetSyncConfigAsync());
    }

    [Fact]
    public async Task GetSyncConfigAsync_ActiveProfileDecryptFails_ThrowsConfigInvalid_NoFallback()
    {
        // Test case 5 — Fail-Closed khi decrypt lỗi (VD: EDGE_DATA_PROTECTION_KEY sai/bị đổi).
        // Quan trọng: KHÔNG được fallback về legacy config dù legacy config hợp lệ.
        using var db = CreateInMemoryContext();
        db.EdgeProvisioningProfiles.Add(new EdgeProvisioningProfile
        {
            IsActive = true,
            NodeId = "edge-9292929-main",
            ShoreBaseUrl = "https://shcdvmu.site",
            NodeApiToken = "enc:edge:v1:corrupted-ciphertext",
            SigningKey = "enc:edge:v1:corrupted-ciphertext"
        });
        await db.SaveChangesAsync();

        var encryptionMock = new Mock<IEdgeDataEncryptionService>();
        encryptionMock.Setup(e => e.Decrypt(It.IsAny<string?>()))
            .Throws(new System.Security.Cryptography.CryptographicException("bad key"));

        // Legacy config IS valid here — must be ignored, proving no silent fallback occurs.
        var config = CreateConfiguration(new Dictionary<string, string?>
        {
            ["SyncSecurity:NodeId"] = "legacy-node-999",
            ["ShoreAPI:BaseUrl"] = "https://shore.example",
            ["NodeApiToken"] = "legacy-token"
        });

        var service = new EdgeRuntimeConfigService(
            db, encryptionMock.Object, config,
            NullLogger<EdgeRuntimeConfigService>.Instance);

        await Assert.ThrowsAsync<ConfigInvalidException>(() => service.GetSyncConfigAsync());
    }

    [Fact]
    public async Task HasActiveProfileAsync_ReturnsTrueOnlyWhenActiveProfileExists()
    {
        using var db = CreateInMemoryContext();
        var config = CreateConfiguration();
        var service = new EdgeRuntimeConfigService(
            db, CreatePassthroughEncryptionMock().Object, config,
            NullLogger<EdgeRuntimeConfigService>.Instance);

        Assert.False(await service.HasActiveProfileAsync());

        db.EdgeProvisioningProfiles.Add(new EdgeProvisioningProfile { IsActive = false, NodeId = "inactive" });
        await db.SaveChangesAsync();
        Assert.False(await service.HasActiveProfileAsync());

        db.EdgeProvisioningProfiles.Add(new EdgeProvisioningProfile { IsActive = true, NodeId = "active" });
        await db.SaveChangesAsync();
        Assert.True(await service.HasActiveProfileAsync());
    }
}
