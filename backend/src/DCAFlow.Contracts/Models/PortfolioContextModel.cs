namespace DCAFlow.Contracts.Models;

public class PortfolioContextModel
{
    public TimeFrameModel TimeFrame { get; set; }

    public List<string> Tickers { get; set; }

    public Dictionary<string, List<KeyValueModel<DateOnly, double>>> ExchangeRates { get; set; }
}
