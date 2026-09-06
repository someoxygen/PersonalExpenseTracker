using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
namespace ExpenseTracker.IntegrationTests;

public sealed class CategoryTests(DatabaseFixture database) : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task Crud_validation_system_rules_and_user_isolation()
    {
        await using var factory = database.CreateFactory();
        using var a = factory.CreateClient(); using var b = factory.CreateClient();
        await DatabaseFixture.Register(a); await DatabaseFixture.Register(b);
        var defaults = await a.GetFromJsonAsync<JsonElement>("/api/categories");
        Assert.Equal(15, defaults.GetArrayLength());
        var systemId = defaults[0].GetProperty("id").GetGuid();
        Assert.Equal(HttpStatusCode.BadRequest, (await a.DeleteAsync($"/api/categories/{systemId}")).StatusCode);
        var request = new { name = "Custom", type = "Expense", icon = "tag", color = "#123456" };
        var created = await a.PostAsJsonAsync("/api/categories", request);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        Assert.Equal(HttpStatusCode.Conflict, (await a.PostAsJsonAsync("/api/categories", request)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await a.PostAsJsonAsync("/api/categories", request with { name = "" })).StatusCode);
        Assert.Equal(15, (await b.GetFromJsonAsync<JsonElement>("/api/categories")).GetArrayLength());
        Assert.Equal(HttpStatusCode.NotFound, (await b.GetAsync($"/api/categories/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await b.PutAsJsonAsync($"/api/categories/{id}", request)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await b.DeleteAsync($"/api/categories/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await a.PutAsJsonAsync($"/api/categories/{id}", request with { name = "Updated" })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await a.DeleteAsync($"/api/categories/{id}")).StatusCode);
    }
}
