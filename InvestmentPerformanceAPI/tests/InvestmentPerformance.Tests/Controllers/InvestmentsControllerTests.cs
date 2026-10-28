using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using InvestmentPerformance.Api.Interfaces;
using InvestmentPerformance.Api.Models.Calculations;
using InvestmentPerformance.Api.Models.Responses;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace InvestmentPerformance.Tests.Controllers;

// Expected values come from SeedData as of TestDates.AsOfDate.
public class InvestmentsControllerTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetInvestments_UserWithInvestments_ReturnsIdAndNameInNameOrder()
    {
        var investments = await _client.GetFromJsonAsync<List<InvestmentSummary>>("/api/users/1/investments", JsonOptions);

        Assert.Equal(
            [
                new InvestmentSummary(1, "Big SOUP Incorporated"),
                new InvestmentSummary(3, "Piggy Bank Total Market Fund"),
                new InvestmentSummary(2, "Rainy Day 10 Year Bond")
            ],
            investments!);
    }

    [Fact]
    public async Task GetInvestments_UserWithNoInvestments_ReturnsEmptyList()
    {
        var investments = await _client.GetFromJsonAsync<List<InvestmentSummary>>("/api/users/3/investments", JsonOptions);

        Assert.Empty(investments!);
    }

    [Fact]
    public async Task GetInvestmentDetails_TwoLots_ReturnsTotalsAndMixedTerm()
    {
        var details = await _client.GetFromJsonAsync<InvestmentDetails>("/api/users/1/investments/1", JsonOptions);

        Assert.Equal("SOUP", details!.Symbol);
        Assert.Equal(15, details.Shares);
        Assert.Equal(170m, details.CostBasisPerShare);
        Assert.Equal(230.50m, details.CurrentPrice);
        Assert.Equal(3457.50m, details.CurrentValue);
        Assert.Equal(907.50m, details.TotalGainLoss);
        Assert.Equal(Term.Mixed, details.Term);
        Assert.Equal(2, details.Lots.Count);
    }

    [Fact]
    public async Task GetInvestmentDetails_WholeNumberShares_HaveNoTrailingZero()
    {
        var body = await _client.GetStringAsync("/api/users/1/investments/1");

        Assert.Contains("\"shares\":15,", body);
    }

    [Fact]
    public async Task GetInvestmentDetails_BoughtExactlyOneYearAgo_IsShortTerm()
    {
        var details = await _client.GetFromJsonAsync<InvestmentDetails>("/api/users/2/investments/4", JsonOptions);

        Assert.Equal(Term.Short, details!.Term);
        Assert.Equal(-118.40m, details.TotalGainLoss);
    }

    [Theory]
    [InlineData("/api/users/99/investments")]    // unknown user
    [InlineData("/api/users/1/investments/999")] // unknown investment
    [InlineData("/api/users/1/investments/4")]   // belongs to user 2
    public async Task Get_NotFound_ReturnsProblemDetails(string url)
    {
        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData("/api/users/0/investments")]
    [InlineData("/api/users/1/investments/-1")]
    public async Task Get_InvalidId_ReturnsBadRequest(string url)
    {
        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetInvestmentDetails_UnexpectedError_Returns500WithoutInternalDetails()
    {
        // A price provider with no prices makes the service throw, which the global handler must turn into a 500.
        var noPrices = new Mock<IMarketPriceProvider>();
        noPrices.Setup(p => p.GetCurrentPriceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((decimal?)null);
        using var brokenFactory = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IMarketPriceProvider>();
            services.AddSingleton(noPrices.Object);
        }));

        var response = await brokenFactory.CreateClient().GetAsync("/api/users/1/investments/1");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.DoesNotContain("No current price", await response.Content.ReadAsStringAsync());
    }
}