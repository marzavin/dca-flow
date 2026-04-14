namespace DCAFlow.Contracts.Documents;

public class ExchangeRateDocument : DocumentBase
{
    public int DayNumber { get; set; }

    public string Ticker { get; set; }

    public double Rate { get; set; }
}
