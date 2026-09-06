using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
namespace ExpenseTracker.IntegrationTests;

public sealed class AccountTests(DatabaseFixture database) : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task Account_crud_deactivation_validation_and_isolation()
    {
        await using var factory = database.CreateFactory();
        using var a = factory.CreateClient(); using var b = factory.CreateClient();
        await DatabaseFixture.Register(a); await DatabaseFixture.Register(b);
        var request = new { name = "Bank", type = "Bank", initialBalance = 1000m, currency = "TRY", isActive = true };
        var response = await a.PostAsJsonAsync("/api/accounts", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var account = await response.Content.ReadFromJsonAsync<JsonElement>();
        var id = account.GetProperty("id").GetGuid();
        Assert.Equal(1000m, account.GetProperty("currentBalance").GetDecimal());
        Assert.Equal(0, (await b.GetFromJsonAsync<JsonElement>("/api/accounts")).GetArrayLength());
        Assert.Equal(HttpStatusCode.NotFound, (await b.GetAsync($"/api/accounts/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await b.PutAsJsonAsync($"/api/accounts/{id}", request)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await b.DeleteAsync($"/api/accounts/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await a.PostAsJsonAsync("/api/accounts", request with { name = "" })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await a.PostAsJsonAsync("/api/accounts", request with { initialBalance = 1.001m })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await a.PutAsJsonAsync($"/api/accounts/{id}", request with { name = "Updated" })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await a.DeleteAsync($"/api/accounts/{id}")).StatusCode);
        Assert.False((await a.GetFromJsonAsync<JsonElement>($"/api/accounts/{id}")).GetProperty("isActive").GetBoolean());
    }
}
