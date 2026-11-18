using DCAFlow.Core.Transactions.Enums;

namespace DCAFlow.Core.Transactions.Models;

public class TransactionModel
{
    public DateTime Timestamp { get; set; }

    public string Ticker { get; set; }

    public string Network { get; set; }

    public TransactionType Type { get; set; }

    public double Cost { get; set; }

    public double Amount { get; set; }

    public List<string> Tags { get; set; }
}
