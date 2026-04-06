namespace DCAFlow.Services;

public static class TickerMapper
{
    public static string GetIdentifierByTicker(string ticker)
    {
        switch (ticker.ToUpper())
        {
            case "BTC": return "bitcoin";
            case "ETH": return "ethereum";
            case "SOL": return "solana";
            default:
                break;
        }

        throw new NotSupportedException($"Ticker '{ticker}' is not supported.");
    }

    public static string GetTickerByIdentifier(string identifier)
    {
        switch (identifier.ToLower())
        {
            case "bitcoin": return "BTC";
            case "ethereum": return "ETH";
            case "solana": return "SOL";
            default:
                break;
        }

        throw new NotSupportedException($"Identifier '{identifier}' is not supported.");
    }
}
