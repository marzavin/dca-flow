using SideEffect.Data;

namespace DCAFlow.Data.Entities;

public class ExchangeRateEntity : EntityBase
{
    public DateOnly Timestamp { get; set; }

    public string Ticker { get; set; }

    public double Rate { get; set; }
}
