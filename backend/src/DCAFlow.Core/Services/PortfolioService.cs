using DCAFlow.Contracts.Enums;
using DCAFlow.Contracts.Models;
using DCAFlow.Core.Mappers;
using DCAFlow.Data.Repositories;

namespace DCAFlow.Core.Services;

public sealed class PortfolioService
{
    private readonly PortfolioRepository _portfolioRepository;
    private readonly TransactionRepository _transactionRepository;
    private readonly ExchangeRateService _exchangeRateProvider;

    public PortfolioService(
        PortfolioRepository portfolioRepository,
        TransactionRepository transactionRepository,
        ExchangeRateService exchangeRateProvider)
    {
        _portfolioRepository = portfolioRepository ?? throw new ArgumentNullException(nameof(portfolioRepository));
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _exchangeRateProvider = exchangeRateProvider ?? throw new ArgumentNullException(nameof(exchangeRateProvider));
    }

    public async Task<PortfolioModel> GetPortfolioByIdAsync(int portfolioId, CancellationToken cancellationToken = default)
    {
        var portfolioDocument = _portfolioRepository.GetPortfolioById(portfolioId)
            ?? throw new ApplicationException($"Portfolio is not found by id ('{portfolioId}')");

        var portfolio = PortfolioMapper.Map(portfolioDocument);

        var transactionDocuments = _transactionRepository.GetPortfolioTransactions(portfolioId);
        var transactions = transactionDocuments?.Select(TransactionMapper.Map).ToList() ?? [];

        if (transactions.Count == 0)
        {
            return portfolio;
        }

        var startDate = DateOnly.FromDateTime(transactions.Min(x => x.Timestamp).AddDays(-1));
        var tickers = transactions.Select(x => x.Ticker).Distinct().ToList();
        var context = await InitPortfolioContextAsync(startDate, tickers, cancellationToken);

        await FillTransactionDataAsync(context, portfolio, transactions, cancellationToken);
        await FillAssetDataAsync(context, portfolio, cancellationToken);
        await FillPortolioTotalDataAsync(context, portfolio, cancellationToken);
        await FillAllocationDataAsync(context, portfolio, cancellationToken);
        await FillTimelineDataAsync(context, portfolio, cancellationToken);

        return portfolio;
    }

    private async Task<PortfolioContextModel> InitPortfolioContextAsync(
        DateOnly startDate,
        List<string> tickers,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (startDate >= today)
        {
            throw new ApplicationException("Invalid calculation time frame.");
        }

        if (tickers is null || tickers.Count == 0)
        {
            throw new ApplicationException("Ticker list is empty.");
        }

        var context = new PortfolioContextModel
        {
            Tickers = tickers,
            TimeFrame = new TimeFrameModel
            {
                StartDate = startDate,
                EndDate = today
            },
            ExchangeRates = []
        };

        foreach (var ticker in tickers)
        {
            var rates = await _exchangeRateProvider.GetExchangeRatesFromDateAsync(ticker, context.TimeFrame.StartDate, cancellationToken);
            context.ExchangeRates.Add(ticker, rates);
        }

        return context;
    }

    private static Task FillTransactionDataAsync(
        PortfolioContextModel context,
        PortfolioModel portfolio,
        List<TransactionModel> transactions,
        CancellationToken cancellationToken = default)
    {
        portfolio.Transactions = transactions.OrderByDescending(x => x.Timestamp).ToList();

        return Task.CompletedTask;
    }

    private static Task FillAssetDataAsync(
        PortfolioContextModel context,
        PortfolioModel portfolio,
        CancellationToken cancellationToken = default)
    {
        var grouped = portfolio.Transactions.GroupBy(x => x.Ticker).ToDictionary(x => x.Key, x => x.ToList());

        var assets = new List<AssetModel>();
        foreach (var tickerGroup in grouped)
        {
            var exchangeRate = context.ExchangeRates[tickerGroup.Key].First(x => x.Key == context.TimeFrame.EndDate).Value;
            var asset = GetAssetDetails(tickerGroup.Key, exchangeRate, tickerGroup.Value);
            assets.Add(asset);
        }

        portfolio.Assets = assets;

        return Task.CompletedTask;
    }

    private static Task FillPortolioTotalDataAsync(
        PortfolioContextModel context,
        PortfolioModel portfolio,
        CancellationToken cancellationToken = default)
    {
        portfolio.TotalInvested = portfolio.Assets.Sum(x => x.TotalInvested);
        portfolio.HoldingsValue = portfolio.Assets.Sum(x => x.HoldingsValue);
        portfolio.TotalReturn = CalculateTotalReturn(portfolio.TotalInvested, portfolio.HoldingsValue);
        portfolio.AnnualizedReturn = CalculateAnnualizedReturn(portfolio.Transactions, DateTime.UtcNow, portfolio.HoldingsValue);

        return Task.CompletedTask;
    }

    private static Task FillAllocationDataAsync(
        PortfolioContextModel context,
        PortfolioModel portfolio,
        CancellationToken cancellationToken = default)
    {
        var totalValue = portfolio.Assets.Sum(x => x.HoldingsValue);

        portfolio.Allocation = [.. portfolio.Assets.Select(x => new FractionModel { Ticker = x.Ticker, Fraction = x.HoldingsValue / totalValue })];

        return Task.CompletedTask;
    }

    private static async Task FillTimelineDataAsync(
        PortfolioContextModel context,
        PortfolioModel portfolio,
        CancellationToken cancellationToken = default)
    {
        var timelines = GetTimelines(context, portfolio.Transactions, cancellationToken);

        portfolio.TotalInvestedTimeline = timelines.Item1;
        portfolio.HoldingsValueTimeline = timelines.Item2;
        portfolio.AnnualizedReturnTimeline = timelines.Item3;
    }

