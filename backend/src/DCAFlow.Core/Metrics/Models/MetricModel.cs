using DCAFlow.Contracts.Models;
using DCAFlow.Core.Metrics.Enums;

namespace DCAFlow.Core.Metrics.Models;

public class MetricModel
{
    public MetricType Type { get; set; }

    public double? Value { get; set; }

    public List<KeyValueModel<DateOnly, double?>> Trend { get; set; }
}
