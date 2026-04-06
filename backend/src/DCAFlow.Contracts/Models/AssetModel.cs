namespace DCAFlow.Contracts.Models;

public class AssetModel
{
    public string Ticker { get; set; }

    public double CurrentMarketPrice { get; set; }

    public double AverageBuyPrice { get; set; }

    public double TotalInvested { get; set; }

    public double TotalHoldings { get; set; }

    public double TotalReturn { get; set; }

    public double HoldingsValue { get; set; }
}
