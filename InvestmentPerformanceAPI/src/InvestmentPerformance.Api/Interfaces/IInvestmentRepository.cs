using InvestmentPerformance.Api.Models.Entities;

namespace InvestmentPerformance.Api.Interfaces;

public interface IInvestmentRepository
{
    Task<bool> UserExistsAsync(int userId, CancellationToken cancellationToken);

    /// <summary>The user's investments that have at least one lot, with their security, ordered by name.</summary>
    Task<List<Investment>> GetInvestmentsAsync(int userId, CancellationToken cancellationToken);

    /// <summary>The investment with its security and lots, or null if it doesn't exist, has no lots, or belongs to another user.</summary>
    Task<Investment?> GetInvestmentAsync(int userId, int investmentId, CancellationToken cancellationToken);
}