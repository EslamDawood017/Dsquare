using System.ComponentModel.DataAnnotations;
using AutoMapper;
using LoyaltyHub.Application.Common;
using LoyaltyHub.Application.DTOs;
using LoyaltyHub.Application.Mappings;
using LoyaltyHub.Application.Services;
using LoyaltyHub.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace LoyaltyHub.Tests.Application;

public class AwardServiceTests
{
    private readonly AwardService _awardService;
    private readonly AwardRepository _awardRepository;
    private readonly WalletRepository _walletRepository;

    public AwardServiceTests()
    {
        WalletRepository.Reset();
        AwardRepository.Reset();
        _awardRepository = new AwardRepository();
        _walletRepository = new WalletRepository();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
        var mapper = services.BuildServiceProvider().GetRequiredService<IMapper>();

        _awardService = new AwardService(_awardRepository, _walletRepository, mapper);
    }

    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, validationResults, true);
        return validationResults;
    }

    [Fact]
    public void ProcessAward_NullRequest_ReturnsValidationError()
    {
        var result = _awardService.ProcessAward(null);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Equal("INVALID_REQUEST", result.Error.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AwardRequestDto_InvalidSourceId_FailsValidation(string? invalidSourceId)
    {
        var request = new AwardRequestDto
        {
            SourceId = invalidSourceId!,
            ExternalTransactionId = "tx_01",
            WalletId = "W100",
            Points = 50
        };

        var errors = ValidateModel(request);

        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(AwardRequestDto.SourceId)) || e.ErrorMessage!.Contains("sourceId"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AwardRequestDto_InvalidExternalTransactionId_FailsValidation(string? invalidTxId)
    {
        var request = new AwardRequestDto
        {
            SourceId = "src_1",
            ExternalTransactionId = invalidTxId!,
            WalletId = "W100",
            Points = 50
        };

        var errors = ValidateModel(request);

        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(AwardRequestDto.ExternalTransactionId)) || e.ErrorMessage!.Contains("externalTransactionId"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AwardRequestDto_InvalidWalletId_FailsValidation(string? invalidWalletId)
    {
        var request = new AwardRequestDto
        {
            SourceId = "src_1",
            ExternalTransactionId = "tx_01",
            WalletId = invalidWalletId!,
            Points = 50
        };

        var errors = ValidateModel(request);

        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(AwardRequestDto.WalletId)) || e.ErrorMessage!.Contains("walletId"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-500)]
    [InlineData(1001)]
    [InlineData(5000)]
    public void AwardRequestDto_InvalidPoints_FailsValidation(int invalidPoints)
    {
        var request = new AwardRequestDto
        {
            SourceId = "src_1",
            ExternalTransactionId = "tx_01",
            WalletId = "W100",
            Points = invalidPoints
        };

        var errors = ValidateModel(request);

        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(AwardRequestDto.Points)) || e.ErrorMessage!.Contains("points"));
    }

    [Fact]
    public void ProcessAward_ValidNewAward_CreatesAwardAndUpdatesBalance()
    {
        var request = new AwardRequestDto
        {
            SourceId = "partner_1",
            ExternalTransactionId = "tx_100",
            WalletId = "W100",
            Points = 50
        };

        var result = _awardService.ProcessAward(request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.True(result.Value.IsNewAward);
        Assert.False(string.IsNullOrWhiteSpace(result.Value.AwardId));
        Assert.Equal("partner_1", result.Value.SourceId);
        Assert.Equal("tx_100", result.Value.ExternalTransactionId);
        Assert.Equal("W100", result.Value.WalletId);
        Assert.Equal(50, result.Value.Points);

        var wallet = _walletRepository.GetById("W100");
        Assert.NotNull(wallet);
        Assert.Equal(150, wallet.Balance);
    }

    [Fact]
    public void ProcessAward_MatchingRetry_ReturnsOriginalAwardAndDoesNotDuplicatePoints()
    {
        var request = new AwardRequestDto
        {
            SourceId = "partner_1",
            ExternalTransactionId = "tx_100",
            WalletId = "W100",
            Points = 50
        };

        var firstResult = _awardService.ProcessAward(request);
        Assert.True(firstResult.IsSuccess);
        Assert.True(firstResult.Value.IsNewAward);

        var retryResult = _awardService.ProcessAward(request);

        Assert.True(retryResult.IsSuccess);
        Assert.False(retryResult.Value.IsNewAward);
        Assert.Equal(firstResult.Value.AwardId, retryResult.Value.AwardId);

        var wallet = _walletRepository.GetById("W100");
        Assert.Equal(150, wallet!.Balance);
    }

    [Fact]
    public void ProcessAward_ConflictingRetryPoints_ReturnsConflictErrorAndPreservesBalance()
    {
        var request1 = new AwardRequestDto
        {
            SourceId = "partner_1",
            ExternalTransactionId = "tx_100",
            WalletId = "W100",
            Points = 50
        };
        _awardService.ProcessAward(request1);

        var request2 = new AwardRequestDto
        {
            SourceId = "partner_1",
            ExternalTransactionId = "tx_100",
            WalletId = "W100",
            Points = 80
        };
        var conflictResult = _awardService.ProcessAward(request2);

        Assert.True(conflictResult.IsFailure);
        Assert.Equal(ErrorType.Conflict, conflictResult.Error!.Type);
        Assert.Equal("AWARD_CONFLICT", conflictResult.Error.Code);

        var wallet = _walletRepository.GetById("W100");
        Assert.Equal(150, wallet!.Balance);
    }

    [Fact]
    public void ProcessAward_ConflictingRetryWallet_ReturnsConflictErrorAndPreservesBalances()
    {
        var request1 = new AwardRequestDto
        {
            SourceId = "partner_1",
            ExternalTransactionId = "tx_100",
            WalletId = "W100",
            Points = 50
        };
        _awardService.ProcessAward(request1);

        var request2 = new AwardRequestDto
        {
            SourceId = "partner_1",
            ExternalTransactionId = "tx_100",
            WalletId = "W200",
            Points = 50
        };
        var conflictResult = _awardService.ProcessAward(request2);

        Assert.True(conflictResult.IsFailure);
        Assert.Equal(ErrorType.Conflict, conflictResult.Error!.Type);

        var w100 = _walletRepository.GetById("W100");
        var w200 = _walletRepository.GetById("W200");
        Assert.Equal(150, w100!.Balance);
        Assert.Equal(50, w200!.Balance);
    }

    [Fact]
    public void ProcessAward_UnknownWallet_ReturnsNotFoundErrorAndAllowsLaterIdentityReuse()
    {
        var requestBad = new AwardRequestDto
        {
            SourceId = "partner_1",
            ExternalTransactionId = "tx_reuse",
            WalletId = "W999",
            Points = 50
        };
        var badResult = _awardService.ProcessAward(requestBad);

        Assert.True(badResult.IsFailure);
        Assert.Equal(ErrorType.NotFound, badResult.Error!.Type);
        Assert.Equal("WALLET_NOT_FOUND", badResult.Error.Code);

        var requestGood = new AwardRequestDto
        {
            SourceId = "partner_1",
            ExternalTransactionId = "tx_reuse",
            WalletId = "W100",
            Points = 50
        };
        var goodResult = _awardService.ProcessAward(requestGood);

        Assert.True(goodResult.IsSuccess);
        Assert.True(goodResult.Value.IsNewAward);

        var wallet = _walletRepository.GetById("W100");
        Assert.Equal(150, wallet!.Balance);
    }

    [Fact]
    public async Task ConcurrentMatchingRequests_ProducesExactlyOneNewAwardAndRestMatchingRetries()
    {
        const int concurrentCount = 20;
        var startGate = new TaskCompletionSource();

        var tasks = Enumerable.Range(0, concurrentCount).Select(_ => Task.Run(async () =>
        {
            await startGate.Task;
            return _awardService.ProcessAward(new AwardRequestDto
            {
                SourceId = "partner_1",
                ExternalTransactionId = "concurrent_tx",
                WalletId = "W100",
                Points = 50
            });
        })).ToList();

        startGate.SetResult();
        var results = await Task.WhenAll(tasks);

        Assert.All(results, r => Assert.True(r.IsSuccess));

        var newAwardCount = results.Count(r => r.Value.IsNewAward);
        var matchingRetryCount = results.Count(r => !r.Value.IsNewAward);

        Assert.Equal(1, newAwardCount);
        Assert.Equal(concurrentCount - 1, matchingRetryCount);

        var originalAwardId = results.First(r => r.Value.IsNewAward).Value.AwardId;
        Assert.All(results, r => Assert.Equal(originalAwardId, r.Value.AwardId));

        var wallet = _walletRepository.GetById("W100");
        Assert.Equal(150, wallet!.Balance);
    }

    [Fact]
    public async Task ConcurrentDistinctAwards_ToSameWallet_AllAccumulateInBalance()
    {
        const int concurrentCount = 10;
        var startGate = new TaskCompletionSource();

        var tasks = Enumerable.Range(0, concurrentCount).Select(i => Task.Run(async () =>
        {
            await startGate.Task;
            return _awardService.ProcessAward(new AwardRequestDto
            {
                SourceId = "partner_1",
                ExternalTransactionId = $"distinct_tx_{i}",
                WalletId = "W100",
                Points = 10
            });
        })).ToList();

        startGate.SetResult();
        var results = await Task.WhenAll(tasks);

        Assert.All(results, r =>
        {
            Assert.True(r.IsSuccess);
            Assert.True(r.Value.IsNewAward);
        });

        var wallet = _walletRepository.GetById("W100");
        Assert.Equal(100 + (concurrentCount * 10), wallet!.Balance);
    }

    [Fact]
    public async Task ConcurrentRace_SameIdentityDifferentPoints_OneSucceedsOneConflicts()
    {
        var startGate = new TaskCompletionSource();

        var taskA = Task.Run(async () =>
        {
            await startGate.Task;
            return _awardService.ProcessAward(new AwardRequestDto
            {
                SourceId = "partner_1",
                ExternalTransactionId = "race_conflict_tx",
                WalletId = "W100",
                Points = 100
            });
        });

        var taskB = Task.Run(async () =>
        {
            await startGate.Task;
            return _awardService.ProcessAward(new AwardRequestDto
            {
                SourceId = "partner_1",
                ExternalTransactionId = "race_conflict_tx",
                WalletId = "W100",
                Points = 200
            });
        });

        startGate.SetResult();
        var results = await Task.WhenAll(taskA, taskB);

        var successCount = results.Count(r => r.IsSuccess);
        var conflictCount = results.Count(r => r.IsFailure && r.Error!.Type == ErrorType.Conflict);

        Assert.Equal(1, successCount);
        Assert.Equal(1, conflictCount);
    }
}
