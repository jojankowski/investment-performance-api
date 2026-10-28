using InvestmentPerformance.Api.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

namespace InvestmentPerformance.Tests.Api;

// Runs the real API in memory against its own SQLite file, seeded as of the test date.
public class InvestmentApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"investments-test-{Guid.NewGuid():N}.db");

    /// <summary>Replaces the database price lookup, for tests that need prices to be missing.</summary>
    public IMarketPriceProvider? PriceProvider { get; init; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={_databasePath}");
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<TimeProvider>(new FakeTimeProvider(new DateTimeOffset(TestDates.AsOfDate, TimeOnly.MinValue, TimeSpan.Zero)));

            if (PriceProvider is not null)
            {
                services.AddSingleton(PriceProvider);
            }
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        // Pooled connections keep the file locked on Windows until the pool is cleared.
        SqliteConnection.ClearAllPools();
        File.Delete(_databasePath);
    }
}
