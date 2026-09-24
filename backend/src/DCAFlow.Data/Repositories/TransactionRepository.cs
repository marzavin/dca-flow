using DCAFlow.Data.Entities;
using LiteDB;

namespace DCAFlow.Data.Repositories;

public class TransactionRepository : RepositoryBase<TransactionEntity>
{
    protected override string CollectionName => "transactions";

    public TransactionRepository(LiteDatabase database)
        : base(database)
    { }
}
