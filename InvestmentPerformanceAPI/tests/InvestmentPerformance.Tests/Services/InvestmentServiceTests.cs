using InvestmentPerformance.Api.Interfaces;
using InvestmentPerformance.Api.Models.Calculations;
using InvestmentPerformance.Api.Models.Entities;
using InvestmentPerformance.Api.Models.Responses;
using InvestmentPerformance.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace InvestmentPerformance.Tests.Services;

public class InvestmentServiceTests
{
    private static readonly DateOnly AsOfDate = TestDates.AsOfDate;

    // Strict mocks fail the test if the service makes a call that wasn't set up.
    private readonly Mock<IInvestmentRepository> _investmentRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IMarketPriceProvider> _marketPriceProviderMock = new(MockBehavior.Strict);
    private readonly Mock<IPerformanceCalculator> _performanceCalculatorMock = new(MockBehavior.Strict);

    private InvestmentService CreateSut() => new(
        _investmentRepositoryMock.Object,
        _marketPriceProviderMock.Object,
        _performanceCalculatorMock.Object,
        new FakeTimeProvider(new DateTimeOffset(AsOfDate, TimeOnly.MinValue, TimeSpan.Zero)),
        NullLogger<InvestmentService>.Instance);

    [Fact]
    public async Task GetInvestmentsAsync_UnknownUser_ReturnsNull()
    {
        _investmentRepositoryMock.Setup(r => r.UserExistsAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await CreateSut().GetInvestmentsAsync(99, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetInvestmentsAsync_KnownUser_ReturnsIdAndNameOfEach()
    {
        _investmentRepositoryMock.Setup(r => r.UserExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _investmentRepositoryMock.Setup(r => r.GetInvestmentsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Investment> { CreateInvestment() });

        var result = await CreateSut().GetInvestmentsAsync(1, CancellationToken.None);

        Assert.Equal([new InvestmentSummary(1, "Big SOUP Incorporated")], result!);
    }

    // An existing user with nothing invested gets an empty list, not the null that means "no such user".
    [Fact]
    public async Task GetInvestmentsAsync_KnownUserWithNoInvestments_ReturnsEmptyList()
    {
        _investmentRepositoryMock.Setup(r => r.UserExistsAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _investmentRepositoryMock.Setup(r => r.GetInvestmentsAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var result = await CreateSut().GetInvestmentsAsync(3, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetInvestmentDetailsAsync_UnknownInvestment_ReturnsNullWithoutFetchingPrice()
    {
        _investmentRepositoryMock.Setup(r => r.GetInvestmentAsync(1, 99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Investment?)null);

        var result = await CreateSut().GetInvestmentDetailsAsync(1, 99, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetInvestmentDetailsAsync_KnownInvestment_CombinesInvestmentWithPerformance()
    {
        var investment = CreateInvestment();
        var lot = investment.Lots[0];
        var performance = new PerformanceResult(10, 150m, 2305m, Term.Short, 805m, [new LotPerformance(lot, 2305m, Term.Short, 805m)]);
        _investmentRepositoryMock.Setup(r => r.GetInvestmentAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(investment);
        _marketPriceProviderMock.Setup(p => p.GetCurrentPriceAsync("SOUP", It.IsAny<CancellationToken>())).ReturnsAsync(230.50m);
        _performanceCalculatorMock.Setup(c => c.Calculate(investment.Lots, 230.50m, AsOfDate)).Returns(performance);

        var result = await CreateSut().GetInvestmentDetailsAsync(1, 1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Big SOUP Incorporated", result.Name);
        Assert.Equal("SOUP", result.Symbol);
        Assert.Equal(10, result.Shares);
        Assert.Equal(150m, result.CostBasisPerShare);
        Assert.Equal(230.50m, result.CurrentPrice);
        Assert.Equal(2305m, result.CurrentValue);
        Assert.Equal(Term.Short, result.Term);
        Assert.Equal(805m, result.TotalGainLoss);
        Assert.Equal(new LotDetails(1, lot.PurchaseDate, 10, 150m, 2305m, Term.Short, 805m), Assert.Single(result.Lots));
    }

    [Fact]
    public async Task GetInvestmentDetailsAsync_NoCurrentPrice_Throws()
    {
        _investmentRepositoryMock.Setup(r => r.GetInvestmentAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateInvestment());
        _marketPriceProviderMock.Setup(p => p.GetCurrentPriceAsync("SOUP", It.IsAny<CancellationToken>())).ReturnsAsync((decimal?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateSut().GetInvestmentDetailsAsync(1, 1, CancellationToken.None));
    }

    private static Investment CreateInvestment() => new()
    {
        Id = 1,
        UserId = 1,
        Security = new Security { Symbol = "SOUP", Name = "Big SOUP Incorporated" },
        Lots = [new Lot { Id = 1, Shares = 10, CostBasisPerShare = 150m, PurchaseDate = AsOfDate.AddMonths(-3) }]
    };
}