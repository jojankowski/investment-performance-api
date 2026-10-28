using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;

namespace InvestmentPerformance.Tests;

// Runs the real API against a private in-memory database, seeded with the demo data as of a fixed date.
public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString = $"Data Source=test-{Guid.NewGuid()};Mode=Memory;Cache=Shared";
    private readonly SqliteConnection _connection;

    public ApiFactory()
    {
        // SQLite deletes an in-memory database once its last connection closes. This one stays open for the whole test run.
        _connection = new SqliteConnection(_connectionString);
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Outside Development the initializer only seeds an empty database and never deletes one.
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", _connectionString);
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(new FakeTimeProvider(new DateTimeOffset(TestDates.AsOfDate, TimeOnly.MinValue, TimeSpan.Zero)));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
    }
}