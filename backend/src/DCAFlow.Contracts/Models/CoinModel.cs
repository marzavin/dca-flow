namespace DCAFlow.Contracts.Models;

public class CoinModel
{
    public string Ticker { get; set; }

    public string Name { get; set; }

    public List<string> Networks { get; set; }

    public string CoinGeckoId { get; set; }
}
