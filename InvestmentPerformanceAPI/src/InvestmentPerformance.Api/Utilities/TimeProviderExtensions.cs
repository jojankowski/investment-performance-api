namespace InvestmentPerformance.Api.Utilities;

public static class TimeProviderExtensions
{
    /// <summary>Today's date in UTC. Terms are worked out from this date.</summary>
    public static DateOnly GetUtcToday(this TimeProvider clock) =>
        DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
}