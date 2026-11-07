using LoyaltyHub.Application.Common;

namespace LoyaltyHub.Application.Errors;

public static class ApplicationErrors
{
    public static ApplicationError InvalidRequest(string message, IReadOnlyList<string>? details = null) =>
        new("INVALID_REQUEST", message, ErrorType.Validation, details);

    public static ApplicationError WalletNotFound(string walletId) =>
        new("WALLET_NOT_FOUND", $"Wallet '{walletId}' was not found.", ErrorType.NotFound);

    public static ApplicationError ConflictingRetry =>
        new("AWARD_CONFLICT", "An award with the specified identity already exists with different parameters.", ErrorType.Conflict);
}

