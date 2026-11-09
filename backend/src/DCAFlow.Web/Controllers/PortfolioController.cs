using DCAFlow.Core.Metrics;
using DCAFlow.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace DCAFlow.Web.Controllers;

[ApiController]
[Route("api/portfolios")]
public class PortfolioController : ControllerBase
{
    private readonly PortfolioService _portfolioService;

    private readonly MetricService _metricService;

    public PortfolioController(
        PortfolioService portfolioService,
        MetricService metricService)
    {
        _portfolioService = portfolioService ?? throw new ArgumentNullException(nameof(portfolioService));
        _metricService = metricService ?? throw new ArgumentNullException(nameof(metricService));
    }

    [HttpGet("{portfolioId:int}")]
    public async Task<IActionResult> GetPortfolioAsync(int portfolioId, CancellationToken cancellationToken = default)
    {
        var model = await _portfolioService.GetPortfolioByIdAsync(portfolioId, cancellationToken);
        return Ok(model);
    }

    [HttpGet("{portfolioId:int}/metrics")]
    public async Task<IActionResult> GetMetricsAsync(int portfolioId, CancellationToken cancellationToken = default)
    {
        var metrics = await _metricService.GetPortfolioMetricsAsync(portfolioId, cancellationToken);
        return Ok(metrics);
    }

    [HttpPost("import/csv")]
    public async Task<IActionResult> ImportPortfolioAsCsvAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        using var stream = file.OpenReadStream();
        await _portfolioService.ImportPortfolioAsCsvFileAsync(file.Name, stream);
        return NoContent();
    }
}
