using DCAFlow.Contracts.Enums;
using DCAFlow.Contracts.Models;
using DCAFlow.Data.Repositories;

namespace DCAFlow.Services;

public sealed class PortfolioService
{
    private readonly PortfolioRepository _portfolioRepository;
    private readonly TransactionRepository _transactionRepository;
    private readonly DatabaseRateProvider _databaseRateProvider;

    public PortfolioService(
        PortfolioRepository portfolioRepository,
        TransactionRepository transactionRepository,
        DatabaseRateProvider databaseRateProvider)
    {
        _portfolioRepository = portfolioRepository ?? throw new ArgumentNullException(nameof(portfolioRepository));
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _databaseRateProvider = databaseRateProvider ?? throw new ArgumentNullException(nameof(databaseRateProvider));
    }

    public async Task<PortfolioModel> GetPortfolioByIdAsync(int portfolioId, CancellationToken cancellationToken = default)
    {
        var portfolioDocument = _portfolioRepository.GetPortfolioById(portfolioId) 
            ?? throw new ApplicationException($"Portfolio is not found by id ('{portfolioId}')");

        var transactionDocuments = _transactionRepository.GetPortfolioTransactions(portfolioId);
        var transactions = transactionDocuments?.Select(x => new TransactionModel
        {
            Timestamp = x.Timestamp,
            Ticker = x.Ticker,
            Type = x.Type,
            Amount = x.Amount,
            Cost = x.Cost
        }).ToList() ?? [];

        var assets = await GetAssetsAsync(transactions, cancellationToken);

        var totalInvested = assets.Sum(x => x.TotalInvested);
        var holdingsValue = assets.Sum(x => x.HoldingsValue);

        return new PortfolioModel
        {
            Id = portfolioDocument.Id,
            Name = portfolioDocument.Name,
            Assets = assets,
            TotalInvested = totalInvested,
            HoldingsValue = holdingsValue,
            TotalReturn = CalculateTotalReturn(totalInvested, holdingsValue),
            Allocation = CalculateAllocation(assets),
            Investments = GetInvestments(transactions)
        };
    }

    private List<KeyValueModel<DateTime, double>> GetInvestments(List<TransactionModel> transactions)
    {
        if (transactions is null || transactions.Count == 0)
        {
            return [];
        }

        var ordered = transactions.OrderBy(x => x.Timestamp).ToList();
        
        var date = ordered.First().Timestamp.AddDays(-1).Date;
        var value = 0D;

        var result = new List<KeyValueModel<DateTime, double>> { new() { Key = date, Value = value } };

        var currentDate = DateTime.UtcNow.Date;

        while (date < currentDate)
        { 
            date = date.AddDays(1);

            value += transactions.Where(x => x.Timestamp.Date == date).Sum(x => x.Cost);

            result.Add(new KeyValueModel<DateTime, double> { Key = date, Value = value });
        }

        return result;
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
            CurrentMarketPrice = currentRate.Rate,
            TotalHoldings = 0D,
            TotalInvested = 0D
        };

        var orderedTransactions = transactions.OrderBy(x => x.Timestamp).ToList();

        foreach (var transaction in orderedTransactions)
        {
            if (transaction.Type == TransactionType.Buy)
            {
                model.TotalInvested += transaction.Cost;
                model.TotalHoldings += transaction.Amount;
            }

            if (transaction.Type == TransactionType.Sell)
            {
                model.TotalInvested -= transaction.Cost;
                model.TotalHoldings -= transaction.Amount;
            }
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
        return (value - invested) / invested;
    }
}
