using InvestmentPerformance.Api.Models.Responses;

namespace InvestmentPerformance.Api.Interfaces;

public interface IInvestmentService
{
    /// <summary>The user's investments, or null if the user doesn't exist.</summary>
    Task<IReadOnlyList<InvestmentSummary>?> GetInvestmentsAsync(int userId, CancellationToken cancellationToken);

    /// <summary>The investment's performance, or null if the user has no current investment with this id.</summary>
    /// <exception cref="InvalidOperationException">There is no current price for the investment's security.</exception>
    Task<InvestmentDetails?> GetInvestmentDetailsAsync(int userId, int investmentId, CancellationToken cancellationToken);
}