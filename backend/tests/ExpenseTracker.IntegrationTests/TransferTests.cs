using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
namespace ExpenseTracker.IntegrationTests;

public sealed class TransferTests(DatabaseFixture database) : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task Transfer_is_paired_changes_balances_and_validates_ownership()
    {
        await using var factory = database.CreateFactory(); using var a = factory.CreateClient(); using var b = factory.CreateClient();
        await DatabaseFixture.Register(a); await DatabaseFixture.Register(b);
        var source = await ApiData.Account(a); var target = await ApiData.Account(a, 0); var foreign = await ApiData.Account(b);
        var request = new { sourceAccountId = source, targetAccountId = target, amount = 200m, transactionDate = "2026-01-15", description = "ATM" };
        var transfer = await ApiData.Create(a, "/api/transfers", request);
        Assert.Equal(800m, await ApiData.Balance(a, source)); Assert.Equal(200m, await ApiData.Balance(a, target));
        Assert.Equal(0, (await a.GetFromJsonAsync<JsonElement>("/api/transactions")).GetProperty("totalCount").GetInt32());
        Assert.Equal(HttpStatusCode.NotFound, (await b.GetAsync($"/api/transfers/{transfer.GetProperty("id").GetGuid()}")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await a.PostAsJsonAsync("/api/transfers", request with { targetAccountId = source })).StatusCode);
        foreach (var amount in new[] { 0m, -1m }) Assert.Equal(HttpStatusCode.BadRequest, (await a.PostAsJsonAsync("/api/transfers", request with { amount = amount })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await a.PostAsJsonAsync("/api/transfers", request with { sourceAccountId = foreign })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await a.PostAsJsonAsync("/api/transfers", request with { targetAccountId = foreign })).StatusCode);
    }
    [Fact]
    public async Task Failure_after_insert_rolls_back_the_whole_transfer()
    {
        await using var factory = database.CreateFactory().WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddDbContext<AppDbContext>(options => options.AddInterceptors(new FailAfterTransferSave()))));
        using var client = factory.CreateClient(); await DatabaseFixture.Register(client);
        var source = await ApiData.Account(client); var target = await ApiData.Account(client, 0);
        var response = await client.PostAsJsonAsync("/api/transfers", new { sourceAccountId = source, targetAccountId = target, amount = 50m, transactionDate = "2026-01-15" });
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(1000m, await ApiData.Balance(client, source)); Assert.Equal(0m, await ApiData.Balance(client, target));
        Assert.Equal(0, (await client.GetFromJsonAsync<JsonElement>("/api/transfers")).GetProperty("totalCount").GetInt32());
    }
    private sealed class FailAfterTransferSave : SaveChangesInterceptor
    {
        public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context!.ChangeTracker.Entries<Transfer>().Any()) throw new InvalidOperationException("Injected failure after insert, before commit.");
            return ValueTask.FromResult(result);
        }
    }
}
