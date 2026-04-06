using DCAFlow.Contracts.Documents;
using DCAFlow.Contracts.Models;
using DCAFlow.Data.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace DCAFlow.Services;

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

    public async Task<List<KeyValueModel<DateOnly, double>>> GetHistoricalRatesAsync(string ticker, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var rates = _exchangeRateRepository.GetHistoricalDailyRates(ticker, from, to)
            .Select(x => new KeyValueModel<DateOnly, double> { Key = x.Timestamp, Value = x.Rate })
            .ToList();
  
        if (rates.Count < (to.ToDateTime(new TimeOnly(0, 0, 0)) - from.ToDateTime(new TimeOnly(0, 0, 0))).Days + 1)
        {
            var newRates = new List<ExchangeRateDocument>();

            var thirdPartyRates = await _coinGeckoProvider.GetHistoricalRatesAsync(ticker, from, to, cancellationToken);
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
