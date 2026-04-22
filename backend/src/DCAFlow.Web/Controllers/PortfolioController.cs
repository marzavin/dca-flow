using DCAFlow.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DCAFlow.Web.Controllers;

[ApiController]
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
}
