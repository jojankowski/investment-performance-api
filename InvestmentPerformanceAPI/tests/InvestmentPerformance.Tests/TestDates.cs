using Microsoft.Extensions.Time.Testing;

namespace InvestmentPerformance.Tests;

// Tests run as if this were the current date. Terms and purchase dates come out the same whenever they run.
internal static class TestDates
{
    public static readonly DateOnly AsOfDate = new(2026, 8, 29);

    /// <summary>A clock that is always at midnight UTC on the as-of date.</summary>
    public static FakeTimeProvider CreateClock() => new(new DateTimeOffset(AsOfDate, TimeOnly.MinValue, TimeSpan.Zero));
}
