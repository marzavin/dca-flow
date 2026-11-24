using DCAFlow.Core.Assets;
using Microsoft.AspNetCore.Mvc;

namespace DCAFlow.Web.Controllers;

[ApiController]
[Route("api/assets")]
public class AssetController : ControllerBase
{
    private readonly AssetService _assetService;

    public AssetController(AssetService assetService)
    {
        _assetService = assetService ?? throw new ArgumentNullException(nameof(assetService));
    }

    [HttpGet]
    [HttpGet("{tag}")]
    public async Task<IActionResult> GetAllocationChartAsync([FromRoute] string tag = null, CancellationToken cancellationToken = default)
    {
        var metrics = await _assetService.GetAssetsAsync(tag, cancellationToken);
        return Ok(metrics);
    }
}
