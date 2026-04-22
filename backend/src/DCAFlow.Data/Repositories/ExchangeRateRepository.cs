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

    public ExchangeRateDocument GetRateOnDate(string ticker, DateOnly timestamp)
    {
        var collection = _database.GetCollection<ExchangeRateDocument>("rates");

        return collection.Find(x => x.Ticker == ticker && x.Timestamp == timestamp).FirstOrDefault();
    }

    public List<ExchangeRateDocument> GetHistoricalDailyRates(string ticker, DateOnly from, DateOnly to)
    {
        var collection = _database.GetCollection<ExchangeRateDocument>("rates");

        return [.. collection.Find(x => x.Ticker == ticker && x.Timestamp >= from && x.Timestamp <= to)];
    }

    public void InsertRates(List<ExchangeRateDocument> exchangeRates)
    {
        var collection = _database.GetCollection<ExchangeRateDocument>("rates");
        collection.Insert(exchangeRates);
    }
}
