using System.Net.Http.Headers;
using System.Net.Http.Json;
using ExpenseTracker.Application.Features.Auth;
using ExpenseTracker.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ExpenseTracker.IntegrationTests;

public sealed class DatabaseFixture : IAsyncLifetime
{
    private readonly string name = "expense_tests_" + Guid.NewGuid().ToString("N");
    private readonly string admin = Environment.GetEnvironmentVariable("TEST_POSTGRES_CONNECTION")
        ?? "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres";
    public string ConnectionString => new NpgsqlConnectionStringBuilder(admin) { Database = name }.ConnectionString;
    public async Task InitializeAsync()
    {
        await using var connection = new NpgsqlConnection(admin);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", connection);
        await command.ExecuteNonQueryAsync();
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(ConnectionString).Options);
        await db.Database.MigrateAsync();
    }
    public async Task DisposeAsync()
    {
        NpgsqlConnection.ClearAllPools();
        await using var connection = new NpgsqlConnection(admin);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{name}\" WITH (FORCE)", connection);
        await command.ExecuteNonQueryAsync();
    }
    public WebApplicationFactory<Program> CreateFactory() => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:DefaultConnection", ConnectionString);
        builder.UseSetting("Jwt:Key", "test-only-signing-key-with-at-least-32-bytes");
    });
    public static async Task<AuthResponse> Register(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Test", "User", $"{Guid.NewGuid():N}@example.com", "A-long-test-password!"));
        response.EnsureSuccessStatusCode();
        var auth = (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        return auth;
    }
}
