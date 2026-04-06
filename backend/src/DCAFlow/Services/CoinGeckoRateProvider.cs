using DCAFlow.Contracts.Models;
using DCAFlow.Settings;
using System.Text.Json;

namespace DCAFlow.Services;

public class CoinGeckoRateProvider : IExchangeRateProvider
{
    private readonly CoinGeckoSettings _settings;
    private readonly HttpClient _client;

    public CoinGeckoRateProvider(CoinGeckoSettings settings, HttpClient client)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<ExchangeRateModel> GetCurrentExchageRateAsync(string ticker, CancellationToken cancellationToken = default)
    {
        var coinId = TickerMapper.GetIdentifierByTicker(ticker);
        var url = $"{_settings.BaseUrl}/simple/price?vs_currencies=usd&ids={coinId}&x_cg_demo_api_key={_settings.Key}";

        var response = await _client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        using var doc = JsonDocument.Parse(json);
        return new ExchangeRateModel
        {
            Timestamp = DateTime.UtcNow,
            Rate = doc.RootElement.GetProperty(coinId).GetProperty("usd").GetDouble()
        };
    }

    public Task<ExchangeRateModel> GetExchangeRateOnDateAsync(string ticker, DateTime ts, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();

        //var coinId = TickerMapper.GetIdentifierByTicker(ticker);
        //var url = $"{_settings.BaseUrl}/coins/{coinId}/history?date={}&x_cg_demo_api_key={_settings.Key}";
    }

    public Task<List<ExchangeRateModel>> GetHistoricalRatesAsync(string ticker, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
