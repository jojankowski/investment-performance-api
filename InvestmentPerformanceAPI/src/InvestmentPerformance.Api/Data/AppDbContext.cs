using InvestmentPerformance.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPerformance.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Security> Securities => Set<Security>();
    public DbSet<Investment> Investments => Set<Investment>();

    // Applies to every property of these types instead of configuring each one by hand.
    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.Properties<decimal>().HavePrecision(18, 4);
        builder.Properties<string>().HaveMaxLength(200);
        builder.Properties<AssetType>().HaveConversion<string>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Security>().HasIndex(s => s.Symbol).IsUnique();

        // A user holds each security once. Repeat purchases are extra lots, not extra investments.
        modelBuilder.Entity<Investment>().HasIndex(i => new { i.UserId, i.SecurityId }).IsUnique();
    }
}