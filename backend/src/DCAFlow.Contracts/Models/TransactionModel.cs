using DCAFlow.Contracts.Enums;

namespace DCAFlow.Contracts.Models;

public class TransactionModel
{
    public DateTime Timestamp { get; set; }

    public string Ticker { get; set; }

    public TransactionType Type { get; set; }

    public double Cost { get; set; }

    public double Amount { get; set; }
}
