using DCAFlow.Contracts.Models;

namespace DCAFlow.Contracts.Interfaces;

public interface IExchangeRateProvider
{
    public Task<double> GetCurrentExchageRateAsync(string ticker, CancellationToken cancellationToken = default);

    public Task<KeyValueModel<DateOnly, double>> GetExchangeRateOnDateAsync(string ticker, DateOnly ts, CancellationToken cancellationToken = default);

    public Task<List<KeyValueModel<DateTime, double>>> GetHistoricalRatesAsync(string ticker, DateOnly fromInclusive, DateOnly toExclusive, CancellationToken cancellationToken = default);
}
