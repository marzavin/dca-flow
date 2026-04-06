using DCAFlow.Contracts.Models;
using Microsoft.Extensions.Caching.Memory;

namespace DCAFlow.Services;

public class DatabaseRateProvider : IExchangeRateProvider
{
    private readonly IExchangeRateProvider _exchangeRateProvider;
    private readonly IMemoryCache _cache;

    public DatabaseRateProvider(CoinGeckoRateProvider exchangeRateProvider, IMemoryCache memoryCache)
    {
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

    public Task<List<ExchangeRateModel>> GetHistoricalRatesAsync(string ticker, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
