using DCAFlow.Core.Common;

namespace DCAFlow.Core.ExchangeRates;

public interface IExchangeRateProvider
{
    public Task<double> GetCurrentExchageRateAsync(string ticker, CancellationToken cancellationToken = default);

    public Task<KeyValueModel<DateOnly, double>> GetExchangeRateOnDateAsync(string ticker, DateOnly ts, CancellationToken cancellationToken = default);

    public Task<List<KeyValueModel<DateTime, double>>> GetHistoricalRatesAsync(string ticker, DateOnly fromInclusive, DateOnly toExclusive, CancellationToken cancellationToken = default);
}
