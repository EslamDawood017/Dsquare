using LoyaltyHub.Infrastructure.Repositories;

namespace LoyaltyHub.Tests.Infrastructure;

public class WalletRepositoryTests
{
    private readonly WalletRepository _repository;

    public WalletRepositoryTests()
    {
        WalletRepository.Reset();
        _repository = new WalletRepository();
    }

    [Fact]
    public void GetById_ExistingSeedWallet_ReturnsCorrectWalletAndBalance()
    {
        var w100 = _repository.GetById("W100");
        var w200 = _repository.GetById("W200");

        Assert.NotNull(w100);
        Assert.Equal("W100", w100.WalletId);
        Assert.Equal(100, w100.Balance);

        Assert.NotNull(w200);
        Assert.Equal("W200", w200.WalletId);
        Assert.Equal(50, w200.Balance);
    }

    [Fact]
    public void GetById_UnknownWallet_ReturnsNull()
    {
        var result = _repository.GetById("W999");
        Assert.Null(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetById_NullOrWhitespace_ReturnsNull(string? invalidWalletId)
    {
        var result = _repository.GetById(invalidWalletId!);
        Assert.Null(result);
    }

    [Fact]
    public void GetById_MutatingReturnedObject_DoesNotMutateRepositoryStore()
    {
        var wallet = _repository.GetById("W100");
        Assert.NotNull(wallet);
        wallet.Balance += 500;

        var freshWallet = _repository.GetById("W100");

        Assert.NotNull(freshWallet);
        Assert.Equal(100, freshWallet.Balance);
    }

    [Fact]
    public void Exists_ExistingWallet_ReturnsTrue()
    {
        var exists = _repository.Exists("W100");
        Assert.True(exists);
    }

    [Fact]
    public void Exists_NonExistingWallet_ReturnsFalse()
    {
        var exists = _repository.Exists("W999");
        Assert.False(exists);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Exists_NullOrWhitespace_ReturnsFalse(string? invalidWalletId)
    {
        var exists = _repository.Exists(invalidWalletId!);
        Assert.False(exists);
    }
}
