using DCAFlow.Contracts.Documents;
using LiteDB;

namespace DCAFlow.Data.Repositories;

public class TransactionRepository
{
    private readonly LiteDatabase _database;

    public TransactionRepository(LiteDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public List<TransactionDocument> GetPortfolioTransactions(int portfolioId)
    {
        var collection = _database.GetCollection<TransactionDocument>("transactions");

        return [.. collection.Find(x => x.PortfolioId == portfolioId)];
    }
}
