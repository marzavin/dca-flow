using DCAFlow.Contracts.Enums;

namespace DCAFlow.Core.Helpers.CSV;

public class TransactionRecord
{
    public DateTime Timestamp { get; set; }

    public string Ticker { get; set; }

    public string Network { get; set; }

    public TransactionType Type { get; set; }

    public double Cost { get; set; }

    public double Amount { get; set; }
}
