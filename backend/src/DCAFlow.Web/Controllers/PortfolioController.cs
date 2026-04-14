using DCAFlow.Contracts.Models;
using DCAFlow.Web.Models;
using DCAFlow.Web.Services;
using DCAFlow.Web.Validators;
using Microsoft.AspNetCore.Mvc;

namespace DCAFlow.Controllers;

[ApiController]
[Route("api/portfolios")]
public class PortfolioController : ControllerBase
{
    private readonly PortfolioService _portfolioService;

    private readonly CoinService _coinService;

    public PortfolioController(PortfolioService portfolioService, CoinService coinService)
    {
        _portfolioService = portfolioService ?? throw new ArgumentNullException(nameof(portfolioService));
        _coinService = coinService ?? throw new ArgumentNullException(nameof(coinService));
    }

    [HttpGet("{portfolioId:int}")]
    public async Task<IActionResult> GetPortfolioAsync(int portfolioId, CancellationToken cancellationToken = default)
    {
        var model = await _portfolioService.GetPortfolioByIdAsync(portfolioId, cancellationToken);
        return Ok(model);
    }

    //TODO: Move to TransactionController
    [HttpPost("{portfolioId:int}/transactions")]
    public async Task<IActionResult> AddTransaction([FromRoute] int portfolioId, [FromBody] TransactionModel model, CancellationToken cancellationToken = default)
    {
        model.PortfolioId = portfolioId;

        var validator = new TransactionModelValidator(_coinService.GetSupportedCoins());
        var validationResult = await validator.ValidateAsync(model, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new BadRequestModel 
            { 
                Errors = validationResult.Errors?.Select(x => new ErrorModel { Key = x.PropertyName, Message = x.ErrorMessage }).ToList()
            });
        }

        await _portfolioService.AddTransactionAsync(model);

        return NoContent();
    }

    [HttpDelete("{portfolioId:int}/transactions/{transactionId}")]
    public async Task<IActionResult> DeleteTransaction([FromRoute] int portfolioId, [FromRoute] int transactionId, CancellationToken cancellationToken = default)
    {
        //TODO: Validate portfolio id
        await _portfolioService.DeleteTransactionAsync(transactionId);

        return NoContent();
    }
}
