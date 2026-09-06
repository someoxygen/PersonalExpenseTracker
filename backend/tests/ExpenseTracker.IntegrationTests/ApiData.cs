using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
namespace ExpenseTracker.IntegrationTests;

internal static class ApiData
{
    public static async Task<JsonElement> Create(HttpClient client, string path, object request)
    {
        var response = await client.PostAsJsonAsync(path, request);
        Assert.True(response.StatusCode == HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }
    public static async Task<Guid> Account(HttpClient client, decimal balance = 1000m) =>
        (await Create(client, "/api/accounts", new { name = "Bank", type = "Bank", initialBalance = balance, currency = "TRY" })).GetProperty("id").GetGuid();
    public static async Task<Guid> Category(HttpClient client, string type) =>
        (await client.GetFromJsonAsync<JsonElement>("/api/categories")).EnumerateArray().First(x => x.GetProperty("type").GetString() == type).GetProperty("id").GetGuid();
    public static object Transaction(Guid account, Guid category, decimal amount, string type = "Expense", string date = "2026-01-15", string description = "Groceries") =>
        new { accountId = account, categoryId = category, type, amount, transactionDate = date, description };
    public static async Task<decimal> Balance(HttpClient client, Guid id) =>
        (await client.GetFromJsonAsync<JsonElement>($"/api/accounts/{id}")).GetProperty("currentBalance").GetDecimal();
}
