using System.ComponentModel.DataAnnotations;

namespace LoyaltyHub.Application.DTOs;

public class AwardRequestDto
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "sourceId is required.")]
    public string SourceId { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "externalTransactionId is required.")]
    public string ExternalTransactionId { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "walletId is required.")]
    public string WalletId { get; set; } = string.Empty;

    [Range(1, 1000, ErrorMessage = "points must be an integer between 1 and 1000.")]
    public int Points { get; set; }
}
