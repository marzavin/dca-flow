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

    public ExchangeRateDocument GetRateOnDate(string ticker, int dayNumber)
    {
        var collection = _database.GetCollection<ExchangeRateDocument>("rates");

        return collection.Find(x => x.Ticker == ticker && x.DayNumber == dayNumber).FirstOrDefault();
    }

    public List<ExchangeRateDocument> GetHistoricalDailyRates(string ticker, int fromDayNumber, int toDayNumber)
    {
        var collection = _database.GetCollection<ExchangeRateDocument>("rates");

        return [.. collection.Find(x => x.Ticker == ticker && x.DayNumber >= fromDayNumber && x.DayNumber <= toDayNumber)];
    }

    public void InsertRates(List<ExchangeRateDocument> exchangeRates)
    {
        var collection = _database.GetCollection<ExchangeRateDocument>("rates");
        collection.InsertBulk(exchangeRates);
    }
}
