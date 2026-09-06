using ExpenseTracker.Domain.Entities;
using FluentValidation;
namespace ExpenseTracker.Application.Features.Accounts;

public sealed record AccountRequest(string Name, AccountType Type, decimal InitialBalance, string Currency = "TRY", bool IsActive = true);
public sealed record AccountResponse(Guid Id, string Name, AccountType Type, decimal InitialBalance, decimal CurrentBalance, string Currency, bool IsActive);
public sealed class AccountValidator : AbstractValidator<AccountRequest>
{
    public AccountValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.InitialBalance).PrecisionScale(18, 2, true);
        RuleFor(x => x.Currency).NotEmpty().Must(x => x is "TRY" or "USD" or "EUR");
    }
}
