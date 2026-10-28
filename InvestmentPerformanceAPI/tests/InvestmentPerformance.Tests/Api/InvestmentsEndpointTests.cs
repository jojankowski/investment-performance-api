using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using InvestmentPerformance.Api.Interfaces;
using InvestmentPerformance.Api.Models.Responses;
using Moq;

namespace InvestmentPerformance.Tests.Api;

// Each request goes through the full HTTP pipeline: routing, validation, JSON settings and error handling.
public class InvestmentsEndpointTests(InvestmentApiFactory factory) : IClassFixture<InvestmentApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetInvestments_KnownUser_ReturnsIdAndNameOrderedByName()
    {
        var investments = await _client.GetFromJsonAsync<List<InvestmentSummary>>("/api/users/1/investments");

        Assert.Equal(
            [
                new InvestmentSummary(1, "Big SOUP Incorporated"),
                new InvestmentSummary(3, "Piggy Bank Total Market Fund"),
                new InvestmentSummary(2, "Rainy Day 10 Year Bond")
            ],
            investments!);
    }

    [Theory]
    [InlineData("/api/users/99/investments")]  // unknown user
    [InlineData("/api/users/1/investments/4")] // belongs to user 2
    public async Task Get_MissingResource_Returns404ProblemDetails(string url)
    {
        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetInvestments_UserIdBelowOne_Returns400()
    {
        var response = await _client.GetAsync("/api/users/0/investments");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetInvestmentDetails_TwoLots_ReturnsPerformanceWithTermAsText()
    {
        var body = await _client.GetStringAsync("/api/users/1/investments/1");
        var details = JsonDocument.Parse(body).RootElement;

        Assert.Equal(15m, details.GetProperty("shares").GetDecimal());
        Assert.Equal(170m, details.GetProperty("costBasisPerShare").GetDecimal());
        Assert.Equal(230.5m, details.GetProperty("currentPrice").GetDecimal());
        Assert.Equal(3457.50m, details.GetProperty("currentValue").GetDecimal());
        Assert.Equal(907.50m, details.GetProperty("totalGainLoss").GetDecimal());
        Assert.Equal("Mixed", details.GetProperty("term").GetString());
        Assert.Contains("\"shares\":15,", body); // whole numbers come out without a trailing ".0"
    }

    [Fact]
    public async Task GetInvestmentDetails_NoCurrentPrice_Returns500WithoutInternalDetails()
    {
        var noPrices = new Mock<IMarketPriceProvider>();
        noPrices
            .Setup(p => p.GetCurrentPriceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((decimal?)null);
        using var failingApi = new InvestmentApiFactory { PriceProvider = noPrices.Object };

        var response = await failingApi.CreateClient().GetAsync("/api/users/1/investments/1");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Contains("An unexpected error occurred.", body);
        Assert.DoesNotContain("No current price", body);
    }
}
