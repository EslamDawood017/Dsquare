using LoyaltyHub.Domain.ValueObjects;

namespace LoyaltyHub.Tests.Domain;

public class AwardIdentityTests
{
    [Fact]
    public void Properties_SetAndGetCorrectly()
    {
        // Act
        var identity = new AwardIdentity("sourceA", "tx123");

        // Assert
        Assert.Equal("sourceA", identity.SourceId);
        Assert.Equal("tx123", identity.ExternalTransactionId);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        // Arrange
        var id1 = new AwardIdentity("pos_terminal_1", "tx_999");
        var id2 = new AwardIdentity("pos_terminal_1", "tx_999");

        // Act & Assert
        Assert.Equal(id1, id2);
        Assert.True(id1 == id2);
    }

    [Fact]
    public void Equality_DifferentSourceIdSameTxId_AreNotEqual()
    {
        // Arrange
        var id1 = new AwardIdentity("source_alpha", "tx_999");
        var id2 = new AwardIdentity("source_beta", "tx_999");

        // Act & Assert
        Assert.NotEqual(id1, id2);
        Assert.True(id1 != id2);
    }

    [Fact]
    public void Equality_SameSourceIdDifferentTxId_AreNotEqual()
    {
        // Arrange
        var id1 = new AwardIdentity("source_alpha", "tx_001");
        var id2 = new AwardIdentity("source_alpha", "tx_002");

        // Act & Assert
        Assert.NotEqual(id1, id2);
        Assert.True(id1 != id2);
    }
}
