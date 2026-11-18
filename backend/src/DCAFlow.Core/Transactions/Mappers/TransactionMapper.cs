using DCAFlow.Core.Transactions.Enums;
using DCAFlow.Core.Transactions.Models;
using DCAFlow.Data.Entities;

namespace DCAFlow.Core.Transactions.Mappers;

public static class TransactionMapper
{
    public static TransactionViewModel Map(TransactionEntity entity)
    {
        if (entity is null)
        {
            return null;
        }

        return new TransactionViewModel
        {
            Id = entity.Id,
            Tags = entity.Tags,
            Ticker = entity.Ticker,
            Network = entity.Network,
            Amount = entity.Amount,
            Cost = entity.Cost,
            Type = (TransactionType)entity.Type,
            Timestamp = entity.Timestamp.ToUniversalTime()
        };
    }
}
