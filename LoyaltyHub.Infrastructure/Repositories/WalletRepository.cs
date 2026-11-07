using LoyaltyHub.Application.Interfaces.Repositories;
using LoyaltyHub.Domain.Entities;

namespace LoyaltyHub.Infrastructure.Repositories;

public class WalletRepository : IWalletRepository
{
    public static readonly List<Wallet> Wallets = new();

    static WalletRepository()
    {
        Reset();
    }

    public static void Reset()
    {
        Wallets.Clear();
        Wallets.Add(new Wallet { WalletId = "W100", Balance = 100 });
        Wallets.Add(new Wallet { WalletId = "W200", Balance = 50 });
    }

    public Wallet? GetById(string walletId)
    {
        
        var wallet = Wallets.FirstOrDefault(w => w.WalletId == walletId);

        if (wallet == null)
            return null;

        return new Wallet { WalletId = wallet.WalletId, Balance = wallet.Balance };
    }

    public void Update(Wallet wallet)
    {
        if (wallet == null)
            return;

        var existing = Wallets.FirstOrDefault(w => w.WalletId == wallet.WalletId);

        if (existing != null)
            existing.Balance = wallet.Balance;
    }

    public bool Exists(string walletId)
    {
        return Wallets.Any(w => w.WalletId == walletId);
    }
}
