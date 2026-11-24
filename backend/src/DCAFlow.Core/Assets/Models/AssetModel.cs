namespace DCAFlow.Core.Assets.Models;

public class AssetModel
{
    public string Ticker { get; set; }

    public double Amount { get; set; }

    public double MarketPrice { get; set; }

    public double AverageBuyPrice { get; set; }

    public double TotalInvestments { get; set; }

    public double TotalHoldings { get; set; }

    public double ProfitAndLoss { get; set; }

    public double ReturnOfInvestments { get; set; }
}
