using DCAFlow.Contracts.Models;

namespace DCAFlow.Web.Services;

public interface IExchangeRateProvider
{
    public Task<KeyValueModel<DateOnly, double>> GetExchangeRateOnDateAsync(string ticker, DateOnly ts, CancellationToken cancellationToken = default);

    public Task<KeyValueModel<DateOnly, double>> GetCurrentExchageRateAsync(string ticker, CancellationToken cancellationToken = default);

    public Task<List<KeyValueModel<DateOnly, double>>> GetHistoricalRatesAsync(string ticker, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
}
