using DCAFlow.Contracts.Enums;
using DCAFlow.Contracts.Models;
using FluentValidation;

namespace DCAFlow.Web.Validators;

public sealed class TransactionModelValidator : AbstractValidator<TransactionModel>
{
    private readonly List<CoinModel> _supportedCoins;

    public TransactionModelValidator(List<CoinModel> supportedCoins)
    {
        _supportedCoins = supportedCoins ?? throw new ArgumentNullException(nameof(supportedCoins));

        RuleFor(x => x.Ticker).NotEmpty().Must(x => _supportedCoins.Any(y => y.Ticker == x));
        RuleFor(x => x.PortfolioId).GreaterThan(0);
        RuleFor(x => x.Type).Must(x => Enum.GetValues<TransactionType>().Contains(x));
        RuleFor(x => x.Amount).GreaterThan(0D);
        RuleFor(x => x.Cost).Null().When(x => x.Type == TransactionType.TransferIn || x.Type == TransactionType.TransferOut);
        RuleFor(x => x.Cost).NotNull().When(x => x.Type == TransactionType.Sell || x.Type == TransactionType.Buy);
        RuleFor(x => x.Cost).GreaterThan(0D).When(x => x.Type == TransactionType.Sell || x.Type == TransactionType.Buy);
    }
}
