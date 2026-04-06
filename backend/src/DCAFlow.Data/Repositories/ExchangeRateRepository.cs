using DCAFlow.Contracts.Documents;
using LiteDB;

namespace DCAFlow.Data.Repositories;

public class ExchangeRateRepository
{
    private readonly LiteDatabase _database;

    public ExchangeRateRepository(LiteDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public ExchangeRateDocument GetRateOnDate(string ticker, DateTime ts)
    {
        var collection = _database.GetCollection<ExchangeRateDocument>("rates");

        return collection.Find(x => x.Ticker == ticker && x.Timestamp == ts).FirstOrDefault();
    }

    public List<ExchangeRateDocument> GetHistoricalRates(string ticker, DateTime from, DateTime to)
    {
        var collection = _database.GetCollection<ExchangeRateDocument>("rates");

        return [.. collection.Find(x => x.Ticker == ticker && x.Timestamp >= from && x.Timestamp <= to)];
    }

    public void InsertRates(List<ExchangeRateDocument> exchangeRates)
    {
        var collection = _database.GetCollection<ExchangeRateDocument>("rates");

        foreach (var exchangeRate in exchangeRates)
        {
            collection.Insert(exchangeRate);
        }
    }
}
