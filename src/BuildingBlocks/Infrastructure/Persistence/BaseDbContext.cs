using Aev.Integration.BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Aev.Integration.BuildingBlocks.Infrastructure.Persistence;

public abstract class BaseDbContext(DbContextOptions options) : DbContext(options), IUnitOfWork
{
    private IDbContextTransaction? _currentTransaction;

    public bool HasActiveTransaction => _currentTransaction != null;

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
        return _currentTransaction;
    }

    public async Task CommitTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        if (transaction != _currentTransaction)
            throw new InvalidOperationException($"Transaction {transaction.TransactionId} is not current");

        try
        {
            await SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            RollbackTransaction();
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public void RollbackTransaction()
    {
        try
        {
            _currentTransaction?.Rollback();
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        DispatchDomainEventsBeforeSave();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void DispatchDomainEventsBeforeSave()
    {
        var entities = ChangeTracker.Entries()
            .Where(e => e.Entity is Entity<Guid> entity && entity.DomainEvents.Count > 0)
            .Select(e => (Entity<Guid>)e.Entity)
            .ToList();

        foreach (var entity in entities)
            entity.ClearDomainEvents();
    }
}
