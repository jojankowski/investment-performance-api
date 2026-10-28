using InvestmentPerformance.Api.Utilities;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPerformance.Api.Data;

// Hosted services finish starting before the server takes requests. That makes this the place to set up the database.
public class DatabaseInitializer(
    IServiceScopeFactory scopeFactory,
    IHostEnvironment environment,
    TimeProvider clock,
    ILogger<DatabaseInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // DbContext is scoped. A singleton like this one can't take it through the constructor and has to open a scope.
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Locally, the demo database is rebuilt on every start so purchase dates stay relative to today.
        // Anywhere else, existing data is never deleted.
        if (environment.IsDevelopment())
        {
            await db.Database.EnsureDeletedAsync(cancellationToken);
        }

        await db.Database.EnsureCreatedAsync(cancellationToken);
        if (await db.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var today = clock.GetUtcToday();
        db.Users.AddRange(SeedData.Create(today));
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded demo data for {Today}", today);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}