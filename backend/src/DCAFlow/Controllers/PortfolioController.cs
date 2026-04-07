using DCAFlow.Contracts.Models;
using DCAFlow.Services;
using Microsoft.AspNetCore.Mvc;

namespace DCAFlow.Controllers;

[Route("api/portfolios")]
public class PortfolioController : ControllerBase
{
    private readonly PortfolioService _portfolioService;

    public PortfolioController(PortfolioService portfolioService)
    {
        _portfolioService = portfolioService ?? throw new ArgumentNullException(nameof(portfolioService));
    }

    [HttpGet("{portfolioId:int}")]
    public async Task<IActionResult> GetPortfolioAsync(int portfolioId, CancellationToken cancellationToken = default)
    {
        var model = await _portfolioService.GetPortfolioByIdAsync(portfolioId, cancellationToken);
        return Ok(model);
    }

    [HttpPost("{portfolioId:int}/transactions")]
    public async Task<IActionResult> PostTransaction([FromBody] TransactionModel model, CancellationToken cancellationToken = default)
    {
        return NoContent();
    }
}
