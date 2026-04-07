namespace Aev.Integration.BuildingBlocks.Domain;

public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    private int _version;
    public int Version => _version;

    protected void IncrementVersion() => _version++;
}
