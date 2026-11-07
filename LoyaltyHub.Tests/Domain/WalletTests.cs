using LoyaltyHub.Domain.Entities;

namespace LoyaltyHub.Tests.Domain;

public class WalletTests
{
    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        // Act
        var wallet = new Wallet
        {
            WalletId = "W100",
            Balance = 100
        };

        // Assert
        Assert.Equal("W100", wallet.WalletId);
        Assert.Equal(100, wallet.Balance);
    }
}
