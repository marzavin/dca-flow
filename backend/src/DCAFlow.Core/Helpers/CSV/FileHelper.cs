using CsvHelper;
using System.Globalization;

namespace DCAFlow.Core.Helpers.CSV;

public static class FileHelper
{
    public static Task<List<TransactionRecord>> ReadTransactionsAsync(Stream stream)
    {
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        
        var records = csv.GetRecords<TransactionRecord>()?.ToList() ?? [];
        
        return Task.FromResult(records);
    }
}
