using DCAFlow.Contracts.Models;

namespace DCAFlow.Services;

public interface IExchangeRateProvider
{
    public Task<ExchangeRateModel> GetExchangeRateOnDateAsync(string ticker, DateTime ts, CancellationToken cancellationToken = default);

    public Task<ExchangeRateModel> GetCurrentExchageRateAsync(string ticker, CancellationToken cancellationToken = default);

    public Task<List<ExchangeRateModel>> GetHistoricalRatesAsync(string ticker, DateTime from, DateTime to, CancellationToken cancellationToken = default);
}
