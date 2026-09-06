using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using ExpenseTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
namespace ExpenseTracker.IntegrationTests;

public sealed class SecurityTests(DatabaseFixture database) : IClassFixture<DatabaseFixture>
{
    [Theory]
    [InlineData("/api/accounts")]
    [InlineData("/api/categories")]
    [InlineData("/api/transactions")]
    [InlineData("/api/transfers")]
    [InlineData("/api/budgets")]
    [InlineData("/api/dashboard/summary")]
    public async Task Financial_endpoints_require_authentication(string path)
    {
        await using var factory = database.CreateFactory(); using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(path)).StatusCode);
    }
    [Fact]
    public async Task Invalid_signature_expiry_issuer_audience_and_overposting_are_rejected()
    {
        await using var factory = database.CreateFactory(); using var client = factory.CreateClient();
        var auth = await DatabaseFixture.Register(client);
        var raw = new JwtSecurityTokenHandler().ReadJwtToken(auth.Token);
        Assert.DoesNotContain(raw.Claims, c => c.Type.Contains("email", StringComparison.OrdinalIgnoreCase) || c.Type.Contains("password", StringComparison.OrdinalIgnoreCase));
        const string key = "test-only-signing-key-with-at-least-32-bytes";
        foreach (var variant in new[] { "signature", "expiry", "issuer", "audience" })
        {
            var token = new JwtSecurityToken(variant == "issuer" ? "Wrong" : "ExpenseTracker", variant == "audience" ? "Wrong" : "ExpenseTracker.Web",
                [new Claim("sub", auth.User.Id.ToString())], DateTime.UtcNow.AddHours(-2),
                variant == "expiry" ? DateTime.UtcNow.AddHours(-1) : DateTime.UtcNow.AddMinutes(5),
                new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(variant == "signature" ? key + "wrong" : key)), SecurityAlgorithms.HmacSha256));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(token));
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
        }
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        using var other = factory.CreateClient(); var foreign = await DatabaseFixture.Register(other);
        await ApiData.Create(client, "/api/accounts", new { name = "Owned", type = "Bank", initialBalance = 0, currency = "TRY", userId = foreign.User.Id });
        Assert.Equal(0, (await other.GetFromJsonAsync<JsonElement>("/api/accounts")).GetArrayLength());
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(database.ConnectionString).Options);
        var user = await db.Users.FindAsync(auth.User.Id);
        Assert.NotEqual("A-long-test-password!", user!.PasswordHash);
        Assert.DoesNotContain("passwordHash", JsonSerializer.Serialize(auth), StringComparison.OrdinalIgnoreCase);
    }
    [Fact]
    public async Task Concurrent_duplicate_budgets_have_exactly_one_winner()
    {
        await using var factory = database.CreateFactory(); using var client = factory.CreateClient();
        await DatabaseFixture.Register(client); var category = await ApiData.Category(client, "Expense");
        var request = new { categoryId = category, amount = 100m, month = 1, year = 2026 };
        var responses = await Task.WhenAll(client.PostAsJsonAsync("/api/budgets", request), client.PostAsJsonAsync("/api/budgets", request));
        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Created);
        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Conflict);
    }
    [Fact]
    public async Task Database_rejects_cross_owner_fk_and_referenced_category_deletion()
    {
        await using var factory = database.CreateFactory(); using var a = factory.CreateClient(); using var b = factory.CreateClient();
        var owner = await DatabaseFixture.Register(a); await DatabaseFixture.Register(b);
        var account = await ApiData.Account(a); var foreignAccount = await ApiData.Account(b);
        var category = (await ApiData.Create(a, "/api/categories", new { name = "Restricted", type = "Expense", icon = "tag", color = "#123456" })).GetProperty("id").GetGuid();
        var transaction = await ApiData.Create(a, "/api/transactions", ApiData.Transaction(account, category, 10m));
        Assert.Equal(HttpStatusCode.Conflict, (await a.DeleteAsync($"/api/categories/{category}")).StatusCode);
        await using var connection = new NpgsqlConnection(database.ConnectionString); await connection.OpenAsync();
        await using var command = new NpgsqlCommand("UPDATE \"Transactions\" SET \"AccountId\" = @account WHERE \"Id\" = @id AND \"UserId\" = @user", connection);
        command.Parameters.AddWithValue("account", foreignAccount); command.Parameters.AddWithValue("id", transaction.GetProperty("id").GetGuid()); command.Parameters.AddWithValue("user", owner.User.Id);
        var exception = await Assert.ThrowsAsync<PostgresException>(() => command.ExecuteNonQueryAsync());
        Assert.Equal("23503", exception.SqlState);
    }
}
