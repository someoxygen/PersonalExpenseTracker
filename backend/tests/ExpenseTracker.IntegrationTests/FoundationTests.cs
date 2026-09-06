using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ExpenseTracker.IntegrationTests;

public sealed class FoundationTests
{
    private static WebApplicationFactory<Program> CreateFactory(string environment = "Development") =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(environment);
            builder.UseSetting("Jwt:Key", "test-only-signing-key-with-at-least-32-bytes");
            builder.UseSetting("ConnectionStrings:DefaultConnection",
                "Host=127.0.0.1;Port=1;Database=Unavailable;Username=test;Password=test;Timeout=1");
            builder.UseSetting("Cors:AllowedOrigins:0", "http://localhost:5173");
        });

    [Fact]
    public async Task Liveness_does_not_require_a_database()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/health/live");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Readiness_returns_unavailable_without_leaking_connection_details()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("Unhealthy", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("http://localhost:5173", true)]
    [InlineData("https://untrusted.example", false)]
    public async Task Cors_only_allows_configured_origins(string origin, bool allowed)
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/health/live");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "GET");
        using var response = await client.SendAsync(request);
        Assert.Equal(allowed, response.Headers.Contains("Access-Control-Allow-Origin"));
        if (allowed)
            Assert.Equal(origin, Assert.Single(response.Headers.GetValues("Access-Control-Allow-Origin")));
    }

    [Theory]
    [InlineData("/openapi/v1.json")]
    [InlineData("/swagger/index.html")]
    public async Task Api_documentation_is_development_only(string path)
    {
        await using var development = CreateFactory();
        using var developmentClient = development.CreateClient();
        using var devResponse = await developmentClient.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, devResponse.StatusCode);
        if (path == "/openapi/v1.json")
        {
            using var document = System.Text.Json.JsonDocument.Parse(await devResponse.Content.ReadAsStringAsync());
            var root = document.RootElement;
            Assert.Equal("bearer", root.GetProperty("components").GetProperty("securitySchemes").GetProperty("Bearer").GetProperty("scheme").GetString());
            Assert.True(root.GetProperty("paths").GetProperty("/api/accounts").GetProperty("get").TryGetProperty("security", out _));
            Assert.False(root.GetProperty("paths").GetProperty("/api/auth/login").GetProperty("post").TryGetProperty("security", out _));
        }

        await using var production = CreateFactory("Production");
        using var productionClient = production.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        using var prodResponse = await productionClient.GetAsync(path);
        Assert.Equal(HttpStatusCode.Unauthorized, prodResponse.StatusCode);
        Assert.Equal("application/problem+json", prodResponse.Content.Headers.ContentType?.MediaType);
    }
}
