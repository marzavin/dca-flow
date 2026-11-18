using DCAFlow.Contracts.Models;

namespace DCAFlow.Core.ExchangeRates;

public class ExchangeRateStorage
{
    private const int SynchronizationPeriodSeconds = 600;

    private DateTime? _initialTimestamp;

    private DateTime? _synchronizedAt;

    private readonly Dictionary<string, double> _latestExchangeRates;

    private readonly Dictionary<string, List<KeyValueModel<DateOnly, double>>> _historicalExchangeRates;

    private readonly IExchangeRateProvider _exchangeRateProvider;

    public ExchangeRateStorage(IExchangeRateProvider exchangeRateProvider)
    {
        _exchangeRateProvider = exchangeRateProvider ?? throw new ArgumentNullException(nameof(exchangeRateProvider));

        _latestExchangeRates = [];
        _historicalExchangeRates = [];
    }

    public async Task InitializeAsync(DateTime initialTimestamp, List<string> tickers, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var isSynchronizationRequired =
            _initialTimestamp is null ||
            _initialTimestamp > initialTimestamp ||
            _synchronizedAt is null || 
            _synchronizedAt.Value.AddSeconds(SynchronizationPeriodSeconds) < now ||
            (tickers is not null && tickers.Any(x => !_latestExchangeRates.ContainsKey(x) || !_historicalExchangeRates.ContainsKey(x)));

        if (!isSynchronizationRequired)
        {
            return;
        }

        foreach (var ticker in tickers)
        {
            var rate = await _exchangeRateProvider.GetCurrentExchageRateAsync(ticker, cancellationToken);
            if (!_latestExchangeRates.TryAdd(ticker, rate))
            {
                _latestExchangeRates[ticker] = rate;
            }

            var historicalRates = await _exchangeRateProvider.GetHistoricalRatesAsync(ticker, DateOnly.FromDateTime(initialTimestamp), DateOnly.FromDateTime(now), cancellationToken);
            var latestDailyRates = historicalRates
                .GroupBy(x => DateOnly.FromDateTime(x.Key))
                .Select(x => new KeyValueModel<DateOnly, double> { Key = x.Key, Value = x.OrderByDescending(x => x.Key).First().Value })
                .ToList();
            if (!_historicalExchangeRates.TryAdd(ticker, latestDailyRates))
            {
                _historicalExchangeRates[ticker] = latestDailyRates;
            }
        }

        _synchronizedAt = now;
        _initialTimestamp = initialTimestamp;
    }

    public Task<double> GetLatestExchangeRateAsync(string ticker, CancellationToken cancellationToken)
    {
        return Task.FromResult(_latestExchangeRates[ticker]);
    }

    public Task<double> GetExchangeRateOnDateAsync(string ticker, DateOnly ts, CancellationToken cancellationToken)
    {
        return Task.FromResult(_historicalExchangeRates[ticker].First(x => x.Key == ts).Value);
    }
}
