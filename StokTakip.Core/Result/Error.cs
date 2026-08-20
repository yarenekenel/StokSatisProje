namespace StokTakip.Core.Result;

public enum ErrorType
{
    Failure,
    NotFound,
    Validation,
    Conflict
}

public sealed class Error
{
    public string Message { get; }
    public ErrorType Type { get; }

    private Error(string message, ErrorType type)
    {
        Message = message;
        Type = type;
    }

    public static Error Failure(string message) => new(message, ErrorType.Failure);
    public static Error NotFound(string message) => new(message, ErrorType.NotFound);
    public static Error Validation(string message) => new(message, ErrorType.Validation);
    public static Error Conflict(string message) => new(message, ErrorType.Conflict);
}