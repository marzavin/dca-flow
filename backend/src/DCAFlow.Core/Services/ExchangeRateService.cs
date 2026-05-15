using DCAFlow.Contracts.Documents;
using DCAFlow.Contracts.Interfaces;
using DCAFlow.Contracts.Models;
using DCAFlow.Data.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace DCAFlow.Core.Services;

public sealed class ExchangeRateService
{
    private readonly ExchangeRateRepository _exchangeRateRepository;
    private readonly IExchangeRateProvider _exchangeRateProvider;
    private readonly IMemoryCache _cache;

    public ExchangeRateService(
        ExchangeRateRepository exchangeRateRepository,
        IExchangeRateProvider exchangeRateProvider, 
        IMemoryCache memoryCache)
    {
        _exchangeRateRepository = exchangeRateRepository ?? throw new ArgumentNullException(nameof(exchangeRateRepository));
        _exchangeRateProvider = exchangeRateProvider ?? throw new ArgumentNullException(nameof(exchangeRateProvider));
        _cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public async Task<List<KeyValueModel<DateOnly, double>>> GetExchangeRatesFromDateAsync(string ticker, DateOnly from, CancellationToken cancellationToken = default)
    {
        var todayRate = await GetCurrentExchageRateAsync(ticker, cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var yesterday = today.AddDays(-1);

        var rates = _exchangeRateRepository.GetHistoricalDailyRates(ticker, from, yesterday)
            .Select(x => new KeyValueModel<DateOnly, double> { Key = x.Timestamp, Value = x.Rate })
            .ToList();
  
        if (rates.Count < (yesterday.ToDateTime(new TimeOnly(0, 0, 0), DateTimeKind.Utc) - from.ToDateTime(new TimeOnly(0, 0, 0), DateTimeKind.Utc)).Days + 1)
        {
            var newRates = new List<ExchangeRateDocument>();

            var thirdPartyRates = await _exchangeRateProvider.GetHistoricalRatesAsync(ticker, from, today, cancellationToken);
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

        rates.Add(new KeyValueModel<DateOnly, double> { Key = today, Value = todayRate });

        return [.. rates.OrderBy(x => x.Key)];
    }

    private async Task<double> GetCurrentExchageRateAsync(string ticker, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(ticker, out double cachedPrice))
        {
            return cachedPrice;
        }

        var thirdPartyProviderResult = await _exchangeRateProvider.GetCurrentExchageRateAsync(ticker, cancellationToken);

        _cache.Set(ticker, thirdPartyProviderResult, TimeSpan.FromSeconds(30));

        return thirdPartyProviderResult;
    }

    private static List<KeyValueModel<DateOnly, double>> GetLastDailyRates(List<KeyValueModel<DateTime, double>> rates)
    {
        return [.. rates
            .GroupBy(x => new DateOnly(x.Key.Year, x.Key.Month, x.Key.Day))
            .Select(x => new KeyValueModel<DateOnly, double> { Key = x.Key, Value = x.OrderBy(x => x.Key).Last().Value })];
    }
}
