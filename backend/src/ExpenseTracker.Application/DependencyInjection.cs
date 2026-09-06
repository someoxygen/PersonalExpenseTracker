using ExpenseTracker.Application.Features.Auth;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
namespace ExpenseTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterValidator>();
        services.AddScoped<AuthService>();
        services.AddScoped<Features.Categories.CategoryService>();
        services.AddScoped<Features.Accounts.AccountService>();
        services.AddScoped<Features.Transactions.TransactionService>();
        services.AddScoped<Features.Transfers.TransferService>();
        services.AddScoped<Features.Budgets.BudgetService>();
        services.AddScoped<Features.Dashboard.DashboardService>();
        return services;
    }
}
