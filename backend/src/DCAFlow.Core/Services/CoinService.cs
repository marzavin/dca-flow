using DCAFlow.Contracts.Models;

namespace DCAFlow.Core.Services;

public sealed class CoinService
{
    public List<CoinModel> GetSupportedCoins()
    {
        return
        [
            new()
            {
                Ticker = "BTC",
                Name = "Bitcoin",
                Networks = ["Bitcoin"],
                CoinGeckoId = "bitcoin"
            },
            new()
            {
                Ticker = "ETH",
                Name = "Ethereum",
                Networks = ["Ethereum", "Arbitrum"],
                CoinGeckoId = "ethereum"
            },
            new()
            {
                Ticker = "SOL",
                Name = "Solana",
                Networks = ["Solana"],
                CoinGeckoId = "solana"
            },
            new()
            {
                Ticker = "BAT",
                Name = "Basic Attention Token",
                Networks = ["Solana"],
                CoinGeckoId = "basic-attention-token"
            },
            new()
            {
                Ticker = "RAY",
                Name = "Raydium",
                Networks = ["Solana"],
                CoinGeckoId = "raydium"
            }
        ];
    }
}
