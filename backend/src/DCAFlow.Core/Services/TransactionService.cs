using DCAFlow.Contracts.Documents;
using DCAFlow.Contracts.Models;
using DCAFlow.Data.Repositories;

namespace DCAFlow.Core.Services;

public sealed class TransactionService
{
    private readonly TransactionRepository _transactionRepository;

    public TransactionService(TransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
    }

    public Task AddTransactionAsync(TransactionModel transaction, CancellationToken cancellationToken = default)
    {
        var document = new TransactionDocument
        {
            PortfolioId = transaction.PortfolioId,
            Ticker = transaction.Ticker,
            Timestamp = transaction.Timestamp.ToUniversalTime(),
            Amount = transaction.Amount,
            Cost = transaction.Cost,
            Type = (int)transaction.Type
        };

        _transactionRepository.Insert(document);

        return Task.CompletedTask;
    }

    public Task DeleteTransactionAsync(int transactionId, CancellationToken cancellationToken = default)
    {
        _transactionRepository.Delete(transactionId);

        return Task.CompletedTask;
    }
}
