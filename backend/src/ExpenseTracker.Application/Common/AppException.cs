namespace ExpenseTracker.Application.Common;

public sealed class AppException(int status, string message) : Exception(message)
{
    public int Status { get; } = status;
    public static AppException NotFound() => new(404, "Resource not found.");
    public static AppException Conflict(string message) => new(409, message);
}
