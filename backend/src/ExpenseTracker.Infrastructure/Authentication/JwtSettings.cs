namespace ExpenseTracker.Infrastructure.Authentication;

public sealed class JwtSettings
{
    public string Issuer { get; set; } = "ExpenseTracker";
    public string Audience { get; set; } = "ExpenseTracker.Web";
    public string Key { get; set; } = "";
    public int ExpirationMinutes { get; set; } = 15;
}
