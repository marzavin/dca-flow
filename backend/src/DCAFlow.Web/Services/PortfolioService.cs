using DCAFlow.Contracts.Documents;
using DCAFlow.Contracts.Enums;
using DCAFlow.Contracts.Models;
using DCAFlow.Data.Repositories;

namespace DCAFlow.Web.Services;

public sealed class PortfolioService
{
    private readonly PortfolioRepository _portfolioRepository;
    private readonly TransactionRepository _transactionRepository;
    private readonly ExchangeRateProvider _databaseRateProvider;

    public PortfolioService(
        PortfolioRepository portfolioRepository,
        TransactionRepository transactionRepository,
        ExchangeRateProvider databaseRateProvider)
    {
        _portfolioRepository = portfolioRepository ?? throw new ArgumentNullException(nameof(portfolioRepository));
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _databaseRateProvider = databaseRateProvider ?? throw new ArgumentNullException(nameof(databaseRateProvider));
    }

    public Task AddTransactionAsync(TransactionModel transaction)
    {
        var document = new TransactionDocument
        {
            PortfolioId = transaction.PortfolioId,
            Ticker = transaction.Ticker,
            Timestamp = transaction.Timestamp.ToUniversalTime(),
            Amount = transaction.Amount,
            Cost = transaction.Cost,
            Type = (int)transaction.Type
        };

        _transactionRepository.Insert(document);

        return Task.CompletedTask;
    }

    public Task DeleteTransactionAsync(int transactionId)
    {
        _transactionRepository.Delete(transactionId);

        return Task.CompletedTask;
    }

    public async Task<PortfolioModel> GetPortfolioByIdAsync(int portfolioId, CancellationToken cancellationToken = default)
    {
        var portfolioDocument = _portfolioRepository.GetPortfolioById(portfolioId)
            ?? throw new ApplicationException($"Portfolio is not found by id ('{portfolioId}')");

        var transactionDocuments = _transactionRepository.GetPortfolioTransactions(portfolioId);
        var transactions = transactionDocuments?.Select(x => new TransactionModel
        {
            Id = x.Id,
            Timestamp = x.Timestamp,
            Ticker = x.Ticker,
            Type = (TransactionType)x.Type,
            Amount = x.Amount,
            Cost = x.Cost
        }).ToList() ?? [];

        var assets = await GetAssetsAsync(transactions, cancellationToken);

        var totalInvested = assets.Sum(x => x.TotalInvested);
        var holdingsValue = assets.Sum(x => x.HoldingsValue);

        var portfolio = new PortfolioModel
        {
            Id = portfolioDocument.Id,
            Name = portfolioDocument.Name,
            Assets = assets,
            TotalInvested = totalInvested,
            HoldingsValue = holdingsValue,
            TotalReturn = CalculateTotalReturn(totalInvested, holdingsValue),
            Allocation = CalculateAllocation(assets),
            Transactions = transactions.OrderByDescending(x => x.Timestamp).ToList()
        };

        var timelines = await GetTimelinesAsync(transactions, cancellationToken);

        portfolio.TotalInvestedTimeline = timelines.Item1;
        portfolio.HoldingsValueTimeline = timelines.Item2;

        return portfolio;
    }

