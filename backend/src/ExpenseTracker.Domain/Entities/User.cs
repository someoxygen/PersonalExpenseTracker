using ExpenseTracker.Domain.Common;
namespace ExpenseTracker.Domain.Entities;

public sealed class User : Entity
{
    private User() { }
    public string FirstName { get; private set; } = "";
    public string LastName { get; private set; } = "";
    public string Email { get; private set; } = "";
    public string PasswordHash { get; private set; } = "";
    public string Currency { get; private set; } = "TRY";
    public static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();
    public static User Create(string firstName, string lastName, string email, string passwordHash) => new()
    {
        FirstName = Rules.Text(firstName, 100),
        LastName = Rules.Text(lastName, 100),
        Email = NormalizeEmail(Rules.Text(email, 254)),
        PasswordHash = Rules.Text(passwordHash, 1024)
    };
}
