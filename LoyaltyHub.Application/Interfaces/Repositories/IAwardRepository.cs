using LoyaltyHub.Domain.Entities;

namespace LoyaltyHub.Application.Interfaces.Repositories;

public interface IAwardRepository
{
    Award? GetByIdentity(string sourceId, string externalTransactionId);
    void Add(Award award);
}

