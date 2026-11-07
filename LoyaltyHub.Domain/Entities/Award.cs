namespace LoyaltyHub.Domain.Entities;

public class Award
{
    public string AwardId { get; set; } = string.Empty;
    public string SourceId { get; set; } = string.Empty;
    public string ExternalTransactionId { get; set; } = string.Empty;
    public string WalletId { get; set; } = string.Empty;
    public int Points { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
