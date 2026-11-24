using DCAFlow.Core.Assets.Models;
using DCAFlow.Core.Coins.Models;
using DCAFlow.Core.ExchangeRates;
using DCAFlow.Core.Helpers;
using DCAFlow.Core.Transactions.Enums;
using DCAFlow.Data.Entities;
using DCAFlow.Data.Filters;
using DCAFlow.Data.Repositories;

namespace DCAFlow.Core.Assets;

public sealed class AssetService
{
    private readonly TransactionRepository _transactionRepository;

    private readonly ExchangeRateStorage _exchangeRateStorage;

    public AssetService(TransactionRepository transactionRepository, ExchangeRateStorage exchangeRateStorage)
    {
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _exchangeRateStorage = exchangeRateStorage ?? throw new ArgumentNullException(nameof(exchangeRateStorage));
    }

    public async Task<List<AssetModel>> GetAssetsAsync(string tag, CancellationToken cancellationToken = default)
    {
        var filter = new TransactionFilter { TagEq = TagHelper.NormalizeTag(tag) };
        var transactions = await _transactionRepository.SearchAsync(filter, cancellationToken);

        var tickers = transactions.Select(x => x.Ticker).Distinct().ToList();
        var minTransactionTimestamp = transactions.Min(x => x.Timestamp);

        await _exchangeRateStorage.InitializeAsync(minTransactionTimestamp, tickers, cancellationToken);

        var assets = new List<AssetModel>();

        var grouped = transactions.GroupBy(x => x.Ticker).ToDictionary(x => x.Key, x => x.ToList());
        foreach (var tickerGroup in grouped)
        {
            var asset = await GetAssetAsync(tickerGroup.Value, cancellationToken);
            assets.Add(asset);
        }

        return assets;
    }

    private async Task<AssetModel> GetAssetAsync(List<TransactionEntity> transactions, CancellationToken cancellationToken)
    {
        var ticker = transactions.First().Ticker;
        var exchangeRate = await _exchangeRateStorage.GetLatestExchangeRateAsync(ticker, cancellationToken);

        var model = new AssetModel
        {
            Ticker = ticker,
            MarketPrice = exchangeRate,
            Amount = 0D,
            TotalInvestments = 0D
        };

        var orderedTransactions = transactions.OrderBy(x => x.Timestamp).ToList();

        foreach (var transaction in orderedTransactions)
        {
            model.Amount += GetImpactOnAmount(transaction);
            model.TotalInvestments += GetImpactOnInvestments(transaction);
        }

        model.AverageBuyPrice = CalculateAveragePrice(model.TotalInvestments, model.Amount);
        model.TotalHoldings = CalculateTotalHoldings(model.Amount, model.MarketPrice);
        model.ReturnOfInvestments = CalculateReturnOfInvestments(model.TotalInvestments, model.TotalHoldings);
        model.ProfitAndLoss = model.TotalHoldings - model.TotalInvestments;

        return model;
    }

    public static double CalculateTotalHoldings(double amount, double price)
    {
        return price * amount;
    }

    public static double CalculateAveragePrice(double investments, double amount)
    {
        return investments / amount;
    }

    public static double CalculateReturnOfInvestments(double investments, double holdings)
    {
        if (investments < double.Epsilon)
        {
            return 1D;
        }

        return (holdings - investments) / investments;
    }

    public static double GetImpactOnInvestments(TransactionEntity transaction)
    {
        return (TransactionType)transaction.Type switch
        {
            TransactionType.Buy => transaction.Cost,
            TransactionType.Sell => -1D * transaction.Cost,
            TransactionType.TransferIn => 0D,
            TransactionType.TransferOut => 0D,
            _ => 0D
        };
    }

    public static double GetImpactOnAmount(TransactionEntity transaction)
    {
        return (TransactionType)transaction.Type switch
        {
            TransactionType.Buy => transaction.Amount,
            TransactionType.Sell => -1D * transaction.Amount,
            TransactionType.TransferIn => transaction.Amount,
            TransactionType.TransferOut => -1D * transaction.Amount,
            _ => 0D
        };
    }
}
