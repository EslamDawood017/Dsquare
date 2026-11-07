using LoyaltyHub.Domain.Entities;

namespace LoyaltyHub.Tests.Domain;

public class AwardTests
{
    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var timestamp = DateTimeOffset.UtcNow;

        // Act
        var award = new Award
        {
            AwardId = "award_01",
            SourceId = "partner_xyz",
            ExternalTransactionId = "ext_tx_100",
            WalletId = "W100",
            Points = 250,
            CreatedAt = timestamp
        };

        // Assert
        Assert.Equal("award_01", award.AwardId);
        Assert.Equal("partner_xyz", award.SourceId);
        Assert.Equal("ext_tx_100", award.ExternalTransactionId);
        Assert.Equal("W100", award.WalletId);
        Assert.Equal(250, award.Points);
        Assert.Equal(timestamp, award.CreatedAt);
    }
}
