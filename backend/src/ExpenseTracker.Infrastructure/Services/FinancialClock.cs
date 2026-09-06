using ExpenseTracker.Application.Abstractions;
using Microsoft.Extensions.Configuration;
namespace ExpenseTracker.Infrastructure.Services;

public sealed class FinancialClock(TimeProvider clock, IConfiguration configuration) : IFinancialClock
{
    private readonly TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById(configuration["Finance:TimeZone"] ?? "Europe/Istanbul");
    public DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(clock.GetUtcNow(), zone).DateTime);
}
