using System.Text.Json.Serialization;

namespace LoyaltyHub.Application.DTOs;

public class AwardResponseDto
{
    public string AwardId { get; set; } = string.Empty;
    public string SourceId { get; set; } = string.Empty;
    public string ExternalTransactionId { get; set; } = string.Empty;
    public string WalletId { get; set; } = string.Empty;
    public int Points { get; set; }

    [JsonIgnore]
    public bool IsNewAward { get; set; }
}

