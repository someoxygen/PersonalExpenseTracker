using FluentValidation;
namespace ExpenseTracker.Application.Features.Auth;

public sealed record RegisterRequest(string FirstName, string LastName, string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record UserResponse(Guid Id, string FirstName, string LastName, string Email, string Currency);
public sealed record AuthResponse(string Token, DateTimeOffset ExpiresAt, UserResponse User);
public sealed class RegisterValidator : AbstractValidator<RegisterRequest>
{
    public RegisterValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(12).MaximumLength(128);
    }
}
public sealed class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(128);
    }
}
