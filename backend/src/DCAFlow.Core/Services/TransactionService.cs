using DCAFlow.Contracts.Models;
using DCAFlow.Data.Entities;
using DCAFlow.Data.Repositories;

namespace DCAFlow.Core.Services;

public sealed class TransactionService
{
    private readonly TransactionRepository _transactionRepository;

    public TransactionService(TransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
    }

    public async Task AddTransactionAsync(TransactionModel transaction, CancellationToken cancellationToken = default)
    {
        var document = new TransactionEntity
        {
            PortfolioId = transaction.PortfolioId,
            Ticker = transaction.Ticker,
            Timestamp = transaction.Timestamp.ToUniversalTime(),
            Amount = transaction.Amount,
            Cost = transaction.Cost,
            Type = (int)transaction.Type
        };

        await _transactionRepository.InsertAsync(document, cancellationToken);
    }

    public async Task DeleteTransactionAsync(int transactionId, CancellationToken cancellationToken = default)
    {
        await _transactionRepository.DeleteEntityByIdAsync(transactionId, cancellationToken);
    }
}
