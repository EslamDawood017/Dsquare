namespace LoyaltyHub.Application.Common;

public class ApplicationError
{
    public string Code { get; }
    public string Message { get; }
    public ErrorType Type { get; }
    public IReadOnlyList<string>? Details { get; }

    public ApplicationError(string code, string message, ErrorType type, IReadOnlyList<string>? details = null)
    {
        Code = code;
        Message = message;
        Type = type;
        Details = details;
    }
}