    private static AssetModel GetAssetDetails(string ticker, double currentMarketPrice, List<TransactionModel> transactions)
    {
        var model = new AssetModel
        {
            Ticker = ticker,
            CurrentMarketPrice = currentMarketPrice,
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

    private static Tuple<List<KeyValueModel<DateOnly, double>>, List<KeyValueModel<DateOnly, double>>, List<KeyValueModel<DateOnly, double>>> GetTimelines(
        PortfolioContextModel context,
        List<TransactionModel> transactions,
        CancellationToken cancellationToken)
    {
        if (transactions is null || transactions.Count == 0)
        {
            return new Tuple<List<KeyValueModel<DateOnly, double>>, List<KeyValueModel<DateOnly, double>>, List<KeyValueModel<DateOnly, double>>>([], [], []);
        }

        var valuesByTickers = context.Tickers.ToDictionary(x => x, x => 0D);
        var totalInvestedValue = 0D;

        var orderedTransactions = transactions.OrderBy(x => x.Timestamp).ToList();

        var totalInvestedTimeline = new List<KeyValueModel<DateOnly, double>>();
        var holdingsValueTimeline = new List<KeyValueModel<DateOnly, double>>();
        var annualizedReturnTimeline = new List<KeyValueModel<DateOnly, double>>();

        var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var date = context.TimeFrame.StartDate;
        var investmentDayNumber = 0;
        var frequency = GetAnnualizedReturnFrequency(context.TimeFrame);

        while (date <= currentDate)
        {
            var transactionsOnDate = transactions.Where(x => DateOnly.FromDateTime(x.Timestamp) == date).ToList();

            totalInvestedValue += transactionsOnDate.Sum(GetImpactOnInvestedValue);

            totalInvestedTimeline.Add(new KeyValueModel<DateOnly, double> { Key = date, Value = totalInvestedValue });

            var holdingsValueOnDate = new KeyValueModel<DateOnly, double> { Key = date, Value = 0D };

            foreach (var ticker in context.Tickers)
            {
                var tickerTransactions = transactionsOnDate.Where(x => x.Ticker == ticker).ToList();
                if (tickerTransactions.Count > 0)
                {
                    valuesByTickers[ticker] += tickerTransactions.Sum(GetImpactOnHoldingsValue);
                }

                var rate = context.ExchangeRates[ticker].First(x => x.Key == date).Value;
                holdingsValueOnDate.Value += rate * valuesByTickers[ticker];
            }

            holdingsValueTimeline.Add(holdingsValueOnDate);
        
            if (investmentDayNumber % frequency == 0)
            {
                var nextDay = date.AddDays(1);

                if (investmentDayNumber == 0)
                {
                    annualizedReturnTimeline.Add(new KeyValueModel<DateOnly, double> { Key = date, Value = 0D });
                }
                else 
                {
                    var timestamp = date == currentDate ? DateTime.UtcNow : nextDay.ToDateTime(new TimeOnly(), DateTimeKind.Utc);
                    var annualizedReturnValue = CalculateAnnualizedReturn(transactions, timestamp, holdingsValueOnDate.Value);
                    if (annualizedReturnValue.HasValue)
                    {
                        annualizedReturnTimeline.Add(new KeyValueModel<DateOnly, double> { Key = date, Value = annualizedReturnValue.Value });
                    }
                }
            }

            date = date.AddDays(1);

            investmentDayNumber++;
        }

        return new Tuple<List<KeyValueModel<DateOnly, double>>, List<KeyValueModel<DateOnly, double>>, List<KeyValueModel<DateOnly, double>>>(
            totalInvestedTimeline, holdingsValueTimeline, annualizedReturnTimeline);
    }

    private static int GetAnnualizedReturnFrequency(TimeFrameModel timeFrame)
    {
        var investmentPeriodDays = timeFrame.EndDate.DayNumber - timeFrame.StartDate.DayNumber + 1;
        
        if (investmentPeriodDays > 720)
        {
            return 30;
        }
        else if (investmentPeriodDays > 360)
        {
            return 7;
        }
        else if (investmentPeriodDays > 180)
        {
            return 3;
        }
        else if (investmentPeriodDays > 90)
        {
            return 2;
        }

        return 1;
    }

    private static double? CalculateAnnualizedReturn(List<TransactionModel> transactions, DateTime timestamp, double holdingsValue)
    {
        if (transactions.Count == 0)
        {
            return null;
        }

        var cashFlows = GetCashFlowRecords(transactions, timestamp, holdingsValue);

        try
        {
            return AnnualizedReturnCalculator.CalculateAnnualizedReturn(cashFlows);
        }
        catch
        {
            return null;
        }
    }

    private static List<CashFlowModel> GetCashFlowRecords(List<TransactionModel> transactions, DateTime timestamp, double holdingsValue)
    {
        var cashFlows = transactions
            .Where(x => (x.Type == TransactionType.Buy || x.Type == TransactionType.Sell) && timestamp > x.Timestamp)
            .Select(x => new CashFlowModel
            {
                Timestamp = x.Timestamp,
                Value = x.Type == TransactionType.Buy ? -x.Cost : x.Cost
            })
            .ToList();

        cashFlows.Add(new CashFlowModel { Timestamp = timestamp, Value = holdingsValue });

        return cashFlows;
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
            TransactionType.Buy => transaction.Cost,
            TransactionType.Sell => -1D * transaction.Cost,
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
