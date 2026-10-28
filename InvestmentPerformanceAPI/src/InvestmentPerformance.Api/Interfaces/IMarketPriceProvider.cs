namespace InvestmentPerformance.Api.Interfaces;

public interface IMarketPriceProvider
{
    /// <summary>The current price per share, or null if there is no price for the symbol.</summary>
    Task<decimal?> GetCurrentPriceAsync(string symbol, CancellationToken cancellationToken);
}