using DCAFlow.Contracts.Documents;
using DCAFlow.Contracts.Models;
using DCAFlow.Data.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace DCAFlow.Web.Services;

public class ExchangeRateProvider : IExchangeRateProvider
{
    private readonly ExchangeRateRepository _exchangeRateRepository;
    private readonly CoinGeckoRateProvider _coinGeckoProvider;
    private readonly IMemoryCache _cache;

    public ExchangeRateProvider(
        ExchangeRateRepository exchangeRateRepository,
        CoinGeckoRateProvider coinGeckoProvider, 
        IMemoryCache memoryCache)
    {
        _exchangeRateRepository = exchangeRateRepository ?? throw new ArgumentNullException(nameof(exchangeRateRepository));
        _coinGeckoProvider = coinGeckoProvider ?? throw new ArgumentNullException(nameof(coinGeckoProvider));
        _cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public async Task<KeyValueModel<DateOnly, double>> GetCurrentExchageRateAsync(string ticker, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(ticker, out double cachedPrice))
        {
            return new KeyValueModel<DateOnly, double> { Key = DateOnly.FromDateTime(DateTime.UtcNow), Value = cachedPrice };
        }

        var thirdPartyProviderResult = await _coinGeckoProvider.GetCurrentExchageRateAsync(ticker, cancellationToken);

        _cache.Set(ticker, thirdPartyProviderResult.Value, TimeSpan.FromSeconds(30));

        return thirdPartyProviderResult;
    }

    public Task<KeyValueModel<DateOnly, double>> GetExchangeRateOnDateAsync(string ticker, DateOnly ts, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<List<KeyValueModel<DateOnly, double>>> GetExchangeRatesFromDateAsync(string ticker, DateOnly from, CancellationToken cancellationToken = default)
    {
        var todayRate = await _coinGeckoProvider.GetCurrentExchageRateAsync(ticker, cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var yesterday = today.AddDays(-1);

        var rates = _exchangeRateRepository.GetHistoricalDailyRates(ticker, from, yesterday)
            .Select(x => new KeyValueModel<DateOnly, double> { Key = x.Timestamp, Value = x.Rate })
            .ToList();
  
        if (rates.Count < (yesterday.ToDateTime(new TimeOnly(0, 0, 0), DateTimeKind.Utc) - from.ToDateTime(new TimeOnly(0, 0, 0), DateTimeKind.Utc)).Days + 1)
        {
            var newRates = new List<ExchangeRateDocument>();

            var thirdPartyRates = await _coinGeckoProvider.GetHistoricalRatesAsync(ticker, from, today, cancellationToken);
            var lastDailyRates = GetLastDailyRates(thirdPartyRates);

            foreach (var thirdPartyRate in lastDailyRates)
            {
                if (rates.All(x => x.Key != thirdPartyRate.Key))
                {
                    rates.Add(thirdPartyRate);

                    newRates.Add(new ExchangeRateDocument { Ticker = ticker, Timestamp = thirdPartyRate.Key, Rate = thirdPartyRate.Value });
                }
            }

            if (newRates.Count > 0)
            {
                _exchangeRateRepository.InsertRates(newRates);
            }
        }

        rates.Add(todayRate);

        return rates.OrderBy(x => x.Key).ToList();
    }

    private List<KeyValueModel<DateOnly, double>> GetLastDailyRates(List<KeyValueModel<DateTime, double>> rates)
    {
        return rates
            .GroupBy(x => new DateOnly(x.Key.Year, x.Key.Month, x.Key.Day))
            .Select(x => new KeyValueModel<DateOnly, double> { Key = x.Key, Value = x.OrderBy(x => x.Key).Last().Value })
            .ToList();
    }

    private List<KeyValueModel<DateOnly, double>> GetAverageDailyRates(List<KeyValueModel<DateTime, double>> rates)
    {
        return rates
            .GroupBy(x => new DateOnly(x.Key.Year, x.Key.Month, x.Key.Day))
            .Select(x => new KeyValueModel<DateOnly, double> { Key = x.Key, Value = x.OrderBy(x => x.Key).Sum(x => x.Value) / x.Count() })
            .ToList();
    }
}