    private async Task<Tuple<List<KeyValueModel<DateOnly, double>>, List<KeyValueModel<DateOnly, double>>>> GetTimelinesAsync(
        List<TransactionModel> transactions,
        CancellationToken cancellationToken)
    {
        if (transactions is null || transactions.Count == 0)
        {
            return new Tuple<List<KeyValueModel<DateOnly, double>>, List<KeyValueModel<DateOnly, double>>>([], []);
        }

        var allTickers = transactions.Select(x => x.Ticker).Distinct().ToList();
        var startDate = DateOnly.FromDateTime(transactions.Min(x => x.Timestamp).AddDays(-1));

        var ratesByTickers = new Dictionary<string, List<KeyValueModel<DateOnly, double>>>();
        var valuesByTickers = new Dictionary<string, double>();
        var totalInvestedValue = 0D;

        foreach (var ticker in allTickers)
        {
            valuesByTickers.Add(ticker, 0D);

            var rates = await _databaseRateProvider.GetExchangeRatesFromDateAsync(ticker, startDate, cancellationToken);
            ratesByTickers.Add(ticker, rates);
        }

        var orderedTransactions = transactions.OrderBy(x => x.Timestamp).ToList();

        var totalInvestedTimeline = new List<KeyValueModel<DateOnly, double>>();
        var holdingsValueTimeline = new List<KeyValueModel<DateOnly, double>>();

        var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var date = startDate;

        while (date <= currentDate)
        {
            var transactionsOnDate = transactions.Where(x => DateOnly.FromDateTime(x.Timestamp) == date).ToList();

            totalInvestedValue += transactionsOnDate.Sum(GetImpactOnInvestedValue);

            totalInvestedTimeline.Add(new KeyValueModel<DateOnly, double> { Key = date, Value = totalInvestedValue });

            var holdingsValueOnDate = new KeyValueModel<DateOnly, double> { Key = date, Value = 0D };

            foreach (var ticker in allTickers)
            {
                var tickerTransactions = transactionsOnDate.Where(x => x.Ticker == ticker).ToList();
                if (tickerTransactions.Count != 0)
                {
                    valuesByTickers[ticker] += tickerTransactions.Sum(GetImpactOnHoldingsValue);
                }

                var rate = ratesByTickers[ticker].First(x => x.Key == date).Value;
                holdingsValueOnDate.Value += rate * valuesByTickers[ticker];
            }

            holdingsValueTimeline.Add(holdingsValueOnDate);

            date = date.AddDays(1);
        }

        return new Tuple<List<KeyValueModel<DateOnly, double>>, List<KeyValueModel<DateOnly, double>>>(totalInvestedTimeline, holdingsValueTimeline);
    }

    private async Task<List<AssetModel>> GetAssetsAsync(List<TransactionModel> transactions, CancellationToken cancellationToken = default)
    {
        var grouped = transactions.GroupBy(x => x.Ticker).ToDictionary(x => x.Key, x => x.ToList());

        var ts = DateTime.UtcNow;

        var assets = new List<AssetModel>();
        foreach (var asset in grouped)
        {
            assets.Add(await GetAssetOnDateAsync(ts, asset.Key, asset.Value, cancellationToken));
        }

        return assets;
    }

    private async Task<AssetModel> GetAssetOnDateAsync(
        DateTime timestamp,
        string ticker,
        List<TransactionModel> transactions,
        CancellationToken cancellationToken = default)
    {
        var currentRate = await _databaseRateProvider.GetCurrentExchageRateAsync(ticker, cancellationToken);

        var model = new AssetModel
        {
            Ticker = ticker,
            CurrentMarketPrice = currentRate.Value,
            TotalHoldings = 0D,
            TotalInvested = 0D
        };

        var orderedTransactions = transactions.OrderBy(x => x.Timestamp).ToList();

        foreach (var transaction in orderedTransactions)
        {
            model.TotalHoldings += GetImpactOnHoldingsValue(transaction);
            model.TotalInvested += GetImpactOnInvestedValue(transaction);
        }

        model.AverageBuyPrice = CalculateAveragePrice(model.TotalInvested, model.TotalHoldings);
        model.HoldingsValue = CalculateTotalValue(model.TotalHoldings, model.CurrentMarketPrice);
        model.TotalReturn = CalculateTotalReturn(model.TotalInvested, model.HoldingsValue);

        return model;
    }

    private static List<FractionModel> CalculateAllocation(List<AssetModel> assets)
    {
        var totalValue = assets.Sum(x => x.HoldingsValue);

        return assets.Select(x => new FractionModel { Ticker = x.Ticker, Fraction = x.HoldingsValue / totalValue }).ToList();
    }

    private static double CalculateTotalValue(double amount, double price)
    {
        return price * amount;
    }

    private static double CalculateAveragePrice(double invested, double holdings)
    {
        return invested / holdings;
    }

    private static double CalculateTotalReturn(double invested, double value)
    {
        if (invested < double.Epsilon)
        {
            return 1D;
        }

        return (value - invested) / invested;
    }

    private static double GetImpactOnInvestedValue(TransactionModel transaction)
    {
        return transaction.Type switch
        {
            TransactionType.Buy => transaction.Cost.Value,
            TransactionType.Sell => -1D * transaction.Cost.Value,
            TransactionType.TransferIn => 0D,
            TransactionType.TransferOut => 0D,
            _ => 0D
        };
    }

    private static double GetImpactOnHoldingsValue(TransactionModel transaction)
    {
        return transaction.Type switch
        {
            TransactionType.Buy => transaction.Amount,
            TransactionType.Sell => -1D * transaction.Amount,
            TransactionType.TransferIn => transaction.Amount,
            TransactionType.TransferOut => -1D * transaction.Amount,
            _ => 0D
        };
    }
}
