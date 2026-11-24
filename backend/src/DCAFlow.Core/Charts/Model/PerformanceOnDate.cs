namespace DCAFlow.Core.Charts.Model;

public class PerformanceOnDate
{
    public DateOnly Timestamp { get; set; }

    public double TotalInvestments { get; set; }

    public double TotalHoldings { get; set; }

    public double ProfitAndLoss { get; set; }
}
