using InvestmentPerformance.Api.Interfaces;
using InvestmentPerformance.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPerformance.Api.Data;

public class InvestmentRepository(AppDbContext db) : IInvestmentRepository
{
    public Task<bool> UserExistsAsync(int userId, CancellationToken cancellationToken) =>
        db.Users.AnyAsync(u => u.Id == userId, cancellationToken);

    public async Task<IReadOnlyList<Investment>> GetInvestmentsAsync(int userId, CancellationToken cancellationToken) =>
        await CurrentInvestments(userId)
            .OrderBy(i => i.Security.Name)
            .ToListAsync(cancellationToken);

    public Task<Investment?> GetInvestmentAsync(int userId, int investmentId, CancellationToken cancellationToken) =>
        CurrentInvestments(userId)
            .Include(i => i.Lots)
            .FirstOrDefaultAsync(i => i.Id == investmentId, cancellationToken);

    // An investment counts as current while it has at least one lot. Because this filters by user,
    // asking for someone else's investment finds nothing and becomes a 404 instead of a 403.
    private IQueryable<Investment> CurrentInvestments(int userId) =>
        db.Investments
            .AsNoTracking()
            .Include(i => i.Security)
            .Where(i => i.UserId == userId && i.Lots.Any());
}