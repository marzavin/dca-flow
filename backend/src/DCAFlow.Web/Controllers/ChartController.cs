using DCAFlow.Core.Charts;
using Microsoft.AspNetCore.Mvc;

namespace DCAFlow.Web.Controllers;

[ApiController]
[Route("api/charts")]
public class ChartController : ControllerBase
{
    private readonly ChartService _chartService;

    public ChartController(ChartService chartService)
    {
        _chartService = chartService ?? throw new ArgumentNullException(nameof(chartService));
    }

    [HttpGet("allocation")]
    [HttpGet("allocation/{tag}")]
    public async Task<IActionResult> GetAllocationChartAsync([FromRoute] string tag = null, CancellationToken cancellationToken = default)
    {
        var metrics = await _chartService.GetAllocationChartAsync(tag, cancellationToken);
        return Ok(metrics);
    }

    [HttpGet("performance")]
    [HttpGet("performance/{tag}")]
    public async Task<IActionResult> GetPerformanceChartAsync([FromRoute] string tag = null, CancellationToken cancellationToken = default)
    {
        var metrics = await _chartService.GetPerformanceChartAsync(tag, cancellationToken);
        return Ok(metrics);
    }

    [HttpGet("annualized-return")]
    [HttpGet("annualized-return/{tag}")]
    public async Task<IActionResult> GetAnnualizedReturnChartAsync([FromRoute] string tag = null, CancellationToken cancellationToken = default)
    {
        var metrics = await _chartService.GetAnnualizedReturnChartAsync(tag, cancellationToken);
        return Ok(metrics);
    }
}
