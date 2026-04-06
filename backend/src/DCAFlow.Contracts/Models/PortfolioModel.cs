namespace DCAFlow.Contracts.Models;

public class PortfolioModel
{
    public int Id { get; set; }

    public string Name { get; set; }

    public double TotalInvested { get; set; }

    public double HoldingsValue { get; set; }

    public double TotalReturn { get; set; }

    public List<AssetModel> Assets { get; set; }

    public List<FractionModel> Allocation { get; set; }

    public List<KeyValueModel<DateOnly, double>> TotalInvestedTimeline { get; set; }

    public List<KeyValueModel<DateOnly, double>> HoldingsValueTimeline { get; set; }
}
