using DCAFlow.Contracts.Documents;
using DCAFlow.Contracts.Models;
using DCAFlow.Data.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace DCAFlow.Services;

public class DatabaseRateProvider : IExchangeRateProvider
{
    private readonly ExchangeRateRepository _exchangeRateRepository;
    private readonly IExchangeRateProvider _exchangeRateProvider;
    private readonly IMemoryCache _cache;

    public DatabaseRateProvider(
        ExchangeRateRepository exchangeRateRepository,
        CoinGeckoRateProvider exchangeRateProvider, 
        IMemoryCache memoryCache)
    {
        _exchangeRateRepository = exchangeRateRepository ?? throw new ArgumentNullException(nameof(exchangeRateRepository));
        _exchangeRateProvider = exchangeRateProvider ?? throw new ArgumentNullException(nameof(exchangeRateProvider));
        _cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public async Task<ExchangeRateModel> GetCurrentExchageRateAsync(string ticker, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(ticker, out double cachedPrice))
        {
            return new ExchangeRateModel { Timestamp = DateTime.UtcNow, Rate = cachedPrice };
        }

        var thirdPartyProviderResult = await _exchangeRateProvider.GetCurrentExchageRateAsync(ticker, cancellationToken);

        _cache.Set(ticker, thirdPartyProviderResult.Rate, TimeSpan.FromSeconds(30));

        return thirdPartyProviderResult;
    }

    public Task<ExchangeRateModel> GetExchangeRateOnDateAsync(string ticker, DateTime ts, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<List<ExchangeRateModel>> GetHistoricalRatesAsync(string ticker, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var rates = _exchangeRateRepository.GetHistoricalRates(ticker, from, to)
            .Select(x => new ExchangeRateModel { Timestamp = x.Timestamp, Rate = x.Rate })
            .ToList();
  
        if (rates.Count < (to - from).Days + 1)
        {
            var newRates = new List<ExchangeRateDocument>();

            var thirdPartyRates = await _exchangeRateProvider.GetHistoricalRatesAsync(ticker, from, to, cancellationToken);
            foreach (var thirdPartyRate in thirdPartyRates)
            {
                if (rates.All(x => x.Timestamp != thirdPartyRate.Timestamp))
                {
                    rates.Add(thirdPartyRate);

                    newRates.Add(new ExchangeRateDocument { Ticker = ticker, Timestamp = thirdPartyRate.Timestamp, Rate = thirdPartyRate.Rate });
                }
            }

            if (newRates.Count > 0)
            {
                _exchangeRateRepository.InsertRates(newRates);
            }
        }

        return rates.OrderBy(x => x.Timestamp).ToList();
    }
}
