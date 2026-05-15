using DCAFlow.Contracts.Enums;
using DCAFlow.Contracts.Models;
using DCAFlow.Core.Services;
using DCAFlow.Web.Models;
using DCAFlow.Web.Validators;
using Microsoft.AspNetCore.Mvc;

namespace DCAFlow.Web.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly TransactionService _transactionService;

    private readonly CoinService _coinService;

    public TransactionsController(TransactionService transactionService, CoinService coinService)
    {
        _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
        _coinService = coinService ?? throw new ArgumentNullException(nameof(coinService));
    }

    [HttpPost]
    public async Task<IActionResult> AddTransaction([FromBody] TransactionModel model, CancellationToken cancellationToken = default)
    {
        var validator = new TransactionModelValidator(_coinService.GetSupportedCoins());
        var validationResult = await validator.ValidateAsync(model, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new BadRequestModel
            {
                Errors = validationResult.Errors?.Select(x => new ErrorModel { Key = x.PropertyName, Message = x.ErrorMessage }).ToList()
            });
        }

        await _transactionService.AddTransactionAsync(model, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{transactionId}")]
    public async Task<IActionResult> DeleteTransaction([FromRoute] int transactionId, CancellationToken cancellationToken = default)
    {
        await _transactionService.DeleteTransactionAsync(transactionId, cancellationToken);

        return NoContent();
    }

    [HttpGet("types")]
    public IActionResult GetSupportedTransactionTypes()
    {
        return Ok(Enum.GetValues<TransactionType>().Select(x => new KeyValueModel<int, string> { Key = (int)x, Value = x.ToString() }));
    }
}
