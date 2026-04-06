using DCAFlow.Contracts.Models;
using DCAFlow.Settings;
using System.Text.Json;
using static DCAFlow.Constants;

namespace DCAFlow.Services;

public class CoinGeckoRateProvider : IExchangeRateProvider
{
    private readonly CoinGeckoSettings _settings;
    private readonly IHttpClientFactory _clientFactory;

    public CoinGeckoRateProvider(IHttpClientFactory clientFactory, CoinGeckoSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
    }

    public async Task<ExchangeRateModel> GetCurrentExchageRateAsync(string ticker, CancellationToken cancellationToken = default)
    {
        var coinId = TickerMapper.GetIdentifierByTicker(ticker);
        
        var url = $"{_settings.BaseUrl}/simple/price?vs_currencies=usd&ids={coinId}&x_cg_demo_api_key={_settings.Key}";

        var client = _clientFactory.CreateClient(HttpClients.CoinGeckoClient);
        
        var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        using var doc = JsonDocument.Parse(json);
        return new ExchangeRateModel
        {
            Timestamp = DateTime.UtcNow,
            Rate = doc.RootElement.GetProperty(coinId).GetProperty("usd").GetDouble()
        };
    }

    public async Task<ExchangeRateModel> GetExchangeRateOnDateAsync(string ticker, DateTime ts, CancellationToken cancellationToken = default)
    {
        var formattedDate = ts.ToString("dd-MM-yyyy");
        var coinId = TickerMapper.GetIdentifierByTicker(ticker);
        
        var url = $"{_settings.BaseUrl}/coins/{coinId}/history?date={formattedDate}&x_cg_demo_api_key={_settings.Key}";

        var client = _clientFactory.CreateClient(HttpClients.CoinGeckoClient);

        var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        using var doc = JsonDocument.Parse(json);

        return new ExchangeRateModel
        {
            Timestamp = ts,
            Rate = doc.RootElement.GetProperty("market_data").GetProperty("current_price").GetProperty("usd").GetDouble()
        };
    }

    public async Task<List<ExchangeRateModel>> GetHistoricalRatesAsync(string ticker, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var fromUnix = new DateTimeOffset(from).ToUnixTimeSeconds();
        var toUnix = new DateTimeOffset(to).ToUnixTimeSeconds();
        var coinId = TickerMapper.GetIdentifierByTicker(ticker);

        var url = $"{_settings.BaseUrl}/coins/{coinId}/market_chart/range?vs_currency=usd&from={fromUnix}&to={toUnix}&x_cg_demo_api_key={_settings.Key}";

        var client = _clientFactory.CreateClient(HttpClients.CoinGeckoClient);

        var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        using var doc = JsonDocument.Parse(json);

        var result = new List<ExchangeRateModel>();

        foreach (var item in doc.RootElement.GetProperty("prices").EnumerateArray())
        {
            var timestamp = item[0].GetInt64();
            var rate = item[1].GetDouble();

            var date = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;

            result.Add(new ExchangeRateModel{ Timestamp = date, Rate = rate });
        }

        return result;
    }
}
