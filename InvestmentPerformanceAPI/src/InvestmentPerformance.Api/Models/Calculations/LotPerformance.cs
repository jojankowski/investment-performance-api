using InvestmentPerformance.Api.Models.Entities;

namespace InvestmentPerformance.Api.Models.Calculations;

/// <summary>
/// Performance of a single lot. Each lot is rounded to cents on its own, and added together
/// the lot gains can be a cent or two off the investment's total.
/// </summary>
public record LotPerformance(Lot Lot, decimal CurrentValue, Term Term, decimal GainLoss);
