using DCAFlow.Core.Coins;
using Microsoft.AspNetCore.Mvc;

namespace DCAFlow.Web.Controllers;

[ApiController]
[Route("api/coins")]
public class CoinController : ControllerBase
{
    private readonly CoinService _coinService;

    public CoinController(CoinService coinService)
    {
        _coinService = coinService ?? throw new ArgumentNullException(nameof(coinService));
    }

    [HttpGet]
    public async Task<IActionResult> GetSupportedCoinsAsync()
    {
        var supportedCoins = _coinService.GetSupportedCoins();
        return Ok(supportedCoins);
    }
}
