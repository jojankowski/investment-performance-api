using InvestmentPerformance.Api.Models.Calculations;

namespace InvestmentPerformance.Api.Models.Responses;

/// <summary>The performance of one investment, with each purchase (lot) listed oldest first.</summary>
/// <param name="CostBasisPerShare">Average price paid per share, weighted by the size of each purchase.</param>
/// <param name="Term">Short or Long when every lot agrees, otherwise Mixed.</param>
public record InvestmentDetails(
    int Id,
    string Name,
    string Symbol,
    decimal Shares,
    decimal CostBasisPerShare,
    decimal CurrentPrice,
    decimal CurrentValue,
    Term Term,
    decimal TotalGainLoss,
    IReadOnlyList<LotDetails> Lots);