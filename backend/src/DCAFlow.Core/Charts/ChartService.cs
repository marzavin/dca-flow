using DCAFlow.Core.Assets;
using DCAFlow.Core.Charts.Model;
using DCAFlow.Core.Common;
using DCAFlow.Core.ExchangeRates;
using DCAFlow.Core.Helpers;
using DCAFlow.Core.Metrics;
using DCAFlow.Data.Filters;
using DCAFlow.Data.Repositories;

namespace DCAFlow.Core.Charts;

public sealed class ChartService
{
    private readonly TransactionRepository _transactionRepository;

    private readonly ExchangeRateStorage _exchangeRateStorage;

    private readonly AssetService _assetService;

    public ChartService(TransactionRepository transactionRepository, ExchangeRateStorage exchangeRateStorage, AssetService assetService)
    {
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _exchangeRateStorage = exchangeRateStorage ?? throw new ArgumentNullException(nameof(exchangeRateStorage));
        _assetService = assetService ?? throw new ArgumentNullException(nameof(assetService));
    }

    public async Task<List<KeyValueModel<string, double>>> GetAllocationChartAsync(string tag, CancellationToken cancellationToken = default)
    {
        var assets = await _assetService.GetAssetsAsync(tag, cancellationToken);

        var totalHoldings = assets.Sum(x => x.TotalHoldings);

        return [.. assets.Select(x => new KeyValueModel<string, double> { Key = x.Ticker, Value = x.TotalHoldings / totalHoldings })];
    }

    public async Task<List<PerformanceOnDate>> GetPerformanceChartAsync(string tag, CancellationToken cancellationToken = default)
    {
        var filter = new TransactionFilter { TagEq = TagHelper.NormalizeTag(tag) };
        var transactions = await _transactionRepository.SearchAsync(filter, cancellationToken);

        var tickers = transactions.Select(x => x.Ticker).Distinct().ToList();
        var startDate = transactions.Min(x => x.Timestamp).AddDays(-1);

        await _exchangeRateStorage.InitializeAsync(startDate, tickers, cancellationToken);

        var totalInvestments = 0D;
        var amountsByTickers = tickers.ToDictionary(x => x, x => 0D);

        var result = new List<PerformanceOnDate>();

        var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var date = DateOnly.FromDateTime(startDate);

        while (date <= currentDate)
        {
            var transactionsOnDate = transactions.Where(x => DateOnly.FromDateTime(x.Timestamp) == date).ToList();
            totalInvestments += transactionsOnDate.Sum(AssetService.GetImpactOnInvestments);

            var performanceOnDate = new PerformanceOnDate { Timestamp = date, TotalInvestments = totalInvestments, TotalHoldings = 0D };

            foreach (var ticker in tickers)
            {
                var tickerTransactions = transactionsOnDate.Where(x => x.Ticker == ticker).ToList();
                if (tickerTransactions.Count > 0)
                {
                    amountsByTickers[ticker] += tickerTransactions.Sum(AssetService.GetImpactOnAmount);
                }

                var rate = date == currentDate
                    ? await _exchangeRateStorage.GetLatestExchangeRateAsync(ticker, cancellationToken)
                    : await _exchangeRateStorage.GetExchangeRateOnDateAsync(ticker, date, cancellationToken);

                performanceOnDate.TotalHoldings += rate * amountsByTickers[ticker];
            }

            performanceOnDate.ProfitAndLoss = performanceOnDate.TotalHoldings - performanceOnDate.TotalInvestments;

            result.Add(performanceOnDate);

            date = date.AddDays(1);
        }

        return result;
    }

    public async Task<List<KeyValueModel<DateOnly, double>>> GetAnnualizedReturnChartAsync(string tag, CancellationToken cancellationToken = default)
    {
        var filter = new TransactionFilter { TagEq = TagHelper.NormalizeTag(tag) };
        var transactions = await _transactionRepository.SearchAsync(filter, cancellationToken);

        var tickers = transactions.Select(x => x.Ticker).Distinct().ToList();
        var startDate = transactions.Min(x => x.Timestamp);

        await _exchangeRateStorage.InitializeAsync(startDate, tickers, cancellationToken);

        var amountsByTickers = tickers.ToDictionary(x => x, x => 0D);

        var result = new List<KeyValueModel<DateOnly, double>>();

        var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var date = DateOnly.FromDateTime(startDate);

        var investmentDayNumber = 1;
        var frequency = GetAnnualizedReturnFrequency(date, currentDate);

        while (date <= currentDate)
        {
            var transactionsOnDate = transactions.Where(x => DateOnly.FromDateTime(x.Timestamp) == date).ToList();

            var holdingsValueOnDate = 0D;

            foreach (var ticker in tickers)
            {
                var tickerTransactions = transactionsOnDate.Where(x => x.Ticker == ticker).ToList();
                if (tickerTransactions.Count > 0)
                {
                    amountsByTickers[ticker] += tickerTransactions.Sum(AssetService.GetImpactOnAmount);
                }

                var rate = date == currentDate
                    ? await _exchangeRateStorage.GetLatestExchangeRateAsync(ticker, cancellationToken)
                    : await _exchangeRateStorage.GetExchangeRateOnDateAsync(ticker, date, cancellationToken);

                holdingsValueOnDate += rate * amountsByTickers[ticker];
            }

            if (investmentDayNumber % frequency == 0)
            {
                var nextDay = date.AddDays(1);

                var timestamp = date == currentDate ? DateTime.UtcNow : nextDay.ToDateTime(new TimeOnly(), DateTimeKind.Utc);
                var annualizedReturnValue = MetricService.CalculateAnnualizedReturn(transactions, timestamp, holdingsValueOnDate);
                if (annualizedReturnValue.HasValue)
                {
                    result.Add(new KeyValueModel<DateOnly, double> { Key = date, Value = annualizedReturnValue.Value });
                }
            }

            date = date.AddDays(1);

            investmentDayNumber++;
        }

        return result;
    }

    private static int GetAnnualizedReturnFrequency(DateOnly startDate, DateOnly endDate)
    {
        var investmentPeriodDays = endDate.DayNumber - startDate.DayNumber + 1;

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
}
