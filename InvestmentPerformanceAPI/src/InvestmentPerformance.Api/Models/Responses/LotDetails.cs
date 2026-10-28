using InvestmentPerformance.Api.Models.Calculations;

namespace InvestmentPerformance.Api.Models.Responses;

/// <summary>One purchase within an investment.</summary>
/// <param name="CostBasisPerShare">The price paid per share in this purchase.</param>
public record LotDetails(
    int Id,
    DateOnly PurchaseDate,
    decimal Shares,
    decimal CostBasisPerShare,
    decimal CurrentValue,
    Term Term,
    decimal GainLoss);