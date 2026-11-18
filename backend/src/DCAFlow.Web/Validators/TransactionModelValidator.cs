using DCAFlow.Contracts.Models;
using DCAFlow.Core.Transactions.Enums;
using DCAFlow.Core.Transactions.Models;
using FluentValidation;
using SideEffect.Extensions;

namespace DCAFlow.Web.Validators;

public sealed class TransactionModelValidator : AbstractValidator<TransactionModel>
{
    private readonly List<CoinModel> _supportedCoins;

    public TransactionModelValidator(List<CoinModel> supportedCoins)
    {
        _supportedCoins = supportedCoins ?? throw new ArgumentNullException(nameof(supportedCoins));

        RuleFor(x => x.Ticker)
            .NotEmpty()
            .Must(x => _supportedCoins.Any(y => y.Ticker == x));
        
        RuleFor(x => x)
            .Must(x => _supportedCoins.FirstOrDefault(y => y.Ticker == x.Ticker)?.Networks?.Contains(x.Network) ?? false)
            .When(x => !x.Ticker.IsEmpty() && !x.Network.IsEmpty());
        
        RuleFor(x => x.Type)
            .Must(x => Enum.GetValues<TransactionType>().Contains(x));
        
        RuleFor(x => x.Amount)
            .GreaterThan(0D);
        
        RuleFor(x => x.Cost)
            .Equal(0)
            .When(x => x.Type == TransactionType.TransferIn || x.Type == TransactionType.TransferOut);
        
        RuleFor(x => x.Cost)
            .NotNull()
            .When(x => x.Type == TransactionType.Sell || x.Type == TransactionType.Buy);
        
        RuleFor(x => x.Cost)
            .GreaterThan(0D).When(x => x.Type == TransactionType.Sell || x.Type == TransactionType.Buy);
    }
}
