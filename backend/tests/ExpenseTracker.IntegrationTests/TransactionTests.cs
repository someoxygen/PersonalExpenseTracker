using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
namespace ExpenseTracker.IntegrationTests;

public sealed class TransactionTests(DatabaseFixture database) : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task Income_expense_crud_and_balance()
    {
        await using var factory = database.CreateFactory(); using var client = factory.CreateClient();
        await DatabaseFixture.Register(client);
        var account = await ApiData.Account(client); var expense = await ApiData.Category(client, "Expense"); var income = await ApiData.Category(client, "Income");
        var created = await ApiData.Create(client, "/api/transactions", ApiData.Transaction(account, expense, 100m));
        var id = created.GetProperty("id").GetGuid();
        await ApiData.Create(client, "/api/transactions", ApiData.Transaction(account, income, 500m, "Income"));
        Assert.Equal(1400m, await ApiData.Balance(client, account));
        Assert.Equal(HttpStatusCode.NoContent, (await client.PutAsJsonAsync($"/api/transactions/{id}", ApiData.Transaction(account, expense, 200m))).StatusCode);
        Assert.Equal(1300m, await ApiData.Balance(client, account));
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/transactions/{id}")).StatusCode);
        Assert.Equal(1500m, await ApiData.Balance(client, account));
        await ApiData.Create(client, "/api/transactions", ApiData.Transaction(account, expense, 100m, date: "2100-01-01"));
        Assert.Equal(1500m, await ApiData.Balance(client, account));
    }
    [Fact]
    public async Task Invalid_amount_mismatched_type_and_foreign_relations_are_rejected()
    {
        await using var factory = database.CreateFactory(); using var a = factory.CreateClient(); using var b = factory.CreateClient();
        await DatabaseFixture.Register(a); await DatabaseFixture.Register(b);
        var account = await ApiData.Account(a); var foreignAccount = await ApiData.Account(b);
        var category = await ApiData.Category(a, "Expense"); var foreignCategory = await ApiData.Category(b, "Expense");
        foreach (var amount in new[] { 0m, -1m, 1.001m })
            Assert.Equal(HttpStatusCode.BadRequest, (await a.PostAsJsonAsync("/api/transactions", ApiData.Transaction(account, category, amount))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await a.PostAsJsonAsync("/api/transactions", ApiData.Transaction(account, category, 1m, "Income"))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await a.PostAsJsonAsync("/api/transactions", ApiData.Transaction(foreignAccount, category, 1m))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await a.PostAsJsonAsync("/api/transactions", ApiData.Transaction(account, foreignCategory, 1m))).StatusCode);
        var id = (await ApiData.Create(a, "/api/transactions", ApiData.Transaction(account, category, 1m))).GetProperty("id").GetGuid();
        Assert.Equal(HttpStatusCode.NotFound, (await b.GetAsync($"/api/transactions/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await b.PutAsJsonAsync($"/api/transactions/{id}", ApiData.Transaction(foreignAccount, foreignCategory, 1m))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await b.DeleteAsync($"/api/transactions/{id}")).StatusCode);
        Assert.Equal(0, (await b.GetFromJsonAsync<JsonElement>("/api/transactions")).GetProperty("totalCount").GetInt32());
    }
    [Fact]
    public async Task Filters_sorting_and_pagination_execute_on_postgres()
    {
        await using var factory = database.CreateFactory(); using var client = factory.CreateClient();
        await DatabaseFixture.Register(client); var account = await ApiData.Account(client); var category = await ApiData.Category(client, "Expense");
        foreach (var amount in new[] { 10m, 20m, 30m }) await ApiData.Create(client, "/api/transactions", ApiData.Transaction(account, category, amount));
        var page = await client.GetFromJsonAsync<JsonElement>($"/api/transactions?page=2&pageSize=1&sortBy=amount&sortDirection=asc&accountId={account}&categoryId={category}&type=Expense&startDate=2026-01-01&endDate=2026-01-31&minAmount=10&maxAmount=30&search=grocer");
        Assert.Equal(3, page.GetProperty("totalCount").GetInt32()); Assert.Equal(3, page.GetProperty("totalPages").GetInt32());
        Assert.Equal(20m, page.GetProperty("items")[0].GetProperty("amount").GetDecimal());
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/transactions?sortBy=PasswordHash")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/transactions?pageSize=1000")).StatusCode);
    }
}
