using System.Net;
using System.Net.Http.Json;
using ExpenseTracker.Application.Features.Auth;

namespace ExpenseTracker.IntegrationTests;

public sealed class AuthTests(DatabaseFixture database) : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task Register_login_me_and_normalized_duplicate_email()
    {
        await using var factory = database.CreateFactory();
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
        var auth = await DatabaseFixture.Register(client);
        Assert.Equal("TRY", auth.User.Currency);
        var me = await client.GetFromJsonAsync<UserResponse>("/api/auth/me");
        Assert.Equal(auth.User.Id, me!.Id);
        var duplicate = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("A", "B", auth.User.Email.ToLowerInvariant(), "A-long-test-password!"));
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(auth.User.Email.ToLowerInvariant(), "A-long-test-password!"));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var bad = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(auth.User.Email, "wrong"));
        var missing = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("unknown@example.com", "wrong"));
        Assert.Equal(HttpStatusCode.Unauthorized, bad.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, missing.StatusCode);
    }
    [Fact]
    public async Task Weak_password_and_invalid_email_are_rejected()
    {
        await using var factory = database.CreateFactory();
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("A", "B", "invalid", "short"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("errors", await response.Content.ReadAsStringAsync());
    }
}
