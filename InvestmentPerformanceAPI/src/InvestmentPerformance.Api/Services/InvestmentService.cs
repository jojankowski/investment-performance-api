using InvestmentPerformance.Api.Interfaces;
using InvestmentPerformance.Api.Models.Calculations;
using InvestmentPerformance.Api.Models.Entities;
using InvestmentPerformance.Api.Models.Responses;
using InvestmentPerformance.Api.Utilities;

namespace InvestmentPerformance.Api.Services;

public class InvestmentService(
    IInvestmentRepository investmentRepository,
    IMarketPriceProvider marketPriceProvider,
    IPerformanceCalculator performanceCalculator,
    TimeProvider clock,
    ILogger<InvestmentService> logger) : IInvestmentService
{
    public async Task<IReadOnlyList<InvestmentSummary>?> GetInvestmentsAsync(int userId, CancellationToken cancellationToken)
    {
        if (!await investmentRepository.UserExistsAsync(userId, cancellationToken))
        {
            logger.LogInformation("User {UserId} not found", userId);
            return null;
        }

        var investments = await investmentRepository.GetInvestmentsAsync(userId, cancellationToken);
        logger.LogInformation("Fetched {Count} investments for user {UserId}", investments.Count, userId);

        return [.. investments.Select(i => new InvestmentSummary(i.Id, i.Security.Name))];
    }

    public async Task<InvestmentDetails?> GetInvestmentDetailsAsync(int userId, int investmentId, CancellationToken cancellationToken)
    {
        var investment = await investmentRepository.GetInvestmentAsync(userId, investmentId, cancellationToken);
        if (investment is null)
        {
            logger.LogInformation("Investment {InvestmentId} not found for user {UserId}", investmentId, userId);
            return null;
        }

        var symbol = investment.Security.Symbol;
        var currentPrice = await marketPriceProvider.GetCurrentPriceAsync(symbol, cancellationToken)
            ?? throw new InvalidOperationException($"No current price for {symbol}.");

        var performance = performanceCalculator.Calculate(investment.Lots, currentPrice, clock.GetUtcToday());
        logger.LogInformation("Calculated performance for investment {InvestmentId} of user {UserId}", investmentId, userId);

        return ToDetails(investment, currentPrice, performance);
    }

    // Combines what the investment is (id, name, symbol) with how it's performing.
    private static InvestmentDetails ToDetails(Investment investment, decimal currentPrice, PerformanceResult performance) => new(
        investment.Id,
        investment.Security.Name,
        investment.Security.Symbol,
        performance.Shares,
        performance.CostBasisPerShare,
        currentPrice,
        performance.CurrentValue,
        performance.Term,
        performance.TotalGainLoss,
        [.. performance.Lots.Select(lot => new LotDetails(
            lot.Lot.Id,
            lot.Lot.PurchaseDate,
            lot.Lot.Shares,
            lot.Lot.CostBasisPerShare,
            lot.CurrentValue,
            lot.Term,
            lot.GainLoss))
        ]);
}