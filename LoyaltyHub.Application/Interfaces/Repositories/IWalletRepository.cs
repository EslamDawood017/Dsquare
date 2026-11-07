using LoyaltyHub.Domain.Entities;

namespace LoyaltyHub.Application.Interfaces.Repositories;

public interface IWalletRepository
{
    Wallet? GetById(string walletId);
    void Update(Wallet wallet);
    bool Exists(string walletId);
}

