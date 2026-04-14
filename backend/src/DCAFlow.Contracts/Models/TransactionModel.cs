using DCAFlow.Contracts.Enums;

namespace DCAFlow.Contracts.Models;

public class TransactionModel
{
    public int Id { get; set; }

    public int PortfolioId { get; set; }

    public DateTime Timestamp { get; set; }

    public string Ticker { get; set; }

    public TransactionType Type { get; set; }

    public double? Cost { get; set; }

    public double Amount { get; set; }
}
