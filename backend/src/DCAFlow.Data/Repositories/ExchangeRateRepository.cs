using DCAFlow.Data.Entities;
using LiteDB;

namespace DCAFlow.Data.Repositories;

public class ExchangeRateRepository : RepositoryBase<ExchangeRateEntity>
{
    protected override string CollectionName => "rates";

    public ExchangeRateRepository(LiteDatabase database)
        : base(database)
    { }
}
