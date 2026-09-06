using System.Net.Http.Json;
using System.Text.Json;
using ExpenseTracker.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
namespace ExpenseTracker.IntegrationTests;

public sealed class DashboardTests(DatabaseFixture database) : IClassFixture<DatabaseFixture>
{
    private sealed class Clock : IFinancialClock { public DateOnly Today => new(2026, 9, 5); }
    [Theory]
    [InlineData(0, 0)]
    [InlineData(500, 0)]
    [InlineData(0, 100)]
    [InlineData(500, 100)]
    public async Task Sql_dashboard_totals_distribution_months_and_isolation(int income, int expense)
    {
        await using var factory = database.CreateFactory().WithWebHostBuilder(b => b.ConfigureServices(s => s.AddSingleton<IFinancialClock, Clock>()));
        using var a = factory.CreateClient(); using var other = factory.CreateClient();
        await DatabaseFixture.Register(a); await DatabaseFixture.Register(other);
        var account = await ApiData.Account(a, 0); var incomeCategory = await ApiData.Category(a, "Income"); var expenseCategory = await ApiData.Category(a, "Expense");
        if (income > 0) await ApiData.Create(a, "/api/transactions", ApiData.Transaction(account, incomeCategory, income, "Income", "2026-09-01"));
        if (expense > 0) await ApiData.Create(a, "/api/transactions", ApiData.Transaction(account, expenseCategory, expense, date: "2026-09-02"));
        await ApiData.Create(a, "/api/transactions", ApiData.Transaction(account, expenseCategory, 900m, date: "2026-09-30"));
        var target = await ApiData.Account(a, 0);
        await ApiData.Create(a, "/api/transfers", new { sourceAccountId = account, targetAccountId = target, amount = 50m, transactionDate = "2026-09-02" });
        var summary = await a.GetFromJsonAsync<JsonElement>("/api/dashboard/summary");
        Assert.Equal(income - expense, summary.GetProperty("currentBalance").GetDecimal());
        Assert.Equal(income, summary.GetProperty("thisMonthIncome").GetDecimal());
        Assert.Equal(expense, summary.GetProperty("thisMonthExpense").GetDecimal());
        var distribution = await a.GetFromJsonAsync<JsonElement>("/api/dashboard/category-expenses");
        Assert.Equal(expense > 0 ? 1 : 0, distribution.GetArrayLength());
        if (expense > 0) Assert.Equal(100m, distribution[0].GetProperty("percentage").GetDecimal());
        var months = await a.GetFromJsonAsync<JsonElement>("/api/dashboard/monthly");
        Assert.Equal(6, months.GetArrayLength()); Assert.Equal(income, months[5].GetProperty("income").GetDecimal());
        Assert.Equal(expense, months[5].GetProperty("expense").GetDecimal());
        var isolated = await other.GetFromJsonAsync<JsonElement>("/api/dashboard/summary");
        Assert.Equal(0m, isolated.GetProperty("currentBalance").GetDecimal());
        Assert.Equal(0, (await other.GetFromJsonAsync<JsonElement>("/api/dashboard/recent-transactions")).GetArrayLength());
    }
}
