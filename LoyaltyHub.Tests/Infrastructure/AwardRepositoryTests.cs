using LoyaltyHub.Domain.Entities;
using LoyaltyHub.Infrastructure.Repositories;

namespace LoyaltyHub.Tests.Infrastructure;

public class AwardRepositoryTests
{
    private readonly AwardRepository _awardRepository;

    public AwardRepositoryTests()
    {
        AwardRepository.Reset();
        _awardRepository = new AwardRepository();
    }

    [Fact]
    public void GetByIdentity_NonExistingAward_ReturnsNull()
    {
        var result = _awardRepository.GetByIdentity("src_1", "tx_999");
        Assert.Null(result);
    }

    [Fact]
    public void Add_AddsAwardAndCanBeRetrieved()
    {
        var award = new Award
        {
            AwardId = "awd_001",
            SourceId = "src_1",
            ExternalTransactionId = "tx_001",
            WalletId = "W100",
            Points = 50,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _awardRepository.Add(award);

        var retrieved = _awardRepository.GetByIdentity("src_1", "tx_001");

        Assert.NotNull(retrieved);
        Assert.Equal("awd_001", retrieved.AwardId);
        Assert.Equal("src_1", retrieved.SourceId);
        Assert.Equal("tx_001", retrieved.ExternalTransactionId);
        Assert.Equal("W100", retrieved.WalletId);
        Assert.Equal(50, retrieved.Points);
    }

    [Fact]
    public void GetByIdentity_ReturnsDefensiveCopy()
    {
        var award = new Award
        {
            AwardId = "awd_001",
            SourceId = "src_1",
            ExternalTransactionId = "tx_001",
            WalletId = "W100",
            Points = 50,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _awardRepository.Add(award);

        var copy = _awardRepository.GetByIdentity("src_1", "tx_001");
        Assert.NotNull(copy);
        copy.Points = 999;

        var fresh = _awardRepository.GetByIdentity("src_1", "tx_001");
        Assert.NotNull(fresh);
        Assert.Equal(50, fresh.Points);
    }

    [Fact]
    public void Add_DifferentSourcesWithSameTxId_StoredSeparately()
    {
        var a1 = new Award
        {
            AwardId = "awd_1",
            SourceId = "src_A",
            ExternalTransactionId = "shared_tx",
            WalletId = "W100",
            Points = 20
        };

        var a2 = new Award
        {
            AwardId = "awd_2",
            SourceId = "src_B",
            ExternalTransactionId = "shared_tx",
            WalletId = "W100",
            Points = 30
        };

        _awardRepository.Add(a1);
        _awardRepository.Add(a2);

        var r1 = _awardRepository.GetByIdentity("src_A", "shared_tx");
        var r2 = _awardRepository.GetByIdentity("src_B", "shared_tx");

        Assert.NotNull(r1);
        Assert.NotNull(r2);
        Assert.Equal("awd_1", r1.AwardId);
        Assert.Equal("awd_2", r2.AwardId);
    }
}
