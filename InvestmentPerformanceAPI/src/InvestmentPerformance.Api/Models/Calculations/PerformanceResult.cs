namespace InvestmentPerformance.Api.Models.Calculations;

/// <summary>
/// Performance of one investment across all of its lots. Cost basis per share is an average
/// weighted by shares, where larger purchases count for more. Lots are listed oldest first.
/// </summary>
public record PerformanceResult(
    decimal Shares,
    decimal CostBasisPerShare,
    decimal CurrentValue,
    Term Term,
    decimal TotalGainLoss,
    IReadOnlyList<LotPerformance> Lots);
