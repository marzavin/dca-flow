namespace DCAFlow.Contracts.Documents;

public class ExchangeRateDocument : DocumentBase
{
    public DateTime Timestamp { get; set; }

    public string Ticker { get; set; }

    public double Rate { get; set; }
}
