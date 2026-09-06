using ExpenseTracker.Application.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
namespace ExpenseTracker.Infrastructure.Authentication;

public sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<object> hasher = new(Options.Create(new PasswordHasherOptions { IterationCount = 210_000 }));
    private readonly object subject = new();
    private readonly string dummyHash;
    public PasswordService() => dummyHash = Hash(Guid.NewGuid().ToString());
    public string Hash(string password) => hasher.HashPassword(subject, password);
    public bool Verify(string password, string? hash) =>
        hasher.VerifyHashedPassword(subject, hash ?? dummyHash, password) != PasswordVerificationResult.Failed;
}
