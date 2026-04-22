using DCAFlow.Contracts.Documents;
using LiteDB;

namespace DCAFlow.Data.Repositories;

public class TransactionRepository
{
    private const string COLLECTION_NAME = "transactions";

    private readonly LiteDatabase _database;

    public TransactionRepository(LiteDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public List<TransactionDocument> GetPortfolioTransactions(int portfolioId)
    {
        var collection = _database.GetCollection<TransactionDocument>(COLLECTION_NAME);

        return [.. collection.Find(x => x.PortfolioId == portfolioId)];
    }

    public void Insert(TransactionDocument document)
    {
        var collection = _database.GetCollection<TransactionDocument>(COLLECTION_NAME);

        collection.Insert(document);
    }

    public void Delete(int id)
    {
        var collection = _database.GetCollection<TransactionDocument>(COLLECTION_NAME);

        collection.Delete(id);
    }
}
