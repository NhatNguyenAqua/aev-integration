using Aev.Integration.BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore;

namespace Aev.Integration.BuildingBlocks.Infrastructure.Persistence;

public abstract class BaseRepository<TAggregateRoot, TId, TContext>(TContext context)
    : IRepository<TAggregateRoot, TId>
    where TAggregateRoot : AggregateRoot<TId>
    where TId : notnull
    where TContext : BaseDbContext
{
    protected TContext Context => context;
    protected DbSet<TAggregateRoot> DbSet => context.Set<TAggregateRoot>();

    public virtual async Task<TAggregateRoot?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
        => await DbSet.FindAsync([id], cancellationToken);

    public virtual async Task AddAsync(TAggregateRoot aggregateRoot, CancellationToken cancellationToken = default)
        => await DbSet.AddAsync(aggregateRoot, cancellationToken);

    public virtual Task UpdateAsync(TAggregateRoot aggregateRoot, CancellationToken cancellationToken = default)
    {
        DbSet.Update(aggregateRoot);
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(TAggregateRoot aggregateRoot, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(aggregateRoot);
        return Task.CompletedTask;
    }
}
