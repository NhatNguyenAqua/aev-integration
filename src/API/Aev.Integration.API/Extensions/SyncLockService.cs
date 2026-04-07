namespace Aev.Integration.API.Extensions;

public sealed class SyncLockService
{
    private readonly Lock _syncLock = new();

    public void ExecuteWithLock(Action action)
    {
        lock (_syncLock)
        {
            action();
        }
    }

    public T ExecuteWithLock<T>(Func<T> func)
    {
        lock (_syncLock)
        {
            return func();
        }
    }
}
