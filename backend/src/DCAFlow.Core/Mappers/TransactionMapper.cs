using DCAFlow.Contracts.Documents;
using DCAFlow.Contracts.Enums;
using DCAFlow.Contracts.Models;

namespace DCAFlow.Core.Mappers;

public static class TransactionMapper
{
    public static TransactionModel Map(TransactionDocument document)
    {
        if (document is null)
        {
            return null;
        }

        return new TransactionModel 
        {
            Id = document.Id,
            Timestamp = document.Timestamp,
            Ticker = document.Ticker,
            Type = (TransactionType)document.Type,
            Amount = document.Amount,
            Cost = document.Cost
        };
    }
}
