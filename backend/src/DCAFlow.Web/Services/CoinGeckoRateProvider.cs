using DCAFlow.Contracts.Models;
using DCAFlow.Web.Settings;
using System.Text.Json;
using static DCAFlow.Web.Constants;

namespace DCAFlow.Web.Services;

public class CoinGeckoRateProvider
{
    private readonly CoinGeckoSettings _settings;
    private readonly IHttpClientFactory _clientFactory;

    public CoinGeckoRateProvider(IHttpClientFactory clientFactory, CoinGeckoSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
    }

    public async Task<KeyValueModel<DateOnly, double>> GetCurrentExchageRateAsync(string ticker, CancellationToken cancellationToken = default)
    {
        var coinId = TickerMapper.GetIdentifierByTicker(ticker);
        
        var url = $"{_settings.BaseUrl}/simple/price?vs_currencies=usd&ids={coinId}&x_cg_demo_api_key={_settings.Key}";

        var client = _clientFactory.CreateClient(HttpClients.CoinGeckoClient);
        
        var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        using var doc = JsonDocument.Parse(json);
        return new KeyValueModel<DateOnly, double>
        {
            Key = DateOnly.FromDateTime(DateTime.UtcNow),
            Value = doc.RootElement.GetProperty(coinId).GetProperty("usd").GetDouble()
        };
    }

    public async Task<KeyValueModel<DateOnly, double>> GetExchangeRateOnDateAsync(string ticker, DateOnly ts, CancellationToken cancellationToken = default)
    {
        var formattedDate = ts.ToString("dd-MM-yyyy");
        var coinId = TickerMapper.GetIdentifierByTicker(ticker);
        
        var url = $"{_settings.BaseUrl}/coins/{coinId}/history?date={formattedDate}&x_cg_demo_api_key={_settings.Key}";

        var client = _clientFactory.CreateClient(HttpClients.CoinGeckoClient);

        var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        using var doc = JsonDocument.Parse(json);

        return new KeyValueModel<DateOnly, double>
        {
            Key = ts,
            Value = doc.RootElement.GetProperty("market_data").GetProperty("current_price").GetProperty("usd").GetDouble()
        };
    }

    public async Task<List<KeyValueModel<DateTime, double>>> GetHistoricalRatesAsync(
        string ticker, DateOnly fromInclusive, DateOnly toExclusive, CancellationToken cancellationToken = default)
    {
        var fromUnix = new DateTimeOffset(fromInclusive, new TimeOnly(0, 0, 0), TimeSpan.Zero).ToUnixTimeSeconds();
        var toUnix = new DateTimeOffset(toExclusive, new TimeOnly(0, 0, 0), TimeSpan.Zero).ToUnixTimeSeconds();

        var coinId = TickerMapper.GetIdentifierByTicker(ticker);

        var url = $"{_settings.BaseUrl}/coins/{coinId}/market_chart/range?vs_currency=usd&from={fromUnix}&to={toUnix}&x_cg_demo_api_key={_settings.Key}";

        var client = _clientFactory.CreateClient(HttpClients.CoinGeckoClient);

        var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        using var doc = JsonDocument.Parse(json);

        var result = new List<KeyValueModel<DateTime, double>>();

        foreach (var item in doc.RootElement.GetProperty("prices").EnumerateArray())
        {
            var timestamp = item[0].GetInt64();
            var rate = item[1].GetDouble();

            var utcDateTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;
            
            var dateOnly = DateOnly.FromDateTime(utcDateTime);
            if (dateOnly < toExclusive)
            {
                result.Add(new KeyValueModel<DateTime, double> { Key = utcDateTime, Value = rate });
            }         
        }

        return result;
    }
}
