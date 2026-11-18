using DCAFlow.Core.ExchangeRates;
using DCAFlow.Core.Helpers;
using DCAFlow.Core.Transactions.Mappers;
using DCAFlow.Core.Transactions.Models;
using DCAFlow.Data.Entities;
using DCAFlow.Data.Filters;
using DCAFlow.Data.Repositories;
using SideEffect.Extensions;

namespace DCAFlow.Core.Transactions;

public sealed class TransactionService
{
    private readonly TransactionRepository _transactionRepository;

    private readonly ExchangeRateStorage _exchangeRateStorage;

    public TransactionService(TransactionRepository transactionRepository, ExchangeRateStorage exchangeRateStorage)
    {
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _exchangeRateStorage = exchangeRateStorage ?? throw new ArgumentNullException(nameof(exchangeRateStorage));
    }

    public async Task AddTransactionAsync(TransactionModel transaction, CancellationToken cancellationToken = default)
    {
        var document = new TransactionEntity
        {
            Tags = TagHelper.NormalizeTags(transaction.Tags),
            Ticker = transaction.Ticker,
            Timestamp = transaction.Timestamp.ToUniversalTime(),
            Amount = transaction.Amount,
            Cost = transaction.Cost,
            Type = (int)transaction.Type,
            Network = transaction.Network
        };

        await _transactionRepository.InsertAsync(document, cancellationToken);
    }

    public async Task DeleteTransactionAsync(int transactionId, CancellationToken cancellationToken = default)
    {
        await _transactionRepository.DeleteEntityByIdAsync(transactionId, cancellationToken);
    }

    public async Task<List<TransactionViewModel>> GetTransactionsAsync(string tag, CancellationToken cancellationToken = default)
    {
        var filter = new TransactionFilter { TagEq = TagHelper.NormalizeTag(tag) };
        var transactions = await _transactionRepository.SearchAsync(filter, cancellationToken);

        return transactions.IsEmpty()
            ? []
            : [.. transactions.Select(TransactionMapper.Map)];
    }
}
