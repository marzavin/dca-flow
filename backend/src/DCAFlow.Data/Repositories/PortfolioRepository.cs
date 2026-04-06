using DCAFlow.Contracts.Documents;
using LiteDB;

namespace DCAFlow.Data.Repositories;

public class PortfolioRepository
{
    private readonly LiteDatabase _database;

    public PortfolioRepository(LiteDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public List<PortfolioDocument> GetPortfolios()
    {
        var collection = _database.GetCollection<PortfolioDocument>("portfolios");

        return [.. collection.FindAll()];
    }

    public PortfolioDocument GetPortfolioById(int portfolioId)
    {
        var collection = _database.GetCollection<PortfolioDocument>("portfolios");

        return collection.Find(x => x.Id == portfolioId).FirstOrDefault();
    }
}
