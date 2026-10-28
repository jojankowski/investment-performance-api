using InvestmentPerformance.Api.Models.Entities;

namespace InvestmentPerformance.Api.Models.Calculations;

/// <summary>
/// Performance of a single lot. Values are rounded to cents per lot, so the lot gains
/// can be a cent or two off the investment's total.
/// </summary>
public record LotPerformance(Lot Lot, decimal CurrentValue, Term Term, decimal GainLoss);
