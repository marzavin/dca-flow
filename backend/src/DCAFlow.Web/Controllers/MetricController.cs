using DCAFlow.Core.Metrics;
using Microsoft.AspNetCore.Mvc;

namespace DCAFlow.Web.Controllers;

[ApiController]
[Route("api/metrics")]
public class MetricController : ControllerBase
{
    private readonly MetricService _metricService;

    public MetricController(MetricService metricService)
    {
        _metricService = metricService ?? throw new ArgumentNullException(nameof(metricService));
    }

    [HttpGet]
    [HttpGet("{tag}")]
    public async Task<IActionResult> GetMetricsAsync([FromRoute] string tag = null, CancellationToken cancellationToken = default)
    {
        var metrics = await _metricService.GetMetricsAsync(tag, cancellationToken);
        return Ok(metrics);
    }
}
