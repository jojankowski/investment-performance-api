using InvestmentPerformance.Api.Interfaces;
using InvestmentPerformance.Api.Models.Calculations;
using InvestmentPerformance.Api.Models.Entities;

namespace InvestmentPerformance.Api.Services;

public class PerformanceCalculator : IPerformanceCalculator
{
    // Bought 2025-08-29: still short term on 2026-08-29, long term from 2026-08-30.
    public Term GetTerm(DateOnly purchaseDate, DateOnly today) =>
        today > purchaseDate.AddYears(1) ? Term.Long : Term.Short;

    public PerformanceResult Calculate(IReadOnlyCollection<Lot> lots, decimal currentPrice, DateOnly today)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(currentPrice);

        if (lots.Count == 0)
        {
            throw new ArgumentException("At least one lot is required.", nameof(lots));
        }

        if (lots.Any(lot => lot.Shares <= 0 || lot.CostBasisPerShare < 0))
        {
            throw new ArgumentException("Every lot needs positive shares and a non-negative cost basis.", nameof(lots));
        }

        var lotPerformances = lots
            .OrderBy(lot => lot.PurchaseDate)
            .Select(lot => new LotPerformance(
                lot,
                RoundMoney(lot.Shares * currentPrice),
                GetTerm(lot.PurchaseDate, today),
                RoundMoney(lot.Shares * (currentPrice - lot.CostBasisPerShare))))
            .ToList();

        var shares = lots.Sum(lot => lot.Shares);
        var amountPaid = lots.Sum(lot => lot.Shares * lot.CostBasisPerShare);
        var currentValue = shares * currentPrice;
        var terms = lotPerformances.Select(lot => lot.Term).Distinct().ToList();

        return new PerformanceResult(
            shares,
            RoundPrice(amountPaid / shares),
            RoundMoney(currentValue),
            terms.Count == 1 ? terms[0] : Term.Mixed,
            RoundMoney(currentValue - amountPaid),
            lotPerformances);
    }

    // Money rounds to cents. Per-share prices keep four decimal places.
    private static decimal RoundMoney(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);

    private static decimal RoundPrice(decimal price) => Math.Round(price, 4, MidpointRounding.AwayFromZero);
}