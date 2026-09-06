using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
namespace ExpenseTracker.IntegrationTests;

public sealed class BudgetTests(DatabaseFixture database) : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task Budget_crud_calculations_duplicates_and_isolation()
    {
        await using var factory = database.CreateFactory(); using var a = factory.CreateClient(); using var b = factory.CreateClient();
        await DatabaseFixture.Register(a); await DatabaseFixture.Register(b);
        var category = await ApiData.Category(a, "Expense"); var income = await ApiData.Category(a, "Income"); var foreignCategory = await ApiData.Category(b, "Expense");
        var account = await ApiData.Account(a);
        await ApiData.Create(a, "/api/transactions", ApiData.Transaction(account, category, 250m));
        var request = new { categoryId = category, amount = 1000m, month = 1, year = 2026 };
        var budget = await ApiData.Create(a, "/api/budgets", request); var id = budget.GetProperty("id").GetGuid();
        Assert.Equal(250m, budget.GetProperty("spent").GetDecimal()); Assert.Equal(750m, budget.GetProperty("remaining").GetDecimal()); Assert.Equal(25m, budget.GetProperty("percentage").GetDecimal());
        Assert.Equal(HttpStatusCode.Conflict, (await a.PostAsJsonAsync("/api/budgets", request)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await a.PostAsJsonAsync("/api/budgets", request with { categoryId = income })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await a.PostAsJsonAsync("/api/budgets", request with { categoryId = foreignCategory })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await b.GetAsync($"/api/budgets/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await b.PutAsJsonAsync($"/api/budgets/{id}", request)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await b.DeleteAsync($"/api/budgets/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await a.PutAsJsonAsync($"/api/budgets/{id}", request with { amount = 200m })).StatusCode);
        Assert.Equal(125m, (await a.GetFromJsonAsync<JsonElement>($"/api/budgets/{id}")).GetProperty("percentage").GetDecimal());
        var target = await ApiData.Account(a, 0);
        await ApiData.Create(a, "/api/transfers", new { sourceAccountId = account, targetAccountId = target, amount = 100m, transactionDate = "2026-01-15" });
        Assert.Equal(250m, (await a.GetFromJsonAsync<JsonElement>($"/api/budgets/{id}")).GetProperty("spent").GetDecimal());
        await ApiData.Create(a, "/api/transactions", ApiData.Transaction(account, category, 100m, date: "2100-01-01"));
        var future = await ApiData.Create(a, "/api/budgets", request with { year = 2100 });
        Assert.Equal(0m, future.GetProperty("spent").GetDecimal());
        Assert.Equal(HttpStatusCode.NoContent, (await a.DeleteAsync($"/api/budgets/{id}")).StatusCode);
    }
}
