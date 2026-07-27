using System.Text.Json;
using Maritime.Shared.DTOs;
using Maritime.Shared.Enums;
using Xunit;

namespace Maritime.Shared.Tests
{
    public class SyncProtocolTests
    {
        [Fact]
        public void SyncQueueItemDto_Serialization_Deserialization_PreservesData()
        {
            // Arrange
            var original = new SyncQueueItemDto
            {
                Id = Guid.NewGuid(),
                TableName = "crew_members",
                RecordKey = "CREW-1001",
                ActionType = SyncActionType.UPDATE,
                Priority = SyncPriority.Operational,
                PayloadJson = "{\"fullName\":\"Nguyen Van A\",\"rank\":\"Captain\"}",
                OriginNode = "VESSEL_IMO_9876543",
                SyncVersion = 42,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var json = JsonSerializer.Serialize(original);
            var deserialized = JsonSerializer.Deserialize<SyncQueueItemDto>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(original.Id, deserialized.Id);
            Assert.Equal(original.TableName, deserialized.TableName);
            Assert.Equal(original.RecordKey, deserialized.RecordKey);
            Assert.Equal(original.ActionType, deserialized.ActionType);
            Assert.Equal(original.Priority, deserialized.Priority);
            Assert.Equal(original.OriginNode, deserialized.OriginNode);
            Assert.Equal(original.SyncVersion, deserialized.SyncVersion);
        }

        [Theory]
        [InlineData(SyncPriority.Critical, "Safety alarms must always be prioritized")]
        [InlineData(SyncPriority.Operational, "Noon reports must have operational priority")]
        [InlineData(SyncPriority.Low, "Inventory items have low sync priority")]
        public void SyncPriority_EnumValues_AreValid(SyncPriority priority, string reason)
        {
            // Assert
            Assert.True(Enum.IsDefined(typeof(SyncPriority), priority), reason);
        }
    }
}
