using DCAFlow.Contracts.Models;
using DCAFlow.Core.ExchangeRates;
using DCAFlow.Core.Helpers;
using DCAFlow.Core.Metrics.Enums;
using DCAFlow.Core.Metrics.Models;
using DCAFlow.Core.Transactions.Enums;
using DCAFlow.Data.Entities;
using DCAFlow.Data.Filters;
using DCAFlow.Data.Repositories;

namespace DCAFlow.Core.Metrics;

public sealed class MetricService
{
    private readonly TransactionRepository _transactionRepository;

    private readonly ExchangeRateStorage _exchangeRateStorage;

    public MetricService(TransactionRepository transactionRepository, ExchangeRateStorage exchangeRateStorage)
    {
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _exchangeRateStorage = exchangeRateStorage ?? throw new ArgumentNullException(nameof(exchangeRateStorage));
    }

    public async Task<List<MetricViewModel>> GetMetricsAsync(string tag, CancellationToken cancellationToken = default)
    {
        var filter = new TransactionFilter { TagEq = TagHelper.NormalizeTag(tag) };
        var transactions = await _transactionRepository.SearchAsync(filter, cancellationToken);
        if (transactions is null || transactions.Count == 0)
        {
            return BuildEmptyMetrics();
        }

        var tickers = transactions.Select(x => x.Ticker).Distinct().ToList();
        var minTransactionTimestamp = transactions.Min(x => x.Timestamp);

        await _exchangeRateStorage.InitializeAsync(minTransactionTimestamp, tickers, cancellationToken);

        var latestMetricValues = await GetMetricsOnDateAsync(transactions, null, cancellationToken);
        var result = ConvertToMetrics(latestMetricValues);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        for (var i = 1; i <= MetricSettings.TrendPointsNumber; i++)
        {
            var ts = today.AddDays(-i);
            var metricValues = await GetMetricsOnDateAsync(transactions, ts, cancellationToken);

            AddValuesOnDate(result, ts, metricValues);
        }

        return result;
    }

    private async Task<Dictionary<MetricType, double?>> GetMetricsOnDateAsync(
        List<TransactionEntity> transactions,
        DateOnly? timestamp = null,
        CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        var transactionsToAnalyze = timestamp is null
            ? transactions.Where(x => x.Timestamp <= utcNow).ToList()
            : transactions.Where(x => DateOnly.FromDateTime(x.Timestamp) <= timestamp.Value).ToList();

        var totalInvestments = transactionsToAnalyze.Sum(x => GetInvestmentValue((TransactionType)x.Type, x.Cost));

        var totalHoldings = 0D;
        var transactionsByTickers = transactionsToAnalyze.GroupBy(x => x.Ticker).ToDictionary(x => x.Key, x => x.ToList());
        foreach (var tickerTransactions in transactionsByTickers)
        {
            var rate = timestamp is null
                ? await _exchangeRateStorage.GetLatestExchangeRateAsync(tickerTransactions.Key, cancellationToken)
                : await _exchangeRateStorage.GetExchangeRateOnDateAsync(tickerTransactions.Key, timestamp.Value, cancellationToken);

            var amount = tickerTransactions.Value.Sum(x => GetHoldingsValue((TransactionType)x.Type, x.Amount));

            totalHoldings += (rate * amount);
        }

        var profitAndLoss = totalHoldings - totalInvestments;
        var returnOfInvestments = CalculateTotalReturn(totalInvestments, totalHoldings);

        var annualizedReturnTimestamp = timestamp is null ? utcNow : timestamp.Value.AddDays(1).ToDateTime(new TimeOnly(), DateTimeKind.Utc);
        var annualizedReturn = CalculateAnnualizedReturn(transactions, annualizedReturnTimestamp, totalHoldings);

        return new Dictionary<MetricType, double?>
        {
            { MetricType.TotalInvestmens, totalInvestments },
            { MetricType.TotalHoldings, totalHoldings },
            { MetricType.ReturnOfInvestments, returnOfInvestments },
            { MetricType.ProfitAndLoss, profitAndLoss },
            { MetricType.AnnualizedReturn, annualizedReturn }
        };
    }

    private static double? CalculateAnnualizedReturn(List<TransactionEntity> transactions, DateTime timestamp, double totalHoldings)
    {
        if (transactions.Count == 0)
        {
            return null;
        }

        var cashFlows = GetCashFlowRecords(transactions, timestamp, totalHoldings);

        try
        {
            return AnnualizedReturnCalculator.CalculateAnnualizedReturn(cashFlows);
        }
        catch
        {
            return null;
        }
    }

    private static List<CashFlowModel> GetCashFlowRecords(List<TransactionEntity> transactions, DateTime timestamp, double totalHoldings)
    {
        var cashFlows = transactions
            .Where(x => (x.Type == (int)TransactionType.Buy || x.Type == (int)TransactionType.Sell) && timestamp > x.Timestamp)
            .Select(x => new CashFlowModel
            {
                Timestamp = x.Timestamp,
                Value = x.Type == (int)TransactionType.Buy ? -x.Cost : x.Cost
            })
            .ToList();

        cashFlows.Add(new CashFlowModel { Timestamp = timestamp, Value = totalHoldings });

        return cashFlows;
    }

    private static List<MetricViewModel> BuildEmptyMetrics()
    {
        return Enum.GetValues<MetricType>().Select(x => new MetricViewModel { Type = x, Value = null, Trend = [] }).ToList();
    }

    private static List<MetricViewModel> ConvertToMetrics(Dictionary<MetricType, double?> metricValues)
    {
        return [.. metricValues.Select(x => new MetricViewModel { Type = x.Key, Value = x.Value, Trend = [] })];
    }

    private static void AddValuesOnDate(List<MetricViewModel> metrics, DateOnly ts, Dictionary<MetricType, double?> metricValues)
    {
        foreach (var metric in metrics)
        {
            metric.Trend.Add(new KeyValueModel<DateOnly, double?>
            {
                Key = ts,
                Value = metricValues.TryGetValue(metric.Type, out var metricValue) ? metricValue : null
            });
        }
    }

    private static double CalculateTotalReturn(double totalInvestments, double totalHoldings)
    {
        if (totalInvestments < double.Epsilon)
        {
            return 1D;
        }

        return (totalHoldings - totalInvestments) / totalInvestments;
    }

    private static double GetInvestmentValue(TransactionType type, double value)
    {
        return type switch
        {
            TransactionType.Buy => value,
            TransactionType.Sell => -value,
            TransactionType.TransferIn or TransactionType.TransferOut => 0D,
            _ => 0D,
        };
    }

    private static double GetHoldingsValue(TransactionType type, double value)
    {
        return type switch
        {
            TransactionType.Buy or TransactionType.TransferIn => value,
            TransactionType.Sell or TransactionType.TransferOut => -value,
            _ => 0D,
        };
    }
}
