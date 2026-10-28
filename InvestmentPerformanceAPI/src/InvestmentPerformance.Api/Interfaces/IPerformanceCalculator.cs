using InvestmentPerformance.Api.Models.Calculations;
using InvestmentPerformance.Api.Models.Entities;

namespace InvestmentPerformance.Api.Interfaces;

public interface IPerformanceCalculator
{
	/// <summary>Held one year or less is short term, anything longer is long term.</summary>
	Term GetTerm(DateOnly purchaseDate, DateOnly today);

	/// <exception cref="ArgumentException">There are no lots, or a lot has non-positive shares or a negative cost basis.</exception>
	/// <exception cref="ArgumentOutOfRangeException">The current price is negative.</exception>
	PerformanceResult Calculate(IReadOnlyCollection<Lot> lots, decimal currentPrice, DateOnly today);
}
