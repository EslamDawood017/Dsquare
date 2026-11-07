using LoyaltyHub.Application.Interfaces.Repositories;
using LoyaltyHub.Domain.Entities;

namespace LoyaltyHub.Infrastructure.Repositories;

public class AwardRepository : IAwardRepository
{
    public static readonly List<Award> Awards = new();

    static AwardRepository()
    {
        Reset();
    }

    public static void Reset()
    {
        Awards.Clear();
    }

    public Award? GetByIdentity(string sourceId, string externalTransactionId)
    {
        var award = Awards.FirstOrDefault(a =>
            a.SourceId == sourceId &&
            a.ExternalTransactionId == externalTransactionId);

        if (award == null)
            return null;

        return new Award
        {
            AwardId = award.AwardId,
            SourceId = award.SourceId,
            ExternalTransactionId = award.ExternalTransactionId,
            WalletId = award.WalletId,
            Points = award.Points,
            CreatedAt = award.CreatedAt
        };
    }

    public void Add(Award award)
    {
        if (award == null)
            return;

        Awards.Add(new Award
        {
            AwardId = award.AwardId,
            SourceId = award.SourceId,
            ExternalTransactionId = award.ExternalTransactionId,
            WalletId = award.WalletId,
            Points = award.Points,
            CreatedAt = award.CreatedAt
        });
    }
}
