using LoyaltyHub.Application.Common;
using LoyaltyHub.Application.DTOs;

namespace LoyaltyHub.Application.Interfaces.Services;

public interface IWalletService
{
    Result<WalletResponseDto> GetWallet(string walletId);
}

