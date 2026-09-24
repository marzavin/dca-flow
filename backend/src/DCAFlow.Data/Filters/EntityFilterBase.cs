using System.Linq.Expressions;

namespace DCAFlow.Data.Filters;

public abstract class EntityFilterBase<TEntity>
{
    public string Keyword { get; set; }

    public abstract Expression<Func<TEntity, bool>> ToSearchExpression();
}
