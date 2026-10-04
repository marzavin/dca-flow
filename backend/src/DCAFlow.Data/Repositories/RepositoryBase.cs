using LiteDB;
using SideEffect.Data;

namespace DCAFlow.Data.Repositories;

public abstract class RepositoryBase<TEntity>
    where TEntity : EntityBase
{
    protected abstract string CollectionName { get; }

    protected LiteDatabase Database { get; private set; }

    protected RepositoryBase(LiteDatabase database)
    {
        Database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public Task<List<TEntity>> SearchAsync(EntityFilterBase<TEntity> filter, CancellationToken cancellationToken = default)
    {
        if (filter is null)
        {
            return GetAllAsync(cancellationToken);
        }

        var entities = Database.GetCollection<TEntity>(CollectionName).Find(filter.ToSearchExpression()).ToList();
        return Task.FromResult(entities);
    }

    public Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = Database.GetCollection<TEntity>(CollectionName).FindAll().ToList();
        return Task.FromResult(entities);
    }

    public Task<TEntity> GetEntityByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = Database.GetCollection<TEntity>(CollectionName).FindById(id);
        return Task.FromResult(entity);
    }

    public Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Database.GetCollection<TEntity>(CollectionName).Insert(entity);
        return Task.CompletedTask;
    }

    public Task InsertManyAsync(List<TEntity> entities, CancellationToken cancellationToken = default)
    {
        Database.GetCollection<TEntity>(CollectionName).Insert(entities);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return DeleteEntityByIdAsync(entity.Id, cancellationToken);
    }

    public Task DeleteEntityByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        Database.GetCollection<TEntity>(CollectionName).Delete(id);
        return Task.CompletedTask;
    }
}
