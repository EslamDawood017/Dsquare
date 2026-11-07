using AutoMapper;
using LoyaltyHub.Application.Common;
using LoyaltyHub.Application.Mappings;
using LoyaltyHub.Application.Services;
using LoyaltyHub.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace LoyaltyHub.Tests.Application;

public class WalletServiceTests
{
    private readonly WalletService _walletService;
    private readonly WalletRepository _walletRepository;

    public WalletServiceTests()
    {
        WalletRepository.Reset();
        _walletRepository = new WalletRepository();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();

        _walletService = new WalletService(_walletRepository, mapper);
    }

    [Fact]
    public void GetWallet_ExistingWallet_ReturnsBalance()
    {
        var result = _walletService.GetWallet("W100");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("W100", result.Value.WalletId);
        Assert.Equal(100, result.Value.Balance);
    }

    [Fact]
    public void GetWallet_UnknownWallet_ReturnsNotFound()
    {
        var result = _walletService.GetWallet("W999");

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("WALLET_NOT_FOUND", result.Error.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetWallet_InvalidWalletId_ReturnsValidationError(string? invalidWalletId)
    {
        var result = _walletService.GetWallet(invalidWalletId!);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Equal("INVALID_REQUEST", result.Error.Code);
    }
}
