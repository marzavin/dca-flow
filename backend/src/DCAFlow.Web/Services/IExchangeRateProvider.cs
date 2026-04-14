using DCAFlow.Contracts.Models;

namespace DCAFlow.Web.Services;

public interface IExchangeRateProvider
{
    public Task<KeyValueModel<DateOnly, double>> GetCurrentExchageRateAsync(string ticker, CancellationToken cancellationToken = default);

    public Task<List<KeyValueModel<DateOnly, double>>> GetExchangeRatesFromDateAsync(string ticker, DateOnly from, CancellationToken cancellationToken = default);
}
