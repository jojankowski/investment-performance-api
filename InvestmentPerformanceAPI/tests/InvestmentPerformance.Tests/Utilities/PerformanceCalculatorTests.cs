using InvestmentPerformance.Api.Models.Calculations;
using InvestmentPerformance.Api.Models.Entities;
using InvestmentPerformance.Api.Utilities;

namespace InvestmentPerformance.Tests.Utilities;

public class PerformanceCalculatorTests
{
    private static readonly DateOnly Today = new(2026, 8, 29);

    private readonly PerformanceCalculator _calculator = new();

    [Theory]
    [InlineData("2026-08-29", Term.Short)] // bought today
    [InlineData("2025-08-29", Term.Short)] // exactly one year
    [InlineData("2025-08-28", Term.Long)]  // one year and a day
    public void GetTerm_ByHoldingPeriod_ReturnsExpectedTerm(string purchased, Term expected)
    {
        Assert.Equal(expected, _calculator.GetTerm(DateOnly.Parse(purchased), Today));
    }

    [Fact]
    public void Calculate_SingleLotWithGain_ReturnsTotals()
    {
        var result = _calculator.Calculate(CreateLots((10, 100, "2026-01-01")), currentPrice: 150m, Today);

        Assert.Equal(10, result.Shares);
        Assert.Equal(100m, result.CostBasisPerShare);
        Assert.Equal(1500m, result.CurrentValue);
        Assert.Equal(500m, result.TotalGainLoss);
        Assert.Equal(Term.Short, result.Term);
    }

    [Fact]
    public void Calculate_SingleLotWithLoss_ReturnsNegativeGainAndLongTerm()
    {
        var result = _calculator.Calculate(CreateLots((4, 50, "2020-01-01")), currentPrice: 30m, Today);

        Assert.Equal(-80m, result.TotalGainLoss);
        Assert.Equal(Term.Long, result.Term);
    }

    [Fact]
    public void Calculate_MultipleLots_ReturnsWeightedCostBasisAndMixedTerm()
    {
        var lots = CreateLots((10, 100, "2024-01-10"), (30, 200, "2026-01-10"));

        var result = _calculator.Calculate(lots, currentPrice: 180m, Today);

        Assert.Equal(40, result.Shares);
        Assert.Equal(175m, result.CostBasisPerShare);
        Assert.Equal(7200m, result.CurrentValue);
        Assert.Equal(200m, result.TotalGainLoss);
        Assert.Equal(Term.Mixed, result.Term);
        Assert.Equal([Term.Long, Term.Short], result.Lots.Select(l => l.Term));
        Assert.Equal([800m, -600m], result.Lots.Select(l => l.GainLoss));
    }

    [Fact]
    public void Calculate_FractionalShares_RoundsMoneyToCents()
    {
        var result = _calculator.Calculate(CreateLots((12.5, 130, "2026-01-01")), currentPrice: 142.85m, Today);

        Assert.Equal(1785.63m, result.CurrentValue);
        Assert.Equal(160.63m, result.TotalGainLoss);
    }

    [Fact]
    public void Calculate_NoLots_Throws()
    {
        Assert.Throws<ArgumentException>(() => _calculator.Calculate(CreateLots(), currentPrice: 10m, Today));
    }

    [Theory]
    [InlineData(0, 100)]  // zero shares would divide by zero
    [InlineData(-5, 100)] // negative shares
    [InlineData(5, -1)]   // negative cost basis
    public void Calculate_InvalidLot_Throws(double shares, double costBasisPerShare)
    {
        var lots = CreateLots((shares, costBasisPerShare, "2026-01-01"));

        Assert.Throws<ArgumentException>(() => _calculator.Calculate(lots, currentPrice: 10m, Today));
    }

    [Fact]
    public void Calculate_NegativeCurrentPrice_Throws()
    {
        var lots = CreateLots((10, 100, "2026-01-01"));

        Assert.Throws<ArgumentOutOfRangeException>(() => _calculator.Calculate(lots, currentPrice: -1m, Today));
    }

    // InlineData doesn't accept decimals. Test values start out as doubles and are converted here.
    private static List<Lot> CreateLots(params (double Shares, double CostBasisPerShare, string PurchaseDate)[] lots) =>
    [
        .. lots.Select((lot, index) => new Lot
        {
            Id = index + 1,
            Shares = (decimal)lot.Shares,
            CostBasisPerShare = (decimal)lot.CostBasisPerShare,
            PurchaseDate = DateOnly.Parse(lot.PurchaseDate)
        })
    ];
}