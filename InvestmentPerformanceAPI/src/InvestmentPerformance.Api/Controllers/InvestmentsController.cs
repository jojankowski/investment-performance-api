using System.ComponentModel.DataAnnotations;
using InvestmentPerformance.Api.Interfaces;
using InvestmentPerformance.Api.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentPerformance.Api.Controllers;

// Only translates HTTP to service calls and back. [ApiController] turns failed validation into a 400.
[ApiController]
[Route("api/users/{userId:int}/investments")]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public class InvestmentsController(IInvestmentService investmentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InvestmentSummary>>> GetInvestments(
        [Range(1, int.MaxValue)] int userId,
        CancellationToken cancellationToken)
    {
        var investments = await investmentService.GetInvestmentsAsync(userId, cancellationToken);
        return investments is null ? NotFound() : Ok(investments);
    }

    [HttpGet("{investmentId:int}")]
    public async Task<ActionResult<InvestmentDetails>> GetInvestmentDetails(
        [Range(1, int.MaxValue)] int userId,
        [Range(1, int.MaxValue)] int investmentId,
        CancellationToken cancellationToken)
    {
        var details = await investmentService.GetInvestmentDetailsAsync(userId, investmentId, cancellationToken);
        return details is null ? NotFound() : Ok(details);
    }
}