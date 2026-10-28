using InvestmentPerformance.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPerformance.Api.Data;

// Reads the last stored price. A real market data feed would be another IMarketPriceProvider.
public class DatabaseMarketPriceProvider(AppDbContext db) : IMarketPriceProvider
{
    // The decimal? cast makes an unknown symbol come back as null instead of 0.
    public Task<decimal?> GetCurrentPriceAsync(string symbol, CancellationToken cancellationToken) =>
        db.Securities
            .Where(s => s.Symbol == symbol)
            .Select(s => (decimal?)s.CurrentPrice)
            .FirstOrDefaultAsync(cancellationToken);
}