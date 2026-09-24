using DCAFlow.Data.Entities;
using LiteDB;

namespace DCAFlow.Data.Repositories;

public class PortfolioRepository : RepositoryBase<PortfolioEntity>
{
    protected override string CollectionName => "portfolios";

    public PortfolioRepository(LiteDatabase database)
        : base(database)
    { }
}
