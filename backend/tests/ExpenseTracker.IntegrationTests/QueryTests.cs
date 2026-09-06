using System.Data.Common;
using ExpenseTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
namespace ExpenseTracker.IntegrationTests;

public sealed class QueryTests(DatabaseFixture database) : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task Account_balances_use_one_sql_query_and_dashboard_has_bounded_query_count()
    {
        var recorder = new Recorder();
        await using var factory = database.CreateFactory().WithWebHostBuilder(b => b.ConfigureServices(s =>
            s.AddDbContext<AppDbContext>(o => o.AddInterceptors(recorder))));
        using var client = factory.CreateClient(); await DatabaseFixture.Register(client);
        for (var i = 0; i < 4; i++) await ApiData.Account(client);
        recorder.Queries.Clear();
        (await client.GetAsync("/api/accounts")).EnsureSuccessStatusCode();
        Assert.Single(recorder.Queries); Assert.Contains("sum(", recorder.Queries[0], StringComparison.OrdinalIgnoreCase);
        recorder.Queries.Clear();
        (await client.GetAsync("/api/dashboard/summary")).EnsureSuccessStatusCode();
        Assert.InRange(recorder.Queries.Count, 1, 5);
    }
    private sealed class Recorder : DbCommandInterceptor
    {
        public List<string> Queries { get; } = [];
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        { Queries.Add(command.CommandText); return ValueTask.FromResult(result); }
    }
}
