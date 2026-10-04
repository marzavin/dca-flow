using DCAFlow.Data.Entities;
using SideEffect.Data;
using System.Linq.Expressions;

namespace DCAFlow.Data.Filters;

public class TransactionFilter : EntityFilterBase<TransactionEntity>
{
    public int? PortfolioIdEq { get; set; }

    public override Expression<Func<TransactionEntity, bool>> ToSearchExpression()
    {
        return x => PortfolioIdEq == null || PortfolioIdEq == x.PortfolioId;
    }
}
