using DCAFlow.Data.Entities;
using System.Linq.Expressions;

namespace DCAFlow.Data.Filters;

public class ExchangeRateFilter : EntityFilterBase<ExchangeRateEntity>
{
    public string TickerEq { get; set; }

    public DateOnly? TimestampEq { get; set; }

    public DateOnly? TimestampGte { get; set; }

    public DateOnly? TimestampLte { get; set; }

    public override Expression<Func<ExchangeRateEntity, bool>> ToSearchExpression()
    {
        return x => (TickerEq == null || TickerEq == x.Ticker)
            && (TimestampEq == null || TimestampEq == x.Timestamp)
            && (TimestampGte == null || TimestampGte <= x.Timestamp)
            && (TimestampLte == null || TimestampLte >= x.Timestamp);
    }
}
