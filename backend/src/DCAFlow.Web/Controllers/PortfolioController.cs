using DCAFlow.Core.Services;
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

    [HttpPost("import/csv")]
    public async Task<IActionResult> ImportPortfolioAsCsvAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        using var stream = file.OpenReadStream();
        await _portfolioService.ImportPortfolioAsCsvFileAsync(file.Name, stream);
        return NoContent();
    }
}
